using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRoad.Experiments
{
    public class TPRTreeAccuracyExperiments : AccuracyExperiment
    {
        public double Radius { get; set; }

        public TPRTreeAccuracyExperiments(string dataDirectory, string input, bool accumulate, int steps, double radius) :
            base(dataDirectory, input, accumulate, steps)
        {
            Radius = radius;
        }

        protected override string ResultsFilePrefix { get { return string.Format("acc-tpr-r={0}-", Radius); } }

        protected override void Conduct(List<string> lines)
        {
            TPRTree tree = new TPRTree(RoadNetwork, Radius);
            int i = 0;

            while (i < lines.Count)
            {
                RoadNetworkNode nodeA = null, nodeB = null, nodeC = null;
                nodeA = GetNode(lines[i++]);
                while (i < lines.Count && (nodeB == null || nodeB.Id == nodeA.Id)) { nodeB = GetNode(lines[i++]); }
                while (i < lines.Count && (nodeC == null || nodeC.Id == nodeA.Id || nodeC.Id == nodeB.Id)) { nodeC = GetNode(lines[i++]); }

                if (nodeA == null || nodeB == null || nodeC == null)
                {
                    break;
                }

                double probability = tree.Predict(nodeA, nodeB, nodeC);
                if (probability != -1)
                {
                    AddProbability(probability);                    
                }
            }
        }
    }
}
