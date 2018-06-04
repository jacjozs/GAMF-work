using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FuzzyManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static void MoveWindowInsideBounds(Window window)
        {
            if (window.Height > SystemParameters.WorkArea.Height)
                window.Height = SystemParameters.WorkArea.Height;
            if (window.Width > SystemParameters.WorkArea.Width)
                window.Width = SystemParameters.WorkArea.Width;

            if (window.Top < 0)
                window.Top = 0;
            if (window.Left < 0)
                window.Left = 0;

            if (window.Top > SystemParameters.WorkArea.Height - window.Height)
                window.Top = SystemParameters.WorkArea.Height - window.Height;
            if (window.Left > SystemParameters.WorkArea.Width - window.Width)
                window.Left = SystemParameters.WorkArea.Width - window.Width;
        }
    }
}
