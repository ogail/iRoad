using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRoad
{
    public class PredictedObjectRecord
    {
        int objectId { get; set; }
        double probability { get; set; }
        double distanceToNode { get; set; } // From Object current Location to a reachable node

        public PredictedObjectRecord(int objId, double prob, double distance)
        {
            this.objectId = objId;
            this.probability = prob;
            this.distanceToNode = distance;
        }

        public int GetObjectId() 
        {
            return this.objectId;
        }

        public double GetProbability()
        {
            return this.probability;
        }

        public double GetDistanceToNode()
        {
            return this.distanceToNode;
        }

    }
}
