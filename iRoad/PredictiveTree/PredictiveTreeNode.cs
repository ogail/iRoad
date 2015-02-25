using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace QueryProcessing.DataStructures
{
    public class PredictiveTreeNode
    {
        public int Id { get; set; }

        public PredictiveTreeNode Parent { get; set; }

        public RoadNetworkNode Node { get; set; }

        public double Probability { get; set; }

        public double DistanceToRoot { get; set; }

        public List<PredictiveTreeNode> Children { get; set; }

        public PredictiveTreeNode()
        {
            Children = new List<PredictiveTreeNode>();
            this.Probability = 0;
        }
        
        public PredictiveTreeNode(RoadNetworkNode n)
        {
            this.Id = n.Id;
            this.Node = n;
            this.Probability = 0;
            Children = new List<PredictiveTreeNode>();
        }

        public void AddChild(PredictiveTreeNode ti)
        {
            Debug.Assert(ti.Parent != null && ti.Parent.Id == Id);
            Debug.Assert(!Children.Any(n => n.Id == ti.Id));
            this.Children.Add(ti);
        }

        public void RemoveChild(PredictiveTreeNode ti)
        {
            Debug.Assert(ti.Parent != null && ti.Parent.Id == Id);
            Debug.Assert(Children.Any(n => n.Id == ti.Id));
            this.Children.Remove(ti);
            ti.Parent = null;
        }

        public override bool Equals(Object t)
        {
            return this.Id.Equals(((PredictiveTreeNode)t).Id);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public PredictiveTreeNode GetNextTreeNode(int nextNodeId)
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

        public PredictiveTreeNode Clone(PredictiveTreeNode parent)
        {
            return new PredictiveTreeNode()
            {
                DistanceToRoot = this.DistanceToRoot,
                Id = this.Id,
                Node = this.Node,
                Parent = parent,
                Probability = this.Probability,
            };
        }
    }
}
