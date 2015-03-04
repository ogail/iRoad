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

        public static Coordinates operator - (Coordinates right, Coordinates left)
        {
            return new Coordinates(
                left.Latitude - right.Latitude,
                left.Longitude - right.Longitude);
        }

        public String myToString()
        {
            return Latitude + "\t" + Longitude;
        }

        public String myToFileString()
        {
            return Latitude + "," + Longitude;
        }
    }
}
