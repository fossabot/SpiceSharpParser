﻿using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Plots;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Prints;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Configurations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface IResultService
    {
        SimulationConfiguration SimulationConfiguration { get; }

        MonteCarloResult MonteCarlo { get; }

        IEnumerable<Simulation> Simulations { get; }

        IEnumerable<Export> Exports { get; }

        Circuit Circuit { get; }

        int? Seed { get; set; }

        void AddWarning(string warning);

        void AddComment(CommentLine statement);

        void AddExport(Export export);

        void AddPlot(XyPlot plot);

        void AddEntity(Entity entity);

        void AddSimulation(BaseSimulation simulation);

        bool FindObject(string objectId, out Entity entity);

        void AddPrint(Print print);
    }
}