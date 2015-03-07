using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace iRoad
{
    public class RoadNetworks
    {

        public Coordinates MAX;
        public Coordinates MIN;
        public Dictionary<int, RoadNetworkNode> AllNodes;
        public Dictionary<int, RoadNetworkNode> Nodes; //inner nodes
        public Dictionary<int, Edge> Edges;// inner edges
        public Dictionary<int, Dictionary<int, Edge>> SpatialIndex;
        public int gridsize;

        public double difLat;
        public double difLng;


        const double _eQuatorialEarthRadius = 6378.1370D;
        const double SecCoDiff = 30.887;
        const double _d2r = (Math.PI / 180D);




        void ReadEdgeConnection(string datafilepath, string edgesFileName)
        {

            int count = 0;
            TextReader tr = new StreamReader(Path.Combine( datafilepath, edgesFileName));
            string line = tr.ReadLine();

            while (line != null)
            {
                count++;
                string[] sArray = line.Split(' ');
                int EdgeID = Convert.ToInt32(sArray[0]);
                int From = Convert.ToInt32(sArray[1]);
                int To = Convert.ToInt32(sArray[2]);
                int Cost = Convert.ToInt32(sArray[3]);

                if (Nodes.Keys.Contains(From) == true || Nodes.Keys.Contains(To) == true)
                {
                    if (Nodes.Keys.Contains(From) == false)
                    {
                        RoadNetworkNode tmp = this.AllNodes[From];
                        Nodes.Add(tmp.Id, tmp);
                    }
                    if (Nodes.Keys.Contains(To) == false)
                    {
                        RoadNetworkNode tmp = this.AllNodes[To];
                        Nodes.Add(tmp.Id, tmp);
                    }

                    RoadNetworkNode from = this.Nodes[From];
                    RoadNetworkNode to = this.Nodes[To];
                    Edge e = new Edge(EdgeID, from, to, CreateEdgeCost(Cost));
                    from.OutEdges.Add(to, e);
                    to.InEdges.Add(from, e);
                    this.Edges.Add(EdgeID, e);

                }
                line = tr.ReadLine();
            }

            tr.Close();
        }

        private Dictionary<int, Tuple<int, int>> CreateEdgeCost(int Cost)
        {
            Dictionary<int, Tuple<int, int>> cost = new Dictionary<int, Tuple<int, int>>();
            for (int i = 1; i <= 24; i++)
            {
                cost[i] = Tuple.Create(Cost, Cost);
            }

            return cost;
        }


        void ReadEdgeGeo(string datafilepath, string edgeGeometeryFileName)//read the shape of the edge
        {
            int count = 0;
            TextReader tr = new StreamReader(Path.Combine(datafilepath, edgeGeometeryFileName));
            string line = tr.ReadLine();

            while (line != null)
            {

                if (Edges.Keys.Contains(count))
                {
                    string[] sArray = line.Split('^');
                    int EdgeID = Convert.ToInt32(sArray[0]); ;
                    if (Edges.Keys.Contains(EdgeID))
                    {
                        Edge e = Edges[EdgeID];
                        e.Name = sArray[1];
                        e.Type = sArray[2];

                        for (int i = 4; i < sArray.Count(); i++)
                        {
                            double lat = Convert.ToDouble(sArray[i]);
                            i++;
                            double lng = Convert.ToDouble(sArray[i]);
                            e.AddCoordinate(lat, lng);
                        }

                        insertEdgeIndex(e);
                    }
                }
                line = tr.ReadLine();
                count++;
            }
            tr.Close();
        }

        public static Boolean  inbox(Coordinates loc, Coordinates MAX, Coordinates MIN)
        {
            if (loc.Latitude < MAX.Latitude && loc.Latitude > MIN.Latitude && loc.Longitude < MAX.Longitude && loc.Longitude > MIN.Longitude)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual double DistanceInKM(Coordinates c1, Coordinates c2)
        {
            double lat1 = c1.Latitude;
            double long1 = c1.Longitude;
            double lat2 = c2.Latitude;
            double long2 = c2.Longitude;
            double dlong = (long2 - long1) * _d2r;
            double dlat = (lat2 - lat1) * _d2r;
            double a = Math.Pow(Math.Sin(dlat / 2D), 2D) + Math.Cos(lat1 * _d2r) * Math.Cos(lat2 * _d2r) * Math.Pow(Math.Sin(dlong / 2D), 2D);
            double c = 2D * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1D - a));
            double d = _eQuatorialEarthRadius * c;

            return d;
        }

        public RoadNetworkNode Nearest(double latitude, double longitude, int edgeId)
        {
            Edge tempEdge = Edges[edgeId];
            int fromNodeId = tempEdge.From.Id;
            int toNodeId = tempEdge.To.Id;
            Coordinates objectCordinates = new Coordinates(latitude, longitude);
            double distanceToFromNode = DistanceInKM(Nodes[fromNodeId].Location, objectCordinates);
            double distanceToToNode = DistanceInKM(Nodes[toNodeId].Location, objectCordinates);

            return distanceToFromNode < distanceToToNode ? Nodes[fromNodeId] : Nodes[toNodeId];
        }

        public virtual RoadNetworkNode Nearest(double latitude, double longitude)
        {
            Coordinates c = new Coordinates(latitude, longitude);
            int cellid = Mapping(c);
         
            if (SpatialIndex.ContainsKey(cellid) == false ) 
            {
                return null;
            }

            if (SpatialIndex[cellid].Values.Count < 1)
            {
                return null;
            }

            RoadNetworkNode n = null;
            double distance = double.MaxValue;

            foreach (var pair in SpatialIndex[cellid])
            {
                Edge e = pair.Value;
                RoadNetworkNode tmp = e.From;
                double ndis = DistanceInKM(c, tmp.Location);
                if (distance > ndis)
                {
                    distance = ndis;
                    n = tmp;
                }
            }

            return n ?? SpatialIndex[cellid].First().Value.From;
        }

        /// <summary>
        /// Gets neighbor nodes to a node in a given range.
        /// </summary>
        /// <param name="center">The origin node</param>
        /// <param name="rangeInKM">The range value in kilemeters. By default it's 0.2</param>
        /// <returns>The list of node in range</returns>
        public virtual List<RoadNetworkNode> GetNeighbors(RoadNetworkNode center, double rangeInKM)
        {
            Dictionary<int, RoadNetworkNode> neighbors = new Dictionary<int, RoadNetworkNode>();
            Coordinates c = new Coordinates(center.Location.Latitude, center.Location.Longitude);
            int cellid = Mapping(c);

            if (SpatialIndex.ContainsKey(cellid) == false)
            {
                return null;
            }

            if (SpatialIndex[cellid].Values.Count < 1)
            {
                return null;
            }

            //neighbors.Add(SpatialIndex[cellid].ElementAt(0).Value.From);

            foreach (var pair in SpatialIndex[cellid])
            {
                Edge e = pair.Value;
                RoadNetworkNode node = e.From;
                double distance = DistanceInKM(c, node.Location);
                if (distance <= rangeInKM && !neighbors.ContainsKey(node.Id))
                {
                    neighbors.Add(node.Id, node);
                }
            }

            return neighbors.Select(p => p.Value).ToList();
        }


        int ReadNodes(string datafilepath, string nodesFilename)   //read the node data file
        {
            int count = 0;
            TextReader tr = new StreamReader(Path.Combine(datafilepath, nodesFilename));
            string line = tr.ReadLine();
            while (line != null)
            {
                //Console.Out.WriteLine(line);
                string[] sArray = line.Split(' ');
                int nodeid = Convert.ToInt32(sArray[0]);
                double latitude = Convert.ToDouble(sArray[1]);
                double longitude = Convert.ToDouble(sArray[2]);

                RoadNetworkNode N = new RoadNetworkNode(nodeid, latitude, longitude);
                AllNodes.Add(nodeid, N);
                if (inbox(N.Location, MAX, MIN))
                {
                    Nodes.Add(nodeid, N);
                }
                count++;
                line = tr.ReadLine();
            }
            //Console.Out.WriteLine(count);
            //Console.Out.WriteLine(AllNodes.Count);
            //Console.Out.WriteLine(Nodes.Count);
            tr.Close();
            return count;
        }

        public void initializeSpatialIndex()
        {
            difLat = (MAX.Latitude - MIN.Latitude) / gridsize;
            difLng = (MAX.Longitude - MIN.Longitude) / gridsize;

            //////////////////////////
            // Console.Out.WriteLine(difLat + "\t" + difLng);

            for (int i = 0; i < gridsize; i++)
            {
                for (int j = 0; j < gridsize; j++)
                {
                    Dictionary<int, Edge> griditem = new Dictionary<int, Edge>();
                    SpatialIndex.Add(i * gridsize + j, griditem);
                }
            }
        }


        public void insertEdgeIndex(Edge e)
        {

            int i1 = (int)((e.MIN.Latitude - MIN.Latitude) / difLat);
            int i2 = (int)((e.MAX.Latitude - MIN.Latitude) / difLat);

            if (i2 >= gridsize)
            {
                i2 = gridsize - 1;
            }

            if (i1 <= 0)
            {
                i1 = 0;
            }
            int j1 = (int)((e.MIN.Longitude - MIN.Longitude) / difLng);
            int j2 = (int)((e.MAX.Longitude - MIN.Longitude) / difLng);

            if (j1 < 0)
            {
                j1 = 0;
            }

            if (j2 >= gridsize)
            {
                j2 = gridsize - 1;
            }


            for (int i = i1; i <= i2; i++)
            {
                for (int j = j1; j <= j2; j++)
                {
                    int cellid = i * gridsize + j;
                    SpatialIndex[cellid].Add(e.Id, e);

                }
            }
        }

        public int Mapping(Coordinates loc)
        {
            double lat = loc.Latitude - MIN.Latitude;
            double lng = loc.Longitude - MIN.Longitude;
            int result = 0; ;
            int i = (int)(lat / difLat);
            int j = (int)(lng / difLng);
            result = i * gridsize + j;


            if (i < 0)
            {
                i = 0;
            }
            if (j < 0)
            {
                j = 0;
            }

            if (i >= gridsize)
            {
                i = gridsize - 1;
            }

            if (j >= gridsize)
            {
                j = gridsize - 1;
            }
            //**

            return result;
        }

        public RoadNetworks()
        {
            //initialization
            Nodes = new Dictionary<int, RoadNetworkNode>();
            AllNodes = new Dictionary<int, RoadNetworkNode>();
            Edges = new Dictionary<int, Edge>();
            SpatialIndex = new Dictionary<int, Dictionary<int, Edge>>();

            gridsize = 50;

            //Console.Out.WriteLine("Reading Geographical information");

        }

        public void ReadRoadNetworks(String network, Coordinates inMAX, Coordinates inMIN, string nodesFilename, string edgesFileName, string edgeGeometeryFileName)
        {
            this.MAX = inMAX;
            this.MIN = inMIN;

            Console.Out.WriteLine("Initial spatial index");
            initializeSpatialIndex();

            Console.Out.WriteLine("Reading Nodes");
            ReadNodes(network, nodesFilename);
            Console.Out.WriteLine("Reading Connection Graph");
            ReadEdgeConnection(network, edgesFileName);
            Console.Out.WriteLine("Reading Geo");
            ReadEdgeGeo(network, edgeGeometeryFileName);
        }
    }
}
