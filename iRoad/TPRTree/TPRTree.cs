using System;
using System.Collections.Generic;
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

        public TPRTree(RoadNetworks roadNetworks)
        {
            RoadNetworks = roadNetworks;
        }

        public int Predict(RoadNetworkNode nodeA, RoadNetworkNode nodeB, RoadNetworkNode nodeC)
        {
            Line = new Line { Start = nodeA.Location, End = nodeB.Location };
            double distance = Math.Abs(RoadNetworks.DistanceInKM(nodeA.Location, nodeC.Location));
            Circle = new Circle { Center = nodeA.Location, Radius = distance };
            Tuple<Coordinates, Coordinates> intersection = Circle.Intersect(Line);
            RoadNetworkNode nearest1 = RoadNetworks.Nearest(intersection.Item1.Latitude, intersection.Item1.Longitude);
            RoadNetworkNode nearest2 = RoadNetworks.Nearest(intersection.Item2.Latitude, intersection.Item2.Longitude);

            double distance1 = nearest1 == null ? 0 : RoadNetworks.DistanceInKM(nearest1.Location, nodeC.Location);
            double distance2 = nearest2 == null ? 0 : RoadNetworks.DistanceInKM(nearest2.Location, nodeC.Location);

            return (nearest1 != null && nearest1.Id == nodeC.Id) || 
                   (nearest2 != null && nearest2.Id == nodeC.Id) ? 1 : 0;
        }
    }
}
