using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRoad
{
    public class Circle
    {
        public Coordinates Center { get; set; }

        public double Radius { get; set; }

        private bool IsInsideCircle(Coordinates location)
        {
            return Math.Sqrt(Math.Pow((Center.Latitude - location.Latitude), 2) +
                   Math.Pow((Center.Longitude - location.Longitude), 2)) < Radius;
        }

        private bool IsIntersecting(Line line)
        {
            return IsInsideCircle(line.Start) || IsInsideCircle(line.End);
        }

        /// <summary>
        /// Calculates the intersection Coordinates between a given line and the current circle.
        /// Used algorithm is documented here: http://mathworld.wolfram.com/Circle-LineIntersection.html
        /// </summary>
        /// <param name="v">The line to intersect with</param>
        /// <returns>The intersection points</returns>
        public Coordinates Intersect(Line v)
        {
            if (IsIntersecting(v))
            {
                //Calculate terms of the linear and quadratic equations
                double M = (v.End.Longitude - v.Start.Longitude) / (v.End.Latitude - v.Start.Latitude);
                double B = v.Start.Longitude - M * v.Start.Latitude;
                double a = 1 + M * M;
                double b = 2 * (M * B - M * Center.Longitude - Center.Latitude);
                double c = Center.Latitude * Center.Latitude + B * B + Center.Longitude * Center.Longitude - Radius * Radius - 2 * B * Center.Longitude;
                // solve quadratic equation
                var sqRtTerm = Math.Sqrt(b * b - 4 * a * c);
                var x = ((-b) + sqRtTerm) / (2 * a);
                // make sure we have the correct root for our line segment
                if ((x < Math.Min(v.Start.Latitude, v.End.Latitude) || (x > Math.Max(v.Start.Latitude, v.End.Latitude))))
                {
                    x = ((-b) - sqRtTerm) / (2 * a);
                }
                //solve for the y-component
                var y = M * x + B;
                // Intersection Calculated

                return new Coordinates(x, y);
            }
            else
            {
                // Line segment does not intersect at one point.  It is either 
                // fully outside, fully inside, intersects at two points, is 
                // tangential to, or one or more points is exactly on the 
                // circle radius.
                return new Coordinates(double.NaN, double.NaN);
            }
        }
    }
}
