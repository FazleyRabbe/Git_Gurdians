using NeoCortexApi.Entities;
using NeoCortexApi;
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

        ///////////////////////////////////////////////////

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
    }
}
