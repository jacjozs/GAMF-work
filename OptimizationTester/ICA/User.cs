﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationTester.ICA
{
    public class User
    {
        /// <summary>
        /// Poziciós id
        /// </summary>
        public int pos { get; set; }
        /// <summary>
        /// Következő pont ahova indulhat
        /// </summary>
        public int posTo { get; set; }
        /// <summary>
        /// A saját és a következő pont közti távolság
        /// </summary>
        public double km { get; set; }
        /// <summary>
        /// Felhasználói igény mértéke
        /// </summary>
        public int dem { get; set; }

        public Point position { get; set; } 
    }
}
