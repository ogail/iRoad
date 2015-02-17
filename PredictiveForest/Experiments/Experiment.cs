using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryProcessing.DataStructures.Experiments
{
    public abstract class Experiment
    {
        public JObject Results { get; set; }

        public RoadNetworks RoadNetwork { get; set; }

        public string DataDirectory { get; set; }

        public Experiment(string dataDirectory)
        {
            DataDirectory = dataDirectory;
            Results = new JObject();
            RoadNetwork = new RoadNetworks();
            RoadNetwork.ReadRoadNetworks(
                DataDirectory,
                Constants.inboxMaxCoordinates,
                Constants.inboxMinCoordinates,
                Constants.nodesFilename,
                Constants.edgesFileName,
                Constants.edgeGeometeryFileName);
        }

        public abstract void Conduct();

        public void Save()
        {
            File.WriteAllText(this.GetType().Name + ".txt", Results.ToString(Formatting.Indented));
        }
    }
}
