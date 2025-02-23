using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ApprovedMultiSequenceLearningNew
{
    public class MultiSequenceLearning
    {
        public Predictor Run(List<Sequence> sequences)
        {
            Console.WriteLine($"Hello NeocortexApi! {nameof(MultiSequenceLearning)}");
            int inputBits = 300;
            int numColumns = 2048;

            //  CHANGED  – use the enhanced config & encoder:
            HtmConfig cfg = HelperMethods.FetchEnhancedHTMConfig(inputBits, numColumns);
            EncoderBase encoder = HelperMethods.GetEnhancedEncoder(inputBits);
            return RunExperiment(inputBits, cfg, encoder, sequences);
        }
        private Predictor RunExperiment(int inputBits, HtmConfig cfg, EncoderBase encoder, List<Sequence> sequences)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int maxMatchCnt = 0;
            var mem = new Connections(cfg);
            bool isInStableState = false;
            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();
            var numUniqueInputs = GetNumberOfInputs(sequences);
            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");
            TemporalMemory tm = new TemporalMemory();

            Console.WriteLine("************** START Predicting **************");

            HomeostaticPlasticityController hpc = new HomeostaticPlasticityController(mem, numUniqueInputs * 150, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)

                    Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else

                    Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                isInStableState = isStable;

            }, numOfCyclesToWaitOnChange: 50);


            SpatialPoolerMT sp = new SpatialPoolerMT(hpc);
            sp.Init(mem);
            tm.Init(mem);
            layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp);

            int[] prevActiveCols = new int[0];

            int cycle = 0;
            int matches = 0;

            var lastPredictedValues = new List<string>(new string[] { "0" });

            int maxCycles = 4000;

            // *** CHANGED *** – multiple newborn passes to stabilize SP:
            int trainingPasses = 3; // *** CHANGED *** (you can tweak the pass count)



           
            // *** CHANGED *** – NEWBORN STAGE: multiple passes training only SP
            for (int pass = 0; pass < trainingPasses && !isInStableState; pass++)
            {
                Console.WriteLine($"=== Newborn SP Training Pass {pass + 1} ===");



                for (int i = 0; i < maxCycles && isInStableState == false; i++)
                {
                    matches = 0;

                    cycle++;

                    Debug.WriteLine($"************** Newborn SP Cycle {cycle} **************");
                    Console.WriteLine($"************** Newborn SP Cycle {cycle} **************");

                    foreach (var inputs in sequences)
                    {
                        foreach (var input in inputs.data)
                        {
                            Debug.WriteLine($" -- {inputs.name} - {input} --");

                            var lyrOut = layer1.Compute(input, true);

                            if (isInStableState)
                                break;
                        }

                        if (isInStableState)
                            break;
                    }
                }
            }

            // Clear all learned patterns in the classifier.
            cls.ClearState();

            // We activate here the Temporal Memory algorithm.
            layer1.HtmModules.Add("tm", tm);

            //
            // Loop over all sequences.
            foreach (var sequenceKeyPair in sequences)
            {
                Debug.WriteLine($"************** Sequences {sequenceKeyPair.name} **************");
                Console.WriteLine($"************** Sequences {sequenceKeyPair.name} **************");

                int maxPrevInputs = sequenceKeyPair.data.Length - 1;

                List<string> previousInputs = new List<string>();

                previousInputs.Add("-1");

                //  The learning stage. In this stage, the SP is trained on the input patterns and the TM is trained on the SP output.
                for (int i = 0; i < maxCycles; i++)
                {
                    matches = 0;

                    cycle++;

                    Debug.WriteLine("");

                    Debug.WriteLine($"************** Cycle SP+TM{cycle} **************");
                    Console.WriteLine($"************** Cycle SP+TM {cycle} **************");

                    foreach (var input in sequenceKeyPair.data)
                    {
                        Debug.WriteLine($"************** {input} **************");

                        var lyrOut = layer1.Compute(input, true) as ComputeCycle;

                        var activeColumns = layer1.GetResult("sp") as int[];

                        previousInputs.Add(input.ToString());
                        if (previousInputs.Count > (maxPrevInputs + 1))
                            previousInputs.RemoveAt(0);

                       

                        if (previousInputs.Count < maxPrevInputs)
                            continue;

                        string key = GetKey(previousInputs, input, sequenceKeyPair.name);

                        List<Cell> actCells;

                        if (lyrOut.ActiveCells.Count == lyrOut.WinnerCells.Count)
                        {
                            actCells = lyrOut.ActiveCells;
                        }
                        else
                        {
                            actCells = lyrOut.WinnerCells;
                        }

                        cls.Learn(key, actCells.ToArray());

                        Debug.WriteLine($"Col  SDR: {Helpers.StringifyVector(lyrOut.ActivColumnIndicies)}");
                        Debug.WriteLine($"Cell SDR: {Helpers.StringifyVector(actCells.Select(c => c.Index).ToArray())}");

                        
                        if (lastPredictedValues.Contains(key))
                        {
                            matches++;
                            Debug.WriteLine($"Match. Actual value: {key} - Predicted value: {lastPredictedValues.FirstOrDefault(key)}.");
                        }
                        else
                            Debug.WriteLine($"Missmatch! Actual value: {key} - Predicted values: {String.Join(',', lastPredictedValues)}");

                        if (lyrOut.PredictiveCells.Count > 0)
                        {
                            // Get the predicted input values.
                            var predictedInputValues = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

                            foreach (var item in predictedInputValues)
                            {
                                Debug.WriteLine($"Current Input: {input} \t| Predicted Input: {item.PredictedInput} - {item.Similarity}");
                            }

                            lastPredictedValues = predictedInputValues.Select(v => v.PredictedInput).ToList();
                        }
                        else
                        {
                            Debug.WriteLine($"Nothing PREDICTED for next cycle.");
                            lastPredictedValues = new List<string>();
                        }
                    }

                    // This resets the learned state, so the first element starts allways from the beginning.
                    double maxPossibleAccuraccy = (double)((double)sequenceKeyPair.data.Length - 1) / (double)sequenceKeyPair.data.Length * 100.0;

                    double accuracy = (double)matches / (double)sequenceKeyPair.data.Length * 100.0;

                    Debug.WriteLine($"Cycle: {cycle}\nMatches={matches} of {sequenceKeyPair.data.Length}\nAccuracy = {accuracy}%");
                    Console.WriteLine($"Cycle: {cycle}\nMatches={matches} of {sequenceKeyPair.data.Length}\nAccuracy = {accuracy}%");

                    if (accuracy >= maxPossibleAccuraccy)
                    {
                        maxMatchCnt++;
                        Debug.WriteLine($"100% accuracy reched {maxMatchCnt} times.");

                        // If we have 20 repeats with 100% accuracy, we can assume that the algorithm is in the stable state.
                        if (maxMatchCnt >= 20)
                        {
                            sw.Stop();
                            Debug.WriteLine($"Sequence learned. The algorithm is in the stable state after 20 repeats with with accuracy {accuracy} of maximum possible {maxMatchCnt}. Elapsed sequence {sequenceKeyPair.name} learning time: {sw.Elapsed}.");
                            break;
                        }
                    }
                    else if (maxMatchCnt > 0)
                    {
                        Debug.WriteLine($"At 100% accuracy after {maxMatchCnt} repeats we get a drop of accuracy with accuracy {accuracy}. This indicates instable state. Learning will be continued.");
                        maxMatchCnt = 0;
                    }

                    // If the algorithm is not in the stable state, we need to reset the SP and TM.
                    tm.Reset(mem);
                }
            }

            Debug.WriteLine("************** END **************");

            return new Predictor(layer1, mem, cls);
        }


        /// <summary>
        /// Gets the number of inputs.
        /// </summary>
        /// <param name="sequences">Alle sequences.</param>
        /// <returns></returns>
        private int GetNumberOfInputs(List<Sequence> sequences)
        {
            int num = 0;

            foreach (var inputs in sequences)
            {
                //num += inputs.Value.Distinct().Count();
                num += inputs.data.Length;
            }

            return num;
        }
        /// <summary>
        /// Gets the key. The key is a combination of the previous inputs and the current input. 
        /// </summary>
        /// <param name="prevInputs"></param>
        /// <param name="input"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        private static string GetKey(List<string> prevInputs, double input, string sequence)
        {
            string key = String.Empty;

            for (int i = 0; i < prevInputs.Count; i++)
            {
                if (i > 0)
                    key += "-";
                key += (prevInputs[i]);
            }
            return $"{sequence}_{key}";
        }
    }
}


