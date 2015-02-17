using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryProcessing.DataStructures
{
    public class Edge
    {
        public int EdgeID;
        public RoadNetworkNode From;   //Start node id
        public RoadNetworkNode To;         //End node id
        public int  Cost; // time to travel this segment
        public string Name;
        public string Type;
        //public double probability;
        public List<Coordinates> Shape;
        public Coordinates MAX;
        public Coordinates MIN;

        public double Length; // in kilometers

        const double _eQuatorialEarthRadius = 6378.1370D;
        const double SecCoDiff = 30.887;
        const double _d2r = (Math.PI / 180D);


        double DistanceInKM(Coordinates c1, Coordinates c2)
        {
            double lat1 = c1.Latitude;
            double long1 = c1.Longitude;
            double lat2 = c2.Latitude;
            double long2 = c2.Longitude;
            double dlong = (long2 - long1) * _d2r;
            double dlat = (lat2 - lat1) * _d2r;
            double a = Math.Pow(Math.Sin(dlat / 2D), 2D) + Math.Cos(lat1 * _d2r) * Math.Cos(lat2 * _d2r) * Math.Pow(Math.Sin(dlong / 2D), 2D);
            double c = 2D * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1D - a));
            double d = _eQuatorialEarthRadius * c;

            return d;
        }

        public Edge(int EdgeID, RoadNetworkNode from, RoadNetworkNode to, int cost)
        {
            this.EdgeID = EdgeID;
            this.From = from;
            this.To = to;
            this.Cost = cost;
            Shape = new List<Coordinates>();
            MAX = new Coordinates(Double.MinValue, Double.MinValue);
            MIN = new Coordinates(Double.MaxValue, Double.MaxValue);
            double latfix = 0.002;
            double lngfix = 0.002;
            if (From.Location.Latitude > To.Location.Latitude)
            {
                MAX.Latitude = From.Location.Latitude+latfix;
                MIN.Latitude = To.Location.Latitude-latfix;
            }
            else
            {
                MAX.Latitude = To.Location.Latitude+latfix;
                MIN.Latitude = From.Location.Latitude-latfix;
            }


            if (From.Location.Longitude > To.Location.Longitude)
            {
                MAX.Longitude = From.Location.Longitude+lngfix;
                MIN.Longitude = To.Location.Longitude-lngfix;
            }
            else
            {
                MAX.Longitude = To.Location.Longitude+lngfix;
                MIN.Longitude = From.Location.Longitude-lngfix;
            }

            this.Length = DistanceInKM(from.Location, to.Location);
        }


        public void AddCoordinate(double lat, double lng)
        {
            Coordinates c = new Coordinates(lat, lng);
            Shape.Add(c);
            //update MBR
            if (lat > MAX.Latitude)
            {
                MAX.Latitude = lat;
            }
            if (lat < MIN.Latitude)
            {
                MIN.Latitude = lat;
            }
            if (lng > MAX.Longitude)
            {
                MAX.Longitude = lng;
            }
            if (lng < MIN.Longitude)
            {
                MIN.Longitude = lng;
            }
        }

        public Boolean inEdge(Coordinates point)
        {
            if (point.Latitude <= this.MAX.Latitude && point.Latitude >= this.MIN.Latitude && point.Longitude <= MAX.Longitude && point.Longitude >= MIN.Longitude)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public String myToFileString()
        {
            return MAX.Latitude + "," + MAX.Longitude + "," + MIN.Latitude + "," + MIN.Longitude;
        }
        //public 
        //List<>
    }
}
