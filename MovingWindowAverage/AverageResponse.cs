using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovingWindowAverage
{
    public class AverageResponse
    {
        public DateTime WindowStartTime { get; set; }
        public DateTime WindowEndTime { get; set; }
        public double Average { get; set; }
    }
}
