﻿using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.CurrentExports
{
    /// <summary>
    /// Magnitude of a complex current export.
    /// </summary>
    public class CurrentDecibelExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentDecibelExport"/> class.
        /// </summary>
        /// <param name="name">Name of export.</param>
        /// <param name="simulation">A simulation</param>
        /// <param name="source">An identifier</param>
        public CurrentDecibelExport(string name, Simulation simulation, string source)
            : base(simulation)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            ExportImpl = new ComplexPropertyExport(simulation, source, "i");
        }

        /// <summary>
        /// Gets the main node.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets the quantity unit.
        /// </summary>
        public override string QuantityUnit => "Current (db A)";

        /// <summary>
        /// Gets the complex property export.
        /// </summary>
        protected ComplexPropertyExport ExportImpl { get; }

        /// <summary>
        /// Extracts current magnitude value.
        /// </summary>
        /// <returns>
        /// Current magnitude value.
        /// </returns>
        public override double Extract()
        {
            if (!ExportImpl.IsValid)
            {
                if (ExceptionsEnabled)
                {
                    throw new ReadingException($"Current decibel export '{Name}' is invalid");
                }

                return double.NaN;
            }

            // TODO: Verify with Sven....
            return 20.0 * Math.Log10(ExportImpl.Value.Magnitude);
        }
    }
}