﻿using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Plots
{
    /// <summary>
    /// Data series
    /// </summary>
    public class Series
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Series"/> class.
        /// </summary>
        public Series()
        {
            Points = new List<Point>();
        }

        /// <summary>
        /// Gets the series points
        /// </summary>
        public List<Point> Points { get; }
    }
}
