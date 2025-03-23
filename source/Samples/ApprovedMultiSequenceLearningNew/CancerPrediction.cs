using NeoCortexApi.Classifiers;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ApprovedMultiSequenceLearningNew
{
    internal class CancerPrediction
    {
        // ------------------------------------------------------------------------------------
        //  NESTED DATA TYPES (merged from Sequence.cs and Report.cs)
        // ------------------------------------------------------------------------------------

        /// <summary>
        /// Equivalent to 'Sequence.cs'
        /// </summary>
        public class Sequence
        {
            public string name { get; set; }
            public char[] data { get; set; }
        }

        /// <summary>
        /// Equivalent to 'Report.cs'
        /// </summary>
        public class Report
        {
            public Report()
            {
                PredictionLog = new List<string>();
            }

            public string SequenceName { get; set; }
            public char[] SequenceData { get; set; }
            public List<string> PredictionLog { get; set; }
            public double Accuracy { get; set; }
        }

        // ------------------------------------------------------------------------------------
        //  MAIN ENTRY POINT (merged from Program.cs)
        // ------------------------------------------------------------------------------------


        public static void RunAll(string datasetFileName = "dataset_04.json", string testsetFileName = "test_02.json")
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            // 1) Read the training (dataset) sequences
            string datasetPath = Path.Combine(basePath, "dataset", datasetFileName);
            List<Sequence> sequences = ReadDataset(datasetPath);

            // 2) Read the test sequences
            string testsetPath = Path.Combine(basePath, "dataset", testsetFileName);
            List<Sequence> sequencesTest = ReadDataset(testsetPath);

            // 3) Train & test
            List<Report> reports = RunMultiSequenceLearningExperiment(sequences, sequencesTest);

            // 4) Save the final predictions/accuracy into a text report
            WriteReport(reports, basePath);
        }


        // ------------------------------------------------------------------------------------
        //  HIGH-LEVEL PIPELINE (similar to Program.cs)
        // ------------------------------------------------------------------------------------

        /// <summary>
        /// Orchestrates the MultiSequenceLearning pipeline: trains on the given sequences
        /// and then tests with separate test sequences.
        /// </summary>
        private static List<Report> RunMultiSequenceLearningExperiment(List<Sequence> sequences, List<Sequence> sequencesTest)
        {
            var reports = new List<Report>();
            // "MultiSequenceLearning" logic inlined as private static methods below
            var predictor = RunMultiSequenceLearning(sequences);

            // Test each sequence
            foreach (Sequence item in sequencesTest)
            {
                var report = new Report
                {
                    SequenceName = item.name,
                    SequenceData = item.data
                };
                Console.WriteLine("*******#######*******");
                Console.WriteLine($"Test Sequence  {item.name}: {string.Join("", item.data)}");
                double accuracy = PredictNextElement(predictor, item.data, report);
                report.Accuracy = accuracy;
                reports.Add(report);

                Console.WriteLine($"Accuracy for {item.name} sequence: {accuracy}%");
                Console.WriteLine("********############********");
            }

            return reports;
        }

        /// <summary>
        /// Runs the predictor on a test sequence to measure accuracy.
        /// </summary>
        private static double PredictNextElement(Predictor predictor, char[] list, Report report)
        {
            int matchCount = 0;
            int predictions = 0;
            List<string> logs = new List<string>();

            // Reset the predictor's internal state between sequences
            predictor.Reset();

            // For each pair of (current, next) in the test data
            for (int i = 0; i < list.Length - 1; i++)
            {
                char current = list[i];
                char next = list[i + 1];

                logs.Add(PredictElement(predictor, current, next, ref matchCount));
                predictions++;
            }

            report.PredictionLog = logs;
            return CalculateAccuracy(matchCount, predictions);
        }

        /// <summary>
        /// Feeds the current char to the predictor, checks its predicted next char, and logs the result.
        /// </summary>
        private static string PredictElement(Predictor predictor, char current, char next, ref int matchCount)
        {
            Console.WriteLine($"Input: {current}");

            int currentIdx = CharToIndex(current);
            var predictions = predictor.Predict(currentIdx);

            if (predictions.Any())
            {
                // Sort by highest similarity
                var best = predictions.OrderByDescending(p => p.Similarity).First();

                // "S1_A"
                string predictedKey = best.PredictedInput;
                string[] parts = predictedKey.Split('_');
                string predictedSeq = parts[0];
                char predictedChar = parts[1][0];  // if "AB", just take the first char

                Console.WriteLine($"Predicted Sequence: {predictedSeq} - Predicted next element: {predictedChar}");

                if (predictedChar == next)
                    matchCount++;

                return $"Input: {current}, Predicted Sequence: {predictedSeq}, Predicted next element: {predictedChar}";
            }
            else
            {
                Console.WriteLine("No Prediction");
                return $"Input: {current}, No Prediction";
            }
        }

        /// <summary>
        /// Computes simple percentage accuracy = (# correct) / (# predictions).
        /// </summary>
        private static double CalculateAccuracy(int matchCount, int predictions)
        {
            if (predictions == 0) return 0.0;
            return (double)matchCount / predictions * 100.0;
            Console.WriteLine("*******#######*******");
        }

        /// <summary>
        /// Saves the final predictions and accuracies to a text file.
        /// </summary>
        private static void WriteReport(List<Report> reports, string basePath)
        {
            string reportFolder = EnsureDirectory(Path.Combine(basePath, "report"));
            string reportPath = Path.Combine(reportFolder, $"report_{DateTime.Now.Ticks}.txt");

            using (StreamWriter sw = File.CreateText(reportPath))
            {
                foreach (var r in reports)
                {
                    sw.WriteLine("**************");
                    sw.WriteLine($"Using test sequence: {r.SequenceName} -> {string.Join("-", r.SequenceData)}");

                    foreach (string log in r.PredictionLog)
                    {
                        sw.WriteLine($"\t{log}");
                    }

                    sw.WriteLine($"\tAccuracy: {r.Accuracy}%");
                    sw.WriteLine("**************");
                }
            }
        }

        
        /// <summary>
        /// Trains a Spatial Pooler & Temporal Memory and returns a predictor used for inference.
        /// </summary>
        private static Predictor RunMultiSequenceLearning(List<Sequence> sequences)
        {
            Console.WriteLine($"Hello NeocortexApi! Cancer Prediction By Git Gurduans");

            // Fewer bits/columns for speed
            int inputBits = 100;
            int numColumns = 1024;

            // 1) Build HTM config & an encoder
            HtmConfig cfg = FetchHTMConfig(inputBits, numColumns);
            EncoderBase encoder = GetEncoder(inputBits);

            // 2) Actually do the training and return the Predictor
            return RunExperiment(cfg, encoder, sequences);
        }

        /// <summary>
        /// Does the SP training pass, then SP+TM pass, then returns the final predictor.
        /// </summary>
        private static Predictor RunExperiment(HtmConfig cfg, EncoderBase encoder, List<Sequence> sequences)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Create the HTM connections & layer
            var mem = new Connections(cfg);
            var layer1 = new CortexLayer<object, object>("L1");

            // 1) Spatial Pooler
            SpatialPoolerMT sp = new SpatialPoolerMT();
            sp.Init(mem);

            // 2) Temporal Memory
            TemporalMemory tm = new TemporalMemory();
            tm.Init(mem);

            // 3) Classifier
            var cls = new HtmClassifier<string, ComputeCycle>();

            // Add modules to layer
            layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp);

            int matches = 0;

            Console.WriteLine("************** START Predicting **************");

            // Very short training loops
            int maxCycles = 100;
            int spTrainingPasses = 2;
            int tmTrainingPasses = 2;

            // -------------------------------------------------
            // 1) SP-only pass
            // -------------------------------------------------
            Console.WriteLine($"=== Newborn SP Training Pass 1 ===");
            for (int i = 0; i < spTrainingPasses; i++)
            {
                for (int cycle = 0; cycle < maxCycles; cycle++)
                {
                    Debug.WriteLine($"************** Newborn SP Cycle {cycle} **************");
                    Console.WriteLine($"************** Newborn SP Cycle {cycle} **************");

                    foreach (var seq in sequences)
                    {
                        foreach (char ch in seq.data)
                        {
                            int inputVal = CharToIndex(ch);
                            layer1.Compute(inputVal, learn: true); // SP learning only
                        }
                    }
                }
            }

            // Clear classifier after SP stage
            cls.ClearState();

            // Add TM module
            layer1.HtmModules.Add("tm", tm);

            // -------------------------------------------------
            // 2) SP+TM pass
            // -------------------------------------------------
            for (int pass = 0; pass < tmTrainingPasses; pass++) // *** CHANGED FOR HIGHER ACCURACY ***
            {
                Console.WriteLine($"=== SP+TM Training Pass {pass + 1} ===");

                for (int cycle = 0; cycle < maxCycles; cycle++)
                {
                    foreach (var seq in sequences)
                    {
                        Debug.WriteLine($"************** Sequences {seq.name} **************");
                        Console.WriteLine($"************** Sequences {seq.name} **************");
                        int maxPrevInputs = seq.data.Length - 1;

                        List<string> previousInputs = new List<string>();

                        previousInputs.Add("-1");


                        foreach (char ch in seq.data)
                        {
                            Debug.WriteLine($"************** {ch} **************");
                            //Console.WriteLine($"************** {ch} **************");

                            int inputVal = CharToIndex(ch);
                            var cyOut = layer1.Compute(inputVal, learn: true) as ComputeCycle;

                            // Build a label like "S1_A"
                            string key = $"{seq.name}_{ch}";

                            // Use either active or winner cells
                            List<Cell> actCells = (cyOut.ActiveCells.Count == cyOut.WinnerCells.Count)
                                                  ? cyOut.ActiveCells
                                                  : cyOut.WinnerCells;

                            cls.Learn(key, actCells.ToArray());
                        }

                    }
                }

            }

            sw.Stop();
            Console.WriteLine($"Training completed in {sw.Elapsed}.");

            // Return a predictor that uses the layer, memory, and classifier
            return new Predictor(layer1, mem, cls);
        }



  

        /// <summary>
        /// Returns the default HTM config used to build the SP/TM pipeline.
        /// </summary>
        private static HtmConfig FetchHTMConfig(int inputBits, int numColumns)
        {
            return new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                Random = new ThreadSafeRandom(42),
                CellsPerColumn = 25,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                MaxBoost = 10.0,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = 0.75,
                MaxSynapsesPerSegment = (int)(0.02 * numColumns),
                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,
                PredictedSegmentDecrement = 0.1
            };
        }

        /// <summary>
        /// Returns a ScalarEncoder for input in range [0..26], wide enough for A..Z.
        /// </summary>
        private static EncoderBase GetEncoder(int inputBits)
        {
            var settings = new Dictionary<string, object>
            {
                { "W", 15 },
                { "N", inputBits },
                { "Radius", -1.0 },
                { "MinVal", 0.0 },
                { "MaxVal", 26.0 },
                { "ClipInput", false },
                { "Periodic", false },
                { "Name", "scalar" }
            };

            return new ScalarEncoder(settings);
        }

        /// <summary>
        /// Reads a JSON file containing a List of Sequence objects.
        /// </summary>
        private static List<Sequence> ReadDataset(string path)
        {
            Console.WriteLine("Reading Sequence...");
            try
            {
                string fileContent = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<List<Sequence>>(fileContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read the dataset: {ex.Message}");
                return new List<Sequence>();
            }
        }

        /// <summary>
        /// Creates (or ensures existence of) the given directory path.
        /// </summary>
        private static string EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        // ------------------------------------------------------------------------------------
        //  HELPER: For char <-> index conversions
        // ------------------------------------------------------------------------------------

        private static int CharToIndex(char c)
        {
            c = char.ToUpperInvariant(c);
            return c - 'A';  // 'A' => 0, 'Z' => 25
        }

        // ------------------------------------------------------------------------------------
        //  END
        // ------------------------------------------------------------------------------------

    }
}
