using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApprovedMultiSequenceLearningNew
{
    internal class CancerPrediction
    {
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


        // ------------------------------------------------------------------------------------
        //  REPLACING HelperMethods: SCALAR ENCODER / READ-DATASET / ETC.
        // ------------------------------------------------------------------------------------

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


        ///////////////////////////////////////

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
