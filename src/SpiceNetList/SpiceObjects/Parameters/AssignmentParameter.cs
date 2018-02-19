﻿using System.Collections.Generic;

namespace SpiceNetlist.SpiceObjects.Parameters
{
    /// <summary>
    /// An assigment parameter
    /// </summary>
    public class AssignmentParameter : Parameter
    {
        /// <summary>
        /// Gets or sets the name of the assigment parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the arguments of the assigment parameters
        /// </summary>
        public List<string> Arguments { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the value of assigment parameter
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets the string represenation of the parameter
        /// </summary>
        public override string Image => Name + "(" + string.Join(",", Arguments) + ")=" + Value;
    }
}
