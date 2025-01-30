using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using ApprovedMultiSequenceLearningNew;
using Newtonsoft.Json;

namespace ApprovedMultiSequenceLearningNew
{
    public class HelperMethods
    {
        // Constants for default settings
        private const int DefaultRandomSeed = 42;
        private const double MaxScalarValue = 20.0;
        private const int DefaultCellsPerColumn = 25;
        private const double DefaultGlobalInhibitionDensity = 0.02;
        private const double DefaultPotentialRadiusFactor = 0.15;
        private const double DefaultMaxSynapsesPerSegmentFactor = 0.02;
        private const double DefaultMaxBoost = 10.0;
        private const int DefaultDutyCyclePeriod = 25;
        private const double DefaultMinPctOverlapDutyCycles = 0.75;
        private const int DefaultActivationThreshold = 15;
        private const double DefaultConnectedPermanence = 0.5;
        private const double DefaultPermanenceDecrement = 0.25;
        private const double DefaultPermanenceIncrement = 0.15;
        private const double DefaultPredictedSegmentDecrement = 0.1;

        /// <summary>
        /// HTM Config for creating Connections
        /// </summary>
       
        /// CHANGED  - using New method: FetchEnhancedHTMConfig
        public static HtmConfig FetchEnhancedHTMConfig(int inputBits, int numColumns)
        {
            return new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                Random = new ThreadSafeRandom(42),

                // *** CHANGED *** – changed from 25 to 32
                CellsPerColumn = 32,

                // *** CHANGED *** – use local inhibition
                GlobalInhibition = false,
                LocalAreaDensity = 0.02,

                // *** CHANGED *** – 2% active columns
                NumActiveColumnsPerInhArea = 0.02 * numColumns,

                // *** CHANGED *** – potential radius half of input for broader coverage
                PotentialRadius = (int)(0.5 * inputBits),

                MaxBoost = 10.0,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = 0.75,

                // *** CHANGED *** – smaller threshold & more forgiving permanence
                MaxSynapsesPerSegment = (int)(0.02 * numColumns),
                ActivationThreshold = 12,
                ConnectedPermanence = 0.2,
                PermanenceDecrement = 0.015,
                PermanenceIncrement = 0.03,
                PredictedSegmentDecrement = 0.01,
            };

        }

        /// <summary>
        /// Get the encoder with settings
        /// </summary>  
       
        /// CHANGED - Using New method: GetEnhancedEncoder
        public static EncoderBase GetEnhancedEncoder(int inputBits)  
        {
            var settings = new Dictionary<string, object>
            {
                { "W", 15 },
                { "N", inputBits },
                { "Radius", -1.0 },
                { "MinVal", 0.0 },
                { "Periodic", false },
                { "Name", "scalar" },
                { "ClipInput", false },
                { "MaxVal", MaxScalarValue }
            };

            return new ScalarEncoder(settings);
        }

        /// <summary>
        /// Reads dataset from the file
        /// </summary>
        public static List<Sequence> ReadDataset(string path)
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
                return new List<Sequence>(); // Return an empty list in case of failure
            }
        }

        /// <summary>
        /// Saves dataset to the file
        /// </summary>
        public static string SaveDataset(List<Sequence> sequences)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string datasetFolder = Path.Combine(basePath, "dataset");
            Directory.CreateDirectory(datasetFolder); // CreateDirectory is safe to call if directory exists
            string datasetPath = Path.Combine(datasetFolder, $"dataset_{DateTime.Now.Ticks}.json");

            Console.WriteLine("Saving dataset...");
            File.WriteAllText(datasetPath, JsonConvert.SerializeObject(sequences));
            return datasetPath;
        }

        /// <summary>
        /// Writes report to the file
        /// </summary>
        public static List<Sequence> CreateSequences(int count, int size, int startVal, int stopVal)
        {
            return Enumerable.Range(1, count).Select(i =>
                new Sequence
                {
                    name = $"S{i}",
                    data = GenerateRandomSequence(size, startVal, stopVal)
                })
                .ToList();
        }

        private static int[] GenerateRandomSequence(int size, int startVal, int stopVal)
        {
            var rnd = new Random();
            var sequence = new HashSet<int>();

            while (sequence.Count < size)
            {
                int number = rnd.Next(startVal, stopVal + 1);
                sequence.Add(number);
            }

            return sequence.OrderBy(n => n).ToArray();
        }
    }
}