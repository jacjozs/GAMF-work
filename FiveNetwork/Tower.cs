using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FiveNetwork
{
    public class Tower
    {
        public int radius { get; set; }
        public Point position { get; set; }
        public Tower(Point pos, int radius)
        {
            this.position = pos;
            this.radius = radius;
        }
    }
}
