﻿using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    /// <summary>
    /// Property export.
    /// </summary>
    public class PropertyExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyExport"/> class.
        /// </summary>
        /// <param name="name">Name of export.</param>
        /// <param name="simulation">A simulation.</param>
        /// <param name="source">A identifier of component.</param>
        /// <param name="property">Name of property for export.</param>
        /// <param name="comparer">Entity property name comparer.</param>
        public PropertyExport(string name, Simulation simulation, string source, string property, IEqualityComparer<string> comparer)
            : base(simulation)
        {
            Name = name ?? throw new NullReferenceException(nameof(name));
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            Source = source ?? throw new NullReferenceException(nameof(source));
            ExportRealImpl = new RealPropertyExport(simulation, source, property, comparer);
        }

        /// <summary>
        /// Gets the main node.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets the quantity unit.
        /// </summary>
        public override string QuantityUnit => string.Empty;

        /// <summary>
        /// Gets the real exporter.
        /// </summary>
        protected RealPropertyExport ExportRealImpl { get; }

        /// <summary>
        /// Extracts the current value
        /// </summary>
        /// <returns>
        /// A current value at the source
        /// </returns>
        public override double Extract()
        {
            if (!ExportRealImpl.IsValid)
            {
                if (ExceptionsEnabled)
                {
                    throw new ReadingException($"Property export {Name} is invalid");
                }

                return double.NaN;
            }

            return ExportRealImpl.Value;
        }
    }
}