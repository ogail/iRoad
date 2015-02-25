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
        private List<int> rootCounts;

        public int Steps { get; set; }

        public int MaxRoots { get; private set; }

        public int MinRoots { get; private set; }

        public double AverageRoots { get; private set; }

        public AccuracyExperiment(string dataDirectory, string input, double radius, bool accumulate, int steps) :
            base(dataDirectory, input, radius, accumulate)
        {
            Steps = steps;
            rootCounts = new List<int>();
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
                Debug.Assert(splitted.Length == 4);
                Console.WriteLine(string.Format("Predicting for step {0}", splitted[0]));
                Region region = new Region(new Coordinates(double.Parse(splitted[1]), double.Parse(splitted[2])), Radius);
                rootCounts.Add(forest.Roots.Count);

                if (forest.Roots.Count != 0)
                {
                    RoadNetworkNode next = RoadNetwork.Nearest(region.Center.Latitude, region.Center.Longitude, int.Parse(splitted[3]));

                    // Don't predict for the same root node again
                    if (forest.Roots.Count == 1 && forest.Roots[0].Id == next.Id)
                    {
                        continue;
                    }

                    PredictiveTreeNode predictedNext = forest.Roots.SelectMany(r => r.Children).FirstOrDefault(n => n.Id == next.Id);
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

            MaxRoots = rootCounts.Max();
            MinRoots = rootCounts.Min();
            AverageRoots = rootCounts.Average();

            ResultsJson["RootsMax"] = MaxRoots;
            ResultsJson["RootsMin"] = MinRoots;
            ResultsJson["RootsAverage"] = AverageRoots;
        }
    }
}
