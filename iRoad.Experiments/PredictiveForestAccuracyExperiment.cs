using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRoad.Experiments
{
    public class PredictiveForestAccuracyExperiment : AccuracyExperiment
    {
        private List<int> rootCounts;

        public int MaxRoots { get; private set; }

        public int MinRoots { get; private set; }

        public double AverageRoots { get; private set; }

        public double Radius { get; private set; }

        protected override string ResultsFilePrefix
        {
            get
            {
                return string.Format("acc-pf-r=:{0}-", Radius);
            }
        }

        public PredictiveForestAccuracyExperiment(string dataDirectory, string input, bool accumulate, int steps, double radius) :
            base(dataDirectory, input, accumulate, steps)
        {
            Radius = radius;
            rootCounts = new List<int>();
        }

        protected override void Conduct(List<string> lines)
        {
            PredictiveForest forest = new PredictiveForest(RoadNetwork, Constants.timeRange, Constants.probabilityThreshold);
            Results.AppendFormat("Radius: {0}\n", Radius);
            RoadNetworkNode previous = null;

            foreach (string line in lines)
            {
                RoadNetworkNode next = GetNode(line);
                Region region = new Region(next.Location, Radius);
                rootCounts.Add(forest.Roots.Count);

                if (forest.Roots.Count != 0)
                {
                    // Don't predict for the same root node again
                    if (previous != null && previous.Id == next.Id)
                    {
                        Debug.Assert(forest.Roots.Count == 1 && forest.Roots[0].Id == next.Id);
                        continue;
                    }

                    PredictiveTreeNode predictedNext = forest.Roots.SelectMany(r => r.Children).FirstOrDefault(n => n.Id == next.Id);
                    if (predictedNext != null)
                    {
                        AddProbability(predictedNext.Probability);

                        if (StepCounter == 0)
                        {
                            forest.Clear();
                        }
                    }
                    else
                    {
                        forest.Clear();
                        StepCounter = 0;
                    }
                }

                forest.Predict(region);
                previous = next;
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
