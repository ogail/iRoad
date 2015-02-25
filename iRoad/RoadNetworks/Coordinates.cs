using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryProcessing.DataStructures
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
