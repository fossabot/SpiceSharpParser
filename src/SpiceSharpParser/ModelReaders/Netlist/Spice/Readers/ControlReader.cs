﻿using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Reads all supported <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class ControlReader : StatementReader<Control>, IControlReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControlReader"/> class.
        /// </summary>
        /// <param name="mapper">The base control mapper.</param>
        public ControlReader(IMapper<BaseControl> mapper)
        {
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Gets the base control mapper.
        /// </summary>
        public IMapper<BaseControl> Mapper { get; }

        /// <summary>
        /// Reads a control statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, ICircuitContext context)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string type = statement.Name;

            if (!Mapper.TryGetValue(type, context.CaseSensitivity.IsDotStatementNameCaseSensitive, out var controlReader))
            {
                context.Result.AddWarning("Unsupported control: " + statement.Name + " at " + statement.LineInfo.LineNumber + " line");
            }
            else
            {
                controlReader.Read(statement, context);
            }
        }
    }
}