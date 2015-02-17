using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryProcessing.DataStructures.Experiments
{
    public class AccuracyExperiment : Experiment
    {
        public string ObjectFile { get; private set; }

        public int Steps { get; set; }

        public AccuracyExperiment(string dataDirectory, string objectFile, int steps) :
            base(dataDirectory)
        {
            Steps = steps;
            ObjectFile = objectFile;
        }

        public override void Conduct()
        {
            IEnumerable<string> lines = File.ReadLines(Path.Combine(DataDirectory, ObjectFile));
            PredictiveForest forest = new PredictiveForest(RoadNetwork, Constants.timeRange, Constants.probabilityThreshold);
            int counter = 0;
            Dictionary<int, List<double>> probabilities = new Dictionary<int, List<double>>();

            for (int i = 0; i < Steps; i++)
            {
                probabilities[i] = new List<double>();
            }

            foreach (string line in lines)
            {
                string[] splitted = line.Split(',');
                if (counter == 0)
                {
                    forest = new PredictiveForest(RoadNetwork, Constants.timeRange, Constants.probabilityThreshold);
                }

                if (int.Parse(splitted[0]) > 5)
                {
                    break;
                }

                Region region = new Region(new Coordinates(double.Parse(splitted[1]), double.Parse(splitted[2])));
                probabilities[counter++].Add(forest.First(n => n.Node.Location.Equals(region.Center)).Probability);
                forest.Predict(region);
                counter %= Steps;
            }

            foreach (var pair in probabilities)
            {
                Results[pair.Key.ToString()] = pair.Value.Average();
            }
        }
    }
}
