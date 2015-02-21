using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryProcessing.DataStructures.Experiments
{
    public class AccuracyExperiment : Experiment
    {
        public int Steps { get; set; }

        public AccuracyExperiment(string dataDirectory, string input, bool accumulate, int steps) :
            base(dataDirectory, input, accumulate)
        {
            Steps = steps;
        }

        protected override void Conduct(string path)
        {
            IEnumerable<string> lines = File.ReadLines(path);
            PredictiveForest forest = new PredictiveForest(RoadNetwork, Constants.timeRange, Constants.probabilityThreshold);
            int counter = 0;
            Dictionary<int, List<double>> probabilities = new Dictionary<int, List<double>>();

            for (int i = 0; i < Steps; i++)
            {
                probabilities[i] = new List<double>();
            }

            foreach (string line in lines)
            {
                string[] splitted = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Console.WriteLine(string.Format("Predicting for step {0}", splitted[0]));
                Region region = new Region(new Coordinates(double.Parse(splitted[1]), double.Parse(splitted[2])));

                if (forest.Roots.Count != 0)
                {
                    RoadNetworkNode next = null;

                    if (splitted.Length == 3)
                    {
                        next = RoadNetwork.Nearest(region.Center.Latitude, region.Center.Longitude);
                    }
                    else
                    {
                        Debug.Assert(splitted.Length == 4);
                        next = RoadNetwork.Nearest(region.Center.Latitude, region.Center.Longitude, int.Parse(splitted[3]));
                    }

                    // Remove this line when changing to Forest
                    if (forest.Roots.Any(r => r.Id == next.Id))
                    {
                        continue;
                    }

                    TreeNode predictedNext = forest.Roots.SelectMany(r => r.Children).FirstOrDefault(n => n.Id == next.Id);
                    if (predictedNext != null)
                    {
                        probabilities[counter++].Add(predictedNext.Probability);
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

                forest.Predict(region);
            }

            foreach (var pair in probabilities)
            {
                ResultsJson[pair.Key.ToString()] = pair.Value.Count > 0 ? pair.Value.Average() : 0;
            }
        }
    }
}
