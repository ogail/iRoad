using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryProcessing.DataStructures
{
    public class RoadNetworkNode
    {
        public int Id { get; set; }

        public Coordinates Location { get; set; }

        public Dictionary<RoadNetworkNode, Edge> OutEdges { get; set; }

        public Dictionary<RoadNetworkNode, Edge> InEdges { get; set; }

        public int Flag { get; set; }

        public Dictionary<int, PredictedObjectRecord> PredictedObjects { get; set; }

        /// <summary>
        /// Contains the list of objects ids for the objects currently around this node
        /// </summary>
        public List<int> CurrentObjectsList { get; set; }

        public RoadNetworkNode(int id, double lat, double lng)
        {
            Id = id;
            Flag = 0;
            Location= new Coordinates(lat, lng);
            OutEdges= new Dictionary<RoadNetworkNode, Edge>();
            InEdges = new Dictionary<RoadNetworkNode,Edge>();            
            PredictedObjects = new Dictionary<int, PredictedObjectRecord>();
            CurrentObjectsList = new List<int>();
        }
        
        public void AddInEdge(Edge EdgeID) //add edge when reading edge representing the connnectivity
        {
            this.InEdges.Add(EdgeID.From, EdgeID);
        }

        public void AddOutEdge(Edge EdgeID) //add edge when reading edge representing the connnectivity
        {
            this.OutEdges.Add(EdgeID.To, EdgeID);
        }


        public void AddPredictedObject(int objectId, double objectProbability, double distanceToRoot)
        {
            if (!this.PredictedObjects.ContainsKey(objectId))
            {
                PredictedObjectRecord predObject = new PredictedObjectRecord(objectId, objectProbability, distanceToRoot);
                this.PredictedObjects.Add(objectId, predObject);
            }

        }

        public void DeletePredictedObject(int objectId)
        {
            if (this.PredictedObjects.ContainsKey(objectId))
            {
                this.PredictedObjects.Remove(objectId);
            }

        }

        public Dictionary<int, PredictedObjectRecord> GetPredictedObjects()
        {
            return this.PredictedObjects;
        }

        // experiments
        public double GetPredictedObjectProbability(int objectId) 
        {
            double objectProbability = 0;
            if(this.PredictedObjects.ContainsKey(objectId))
            {
                objectProbability = this.PredictedObjects[objectId].GetProbability();

            }
            return objectProbability;
        }

        public void AddCurrentObject(int objectId) 
        {
            CurrentObjectsList.Add(objectId);
        }

        public void DeleteCurrentObject(int objectId)
        {
            CurrentObjectsList.Remove(objectId);
        }

        public List<int> GetCurrentObjects()
        {
            return CurrentObjectsList;
        }
     
        public void EmptyPredictedObjects()
        {
            PredictedObjects.Clear();
        }
    }
}
