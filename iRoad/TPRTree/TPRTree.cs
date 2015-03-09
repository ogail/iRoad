using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRoad
{
    public class TPRTree
    {
        public RoadNetworks RoadNetworks { get; private set; }

        public Line Line { get; private set; }

        public Circle Circle { get; private set; }

        public double Radius { get; set; }

        public TPRTree(RoadNetworks roadNetworks, double radius)
        {
            RoadNetworks = roadNetworks;
            Radius = radius;
        }

        public double Predict(RoadNetworkNode nodeA, RoadNetworkNode nodeB, RoadNetworkNode nodeC)
        {
            Line = new Line { Start = nodeA.Location, End = nodeB.Location };
            double radius = Coordinates.EuclideanDistance(nodeB.Location, nodeC.Location);
            Circle = new Circle { Center = nodeB.Location, Radius = radius };
            Coordinates intersection = Circle.Intersect(Line);

            if (intersection.Latitude == double.NaN || intersection.Longitude == double.NaN)
            {
                return -1;
            }

            double distance = RoadNetworks.DistanceInKM(intersection, nodeC.Location);
            //Debug.Assert(distance <= Circle.Radius * 2);
            //Region r = new Region(intersection, distance);
            List<RoadNetworkNode> neighbors = RoadNetworks.GetNeighbors(nodeC.Location, distance + Radius);
            //Debug.Assert(neighbors != null && neighbors.Any(n => n.Id == nodeC.Id));

            return neighbors.Count == 0 ? 0 : 1.0 / neighbors.Count;
        }
    }
}
