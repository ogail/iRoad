using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRoad.Experiments
{
    class Program
    {
        static void Main(string[] args)
        {
            string dataPath = @"C:\projects\iRoad-old\Data";
            string input = @"C:\projects\iRoad-old\Data\AccuracyInput";
            bool accumelate = true;
            int steps = 6;
            List<Experiment> experiments = new List<Experiment>();

            // Create accuracy experiments
            double radius = 0.0;
            double radiusMax = 0.3;
            double radiusIncrease = 0.05;

            while (radius <= radiusMax)
            {
                experiments.Add(new PredictiveForestAccuracyExperiment(dataPath, input, true, steps, radius));
                radius += radiusIncrease;
            }

            // Create TPRTree accuracy experiment
            experiments.Add(new TPRTreeAccuracyExperiments(dataPath, input, accumelate, steps));

            foreach (Experiment experiment in experiments)
            {
                experiment.Conduct();
            }
        }
    }
}
