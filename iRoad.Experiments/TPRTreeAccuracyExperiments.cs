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
        public TPRTreeAccuracyExperiments(string dataDirectory, string input, bool accumulate, int steps) :
            base(dataDirectory, input, accumulate, steps)
        {
        }

        protected override string ResultsFilePrefix
        {
            get
            {
                return "acc-tpr-";
            }
        }

        protected override void Conduct(List<string> lines)
        {
            TPRTree tree = new TPRTree(RoadNetwork);
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

                AddProbability(tree.Predict(nodeA, nodeB, nodeC));
            }
        }
    }
}
