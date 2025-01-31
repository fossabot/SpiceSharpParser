﻿using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .CONNECT <see cref="Control"/> from Spice netlist object model.
    /// </summary>
    public class ConnectControl : BaseControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, ICircuitContext context)
        {
            if (statement.Parameters.Count != 2)
            {
                throw new WrongParametersCountException("Wrong parameter count for .CONNECT", statement.LineInfo);
            }

            string nodeA = statement.Parameters.Get(0).Image;
            string nodeB = statement.Parameters.Get(1).Image;

            var vs = new VoltageSource($"Voltage connector: {nodeA} <-> {nodeB}");
            context.CreateNodes(vs, statement.Parameters);
            context.Result.AddEntity(vs);
        }
    }
}