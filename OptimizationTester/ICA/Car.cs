using Optimization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationTester.ICA
{
    class Car
    {
        public int index { get; set; }
        public int MaxDailyPath { get; set; }
        public int CurrentDailyPath { get; set; }
        public int MaxPathPoint { get; set; }
        public double MaxPathDistance { get; set; }
        public double MaxPathDistanceRes { get; set; }
        public Task<Result> Process { get; set; }

        public void reset()
        {
            this.MaxPathDistance = this.MaxPathDistanceRes;
            this.CurrentDailyPath = 0;
        }
    }
}
