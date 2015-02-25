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

        /// <summary>
        /// Calculates the intersection point between a given line and the current circle.
        /// Used algorithm is documented here: http://mathworld.wolfram.com/Circle-LineIntersection.html
        /// </summary>
        /// <param name="v">The line to intersect with</param>
        /// <returns>The intersection points</returns>
        public Tuple<Coordinates, Coordinates> Intersect(Line v)
        {
            double dx = v.End.Latitude - v.Start.Latitude;
            double dy = v.End.Longitude - v.Start.Longitude;
            double dr = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            double D = (v.Start.Latitude * v.End.Longitude) - (v.End.Latitude * v.Start.Longitude);
            double discriminant = (Math.Pow(Radius, 2) * Math.Pow(dr, 2)) - Math.Pow(D, 2);
            Debug.Assert(discriminant > 0);
         
            Func<double, double, double> solve = (double a, double b) =>
            {
                return ((a + b) * Math.Sqrt((Math.Pow(Radius, 2) * Math.Pow(dr, 2)) - Math.Pow(D, 2))) / Math.Pow(dr, 2);
            };
            
            double x1 = solve(D * dy, (dy < 0 ? -1 : 1) * dx);
            double x2 = solve(D * dy, - (dy < 0 ? -1 : 1) * dx);
            double y1 = solve(-D * dx, Math.Abs(dy));
            double y2 = solve(-D * dx, - Math.Abs(dy));

            return Tuple.Create(new Coordinates(x1, y1), new Coordinates(x2, y2));
        }
    }
}
