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

        private Dictionary<string, Dictionary<int, TreeNode>> generatedTrees;

        private string currentTest;

        private List<RoadNetworkNode> roots;

        public PredectiveForestTests()
        {
            roots = new List<RoadNetworkNode>();
            mockRoadNetworks = new Mock<RoadNetworks>();
            mockRoadNetworks.Setup(m => m.Nearest(It.IsAny<double>(), It.IsAny<double>()));
            mockRoadNetworks.Setup(m => m.GetNeighbors(It.IsAny<RoadNetworkNode>(), It.IsAny<double>())).
                Returns(roots);
            generatedTrees = new Dictionary<string, Dictionary<int, TreeNode>>();
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

        private TreeNode MockTreeBuilder(RoadNetworks roadNetwork, RoadNetworkNode roadNode, double timeRange, double probabilityThreshold)
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
                    generatedTrees[currentForest] = new Dictionary<int, TreeNode>();
                }
                else
                {
                    string[] splitted = line.Split(new char[] { ':', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                    TreeNode node = new TreeNode { Id = int.Parse(splitted.First()) };
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

                        node.AddChild(new TreeNode { Id = id, DistanceToRoot = distance, Parent = node });
                    }
                    Assert.True(!string.IsNullOrEmpty(currentForest));
                    TreeNode parent = generatedTrees[currentForest].Values.FirstOrDefault(n => n.Children.Any(c => c.Id == node.Id));

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

        private void AssertHasIds(List<TreeNode> list, params int[] children)
        {
            Assert.Equal(list.Count, children.Length);
            Assert.True(children.ToList().TrueForAll(n => list.Any(t => t.Id == n)));

            if (list.Count > 0)
            {
                // Assert probabilities
                double parentProbability = list[0].Parent == null ? 1.0 : list[0].Parent.Probability;
                Assert.True(list.All(n => n.Probability == parentProbability / children.Length));
            }
        }

        [Fact]
        public void BuildingSimplePredectiveForestTest()
        {
            GenerateRoadNetworkRoots(0, 1);
            Region region = new Region(new Coordinates(It.IsAny<double>(), It.IsAny<double>()));
            currentTest = "simple forest";
            pForest.Predict(region);

            AssertHasIds(pForest.Roots, 0, 1);
            AssertHasIds(pForest[0].Children, 2, 3);
            AssertHasIds(pForest[1].Children, 4, 5, 6);
        }

        [Fact]
        public void MergingNodesTest()
        {
            GenerateRoadNetworkRoots(0, 1);
            Region region = new Region(new Coordinates(It.IsAny<double>(), It.IsAny<double>()));
            currentTest = "conflict forest";
            pForest.Predict(region);

            AssertHasIds(pForest.Roots, 0, 1);
            AssertHasIds(pForest[0].Children, 3);
            AssertHasIds(pForest[1].Children, 2, 4);
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
            Region region1 = new Region(new Coordinates(It.IsAny<double>(), It.IsAny<double>()));
            RoadNetworkNode center1 = new RoadNetworkNode(0, region1.Center.Latitude, region1.Center.Longitude);
            mockRoadNetworks.Setup(m => m.Nearest(region1.Center.Latitude, region1.Center.Longitude)).Returns(center1);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center1, It.IsAny<double>())).Returns(roots1);
            currentTest = "medium forest 2";
            pForest.Predict(region1);

            AssertHasIds(pForest.Roots, 0, 1, 2, 3);
            AssertHasIds(pForest[0].Children, 4, 5);
            AssertHasIds(pForest[1].Children, 6);
            AssertHasIds(pForest[2].Children, 7, 8, 9, 10);
            AssertHasIds(pForest[3].Children, 11, 12, 13);
            AssertHasIds(pForest[6].Children, 14, 15);
            AssertHasIds(pForest[13].Children, 16);
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
            Region region1 = new Region(new Coordinates(1, 1));
            Region region2 = new Region(new Coordinates(2, 2));
            RoadNetworkNode center1 = new RoadNetworkNode(0, region1.Center.Latitude, region1.Center.Longitude);
            RoadNetworkNode center2 = new RoadNetworkNode(3, region2.Center.Latitude, region2.Center.Longitude);
            mockRoadNetworks.Setup(m => m.Nearest(region1.Center.Latitude, region1.Center.Longitude)).Returns(center1);
            mockRoadNetworks.Setup(m => m.Nearest(region2.Center.Latitude, region2.Center.Longitude)).Returns(center2);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center1, It.IsAny<double>())).Returns(roots1);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center2, It.IsAny<double>())).Returns(roots2);
            currentTest = "medium forest";
            pForest.Predict(region1);

            AssertHasIds(pForest.Roots, 0, 1, 2, 3);
            AssertHasIds(pForest[0].Children, 4, 5);
            AssertHasIds(pForest[1].Children, 6);
            AssertHasIds(pForest[2].Children, 7, 8, 9, 10);
            AssertHasIds(pForest[3].Children, 11, 12, 13);

            pForest.Predict(region2);

            AssertHasIds(pForest.Roots, 1, 8, 3);
            AssertHasIds(pForest[1].Children, 6);
            AssertHasIds(pForest[8].Children);
            AssertHasIds(pForest[3].Children, 11, 12, 13);
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
            Region region1 = new Region(new Coordinates(1, 1));
            Region region2 = new Region(new Coordinates(2, 2));
            RoadNetworkNode center1 = new RoadNetworkNode(0, region1.Center.Latitude, region1.Center.Longitude);
            RoadNetworkNode center2 = new RoadNetworkNode(7, region2.Center.Latitude, region2.Center.Longitude);
            mockRoadNetworks.Setup(m => m.Nearest(region1.Center.Latitude, region1.Center.Longitude)).Returns(center1);
            mockRoadNetworks.Setup(m => m.Nearest(region2.Center.Latitude, region2.Center.Longitude)).Returns(center2);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center1, It.IsAny<double>())).Returns(roots1);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center2, It.IsAny<double>())).Returns(roots2);
            currentTest = "simple forest";
            pForest.Predict(region1);

            AssertHasIds(pForest.Roots, 0, 1);
            AssertHasIds(pForest[0].Children, 2, 3);
            AssertHasIds(pForest[1].Children, 4, 5, 6);

            currentTest = "simple forest 2";
            pForest.Predict(region2);

            AssertHasIds(pForest.Roots, 1, 6);
            AssertHasIds(pForest[1].Children, 4, 5);
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
            Region region1 = new Region(new Coordinates(1, 1));
            Region region2 = new Region(new Coordinates(2, 2));
            RoadNetworkNode center1 = new RoadNetworkNode(0, region1.Center.Latitude, region1.Center.Longitude);
            RoadNetworkNode center2 = new RoadNetworkNode(7, region2.Center.Latitude, region2.Center.Longitude);
            mockRoadNetworks.Setup(m => m.Nearest(region1.Center.Latitude, region1.Center.Longitude)).Returns(center1);
            mockRoadNetworks.Setup(m => m.Nearest(region2.Center.Latitude, region2.Center.Longitude)).Returns(center2);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center1, It.IsAny<double>())).Returns(roots1);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center2, It.IsAny<double>())).Returns(roots2);
            currentTest = "simple forest";
            pForest.Predict(region1);

            AssertHasIds(pForest.Roots, 0, 1);
            AssertHasIds(pForest[0].Children, 2, 3);
            AssertHasIds(pForest[1].Children, 4, 5, 6);

            currentTest = "simple forest 2";
            pForest.Predict(region2);

            AssertHasIds(pForest.Roots, 1);
            AssertHasIds(pForest[1].Children, 4, 5, 6);
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
            Region region1 = new Region(new Coordinates(1, 1));
            Region region2 = new Region(new Coordinates(2, 2));
            RoadNetworkNode center1 = new RoadNetworkNode(0, region1.Center.Latitude, region1.Center.Longitude);
            RoadNetworkNode center2 = new RoadNetworkNode(7, region2.Center.Latitude, region2.Center.Longitude);
            mockRoadNetworks.Setup(m => m.Nearest(region1.Center.Latitude, region1.Center.Longitude)).Returns(center1);
            mockRoadNetworks.Setup(m => m.Nearest(region2.Center.Latitude, region2.Center.Longitude)).Returns(center2);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center1, It.IsAny<double>())).Returns(roots1);
            mockRoadNetworks.Setup(m => m.GetNeighbors(center2, It.IsAny<double>())).Returns(roots2);
            currentTest = "simple forest";
            pForest.Predict(region1);

            AssertHasIds(pForest.Roots, 0, 1);
            AssertHasIds(pForest[0].Children, 2, 3);
            AssertHasIds(pForest[1].Children, 4, 5, 6);

            currentTest = "simple forest 2";
            pForest.Predict(region2);

            AssertHasIds(pForest.Roots, 7, 8);
            AssertHasIds(pForest[7].Children, 9, 10);
            AssertHasIds(pForest[8].Children, 11, 12, 13);
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
            Region region1 = new Region(new Coordinates(1, 1));
            Region region2 = new Region(new Coordinates(2, 2));
            Region region3 = new Region(new Coordinates(3, 3));
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

            AssertHasIds(pForest.Roots, 0, 1, 2, 3);
            AssertHasIds(pForest[0].Children, 4, 5);
            AssertHasIds(pForest[1].Children, 6);
            AssertHasIds(pForest[2].Children, 7, 8, 9, 10);
            AssertHasIds(pForest[3].Children, 11, 12, 13);

            pForest.Predict(region2);

            AssertHasIds(pForest.Roots, 0, 3);
            AssertHasIds(pForest[0].Children, 4, 5);
            AssertHasIds(pForest[3].Children, 11, 12, 13);

            pForest.Predict(region3);

            AssertHasIds(pForest.Roots, 3);
            AssertHasIds(pForest[3].Children, 11, 12, 13);
        }
    }
}
