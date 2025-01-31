﻿using SpiceSharp.Circuits;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    public abstract class ComponentGenerator : IComponentGenerator
    {
        public abstract SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context);

        protected void SetParameters(ICircuitContext context, Entity entity, ParameterCollection parameters, bool onload)
        {
            foreach (Parameter parameter in parameters)
            {
                if (parameter is AssignmentParameter ap)
                {
                    context.SetParameter(entity, ap.Name, ap.Value, true, onload);
                }
                else
                {
                    context.Result.AddWarning("Unsupported parameter: " + parameter.Image);
                }
            }
        }
    }
}