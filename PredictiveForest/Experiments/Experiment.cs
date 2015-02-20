using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryProcessing.DataStructures.Experiments
{
    public abstract class Experiment
    {
        public static RoadNetworks RoadNetwork { get; set; }

        public JObject Results { get; set; }

        public string Input { get; private set; }


        public string DataDirectory { get; set; }

        public bool Accumulate { get; private set; }

        public Experiment(string dataDirectory, string input, bool accumulate)
        {
            DataDirectory = dataDirectory;
            Input = input;
            Accumulate = accumulate;
            Results = new JObject();
            if (RoadNetwork == null)
            {
                RoadNetwork = new RoadNetworks();
                RoadNetwork.ReadRoadNetworks(
                    DataDirectory,
                    Constants.inboxMaxCoordinates,
                    Constants.inboxMinCoordinates,
                    Constants.nodesFilename,
                    Constants.edgesFileName,
                    Constants.edgeGeometeryFileName);
            }
        }

        protected abstract void Conduct(string path);

        public void Conduct()
        {
            List<string> paths = new List<string>();
            if (Directory.Exists(Input))
            {
                paths.AddRange(Directory.EnumerateFiles(Input));
            }
            else
            {
                Debug.Assert(File.Exists(Path.Combine(DataDirectory, Input)));
                paths.Add(Path.Combine(DataDirectory, Input));
            }

            foreach (string path in paths)
            {
                Console.WriteLine(string.Format("Start conducting experiment for file {0}", path));
                Conduct(path);

                if (!Accumulate)
                {
                    Save(string.Format("{0}.{1}", GetType().Name, Path.GetFileName(path)));
                    Results = new JObject();
                }
            }

            if (Accumulate)
            {
                Save(string.Format("{0}.{1}.txt", GetType().Name, Path.GetFileName(Input)));
            }
        }

        private void Save(string path)
        {
            File.WriteAllText(path, Results.ToString(Formatting.Indented));
            Console.WriteLine(string.Format("Results for experiment are saved in {0}", path));
        }
    }
}
