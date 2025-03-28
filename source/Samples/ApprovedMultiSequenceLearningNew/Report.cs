using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApprovedMultiSequenceLearningNew
{
    public class Report
    {
        /// <summary> ///
        /// Represents a report generated from the multi-sequence learning experiment. 
        /// </summary> ///
        /// 
        public Report() { }

        // The name identifier for the test sequence.
        public string SequenceName { get; set; }
        // The array of integer data representing the test sequence.
        public int[] SequenceData { get; set; }
        // A list of strings logging the predictions made for the sequence.
        public List<string> PredictionLog { get; set; }
        // The overall prediction accuracy for the sequence.
        public double Accuracy { get; set; }

    }
}
