﻿using System.Collections.Generic;
using System.Text;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Plots
{
    /// <summary>
    /// Data for plot with data series
    /// </summary>
    public class Plot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Plot"/> class.
        /// </summary>
        /// <param name="name">The name of plot</param>
        public Plot(string name)
        {
            Name = name;
            Series = new List<Series>();
        }

        /// <summary>
        /// Gets the name of the plot
        /// </summary>
        public string Name { get;  }

        /// <summary>
        /// Gets the series in the plot
        /// </summary>
        public List<Series> Series { get; }


        public string ExportToCSV(int seriesIndex = 0)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var point in Series[seriesIndex].Points)
            {
                builder.AppendLine(point.X + ";" + point.Y + ";");
            }

            return builder.ToString();
        }
    }
}
