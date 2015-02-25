using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRoad.Test
{
    internal class Resources
    {
        public const string Forests = @"
simple forest
0: 2, 3
1: 4, 5, 6
conflict forest
0: 2-2, 3-2
1: 2-1, 3-4, 4
simple forest 2
7: 9, 10
8: 11, 12, 13
medium forest
0: 4, 5
1: 6
2: 7, 8, 9, 10
3: 11, 12, 13
medium forest 2
0: 4, 5
1: 6
2: 7, 8, 9, 10
3: 11, 12, 13
6: 14, 15
13: 16";
    }
}
