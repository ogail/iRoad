using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace QueryProcessing.DataStructures
{
    public class TreeNode
    {
        public int Id { get; set; }

        public TreeNode Parent { get; set; }

        public RoadNetworkNode Node { get; set; }

        public double Probability { get; set; }

        public double DistanceToRoot { get; set; }

        public List<TreeNode> Children { get; set; }

        public TreeNode()
        {
            Children = new List<TreeNode>();
            this.Probability = 0;
        }
        
        public TreeNode(RoadNetworkNode n)
        {
            this.Id = n.Id;
            this.Node = n;
            this.Probability = 0;
            Children = new List<TreeNode>();
        }

        public void AddChild(TreeNode ti)
        {
            Debug.Assert(ti.Parent != null && ti.Parent.Id == Id);
            Debug.Assert(!Children.Any(n => n.Id == ti.Id));
            this.Children.Add(ti);
        }

        public override bool Equals(Object t)
        {
            return this.Id.Equals(((TreeNode)t).Id);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public TreeNode GetNextTreeItem(int nextNodeId)
        {
            for (int i = 0; i < this.Children.Count; i++)
            {
                if(this.Children[i].Id == nextNodeId)
                {
                   return this.Children[i];
                }
            }

            return null;
        }

        public TreeNode Clone()
        {
            return new TreeNode()
            {
                DistanceToRoot = this.DistanceToRoot,
                Id = this.Id,
                Node = this.Node,
                Parent = this.Parent,
                Probability = this.Probability,
            };
        }
    }
}
