using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryProcessing.DataStructures.Experiments
{
    public class Constants
    {
        public static Coordinates inboxMaxCoordinates = new Coordinates(48.9999848, -117.0744739);
        public static Coordinates inboxMinCoordinates = new Coordinates(45.6325817, -123.4499719);
        public static int numberOfPassedNodesToStartQuery = 5; // start reading the queries for experiments at this condition, to give objects some time to prune the trees
        public static int objectsSpeedOnScreenInMS = 25; // the speed of the objects on the screen in milliseconds 
        public static int objectsSpeedExperimentsMS = 0; // the speed of the objects for experimental evaluatioin in milliseconds 
        public static double speed = 20;
        public static double T = 10;
        public static double probabilityThreshold = 0.00; // 0.01;
        public static double timeRange = speed * 2 * T / 60 / 1.6;
        public static string objectsFileName = "Object1.txt";
        public static string edgesFileName = "WA_Edges.txt";
        public static string edgeGeometeryFileName = "WA_EdgeGeometry.txt";
        public static string nodesFilename = "WA_Nodes.txt";
        public static string resultFileName = "ExpAccuracyResults\\OneStepPrediction\\RealObjects_on_WA_RoadNetwork_iRoad\\5PassedNode_1NextNode_P=0.00_O1.txt";
    }
}
