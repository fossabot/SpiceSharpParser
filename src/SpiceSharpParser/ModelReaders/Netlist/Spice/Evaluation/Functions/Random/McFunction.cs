﻿using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random
{
    public class McFunction : Function<double, double>
    {
        public McFunction()
        {
            Name = "mc";
            ArgumentsCount = 2;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null)
        {
            if (args.Length != 2)
            {
                throw new Exception("mc expects two arguments");
            }

            var random = context.Randomizer.GetRandomDoubleProvider(context.Seed);

            double x = args[0];
            double tol = args[1];

            double min = x - (tol * x);
            double randomChange = random.NextDouble() * 2.0 * tol * x;

            return min + randomChange;
        }
    }
}
