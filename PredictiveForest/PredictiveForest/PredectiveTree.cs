using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace QueryProcessing.DataStructures
{
    public class PredectiveTree
    {
        private RoadNetworks roadNetwork;

        private PriorityQueue<double, TreeNode> mainQueue;

        private Dictionary<int, TreeNode> tempStorage; //items that are currently in the queue

        private double probabilityThreshold;

        private double range;

        public RoadNetworkNode root;

        public int rootID;

        public TreeNode rootitem;

        public Dictionary<int, TreeNode> myTree;

        public double time;

        static Stopwatch treesStopwatch;

        public PredectiveTree(RoadNetworks RN, RoadNetworkNode n, double range, double probabilityThreshold)
        {
            root = n;
            rootitem = new TreeNode();
            this.rootID = n.Id;
            rootitem.Id = n.Id;
            rootitem.Parent = null;
            rootitem.DistanceToRoot = 0;
            rootitem.Node = n;
            myTree = new Dictionary<int, TreeNode>();
            tempStorage = new Dictionary<int, TreeNode>();
            this.roadNetwork = RN;
            this.range = range;
            this.probabilityThreshold = probabilityThreshold;

            treesStopwatch  = new Stopwatch();

        }

        public void AddItem(TreeNode it)
        {
            if (myTree.Keys.Contains(it.Id) == true)
            {
            }
            else
            {
                //insert into the data set
                myTree.Add(it.Id, it);
                //update the tree structure
                myTree[it.Parent.Id].AddChild(it);

            }
        }

        public void Initialization(TreeNode root)
        {
            //put the root item in the queue
            this.myTree.Add(root.Id, root);

            //put the initial connections in the main queue
            //get the list of connections
            List<TreeNode> connections = GetConnectedItems(root);

            //put them into the main queue
            for (int i = 0; i < connections.Count; i++)
            {
                TreeNode ti = connections.ElementAt(i);
                //InsertQueue(ti);
                mainQueue.Enqueue(ti.DistanceToRoot, ti);
                
                //Console.WriteLine(ti.NodeID + "\t in queue");
                
                tempStorage.Add(ti.Id, ti);
            }
            //this.myTree.Add(root.NodeID, root);
        }

        public List<TreeNode> GetConnectedItems(TreeNode ti)
        {
            List<TreeNode> result = new List<TreeNode>();
            RoadNetworkNode ori = ti.Node;

            foreach (Edge ConnectedEdge in ori.OutEdges.Values)
            {
                RoadNetworkNode nout = ConnectedEdge.To;
                if (myTree.ContainsKey(nout.Id) == false)
                {

                    TreeNode t = new TreeNode(nout);
                    t.Parent = ti;
                    t.DistanceToRoot = ti.DistanceToRoot + ConnectedEdge.Length;
                    result.Add(t);
                }
            }

            return result;
        }
       
        public void GetDistanceFromNodeToTreeRoot(TreeNode ti) // get the distance from the given TreeItem node to the root of an object tree
        {
            if (ti.Parent == null)
            {
                ti.DistanceToRoot = 0;
            }
            else
            {
                double distanceToRoot = 0;
                
                RoadNetworkNode currentNode = ti.Node;
                
                RoadNetworkNode nodeIn = null;

                while (ti.Id != this.rootID && ti.Parent != null)                 
                {
                    foreach (Edge ConnectedEdge in currentNode.InEdges.Values)                    
                    {
                        nodeIn = ConnectedEdge.From;

                        if (nodeIn.Id == ti.Parent.Id)
                        {
                            distanceToRoot = distanceToRoot + ConnectedEdge.Length;
                        }
                    }
                    currentNode = nodeIn;
                    ti = ti.Parent;
                }

                ti.DistanceToRoot = distanceToRoot;
            }
        }

        public double GetNextDistance()
        {
            double result;
            if (mainQueue.IsEmpty == false)
            {
                result = mainQueue.Peek().Key;

            }
            else
            {
                return 0;
            }
            return result;
        }

        public TreeNode GetNextItem()
        {
            //remove from the queue
            TreeNode result = mainQueue.Dequeue().Value;
            //remove from the tempstorage
            if (tempStorage.ContainsKey(result.Id))
            {
                tempStorage.Remove(result.Id);
            }
            return result;
        }

        public void InsertQueue(TreeNode it)
        {

            if (mainQueue.ContainsID(it) == false)
            {
                
                //Console.WriteLine(it.NodeID + "\t in queue");
               
                mainQueue.Enqueue(it.DistanceToRoot, it);
                if (tempStorage.ContainsKey(it.Id) == false)
                {
                    this.tempStorage.Add(it.Id, it);
                }
            }
            else
            {

                TreeNode old = tempStorage[it.Id];

                if (old.DistanceToRoot > it.DistanceToRoot)
                {

                    this.mainQueue.Remove(new KeyValuePair<double, TreeNode>(old.DistanceToRoot, old));
                    this.tempStorage.Remove(old.Id);
                    this.mainQueue.Enqueue(it.DistanceToRoot, it);
                    this.tempStorage.Add(it.Id, it);
                   // Console.WriteLine(it.NodeID + "\t in queues");
                }
                else
                {
                    //skip;
                }

            }
        }

        public void CreateMainQueue()
        {
            mainQueue = new PriorityQueue<double, TreeNode>();
        }

        public void Process()
        {
            //expand the tree and stop when the next item in the heap is over the range
            double cDistance = GetNextDistance();

            while (cDistance < range)
            {
                
                //Console.WriteLine(this.rootID);

                if (this.mainQueue.IsEmpty == false)
                {

                    TreeNode ti = GetNextItem();
                   if(myTree.ContainsKey(ti.Id)==false) {

                        //Console.WriteLine("Queue Size: " + this.mainQueue.Count + "\t Tree Size: " + this.myTree.Count);
                        //Console.WriteLine("Processing Item " + ti.NodeID + "\t" + ti.ThisNode.OutEdges.Count);
                        List<TreeNode> connections = GetConnectedItems(ti);

                        for (int i = 0; i < connections.Count; i++)
                        {
                            TreeNode newti = connections.ElementAt(i);
                           
                            //Console.WriteLine("Inserting " + newti.NodeID);
                            
                            if (myTree.ContainsKey(newti.Id) == false)
                            {
                                InsertQueue(newti);
                            }
                        }
                        this.myTree.Add(ti.Id, ti);
                        this.myTree[ti.Parent.Id].AddChild(ti);


                        if (mainQueue.IsEmpty == false)
                        {
                            cDistance = GetNextDistance();
                            
                            //Console.WriteLine(cDistance);
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }

        public void ProbabilityExpansion(double probabilityThreshold) // assgin probability to the items in the tree
        {
            TreeNode ti = this.rootitem;
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

        public void BuildTree()
        {
            double my_start_time, my_end_time;
            my_start_time = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

            //treesStopwatch.Reset();
            //treesStopwatch.Start();
            
            CreateMainQueue();
            this.Initialization(rootitem); //put initial items in the queue
            this.Process(); //expand the tree
            ProbabilityExpansion(0.01); // assgin probability to the items in the tree           

            //treesStopwatch.Stop();
            //MovementHandler.totalReachabilityTreesConstructionTimeCost += treesStopwatch.ElapsedMilliseconds;

            my_end_time = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

            this.time = my_end_time - my_start_time;
        }

        public void BuildQueryTree(double depth)
        {            
            treesStopwatch.Reset();
            treesStopwatch.Start();

            CreateMainQueue();
            this.Initialization(rootitem); //put initial items in the queue
            this.Process(); //expand the tree
            /// for the queryNode tree we do not assign probabilities to reachable nodes rather it gets all nodes in the range of the query
            //ProbabilityExpansion(0.01); // assgin probability to the items in the tree           

            treesStopwatch.Stop();
            //MovementHandler.totalPredictiveForestsConstructionTimeCost += treesStopwatch.ElapsedMilliseconds;
        }

        public void BuildObjectTree(int objectId)
        {
            //double my_start_time, my_end_time;
            //my_start_time = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
           
            treesStopwatch.Reset();
            treesStopwatch.Start();

            CreateMainQueue();
            this.Initialization(rootitem); //put initial items in the queue
            this.Process(); //expand the tree                        
           
            this.ObjectProbabilityExpansion(objectId, probabilityThreshold);

            treesStopwatch.Stop();
            //MovementHandler.totalPredictiveForestsConstructionTimeCost += treesStopwatch.ElapsedMilliseconds;
            //Console.WriteLine("total reachability trees contsruction time cost : " + MovementHandler.totalReachabilityTreesConstructionTimeCost);

            //my_end_time = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

            //this.time = my_end_time - my_start_time;
        }

        /*
         * Compute probability according to the iRoad prediction model
         */
        public void ObjectProbabilityExpansion(int objectId, double probabilityThreshold) // assgin probability to the items(reachable nodes) in the tree of an object
        {
            TreeNode ti = this.rootitem;
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

                    this.GetDistanceFromNodeToTreeRoot(iqitem); // to get the iqitem.DistanceToRoot
                        

                    if (iqitem.Probability > probabilityThreshold) // this line was commented by J
                    {
                        queue.Enqueue(iqitem);
                        this.roadNetwork.Nodes[iqitem.Id].AddPredictedObject(objectId, iqitem.Probability, iqitem.DistanceToRoot); // add new element to the answer of predictedObjects
                    }

                }
            }

        }

        /*
         * The following method is to compute the probability according to the Network Mobility Model Hoyoung Jeun et al VLDB Journal 2010
         * This model is usded to compare with iRaod prediction model in terms of the accuracy of the prediction 
         * The comparison will be based on the probability given by iRoad to the one given by NMM to next 1 node, next 2 nodes, 3 nodes, ...., where the probability of the actual result is one
         * NMM prediction model assign 1/number of chilren to each child node and does not care about the shortest path nor the trip history. Also, it allows the return at each node, so
         * the passed nodes also will ba having a value
         * NMM does not differentiate between the closest nodes and the far nodes
         */
        public void ObjectProbabilityExpansion_NMM(int objectId, double probabilityThreshold) // assgin probability to the items(reachable nodes) in the tree of an object
        {
            TreeNode ti = this.rootitem;
           // ti.probability = 1.0;
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
                    //iqitem.probability = dqitem.probability / degree;
                    iqitem.Probability = 1 / degree; // each level is separated from the previous levels. Probability of a child node  is 1/number of chilren nodes under this parent node 
                    this.GetDistanceFromNodeToTreeRoot(iqitem); // to get the iqitem.DistanceToRoot


                    if (iqitem.Probability > probabilityThreshold) // this line was commented by J
                    {
                        queue.Enqueue(iqitem); // we add this child node (iquitem) to the queue only if its probability is above the probabilityThreshold
                        this.roadNetwork.Nodes[iqitem.Id].AddPredictedObject(objectId, iqitem.Probability, iqitem.DistanceToRoot); // add new element to the answer of predictedObjects
                    }

                }
            }

        }

        // prune the tree, delete the object instance from the predictedObjects in the reachabel nodes, 
        // add the object with its new probability to the new reachble nodes
        public void UpdateObjectTree(int objectId, int nextNodeId) 
        {
            
            treesStopwatch.Reset();
            treesStopwatch.Start();

            UpdateObjectTree_DeleteFromPredictedObjects(objectId); // delete the object record from the predicted answer
            
            TreeNode treeItem = this.rootitem.GetNextTreeItem(nextNodeId);            
            ObjectSubtree(treeItem); // prune the tree by pointing to the new node as the root of the tree

            this.ObjectProbabilityExpansion(objectId, probabilityThreshold); // insert the object to the nodes of the new tree with its new probability 
            
            treesStopwatch.Stop();
            //MovementHandler.totalPredictiveForestsUpdateTimeCost += treesStopwatch.ElapsedMilliseconds;
            //Console.WriteLine("total reachability trees update time cost : " + MovementHandler.totalReachabilityTreesUpdateTimeCost);

            // To call the NMM probability assignment
            //this.ObjectProbabilityExpansion_NMM(objectId, Parameters.probabilityThreshold); // insert the object to the nodes of the new tree with its new probability 
        }

        public void UpdateObjectTree_DeleteFromPredictedObjects(int objectId) 
        {
            TreeNode ti = this.rootitem;            
            Queue<TreeNode> queue = new Queue<TreeNode>();
            queue.Enqueue(ti);
            while (queue.Count > 0)
            {
                TreeNode dqitem = queue.Dequeue();                
                for (int i = 0; i < dqitem.Children.Count; i++)
                {
                    TreeNode iqitem = dqitem.Children.ElementAt(i);
                    queue.Enqueue(iqitem);
                    this.roadNetwork.Nodes[iqitem.Id].DeletePredictedObject(objectId); // delete an existing element from the answer of predictedObjects
                }
            }

        }

        public void ObjectSubtree(TreeNode tItem) // move the root to one level to point at the new nextNodeId
        {
            if (tItem == null) 
            { 
                //skip
            }
            else 
            {
                if (myTree.ContainsKey(tItem.Id) == true)
                {
                    this.root = myTree[tItem.Id].Node;
                    this.rootitem = myTree[tItem.Id]; //???? double distance = tItem.DistanceToRoot; 
                }
                else
                {
                    //skip
                }
            }
            
        }

        public void RandomStep()
        {
            if (rootitem.Children.Count > 0)
            {
                Random random = new Random();
                int randomNumber = random.Next(0, 100);
                int selection = randomNumber % rootitem.Children.Count;
                TreeNode it = this.rootitem.Children.ElementAt(selection);
                subtree(it);
            }
            else          //no more steps
            {
                return;
            }
        }

        public void subtree(TreeNode it)//get a subtree in reachability tree
        {
            if (myTree.ContainsKey(it.Id) == true)
            {
                this.root = myTree[it.Id].Node;
                this.rootitem = myTree[it.Id];
            }
            else
            {
                //skip
            }
        }

        public void subtree(int id) //get a subtree based on edge id
        {
            int nodeid = this.roadNetwork.Edges[id].To.Id;
            if (myTree.ContainsKey(nodeid) == true)
            {
                this.root = myTree[nodeid].Node;
                this.rootitem = myTree[nodeid];
            }
            else
            {
                //skip
            }
        }

        public bool IsChild(int nodeId) // check if this nodeId is a child of the root node in the tree
        {
            TreeNode ti = this.rootitem;
            for (int i = 0; i < ti.Children.Count; i++)
                if (ti.Children.ElementAt(i).Id == nodeId)
                    return true;

            return false;
        } 
    }
}
