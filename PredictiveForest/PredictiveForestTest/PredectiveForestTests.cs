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
                    generatedTrees[currentForest].Add(node.Id, node);
                }
            }
        }

        private void AssertHasIds(List<TreeNode> list, params int[] children)
        {
            Assert.Equal(list.Count, children.Length);
            Assert.True(children.ToList().TrueForAll(n => list.Any(t => t.Id == n)));
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
    }
}
