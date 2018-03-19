﻿using SpiceNetlist.SpiceSharpConnector.Exceptions;
using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.VoltageExports
{
    /// <summary>
    /// Imaginary part of a complex voltage export.
    /// </summary>
    public class VoltageImaginaryExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageImaginaryExport"/> class.
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltageImaginaryExport(Simulation simulation, Identifier node, Identifier reference = null)
        {
            if (simulation == null)
            {
                throw new System.ArgumentNullException(nameof(simulation));
            }

            Node = node ?? throw new System.ArgumentNullException(nameof(node));
            Reference = reference;

            ExportImpl = new ComplexVoltageExport(simulation, node, reference);
        }

        /// <summary>
        /// Gets the type name
        /// </summary>
        public override string TypeName => "voltage";

        /// <summary>
        /// Gets the name
        /// </summary>
        public override string Name => "vi(" + Node + (Reference == null ? "" : ", " + Reference) + ")";

        /// <summary>
        /// Gets the quantity unit
        /// </summary>
        public override string QuantityUnit => "Voltage (V)";

        /// <summary>
        /// Gets the main node
        /// </summary>
        public Identifier Node { get; }

        /// <summary>
        /// Gets the reference node
        /// </summary>
        public Identifier Reference { get; }

        /// <summary>
        /// Gets the complex voltage export that provide voltage imaginary value
        /// </summary>
        protected ComplexVoltageExport ExportImpl { get; }

        /// <summary>
        /// Extracts a voltage imaginary at the main node
        /// </summary>
        /// <returns>
        /// A voltage imaginary at the main node
        /// </returns>
        public override double Extract()
        {
            if (!ExportImpl.IsValid)
            {
                throw new GeneralConnectorException($"Voltage imaginary export {Name} is invalid");
            }

            return ExportImpl.Value.Imaginary;
        }
    }
}
