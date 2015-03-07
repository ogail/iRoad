using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRoad
{
    public class Edge
    {
        /// <summary>
        /// Edge id.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Start node id.
        /// </summary>
        public RoadNetworkNode From { get; set; }

        /// <summary>
        /// End node id.
        /// </summary>
        public RoadNetworkNode To { get; set; }

        /// <summary>
        /// The travel time between the From to To nodes.
        /// </summary>
        public Dictionary<int, Tuple<int, int>> Cost { get; set; }

        /// <summary>
        /// The edge name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The edge type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The edge shape.
        /// </summary>
        public List<Coordinates> Shape { get; set; }
        
        public Coordinates MAX { get; set; }
        
        public Coordinates MIN { get; set; }

        /// <summary>
        /// The edge length in kilometers.
        /// </summary>
        public double Length { get; set; }

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

        public Edge(int EdgeID, RoadNetworkNode from, RoadNetworkNode to, Dictionary<int, Tuple<int, int>> cost)
        {
            this.Id = EdgeID;
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
            return point.Latitude <= this.MAX.Latitude &&
                   point.Latitude >= this.MIN.Latitude &&
                   point.Longitude <= MAX.Longitude &&
                   point.Longitude >= MIN.Longitude;
        }
    }
}
