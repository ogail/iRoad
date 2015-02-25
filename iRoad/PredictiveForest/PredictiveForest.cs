using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace iRoad
{
    /// <summary>
    /// A predictive forest is a public class that hold range of predictive trees to keep track of
    /// a range of nodes instead of an exact node.
    /// </summary>
    public class PredictiveForest : IEnumerable<PredictiveTreeNode>
    {
        /// <summary>
        /// Represents the forest nodes.
        /// </summary>
        private Dictionary<int, PredictiveTreeNode> forest;

        /// <summary>
        /// Holds the execluded nodes from the history.
        /// </summary>
        private HashSet<int> execluded;

        /// <summary>
        /// The root nodes for the forest.
        /// </summary>
        public List<PredictiveTreeNode> Roots
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

        /// <summary>
        /// The probability threshold used.
        /// </summary>
        public double ProbabilityThreshold { get; private set; }

        /// <summary>
        /// Build a tree for a given road network node.
        /// </summary>
        public Func<RoadNetworks, RoadNetworkNode, double, double, PredictiveTreeNode> TreeBuilder { get; set; }

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
            this.forest = new Dictionary<int, PredictiveTreeNode>();
            this.TimeRange = timeRange;
            this.ProbabilityThreshold = probabilityThreshold;
            this.TreeBuilder = (rn, n, r, p) =>
            {
                PredectiveTree pTree = new PredectiveTree(RoadNetwork, n, TimeRange, ProbabilityThreshold);
                pTree.BuildTree();
                Debug.Assert(pTree.myTree[n.Id].Parent == null);
                return pTree.myTree[n.Id];
            };
        }

        /// <summary>
        /// Assigns the probability for all forest nodes.
        /// </summary>
        private void ProbabilityAssignment()
        {
            foreach (PredictiveTreeNode root in Roots)
            {
                TraverseSubtree(forest[root.Id], n =>
                {
                    if (forest[n].Parent != null)
                    {
                        Debug.Assert(forest[n].Parent.Id != root.Id || forest[n].Parent.Probability == 1.0 / (double)Roots.Count);
                        forest[n].Probability = forest[n].Parent.Probability / forest[n].Parent.Children.Count;
                    }
                    else
                    {
                        forest[n].Probability = 1.0 / (double)Roots.Count;
                    }
                });
            }
        }

        /// <summary>
        /// Updates the forest with the given root nodes.
        /// </summary>
        /// <param name="roots">The new roots for the forest.</param>
        private void Update(List<RoadNetworkNode> roots)
        {
            HashSet<int> included = new HashSet<int>();

            // Fetch the included nodes
            foreach (RoadNetworkNode roadNode in roots)
            {
                Debug.Assert(forest.ContainsKey(roadNode.Id));
                TraverseSubtree(forest[roadNode.Id], id => included.Add(id));

                PredictiveTreeNode parent = forest[roadNode.Id].Parent;
                if (parent != null)
                {
                    forest[parent.Id].RemoveChild(forest[roadNode.Id]);
                    Debug.Assert(forest[roadNode.Id].Parent == null);
                    Debug.Assert(forest[parent.Id].Children.TrueForAll(n => n.Id != roadNode.Id));
                }
            }

            // Remove relation between included children nodes that have parents which will be excluded
            foreach (int nodeId in included)
            {
                PredictiveTreeNode parent = forest[nodeId].Parent;
                if (parent != null && !included.Contains(parent.Id))
                {
                    forest[parent.Id].RemoveChild(forest[nodeId]);
                    Debug.Assert(forest[nodeId].Parent == null);
                    Debug.Assert(forest[parent.Id].Children.TrueForAll(n => n.Id != nodeId));
                }
            }
            
            // Remove all execluded nodes and add them to the exclude list.
            foreach (PredictiveTreeNode root in Roots)
            {
                RemoveSubtree(root, n => !included.Contains(n)).ForEach(n => execluded.Add(n));
            }

            Debug.Assert(roots.TrueForAll(r => Roots.Any(n => n.Id == r.Id)));
        }

        /// <summary>
        /// Builds a predictive forest from the given roots.
        /// </summary>
        /// <param name="roots">The roots for the forest.</param>
        private void Build(List<RoadNetworkNode> roots)
        {
            forest.Keys.ToList().ForEach(id => execluded.Add(id));
            forest.Clear();

            foreach (RoadNetworkNode root in roots)
            {
                Queue<PredictiveTreeNode> nodes = new Queue<PredictiveTreeNode>();
                PredictiveTreeNode rootNode = TreeBuilder(RoadNetwork, root, TimeRange, ProbabilityThreshold);
                Debug.Assert(rootNode.Parent == null);
                nodes.Enqueue(rootNode);

                while (nodes.Count != 0)
                {
                    PredictiveTreeNode node = nodes.Dequeue();

                    if (!execluded.Contains(node.Id))
                    {
                        PredictiveTreeNode forestNode = null;
                        bool exists = forest.TryGetValue(node.Id, out forestNode);

                        if (exists && node.DistanceToRoot < forestNode.DistanceToRoot)
                        {
                            RemoveSubtree(forestNode, r => true);
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

        /// <summary>
        /// Traverses the subtree of the given node using a BFS algorithm.
        /// </summary>
        /// <param name="subtreeRoot">The subtree root node</param>
        /// <param name="action">Custom action to apply for every node in the subtree.</param>
        private void TraverseSubtree(PredictiveTreeNode subtreeRoot, Action<int> action)
        {
            Queue<int> current = new Queue<int>();
            current.Enqueue(subtreeRoot.Id);
            while (current.Count != 0)
            {
                int id = current.Dequeue();
                action(id);
                forest[id].Children.ForEach(n => current.Enqueue(n.Id));
            }
        }

        /// <summary>
        /// Removes a subtree starting from the passed root node.
        /// </summary>
        /// <param name="subtreeRoot">The subtree root node</param>
        /// <param name="condition">The remove condition which if true the node will be removed.</param>
        /// <returns></returns>
        private List<int> RemoveSubtree(PredictiveTreeNode subtreeRoot, Predicate<int> condition)
        {
            Stack<int> subtree = new Stack<int>();
            List<int> removedIds = new List<int>();
            TraverseSubtree(subtreeRoot, id =>
                {
                    if (condition(id))
                    {
                        subtree.Push(id);
                        removedIds.Add(id);
                    }
                });

            while (subtree.Count != 0)
            {
                int id = subtree.Pop();
                Debug.Assert(forest.ContainsKey(id));
                RemoveNode(forest[id]);
            }

            return removedIds;
        }

        /// <summary>
        /// Removes a single node from the forest.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        private void RemoveNode(PredictiveTreeNode node)
        {
            if (node.Parent != null)
            {
                Debug.Assert(forest.ContainsKey(node.Parent.Id));
                Debug.Assert(!Roots.Any(n => n.Id == node.Id));
                forest[node.Parent.Id].RemoveChild(node);
            }

            Debug.Assert(forest.ContainsKey(node.Id));
            Debug.Assert(forest[node.Id].Children.Count == 0);
            forest.Remove(node.Id);
        }

        /// <summary>
        /// Adds a node to the forest.
        /// </summary>
        /// <param name="node">The node to add.</param>
        private void AddNode(PredictiveTreeNode node)
        {
            Debug.Assert(!execluded.Contains(node.Id));
            Debug.Assert(!forest.ContainsKey(node.Id));
            Debug.Assert(node.Parent == null || forest.ContainsKey(node.Parent.Id));
            PredictiveTreeNode clone = node.Clone(node.Parent == null ? null : forest[node.Parent.Id]);
            Debug.Assert(clone.Children.Count == 0);

            forest.Add(node.Id, clone);
            if (node.Parent != null)
            {
                forest[clone.Parent.Id].AddChild(clone);
                Debug.Assert(Object.ReferenceEquals(forest[clone.Parent.Id], clone.Parent));
            }
        }

        /// <summary>
        /// Gets the nodes in the given region.
        /// </summary>
        /// <param name="region">The region</param>
        /// <returns>List of road network nodes in the specific region.</returns>
        private List<RoadNetworkNode> GetRegionNodes(Region region)
        {
            RoadNetworkNode centerNode = RoadNetwork.Nearest(Region.Center.Latitude, Region.Center.Longitude);
            return RoadNetwork.GetNeighbors(centerNode, region.Radius).Where(n => !execluded.Contains(n.Id)).ToList();
        }

        /// <summary>
        /// Gets the enumerator for the predictive forest.
        /// </summary>
        /// <returns>The IEnumerable object</returns>
        public IEnumerator<PredictiveTreeNode> GetEnumerator()
        {
            return forest.Values.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator for the predictive forest.
        /// </summary>
        /// <returns>The IEnumerable object</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Clears the current contents of the forest.
        /// </summary>
        public void Clear()
        {
            this.forest.Clear();
            this.execluded.Clear();
            this.Region = null;
        }

        /// <summary>
        /// Predicts the next steps for the given region.
        /// </summary>
        /// <param name="region">The region to predict for.</param>
        public void Predict(Region region)
        {
            this.Region = region;
            List<RoadNetworkNode> nodes = GetRegionNodes(region);

            if (forest.Count == 0 || nodes.Where(n => forest.ContainsKey(n.Id)).Count() == 0)
            {
                Build(nodes);
            }
            else
            {
                // This will remove nodes that are outside the region
                nodes = nodes.Where(n => forest.ContainsKey(n.Id)).ToList();
                Update(nodes);
            }

            ProbabilityAssignment();
        }

        /// <summary>
        /// Indexer for the forest.
        /// </summary>
        /// <param name="key">The node id</param>
        /// <returns>The node object</returns>
        public PredictiveTreeNode this[int key]
        {
            get { return forest[key]; }
            set { forest[key] = value; }
        }

        /// <summary>
        /// Gets node with id key and if not found returns null.
        /// </summary>
        /// <param name="key">The node id</param>
        /// <returns>The node itself if the key is present in the forest, null otherwise</returns>
        public PredictiveTreeNode GetNode(int key)
        {
            if (forest.ContainsKey(key))
            {
                return forest[key];
            }
            else
            {
                return null;
            }
        }
    }
}
