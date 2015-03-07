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

        public double Radius { get; private set; }

        public TPRTree(RoadNetworks roadNetworks, double radius)
        {
            RoadNetworks = roadNetworks;
            Radius = radius;
        }

        public double Predict(RoadNetworkNode nodeA, RoadNetworkNode nodeB, RoadNetworkNode nodeC)
        {
            Line = new Line { Start = nodeA.Location, End = nodeB.Location };
            double distance = Coordinates.EuclideanDistance(nodeA.Location, nodeB.Location);
            Circle = new Circle { Center = nodeA.Location, Radius = distance };
            Tuple<Coordinates, Coordinates> intersection = Circle.Intersect(Line);
            RoadNetworkNode nearest1 = RoadNetworks.Nearest(intersection.Item1.Latitude, intersection.Item1.Longitude);
            RoadNetworkNode nearest2 = RoadNetworks.Nearest(intersection.Item2.Latitude, intersection.Item2.Longitude);

            double distance1 = nearest1 != null ? RoadNetworks.DistanceInKM(nearest1.Location, nodeC.Location) : double.MaxValue;
            double distance2 = nearest2 != null ? RoadNetworks.DistanceInKM(nearest2.Location, nodeC.Location) : double.MaxValue;
            Debug.Assert(distance1 < Circle.Radius * 2 && distance2 < Circle.Radius * 2);

            return Math.Min(distance1, distance2) < Radius ? 1 : 0;
        }
    }
}
