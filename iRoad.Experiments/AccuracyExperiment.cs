using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRoad.Experiments
{
    public abstract class AccuracyExperiment : Experiment
    {
        public int Steps { get; private set; }

        protected int StepCounter { get; set; }

        private Dictionary<int, List<double>> probabilities;

        public AccuracyExperiment(string dataDirectory, string input, bool accumulate, int steps) :
            base(dataDirectory, input, accumulate)
        {
            Steps = steps;
            probabilities = new Dictionary<int, List<double>>();
            for (int i = 0; i < Steps; i++)
            {
                probabilities[i] = new List<double>();
            }
        }

        protected void AddProbability(double probability)
        {
            probabilities[StepCounter++].Add(probability);
            StepCounter %= Steps;
        }

        protected override void Conduct(string path)
        {
            List<string> lines = File.ReadLines(path).ToList();
            StepCounter = 0;
            if (!Accumulate)
            {
                probabilities.Clear();
            }

            Conduct(lines);

            foreach (var pair in probabilities)
            {
                ResultsJson[pair.Key.ToString()] = pair.Value.Count > 0 ? pair.Value.Average() : 0;
            }
        }

        protected RoadNetworkNode GetNode(string line)
        {
            string[] splitted = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Debug.Assert(splitted.Length == 4);
            Console.WriteLine(string.Format("Predicting for step {0}", splitted[0]));
            return RoadNetwork.Nearest(double.Parse(splitted[1]), double.Parse(splitted[2]), int.Parse(splitted[3]));
        }

        protected abstract void Conduct(List<string> lines);
    }
}
