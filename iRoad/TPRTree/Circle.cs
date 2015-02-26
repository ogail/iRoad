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
        public Tuple<Coordinates, Coordinates> Intersect(Line v)
        {
            if (IsIntersecting(v))
            {
                //Calculate terms of the linear and quadratic equations
                var M = (v.End.Longitude - v.Start.Longitude) / (v.End.Latitude - v.Start.Latitude);
                var B = v.Start.Longitude - M * v.Start.Latitude;
                var a = 1 + M * M;
                var b = 2 * (M * B - M * Center.Longitude - Center.Latitude);
                var c = Center.Latitude * Center.Latitude + B * B + Center.Longitude * Center.Longitude - Radius * Radius - 2 * B * Center.Longitude;
                // solve quadratic equation
                var sqRtTerm = Math.Sqrt(b * b - 4 * a * c);
                var x1 = ((-b) + sqRtTerm) / (2 * a);
                var y1 = M * x1 + B;
                var x2 = ((-b) - sqRtTerm) / (2 * a);
                var y2 = M * x2 + B;
                // Intersection Calculated

                return Tuple.Create(new Coordinates(x1, y1), new Coordinates(x2, y2));
            }
            else
            {
                // Line segment does not intersect at one point.  It is either 
                // fully outside, fully inside, intersects at two points, is 
                // tangential to, or one or more points is exactly on the 
                // circle radius.
                return Tuple.Create(new Coordinates(double.NaN, double.NaN), new Coordinates(double.NaN, double.NaN));
            }
        }
    }
}
