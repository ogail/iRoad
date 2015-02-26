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

            for (int i = 0; i < lines.Count(); i +=3)
            {
                RoadNetworkNode nodeA = GetNode(lines[i]);
                RoadNetworkNode nodeB = GetNode(lines[i + 1]);
                RoadNetworkNode nodeC = GetNode(lines[i + 2]);
                AddProbability(tree.Predict(nodeA, nodeB, nodeC));
            }
        }
    }
}
