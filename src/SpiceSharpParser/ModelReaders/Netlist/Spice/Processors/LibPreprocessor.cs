﻿using SpiceSharpParser.Common.FileSystem;
using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Parsers.Netlist.Spice;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Processors
{
    /// <summary>
    /// Preprocess .lib statements with 2 parameters from netlist file.
    /// </summary>
    public class LibPreprocessor : IProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LibPreprocessor"/> class.
        /// </summary>
        /// <param name="fileReader">File reader.</param>
        /// <param name="tokenProviderPool">Token provider.</param>
        /// <param name="spiceNetlistParser">Single spice netlist parser.</param>
        /// <param name="includesPreprocessor">Includes preprocessor.</param>
        /// <param name="initialDirectoryPathProvider">Initial directory path provider.</param>
        /// <param name="readerSettings">Reader settings.</param>
        /// <param name="lexerSettings">Lexer settings.</param>
        public LibPreprocessor(
            IFileReader fileReader,
            ISpiceTokenProviderPool tokenProviderPool,
            ISingleSpiceNetlistParser spiceNetlistParser,
            IProcessor includesPreprocessor,
            Func<string> initialDirectoryPathProvider,
            SpiceNetlistReaderSettings readerSettings,
            SpiceLexerSettings lexerSettings)
        {
            ReaderSettings = readerSettings ?? throw new ArgumentNullException(nameof(readerSettings));
            TokenProviderPool = tokenProviderPool ?? throw new ArgumentNullException(nameof(tokenProviderPool));
            IncludesPreprocessor = includesPreprocessor ?? throw new ArgumentNullException(nameof(includesPreprocessor));
            SpiceNetlistParser = spiceNetlistParser ?? throw new ArgumentNullException(nameof(spiceNetlistParser));
            FileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
            InitialDirectoryPathProvider = initialDirectoryPathProvider ?? throw new ArgumentNullException(nameof(initialDirectoryPathProvider));
            LexerSettings = lexerSettings ?? throw new ArgumentNullException(nameof(lexerSettings));
        }

        public SpiceLexerSettings LexerSettings { get; }

        public ISpiceTokenProviderPool TokenProviderPool { get; }

        public SpiceNetlistReaderSettings ReaderSettings { get; }

        /// <summary>
        /// Gets the initial directory path.
        /// </summary>
        public string InitialDirectoryPath => InitialDirectoryPathProvider() ?? Directory.GetCurrentDirectory();

        /// <summary>
        /// Gets the file reader.
        /// </summary>
        public IFileReader FileReader { get; }

        /// <summary>
        /// Gets the SPICE netlist parser.
        /// </summary>
        public ISingleSpiceNetlistParser SpiceNetlistParser { get; }

        /// <summary>
        /// Gets the include preprocessor.
        /// </summary>
        public IProcessor IncludesPreprocessor { get; }

        protected Func<string> InitialDirectoryPathProvider { get; }

        /// <summary>
        /// Reads .include statements.
        /// </summary>
        /// <param name="statements">Netlist model to search for .include statements</param>
        public Statements Process(Statements statements)
        {
            if (statements == null)
            {
                throw new ArgumentNullException(nameof(statements));
            }

            return Process(statements, InitialDirectoryPath);
        }

        private static void ReadSingleLibWithOneArgument(Statements statements, Control lib, List<Statement> allStatements)
        {
            var libStatements = allStatements;
            statements.Replace(lib, libStatements);
        }

        private static void ReadSingleLibWithTwoArguments(Statements statements, Control lib, List<Statement> allStatements)
        {
            // Find lib by entry
            var libEntry = allStatements.SingleOrDefault(s => s is Control c && c.Name == "lib" && c.Parameters.Get(0).Image == lib.Parameters.Get(1).Image);
            if (libEntry != null)
            {
                // look for .endl
                int position = allStatements.IndexOf(libEntry);
                int libPosition = position;

                for (; !(allStatements[position] is Control c && c.Name.ToLower() == "endl") && position < allStatements.Count; position++)
                {
                    // iterate to find position
                }

                if (position == allStatements.Count)
                {
                    throw new ReadingException("No .ENDL found");
                }
                else
                {
                    var libStatements = allStatements.Skip(libPosition + 1).Take(position - libPosition);
                    statements.Replace(lib, libStatements);
                }
            }
        }

        /// <summary>
        /// Reads .include statements.
        /// </summary>
        /// <param name="statements">Netlist model to search for .include statements</param>
        /// <param name="currentDirectoryPath">Current directory path.</param>
        private Statements Process(Statements statements, string currentDirectoryPath)
        {
            bool libFound = ReadLibs(statements, currentDirectoryPath);

            while (libFound)
            {
                IncludesPreprocessor.Process(statements);
                libFound = ReadLibs(statements, currentDirectoryPath);
            }

            return statements;
        }

        private bool ReadLibs(Statements statements, string currentDirectoryPath)
        {
            bool result = false;
            var subCircuits = statements.Where(statement => statement is SubCircuit).Cast<SubCircuit>().ToList();
            if (subCircuits.Any())
            {
                foreach (SubCircuit subCircuit in subCircuits)
                {
                    var subCircuitLibs = subCircuit.Statements.Where(statement => statement is Control c && (c.Name.ToLower() == "lib")).Cast<Control>().ToList();

                    foreach (Control include in subCircuitLibs)
                    {
                        result = true;
                        ReadSingleLib(subCircuit.Statements, currentDirectoryPath, include);
                    }
                }
            }

            var libs = statements.Where(statement => statement is Control c && (c.Name.ToLower() == "lib")).Cast<Control>().ToList();

            if (libs.Any())
            {
                result = true;
                foreach (Control include in libs)
                {
                    ReadSingleLib(statements, currentDirectoryPath, include);
                }
            }

            return result;
        }

        private void ReadSingleLib(Statements statements, string currentDirectoryPath, Control lib)
        {
            // get full path of .lib
            string libPath = PathConverter.Convert(lib.Parameters.Get(0).Image);
            bool isAbsolutePath = Path.IsPathRooted(libPath);
            string libFullPath = isAbsolutePath ? libPath : Path.Combine(currentDirectoryPath, libPath);

            // check if file exists
            if (!File.Exists(libFullPath))
            {
                throw new InvalidOperationException($"Netlist include at {libFullPath}  is not found");
            }

            // get lib content
            string libContent = FileReader.ReadAll(libFullPath);
            if (libContent != null)
            {
                var lexerSettings = new SpiceLexerSettings()
                {
                    HasTitle = false,
                    IsDotStatementNameCaseSensitive = LexerSettings.IsDotStatementNameCaseSensitive,
                };

                var tokens = TokenProviderPool.GetSpiceTokenProvider(lexerSettings).GetTokens(libContent);

                foreach (var token in tokens)
                {
                    token.FileName = libFullPath;
                }

                SpiceNetlistParser.Settings = new SingleSpiceNetlistParserSettings(lexerSettings)
                {
                    IsNewlineRequired = false,
                    IsEndRequired = false,
                };

                SpiceNetlist includeModel = SpiceNetlistParser.Parse(tokens);

                var allStatements = includeModel.Statements.ToList();

                if (lib.Parameters.Count == 2)
                {
                    ReadSingleLibWithTwoArguments(statements, lib, allStatements);
                }
                else if (lib.Parameters.Count == 1)
                {
                    ReadSingleLibWithOneArgument(statements, lib, allStatements);
                }
            }
            else
            {
                throw new InvalidOperationException($"Netlist include at {libFullPath} could not be loaded");
            }
        }
    }
}