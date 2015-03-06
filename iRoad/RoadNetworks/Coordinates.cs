using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRoad
{
    public class Coordinates
    {
        public double Latitude { get; set; }
        
        public double Longitude { get; set; }
        
        public Coordinates(double lat, double lng)
        {
            this.Latitude = lat;
            this.Longitude = lng;
        }

        /// <summary>
        /// Returns Euclidean-Distance between two coordinates in plane.
        /// distance = sqrt( (p1.y - p2.y)^2 + (p1.x - p2.x)^2 )
        /// </summary>
        /// <param name="c1">First Coordinate </param>
        /// <param name="c2">Second Coordinate </param>
        /// <returns></returns>
        public static double EuclideanDistance(Coordinates c1, Coordinates c2)
        {
            return Math.Sqrt(Math.Pow(c1.Latitude - c2.Latitude, 2) + Math.Pow(c1.Longitude - c2.Longitude, 2));
        }
    }
}
