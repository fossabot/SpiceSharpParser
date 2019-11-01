﻿using System;
using System.Collections.Generic;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.FileSystem;
using SpiceSharpParser.Common.Mathematics.Probability;
using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Processors;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Parsers.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// The SPICE netlist parser.
    /// </summary>
    public class SpiceParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceParser"/> class.
        /// </summary>
        public SpiceParser()
            : this(new SpiceParserSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceParser"/> class.
        /// </summary>
        /// <param name="settings">SPICE parser settings.</param>
        public SpiceParser(SpiceParserSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            SingleNetlistParser = new SingleSpiceNetlistParser(Settings.Parsing);

            TokenProviderPool = new SpiceTokenProviderPool();
            var includesPreprocessor = new IncludesPreprocessor(
                new FileReader(),
                TokenProviderPool,
                SingleNetlistParser,
                () => Settings.WorkingDirectory,
                Settings.Lexing);

            var libPreprocessor = new LibPreprocessor(
                new FileReader(),
                TokenProviderPool,
                SingleNetlistParser,
                includesPreprocessor,
                () => Settings.WorkingDirectory,
                Settings.Reading,
                Settings.Lexing);

            var appendModelPreprocessor = new AppendModelPreprocessor();
            var sweepsPreprocessor = new SweepsPreprocessor();
            var ifPostprocessor = new IfPreprocessor();

            Preprocessors.AddRange(new IProcessor[] { includesPreprocessor, libPreprocessor, appendModelPreprocessor, sweepsPreprocessor, ifPostprocessor });
        }

        public ISpiceTokenProviderPool TokenProviderPool { get; set; }

        /// <summary>
        /// Gets the parser parserSettings.
        /// </summary>
        public SpiceParserSettings Settings { get; }

        /// <summary>
        /// Gets the preprocessors.
        /// </summary>
        public List<IProcessor> Preprocessors { get; } = new List<IProcessor>();

        /// <summary>
        /// Gets the SPICE netlist parser.
        /// </summary>
        protected ISingleSpiceNetlistParser SingleNetlistParser { get; }

        /// <summary>
        /// Parses the netlist.
        /// </summary>
        /// <param name="spiceNetlist">Netlist to parse.</param>
        /// <returns>
        /// A parsing result.
        /// </returns>
        public SpiceParserResult ParseNetlist(string spiceNetlist)
        {
            if (spiceNetlist == null)
            {
                throw new ArgumentNullException(nameof(spiceNetlist));
            }

            if (Settings == null)
            {
                throw new InvalidOperationException(nameof(Settings));
            }

            // Get tokens
            var tokens = TokenProviderPool.GetSpiceTokenProvider(Settings.Lexing).GetTokens(spiceNetlist);

            SpiceNetlist originalNetlistModel = SingleNetlistParser.Parse(tokens);

            // Preprocessing
            SpiceNetlist preprocessedNetListModel = (SpiceNetlist)originalNetlistModel.Clone();
            Evaluator preprocessorEvaluator = CreatePreprocessorEvaluator();

            ExpressionContext preprocessorContext = new SpiceExpressionContext(
                string.Empty,
                Settings.Reading.EvaluatorMode,
                Settings.Reading.CaseSensitivity.IsParameterNameCaseSensitive,
                Settings.Reading.CaseSensitivity.IsFunctionNameCaseSensitive,
                Settings.Reading.CaseSensitivity.IsExpressionNameCaseSensitive,
                new Randomizer(Settings.Reading.CaseSensitivity.IsDistributionNameCaseSensitive));

            foreach (var preprocessor in Preprocessors)
            {
                if (preprocessor is IEvaluatorConsumer consumer)
                {
                    consumer.Evaluator = preprocessorEvaluator;
                    consumer.ExpressionContext = preprocessorContext;
                    consumer.CaseSettings = Settings.Reading?.CaseSensitivity;
                }

                preprocessedNetListModel.Statements = preprocessor.Process(preprocessedNetListModel.Statements);
            }

            // Reading model
            var reader = new SpiceNetlistReader(Settings.Reading);
            SpiceNetlistReaderResult readerResult = reader.Read(preprocessedNetListModel);

            return new SpiceParserResult()
            {
                OriginalInputModel = originalNetlistModel,
                PreprocessedInputModel = preprocessedNetListModel,
                SpiceSharpModel = readerResult,
            };
        }

        private Evaluator CreatePreprocessorEvaluator()
        {
            Evaluator preprocessorEvaluator = new Evaluator("Preprocessors evaluator");
            return preprocessorEvaluator;
        }
    }
}
