﻿using SpiceNetlist.SpiceObjects;
using SpiceSharp.Simulations;
using System.Linq;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations
{
    /// <summary>
    /// Processes .OP <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class OPControl : SimulationControl
    {
        public override string TypeName => "op";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, ProcessingContext context)
        {
            var op = new OP((context.Simulations.Count() + 1).ToString() + " - OP");

            SetBaseParameters(op.BaseConfiguration, context);
            context.AddSimulation(op);
        }
    }
}
