using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRoad.Experiments
{
    public abstract class Experiment
    {
        public static RoadNetworks RoadNetwork { get; set; }

        public JObject ResultsJson { get; set; }

        public StringBuilder Results { get; set; }

        public string Input { get; private set; }

        public string DataDirectory { get; set; }

        public bool Accumulate { get; private set; }

        public double Radius { get; private set; }

        public Experiment(string dataDirectory, string input, double radius, bool accumulate)
        {
            DataDirectory = dataDirectory;
            Input = input;
            Accumulate = accumulate;
            Radius = radius;
            ResultsJson = new JObject();
            Results = new StringBuilder();
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
            else if (File.Exists(Input))
            {
                paths.Add(Input);
            }
            else
            {
                Debug.Assert(File.Exists(Path.Combine(DataDirectory, Input)));
                paths.Add(Path.Combine(DataDirectory, Input));
            }

            Results.AppendFormat("Radius: {0}\n", Radius);

            foreach (string path in paths)
            {
                Console.WriteLine(string.Format("Start conducting experiment for file {0}", path));
                Conduct(path);

                if (!Accumulate)
                {
                    Results.AppendFormat("{0}.{1}:\n{2}\n", GetType().Name, Path.GetFileName(path), ResultsJson.ToString(Formatting.Indented));
                    ResultsJson = new JObject();
                }
            }

            if (Accumulate)
            {
                Results.AppendFormat("{0}.{1}.txt:\n{2}\n", GetType().Name, Path.GetFileName(Input), ResultsJson.ToString(Formatting.Indented));
            }

            Save();
        }

        private void Save()
        {
            string path = string.Format("r={0}-{1}.txt", Radius, DateTime.Now.ToString("yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture));
            File.WriteAllText(path, Results.ToString());
            Console.WriteLine(string.Format("Results for experiment are saved in {0}", path));
        }
    }
}
