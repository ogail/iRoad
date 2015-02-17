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
            Experiment[] experiments =
            {
                new AccuracyExperiment(dataPath, "Object1.txt", 4)
            };

            foreach (Experiment experiment in experiments)
            {
                experiment.Conduct();
                experiment.Save();
            }
        }
    }
}
