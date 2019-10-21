using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FiveNetwork
{
    public static class rangeAeaCalculator
    {
        public static double CalcrangeAea(Point circleA, double rangeA, Point circleB, double rangeB)
        {
            double D = Math.Sqrt(((circleB.X - circleA.X) * (circleB.X - circleA.X)) + ((circleB.Y - circleA.Y) * (circleB.Y - circleA.Y)));
            double radians = Math.Atan((Math.Sqrt(rangeA + rangeB + D) * Math.Sqrt(rangeA + rangeB - D) * Math.Sqrt(rangeA - rangeB + D) * Math.Sqrt(-rangeA + rangeB + D)) / (-(rangeA * rangeA) + (rangeB * rangeB) - (D * D)));
            if (double.IsNaN(radians)) return 0;
            double alpha = Math.Abs(radians * 180 / Math.PI);

            //Kör sugár vektor
            Point P = new Point((((D - rangeA) * circleA.X) + ((D - (D - rangeA)) * circleB.X)) / ((D - rangeA) + (D - (D - rangeA))), (((D - rangeA) * circleA.Y) + ((D - (D - rangeA)) * circleB.Y)) / ((D - rangeA) + (D - (D - rangeA))));

            //Kör sugár vektor
            double ACVx = P.X - circleA.X;
            double ACVy = P.Y - circleA.Y;

            // Metszés Pont A (felső)
            double angleCos = Math.Cos(radians);
            double angleSin = Math.Sin(radians);

            Point cut1 = new Point(((angleCos * ACVx) - (angleSin * ACVy)) + circleA.X, ((angleSin * ACVx) + (angleCos * ACVy)) + circleA.Y);

            // Metszés Pont B (alsó)
            angleCos = Math.Cos(-radians);
            angleSin = Math.Sin(-radians);

            Point cut2 = new Point(((angleCos * ACVx) - (angleSin * ACVy)) + circleA.X, ((angleSin * ACVx) + (angleCos * ACVy)) + circleA.Y);

            //Két metszés pont közti távolság
            double M = Math.Sqrt(((cut1.X - cut2.X) * (cut1.X - cut2.X)) + ((cut1.Y - cut2.Y) * (cut1.Y - cut2.Y)));

            double beta = Math.Asin(((M / 2) / rangeB));//RAD

            double radAlpha = (alpha * 2) * Math.PI / 180;

            double TA = (((2 * alpha) / 360) * (rangeA * rangeA) * Math.PI) - (((rangeA * rangeA) * Math.Sin(radAlpha)) / 2);

            double TB = (((2 * (beta * 180 / Math.PI)) / 360) * (rangeB * rangeB) * Math.PI) - (((rangeB * rangeB) * Math.Sin(2 * beta)) / 2);
            return TA + TB;
        }
    }
}
