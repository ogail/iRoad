using Moq;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using System.IO;

namespace QueryProcessing.DataStructures.PredictiveForestTest
{
    public class PredectiveForestTests
    {
        private Mock<RoadNetworks> mockRoadNetworks;
        
        private PredictiveForest pForest;

        private const double timeRange = speed * 2 * T / 60 / 1.6;

        private const double speed = 20;

        private const double T = 10;

        private const double probabilityThreshold = 0;

        private Dictionary<string, Dictionary<int, PredictiveTreeNode>> generatedTrees;

        private string currentTest;

        private List<RoadNetworkNode> roots;

        public PredectiveForestTests()
        {
            roots = new List<RoadNetworkNode>();
            mockRoadNetworks = new Mock<RoadNetworks>();
            mockRoadNetworks.Setup(m => m.Nearest(It.IsAny<double>(), It.IsAny<double>()));
            mockRoadNetworks.Setup(m => m.GetNeighbors(It.IsAny<RoadNetworkNode>(), It.IsAny<double>())).
                Returns(roots);
            generatedTrees = new Dictionary<string, Dictionary<int, PredictiveTreeNode>>();
            pForest = new PredictiveForest(mockRoadNetworks.Object, timeRange, probabilityThreshold);
            pForest.TreeBuilder = MockTreeBuilder;
            GenerateTree("Resources\\forests.txt");
        }

        private void GenerateRoadNetworkRoots(params int[] ids)
        {
            for (int i = 0; i < ids.Length; i++)
            {
                roots.Add(new RoadNetworkNode(ids[i], It.IsAny<double>(), It.IsAny<double>()));
            }
        }

        private PredictiveTreeNode MockTreeBuilder(RoadNetworks roadNetwork, RoadNetworkNode roadNode, double timeRange, double probabilityThreshold)
        {
            Assert.True(!string.IsNullOrEmpty(currentTest));
            return generatedTrees[currentTest][roadNode.Id];
        }

        private void GenerateTree(string path)
        {
            IEnumerable<string> lines = File.ReadLines(path);
            string currentForest = null;

            foreach (string line in lines)
            {
                if (line.Contains("forest"))
                {
                    currentForest = line.TrimEnd('\n', '\r');
                    generatedTrees[currentForest] = new Dictionary<int, PredictiveTreeNode>();
                }
                else
                {
                    string[] splitted = line.Split(new char[] { ':', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                    PredictiveTreeNode node = new PredictiveTreeNode { Id = int.Parse(splitted.First()) };
                    for (int i = 1; i < splitted.Length; i++)
                    {
                        int id = int.MinValue;
                        int distance = int.MinValue;
                        if (splitted[i].Contains('-'))
                        {
                            id = int.Parse(splitted[i].Split('-').ElementAt(0));
                            distance = int.Parse(splitted[i].Split('-').ElementAt(1));
                        }
                        else
                        {
                            id = int.Parse(splitted[i]);
                        }

                        node.AddChild(new PredictiveTreeNode { Id = id, DistanceToRoot = distance, Parent = node });
                    }
                    Assert.True(!string.IsNullOrEmpty(currentForest));
                    PredictiveTreeNode parent = generatedTrees[currentForest].Values.FirstOrDefault(n => n.Children.Any(c => c.Id == node.Id));

                    if (parent == null)
                    {
                        generatedTrees[currentForest].Add(node.Id, node);
                    }
                    else
                    {
                        node.Parent = parent;
                        Assert.Equal(1, parent.Children.RemoveAll(n => n.Id == node.Id));
                        parent.AddChild(node);
                    }
                }
            }
        }

        private void AssertValidNode(PredictiveTreeNode node, params int[] children)
        {
            List<PredictiveTreeNode> list = node.Children;
            Assert.Equal(list.Count, children.Length);
            Assert.True(children.ToList().TrueForAll(n => list.Any(t => t.Id == n)));

            if (list.Count > 0)
            {
                // Assert probabilities
                double parentProbability = list[0].Parent == null ? 1.0 : list[0].Parent.Probability;
                Assert.True(list.All(n => n.Probability == parentProbability / children.Length));
            }
        }

        private void AssertRoots(PredictiveForest forest, params int[] ids)
        {
            Assert.Equal(ids.Length, forest.Roots.Count);
            Assert.True(forest.Roots.ToList().TrueForAll(n => ids.Any(t => t == n.Id)));
        }

        [Fact]
        public void BuildingSimplePredectiveForestTest()
        {
            GenerateRoadNetworkRoots(0, 1);
            Region region = new Region(new Coordinates(It.IsAny<double>(), It.IsAny<double>()), 0);
            currentTest = "simple forest";
            pForest.Predict(region);

            AssertRoots(pForest, 0, 1);
            AssertValidNode(pForest[0], 2, 3);
            AssertValidNode(pForest[1], 4, 5, 6);
        }

        [Fact]
        public void MergingNodesTest()
        {
            GenerateRoadNetworkRoots(0, 1);
            Region region = new Region(new Coordinates(It.IsAny<double>(), It.IsAny<double>()), 0);
            currentTest = "conflict forest";
            pForest.Predict(region);

            AssertRoots(pForest, 0, 1);
            AssertValidNode(pForest[0], 3);
            AssertValidNode(pForest[1], 2, 4);
        }

        [Fact]
        public void AssignsProbabilitiesCorrectlyTest()
        {
            List<RoadNetworkNode> roots1 = new List<RoadNetworkNode>
            {
                new RoadNetworkNode(0, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(1, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(2, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(3, It.IsAny<double>(), It.IsAny<double>())
            };
            Region region1 = new Region(new Coordinates(It.IsAny<double>(), It.IsAny<double>()), 0);
            RoadNetworkNode center1 = new RoadNetworkNode(0, region1.Center.Latitude, region1.Center.Longitude);
            mockRoadNetworks.Setup(m => m.Nearest(region1.Center.Latitude, region1.Center.Longitude)).Returns(center1);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center1, It.IsAny<double>())).Returns(roots1);
            currentTest = "medium forest 2";
            pForest.Predict(region1);

            AssertRoots(pForest, 0, 1, 2, 3);
            AssertValidNode(pForest[0], 4, 5);
            AssertValidNode(pForest[1], 6);
            AssertValidNode(pForest[2], 7, 8, 9, 10);
            AssertValidNode(pForest[3], 11, 12, 13);
            AssertValidNode(pForest[6], 14, 15);
            AssertValidNode(pForest[13], 16);
        }

        [Fact]
        public void PredictsAllInRegionTest()
        {
            List<RoadNetworkNode> roots1 = new List<RoadNetworkNode>
            {
                new RoadNetworkNode(0, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(1, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(2, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(3, It.IsAny<double>(), It.IsAny<double>())
            };
            List<RoadNetworkNode> roots2 = new List<RoadNetworkNode>
            {
                new RoadNetworkNode(1, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(8, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(3, It.IsAny<double>(), It.IsAny<double>())
            };
            Region region1 = new Region(new Coordinates(1, 1), 0);
            Region region2 = new Region(new Coordinates(2, 2), 0);
            RoadNetworkNode center1 = new RoadNetworkNode(0, region1.Center.Latitude, region1.Center.Longitude);
            RoadNetworkNode center2 = new RoadNetworkNode(3, region2.Center.Latitude, region2.Center.Longitude);
            mockRoadNetworks.Setup(m => m.Nearest(region1.Center.Latitude, region1.Center.Longitude)).Returns(center1);
            mockRoadNetworks.Setup(m => m.Nearest(region2.Center.Latitude, region2.Center.Longitude)).Returns(center2);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center1, It.IsAny<double>())).Returns(roots1);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center2, It.IsAny<double>())).Returns(roots2);
            currentTest = "medium forest";
            pForest.Predict(region1);

            AssertRoots(pForest, 0, 1, 2, 3);
            AssertValidNode(pForest[0], 4, 5);
            AssertValidNode(pForest[1], 6);
            AssertValidNode(pForest[2], 7, 8, 9, 10);
            AssertValidNode(pForest[3], 11, 12, 13);

            pForest.Predict(region2);

            AssertRoots(pForest, 1, 8, 3);
            AssertValidNode(pForest[1], 6);
            AssertValidNode(pForest[8]);
            AssertValidNode(pForest[3], 11, 12, 13);
        }

        [Fact]
        public void RegionOfSameSubtreeTest()
        {
            List<RoadNetworkNode> roots1 = new List<RoadNetworkNode>
            {
                new RoadNetworkNode(0, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(1, It.IsAny<double>(), It.IsAny<double>())
            };
            List<RoadNetworkNode> roots2 = new List<RoadNetworkNode>
            {
                new RoadNetworkNode(1, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(6, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(8, It.IsAny<double>(), It.IsAny<double>())
            };
            Region region1 = new Region(new Coordinates(1, 1), 0);
            Region region2 = new Region(new Coordinates(2, 2), 0);
            RoadNetworkNode center1 = new RoadNetworkNode(0, region1.Center.Latitude, region1.Center.Longitude);
            RoadNetworkNode center2 = new RoadNetworkNode(7, region2.Center.Latitude, region2.Center.Longitude);
            mockRoadNetworks.Setup(m => m.Nearest(region1.Center.Latitude, region1.Center.Longitude)).Returns(center1);
            mockRoadNetworks.Setup(m => m.Nearest(region2.Center.Latitude, region2.Center.Longitude)).Returns(center2);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center1, It.IsAny<double>())).Returns(roots1);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center2, It.IsAny<double>())).Returns(roots2);
            currentTest = "simple forest";
            pForest.Predict(region1);

            AssertRoots(pForest, 0, 1);
            AssertValidNode(pForest[0], 2, 3);
            AssertValidNode(pForest[1], 4, 5, 6);

            currentTest = "simple forest 2";
            pForest.Predict(region2);

            AssertRoots(pForest, 1, 6);
            AssertValidNode(pForest[1], 4, 5);
        }

        [Fact]
        public void PredictsSomeInRegionTest()
        {
            List<RoadNetworkNode> roots1 = new List<RoadNetworkNode>
            {
                new RoadNetworkNode(0, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(1, It.IsAny<double>(), It.IsAny<double>())
            };
            List<RoadNetworkNode> roots2 = new List<RoadNetworkNode>
            {
                new RoadNetworkNode(1, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(8, It.IsAny<double>(), It.IsAny<double>())
            };
            Region region1 = new Region(new Coordinates(1, 1), 0);
            Region region2 = new Region(new Coordinates(2, 2), 0);
            RoadNetworkNode center1 = new RoadNetworkNode(0, region1.Center.Latitude, region1.Center.Longitude);
            RoadNetworkNode center2 = new RoadNetworkNode(7, region2.Center.Latitude, region2.Center.Longitude);
            mockRoadNetworks.Setup(m => m.Nearest(region1.Center.Latitude, region1.Center.Longitude)).Returns(center1);
            mockRoadNetworks.Setup(m => m.Nearest(region2.Center.Latitude, region2.Center.Longitude)).Returns(center2);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center1, It.IsAny<double>())).Returns(roots1);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center2, It.IsAny<double>())).Returns(roots2);
            currentTest = "simple forest";
            pForest.Predict(region1);

            AssertRoots(pForest, 0, 1);
            AssertValidNode(pForest[0], 2, 3);
            AssertValidNode(pForest[1], 4, 5, 6);

            currentTest = "simple forest 2";
            pForest.Predict(region2);

            AssertRoots(pForest, 1);
            AssertValidNode(pForest[1], 4, 5, 6);
        }

        [Fact]
        public void PredictsNoneInRegionTest()
        {
            List<RoadNetworkNode> roots1 = new List<RoadNetworkNode>
            {
                new RoadNetworkNode(0, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(1, It.IsAny<double>(), It.IsAny<double>())
            };
            List<RoadNetworkNode> roots2 = new List<RoadNetworkNode>
            {
                new RoadNetworkNode(7, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(8, It.IsAny<double>(), It.IsAny<double>())
            };
            Region region1 = new Region(new Coordinates(1, 1), 0);
            Region region2 = new Region(new Coordinates(2, 2), 0);
            RoadNetworkNode center1 = new RoadNetworkNode(0, region1.Center.Latitude, region1.Center.Longitude);
            RoadNetworkNode center2 = new RoadNetworkNode(7, region2.Center.Latitude, region2.Center.Longitude);
            mockRoadNetworks.Setup(m => m.Nearest(region1.Center.Latitude, region1.Center.Longitude)).Returns(center1);
            mockRoadNetworks.Setup(m => m.Nearest(region2.Center.Latitude, region2.Center.Longitude)).Returns(center2);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center1, It.IsAny<double>())).Returns(roots1);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center2, It.IsAny<double>())).Returns(roots2);
            currentTest = "simple forest";
            pForest.Predict(region1);

            AssertRoots(pForest, 0, 1);
            AssertValidNode(pForest[0], 2, 3);
            AssertValidNode(pForest[1], 4, 5, 6);

            currentTest = "simple forest 2";
            pForest.Predict(region2);

            AssertRoots(pForest, 7, 8);
            AssertValidNode(pForest[7], 9, 10);
            AssertValidNode(pForest[8], 11, 12, 13);
        }

        [Fact]
        public void ExecludesExecludedNodesTest()
        {
            List<RoadNetworkNode> roots1 = new List<RoadNetworkNode>
            {
                new RoadNetworkNode(0, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(1, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(2, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(3, It.IsAny<double>(), It.IsAny<double>())
            };
            List<RoadNetworkNode> roots2 = new List<RoadNetworkNode>
            {
                new RoadNetworkNode(0, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(3, It.IsAny<double>(), It.IsAny<double>())
            };
            List<RoadNetworkNode> roots3 = new List<RoadNetworkNode>
            {
                new RoadNetworkNode(1, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(3, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(2, It.IsAny<double>(), It.IsAny<double>())
            };
            Region region1 = new Region(new Coordinates(1, 1), 0);
            Region region2 = new Region(new Coordinates(2, 2), 0);
            Region region3 = new Region(new Coordinates(3, 3), 0);
            RoadNetworkNode center1 = new RoadNetworkNode(0, region1.Center.Latitude, region1.Center.Longitude);
            RoadNetworkNode center2 = new RoadNetworkNode(3, region2.Center.Latitude, region2.Center.Longitude);
            RoadNetworkNode center3 = new RoadNetworkNode(2, region2.Center.Latitude, region2.Center.Longitude);
            mockRoadNetworks.Setup(m => m.Nearest(region1.Center.Latitude, region1.Center.Longitude)).Returns(center1);
            mockRoadNetworks.Setup(m => m.Nearest(region2.Center.Latitude, region2.Center.Longitude)).Returns(center2);
            mockRoadNetworks.Setup(m => m.Nearest(region3.Center.Latitude, region3.Center.Longitude)).Returns(center3);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center1, It.IsAny<double>())).Returns(roots1);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center2, It.IsAny<double>())).Returns(roots2);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center3, It.IsAny<double>())).Returns(roots3);
            currentTest = "medium forest";
            pForest.Predict(region1);

            AssertRoots(pForest, 0, 1, 2, 3);
            AssertValidNode(pForest[0], 4, 5);
            AssertValidNode(pForest[1], 6);
            AssertValidNode(pForest[2], 7, 8, 9, 10);
            AssertValidNode(pForest[3], 11, 12, 13);

            pForest.Predict(region2);

            AssertRoots(pForest, 0, 3);
            AssertValidNode(pForest[0], 4, 5);
            AssertValidNode(pForest[3], 11, 12, 13);

            pForest.Predict(region3);

            AssertRoots(pForest, 3);
            AssertValidNode(pForest[3], 11, 12, 13);
        }

        [Fact]
        public void CalculatesAccuracyCorrectly()
        {
            List<RoadNetworkNode> roots1 = new List<RoadNetworkNode>
            {
                new RoadNetworkNode(0, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(1, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(2, It.IsAny<double>(), It.IsAny<double>()),
                new RoadNetworkNode(3, It.IsAny<double>(), It.IsAny<double>())
            };
            List<RoadNetworkNode> roots2 = new List<RoadNetworkNode>
            {
                new RoadNetworkNode(6, It.IsAny<double>(), It.IsAny<double>())
            };
            List<RoadNetworkNode> roots3 = new List<RoadNetworkNode>
            {
                new RoadNetworkNode(14, It.IsAny<double>(), It.IsAny<double>())
            };
            Region region1 = new Region(new Coordinates(1, 1), 0);
            Region region2 = new Region(new Coordinates(2, 2), 0);
            Region region3 = new Region(new Coordinates(3, 3), 0);
            RoadNetworkNode center1 = new RoadNetworkNode(0, region1.Center.Latitude, region1.Center.Longitude);
            RoadNetworkNode center2 = new RoadNetworkNode(0, region2.Center.Latitude, region2.Center.Longitude);
            RoadNetworkNode center3 = new RoadNetworkNode(0, region3.Center.Latitude, region3.Center.Longitude);
            mockRoadNetworks.Setup(m => m.Nearest(region1.Center.Latitude, region1.Center.Longitude)).Returns(center1);
            mockRoadNetworks.Setup(m => m.Nearest(region2.Center.Latitude, region2.Center.Longitude)).Returns(center2);
            mockRoadNetworks.Setup(m => m.Nearest(region3.Center.Latitude, region3.Center.Longitude)).Returns(center3);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center1, It.IsAny<double>())).Returns(roots1);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center2, It.IsAny<double>())).Returns(roots2);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center3, It.IsAny<double>())).Returns(roots3);
            currentTest = "medium forest 2";

            pForest.Predict(region1);

            AssertRoots(pForest, 0, 1, 2, 3);
            AssertValidNode(pForest[0], 4, 5);
            AssertValidNode(pForest[1], 6);
            AssertValidNode(pForest[2], 7, 8, 9, 10);
            AssertValidNode(pForest[3], 11, 12, 13);
            AssertValidNode(pForest[6], 14, 15);
            AssertValidNode(pForest[13], 16);

            pForest.Predict(region2);

            AssertRoots(pForest, 6);
            AssertValidNode(pForest[6], 14, 15);

            pForest.Predict(region3);

            AssertRoots(pForest, 14);
            AssertValidNode(pForest[14]);
        }
    }
}
