using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryProcessing.DataStructures.Experiments
{
    class Program
    {
        static void Main(string[] args)
        {
            string dataPath = @"C:\projects\iRoad\Data";
            List<Experiment> experiments = new List<Experiment>();

            //experiments.Add(new AccuracyExperiment(dataPath, @"C:\projects\iRoad\Data\AccuracyInput\object_0.txt", 0, false, 6));

            // Create accuracy experiments
            double radius = 0;
            double radiusMax = 0.25;
            double radiusIncrease = 0.05;

            while (radius <= radiusMax)
            {
                experiments.Add(new AccuracyExperiment(dataPath, @"C:\projects\iRoad\Data\AccuracyInput", radius, false, 6));
                radius += radiusIncrease;
            }

            foreach (Experiment experiment in experiments)
            {
                experiment.Conduct();
            }
        }
    }
}
