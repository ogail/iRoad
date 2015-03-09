using Moq;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using System.IO;
using iRoad.Test;
using System;

namespace iRoad.PredictiveForestTest
{
    public class TPRTreeTests
    {
        /// <summary>
        /// The test below uses:
        /// - Line equation: y = 1 * x + 0
        /// - Circle equation: (x + 0)^2 + (y + 0)^2 = 5^2
        /// 
        /// The references for the expected result are listed below. The expected result it: (3.536 , 3.536)
        /// http://www.mathportal.org/calculators/analytic-geometry/circle-line-intersection-calculator.php
        /// http://www.analyzemath.com/Calculators/Circle_Line.html
        /// </summary>
        [Fact]
        public void TestCircleIntersection()
        {
            Circle c = new Circle
            {
                Center = new Coordinates(0, 0),
                Radius = 5
            };
            Line l = new Line
            {
                Start = new Coordinates(0, 0),
                End = new Coordinates(3, 3)
            };

            Coordinates intersection = c.Intersect(l);
            Assert.Equal(-3.5, intersection.Latitude, 1);
            Assert.Equal(-3.5, intersection.Longitude, 1);
        }

        [Fact]
        public void TestRealCircleIntersection()
        {
            RoadNetworks roadNetworks = new RoadNetworks();
            Coordinates pointA = new Coordinates(47.6690602, -122.13593);
            Coordinates pointB = new Coordinates(47.6697653, -122.137741);
            Coordinates pointC = new Coordinates(47.6690024, -122.1377496);

            Circle c = new Circle { Center = pointA, Radius = Coordinates.EuclideanDistance(pointA, pointB) };
            Line l = new Line { Start = pointA, End = pointB };

            Coordinates intersection = c.Intersect(l);
            double distance = Coordinates.EuclideanDistance(intersection, pointC);
            Assert.True(distance < 2 * c.Radius);
        }

        [Fact]
        public void TestTPRTreePrediction()
        {
            Mock<RoadNetworks> mockRoadNetworks = new Mock<RoadNetworks>();
            RoadNetworkNode n1 = new RoadNetworkNode(0, 0, 0);
            RoadNetworkNode n2 = new RoadNetworkNode(1, 3, 3);
            RoadNetworkNode n3 = new RoadNetworkNode(2, 5, 5);
            RoadNetworkNode n4 = new RoadNetworkNode(3, 10, 10);

            mockRoadNetworks.Setup(f => f.DistanceInKM(n1.Location, n3.Location)).Returns(-5);
            mockRoadNetworks.Setup(f => f.GetNeighbors(It.IsAny<Coordinates>(), It.IsAny<double>())).Returns(new List<RoadNetworkNode>() { new RoadNetworkNode(1, 12, 3) });
            mockRoadNetworks.Setup(f => f.Nearest(It.IsAny<double>(), It.IsAny<double>())).Returns(n4);

            double probability = new TPRTree(mockRoadNetworks.Object, 1).Predict(n1, n2, n3);

            Assert.Equal(1, probability);
        }
    }
}
