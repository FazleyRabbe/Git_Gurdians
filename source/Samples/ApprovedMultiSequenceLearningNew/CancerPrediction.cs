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
