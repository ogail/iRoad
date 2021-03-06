﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRoad
{
    public class Region
    {
        /// <summary>
        /// The center location of the region.
        /// </summary>
        public Coordinates Center { get; set; }

        /// <summary>
        /// The radius of the circle in kilometers.
        /// </summary>
        public double Radius { get; set; }

        public Region(Coordinates center, double radius)
        {
            Center = center;
            Radius = radius;
        }
    }
}
