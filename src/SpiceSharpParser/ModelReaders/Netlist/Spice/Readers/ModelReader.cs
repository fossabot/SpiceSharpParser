﻿using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Reads all supported <see cref="Model"/> from SPICE netlist object model.
    /// </summary>
    public class ModelReader : StatementReader<Model>, IModelReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelReader"/> class.
        /// </summary>
        /// <param name="mapper">The model mapper.</param>
        /// <param name="modelsGenerator">The models generator.</param>
        public ModelReader(IMapper<IModelGenerator> mapper, IModelsGenerator modelsGenerator)
        {
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            ModelsGenerator = modelsGenerator ?? throw new ArgumentNullException(nameof(modelsGenerator));
        }

        /// <summary>
        /// Gets the model mapper.
        /// </summary>
        public IMapper<IModelGenerator> Mapper { get; }

        /// <summary>
        /// Gets the models generator.
        /// </summary>
        public IModelsGenerator ModelsGenerator { get; }

        /// <summary>
        /// Reads a model statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Model statement, ICircuitContext context)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string name = statement.Name;

            if (statement.Parameters.Count > 0)
            {
                if (statement.Parameters[0] is BracketParameter b)
                {
                    var type = b.Name;

                    if (!Mapper.TryGetValue(type, context.CaseSensitivity.IsEntityNameCaseSensitive, out var generator))
                    {
                        throw new ReadingException($"Unsupported model type: {type}", b.LineInfo);
                    }

                    var model = ModelsGenerator.GenerateModel(
                        generator,
                        context.NameGenerator.GenerateObjectName(name),
                        name,
                        type,
                        b.Parameters,
                        context);

                    if (model != null)
                    {
                        context.Result.AddEntity(model);
                    }
                }

                if (statement.Parameters[0] is SingleParameter single)
                {
                    var type = single.Image;

                    if (!Mapper.TryGetValue(type, context.CaseSensitivity.IsModelTypeCaseSensitive, out var generator))
                    {
                        throw new ReadingException($"Unsupported model type: {type}", single.LineInfo);
                    }

                    var model = ModelsGenerator.GenerateModel(
                        generator,
                        context.NameGenerator.GenerateObjectName(name),
                        name,
                        type,
                        statement.Parameters.Skip(1),
                        context);

                    if (model != null)
                    {
                        context.Result.AddEntity(model);
                    }
                }
            }
        }
    }
}