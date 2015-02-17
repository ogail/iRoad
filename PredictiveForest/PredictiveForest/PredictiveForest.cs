using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace QueryProcessing.DataStructures
{
    /// <summary>
    /// A predictive forest is a public class that hold range of predictive trees to keep track of
    /// a range of nodes instead of an exact node.
    /// </summary>
    public class PredictiveForest : IEnumerable<TreeNode>
    {
        /// <summary>
        /// Represents the forest nodes.
        /// </summary>
        private Dictionary<int, TreeNode> forest;

        /// <summary>
        /// Holds the execluded nodes from the history.
        /// </summary>
        private HashSet<int> execluded;

        /// <summary>
        /// The root nodes for the forest.
        /// </summary>
        public List<TreeNode> Roots
        {
            get {return forest.Values.Where(n => n.Parent == null).ToList(); }
        }

        /// <summary>
        /// The road network associated with the forest.
        /// </summary>
        public RoadNetworks RoadNetwork { get; private set; }

        /// <summary>
        /// Number of nodes in the forest.
        /// </summary>
        public int Count { get { return forest.Count; } }

        /// <summary>
        /// The search region.
        /// </summary>
        public Region Region { get; private set; }

        /// <summary>
        /// The prediction time range
        /// </summary>
        public double TimeRange { get; private set; }

        public double ProbabilityThreshold { get; private set; }

        public Func<RoadNetworks, RoadNetworkNode, double, double, TreeNode> TreeBuilder { get; set; }

        /// <summary>
        /// Constructor to create the forest.
        /// </summary>
        /// <param name="roadNetwork">The road network object</param>
        /// <param name="center">The center of the forest, which is used to fetch the neighbors</param>
        /// <param name="range">The tree length</param>
        public PredictiveForest(RoadNetworks roadNetwork, double timeRange, double probabilityThreshold)
        {
            this.RoadNetwork = roadNetwork;
            this.execluded = new HashSet<int>();
            this.ProbabilityThreshold = probabilityThreshold;
            this.forest = new Dictionary<int, TreeNode>();
            this.TimeRange = timeRange;
            this.ProbabilityThreshold = probabilityThreshold;
            TreeBuilder = (rn, n, r, p) =>
            {
                PredectiveTree pTree = new PredectiveTree(RoadNetwork, n, TimeRange, ProbabilityThreshold);
                pTree.BuildTree();
                Debug.Assert(pTree.myTree[n.Id].Parent == null);
                return pTree.myTree[n.Id];
            };
        }

        public void Predict(Region region)
        {
            this.Region = region;
            List<RoadNetworkNode> nodes = GetRegionNodes(region);

            if (forest.Count == 0)
            {
                Build(nodes);
            }
            else
            {
                // Remove all nodes that does not belong to the current forest
                nodes = nodes.Where(n => forest.ContainsKey(n.Id)).ToList();

                if (nodes.Count == 0)
                {
                    Build(nodes);
                }
                else
                {
                    Update();
                }
            }

            ProbabilityExpansion();
        }

        public TreeNode this[int key]
        {
            get { return forest[key]; }
            set { forest[key] = value; }
        }

        private void ProbabilityExpansion()
        {
            foreach (TreeNode root in Roots)
            {
                TreeNode ti = forest[root.Id];
                ti.Probability = 1.0;
                int degree = ti.Children.Count;
                Queue<TreeNode> queue = new Queue<TreeNode>();
                queue.Enqueue(ti);
                while (queue.Count > 0)
                {
                    TreeNode dqitem = queue.Dequeue();
                    degree = dqitem.Children.Count;
                    for (int i = 0; i < dqitem.Children.Count; i++)
                    {
                        TreeNode iqitem = dqitem.Children.ElementAt(i);
                        iqitem.Probability = dqitem.Probability / degree;
                        {
                            queue.Enqueue(iqitem);
                        }
                    }
                }
            }
        }

        private void Update()
        {
            throw new NotImplementedException();
        }

        private void Build(List<RoadNetworkNode> roots)
        {
            foreach (RoadNetworkNode root in roots)
            {
                Queue<TreeNode> nodes = new Queue<TreeNode>();
                nodes.Enqueue(TreeBuilder(RoadNetwork, root, TimeRange, ProbabilityThreshold));

                while (nodes.Count != 0)
                {
                    TreeNode node = nodes.Dequeue();

                    if (!execluded.Contains(node.Id))
                    {
                        TreeNode forestNode = null;
                        bool exists = forest.TryGetValue(node.Id, out forestNode);

                        if (exists && node.DistanceToRoot < forestNode.DistanceToRoot)
                        {
                            RemoveSubtree(forestNode);
                            AddNode(node);
                            node.Children.ForEach(n => nodes.Enqueue(n));
                        }
                        else if (!exists)
                        {
                            AddNode(node);
                            node.Children.ForEach(n => nodes.Enqueue(n));
                        }
                        else if (root.Id == node.Id)
                        {
                            Debug.Assert(exists);
                            // This is to handle root node that has been added before in earlier loop iterations.
                            node.Children.ForEach(n => nodes.Enqueue(n));
                        }
                    }
                }
            }
        }

        private void RemoveSubtree(TreeNode forestNode)
        {
            Stack<int> subtree = new Stack<int>();
            Queue<int> current = new Queue<int>();
            current.Enqueue(forestNode.Id);
            while (current.Count != 0)
            {
                int id = current.Dequeue();
                subtree.Push(id);
                forest[id].Children.ForEach(n => current.Enqueue(n.Id));
            }

            while (subtree.Count != 0)
            {
                int id = subtree.Pop();
                Debug.Assert(forest.ContainsKey(id));
                RemoveNode(forest[id]);
            }
        }

        private void RemoveNode(TreeNode node)
        {
            Debug.Assert(forest.ContainsKey(node.Id));
            Debug.Assert(forest.ContainsKey(node.Parent.Id));
            Debug.Assert(forest[node.Id].Children.Count == 0);
            forest.Remove(node.Id);
            int count = forest[node.Parent.Id].Children.RemoveAll(n => n.Equals(node));
            Debug.Assert(count == 1);
        }

        private void AddNode(TreeNode node)
        {
            Debug.Assert(!execluded.Contains(node.Id));
            Debug.Assert(!forest.ContainsKey(node.Id));
            TreeNode clone = node.Clone();
            Debug.Assert(clone.Children.Count == 0);

            forest.Add(node.Id, clone);
            if (node.Parent != null)
            {
                Debug.Assert(forest.ContainsKey(clone.Parent.Id));
                forest[clone.Parent.Id].AddChild(clone);
            }
        }

        private List<RoadNetworkNode> GetRegionNodes(DataStructures.Region region)
        {
            RoadNetworkNode centerNode = RoadNetwork.Nearest(Region.Center.Latitude, Region.Center.Longitude);
            return RoadNetwork.GetNeighbors(centerNode, region.Radius).Where(n => !execluded.Contains(n.Id)).ToList();
        }

        public IEnumerator<TreeNode> GetEnumerator()
        {
            return forest.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
