using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApprovedMultiSequenceLearningNew
{
    public class Sequence
    {
        /// <summary> ///
        /// Represents a sequence used in the multi-sequence learning project. 
        /// </summary>
        public String name { get; set; }
        // The numerical data points that constitute the sequence.
        public int[] data { get; set; }
    }
}
