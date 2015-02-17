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

                // TODO: Remove after getting this experiment to work.
                if (int.Parse(splitted[0]) > 5)
                {
                    break;
                }

                Region region = new Region(new Coordinates(double.Parse(splitted[1]), double.Parse(splitted[2])));
                forest.Predict(region);
                TreeNode predicted = forest.GetNode(RoadNetwork.Nearest(region.Center.Latitude, region.Center.Longitude).Id);
                if (predicted != null)
                {
                    probabilities[counter++].Add(predicted.Probability);
                    counter %= Steps;

                    if (counter == 0)
                    {
                        forest.Clear();
                    }
                }
                else
                {
                    forest.Clear();
                    counter = 0;
                }
            }

            foreach (var pair in probabilities)
            {
                Results[pair.Key.ToString()] = pair.Value.Average();
            }
        }
    }
}
