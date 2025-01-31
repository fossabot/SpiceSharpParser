﻿using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .FUNC <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class FuncControl : BaseControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, ICircuitContext context)
        {
            if (statement.Parameters == null)
            {
                throw new System.ArgumentNullException(nameof(statement.Parameters));
            }

            for (var i = 0; i < statement.Parameters.Count; i++)
            {
                var param = statement.Parameters[i];

                if (param is AssignmentParameter assignmentParameter)
                {
                    if (!assignmentParameter.HasFunctionSyntax)
                    {
                        throw new SpiceSharpParserException("User function needs to be a function", assignmentParameter.LineInfo);
                    }

                    context.Evaluator.AddFunction(assignmentParameter.Name, assignmentParameter.Arguments, assignmentParameter.Value);
                }
                else
                {
                    if (param is BracketParameter bracketParameter)
                    {
                        var arguments = new List<string>();

                        if (bracketParameter.Parameters[0] is VectorParameter vp)
                        {
                            arguments.AddRange(vp.Elements.Select(element => element.Image));
                        }
                        else
                        {
                            if (bracketParameter.Parameters.Count != 0)
                            {
                                arguments.Add(bracketParameter.Parameters[0].Image);
                            }
                        }

                        context.Evaluator.AddFunction(
                            bracketParameter.Name,
                            arguments,
                            statement.Parameters[i + 1].Image);

                        i++;
                    }
                    else
                    {
                        throw new WrongParameterTypeException("Unsupported syntax for .FUNC", param.LineInfo);
                    }
                }
            }
        }
    }
}