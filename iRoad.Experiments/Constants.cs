using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRoad.Experiments
{
    public class Constants
    {
        public static Coordinates InboxMaxCoordinates = new Coordinates(48.9999848, -117.0744739);
        public static Coordinates InboxMinCoordinates = new Coordinates(45.6325817, -123.4499719);
        public static int NumberOfPassedNodesToStartQuery = 5; // start reading the queries for experiments at this condition, to give objects some time to prune the trees
        public static int ObjectsSpeedOnScreenInMS = 25; // the speed of the objects on the screen in milliseconds 
        public static int ObjectsSpeedExperimentsMS = 0; // the speed of the objects for experimental evaluation in milliseconds 
        public static double Speed = 20;
        public static double T = 10;
        public static double ProbabilityThreshold = 0.00; // 0.01;
        public static double TimeRange = Speed * 2 * T / 60 / 1.6;
        public static string ObjectsFileName = "Object1.txt";
        public static string EdgesFileName = "WA_Edges.txt";
        public static string EdgeGeometeryFileName = "WA_EdgeGeometry.txt";
        public static string NodesFilename = "WA_Nodes.txt";
        public static string ResultFileName = "ExpAccuracyResults\\OneStepPrediction\\RealObjects_on_WA_RoadNetwork_iRoad\\5PassedNode_1NextNode_P=0.00_O1.txt";
    }
}
