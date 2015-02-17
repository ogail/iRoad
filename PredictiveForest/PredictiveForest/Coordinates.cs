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

        public override bool Equals(object obj)
        {
            return (this.Latitude - ((Coordinates)obj).Latitude) < 1 &&
                   (this.Longitude - ((Coordinates)obj).Longitude) < 1;

            //return this.Latitude == ((Coordinates)obj).Latitude &&
            //       this.Longitude == ((Coordinates)obj).Longitude;
        }
    }
}
