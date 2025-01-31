using SpiceSharpParser.ModelReaders.Netlist.Spice;
using System.Collections.Generic;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Stochastic
{
    public class RandomTests : BaseTests
    {
        [Fact]
        public void Basic()
        {
            var result = ParseNetlist(
                "Random - Test circuit",
                "V1 0 1 100",
                "R1 1 0 {R+N}",
                ".OP",
                ".SAVE i(R1) @R1[resistance]",
                ".PARAM N=0",
                ".PARAM R={random() * 1000}",
                ".STEP PARAM N LIST 2 3",
                ".END");

            Assert.Equal(4, result.Exports.Count);
            Assert.Equal(2, result.Simulations.Count);

            RunSimulationsAndReturnExports(result);
        }

        [Fact]
        public void OptionsSeed()
        {
            SpiceNetlistReaderResult parseResult2223 = null;
            SpiceNetlistReaderResult parseResult2224 = null;

            var resultsSeed2223 = new List<double>();
            var resultsSeed2224 = new List<double>();

            int n = 2;

            for (var i = 0; i < n; i++)
            {
                parseResult2223 = ParseNetlist(
                    "Random - Seed test circuit",
                    "V1 0 1 100",
                    "R1 1 0 {R+N}",
                    ".OP",
                    ".SAVE @R1[resistance]",
                    ".PARAM N=0",
                    ".PARAM R={random() * 1000}",
                    ".STEP PARAM N LIST 2 3",
                    ".OPTIONS SEED = 2223",
                    ".END");

                var exports = RunSimulationsAndReturnExports(parseResult2223);
                resultsSeed2223.Add((double)exports[0]);
            }

            for (var i = 0; i < n; i++)
            {
                parseResult2224 = ParseNetlist(
                    "Random - Seed test circuit",
                    "V1 0 1 100",
                    "R1 1 0 {R+N}",
                    ".OP",
                    ".SAVE @R1[resistance]",
                    ".PARAM N=0",
                    ".PARAM R={random() * 1000}",
                    ".STEP PARAM N LIST 2 3",
                    ".OPTIONS SEED = 2224",
                    ".END");

                var exports = RunSimulationsAndReturnExports(parseResult2224);
                resultsSeed2224.Add((double)exports[0]);
            }

            for (var i = 0; i < n; i++)
            {
                Assert.Equal(resultsSeed2223[0], resultsSeed2223[i]);
            }

            for (var i = 0; i < n; i++)
            {
                Assert.Equal(resultsSeed2224[0], resultsSeed2224[i]);
            }

            Assert.NotEqual(resultsSeed2224[0], resultsSeed2223[0]);

            Assert.Equal(2223, parseResult2223?.Seed);
            Assert.Equal(2224, parseResult2224?.Seed);
        }

        [Fact]
        public void OptionsSeedOverridesParsingSeed()
        {
            SpiceNetlistReaderResult parseResult2223 = null;
            SpiceNetlistReaderResult parseResult2224 = null;

            var resultsSeed2223 = new List<double>();
            var resultsSeed2224 = new List<double>();

            int n = 5;

            for (var i = 0; i < n; i++)
            {
                parseResult2223 = ParseNetlist(
                    1111,
                    "Random - Seed test circuit",
                    "V1 0 1 100",
                    "R1 1 0 {R+N}",
                    ".OP",
                    ".SAVE i(R1) @R1[resistance]",
                    ".PARAM N=0",
                    ".PARAM R={random() * 1000}",
                    ".STEP PARAM N LIST 2 3",
                    ".OPTIONS SEED = 2223",
                    ".END");

                var exports = RunSimulationsAndReturnExports(parseResult2223);
                resultsSeed2223.Add((double)exports[0]);
            }

            for (var i = 0; i < n; i++)
            {
                parseResult2224 = ParseNetlist(
                    1111,
                    "Random - Seed test circuit",
                    "V1 0 1 100",
                    "R1 1 0 {R+N}",
                    ".OP",
                    ".SAVE i(R1) @R1[resistance]",
                    ".PARAM N=0",
                    ".PARAM R={random() * 1000}",
                    ".STEP PARAM N LIST 2 3",
                    ".OPTIONS SEED = 2224",
                    ".END");

                var exports = RunSimulationsAndReturnExports(parseResult2224);
                resultsSeed2224.Add((double)exports[0]);
            }

            for (var i = 0; i < n; i++)
            {
                Assert.Equal(resultsSeed2223[0], resultsSeed2223[i]);
            }

            for (var i = 0; i < n; i++)
            {
                Assert.Equal(resultsSeed2224[0], resultsSeed2224[i]);
            }

            Assert.NotEqual(resultsSeed2224[0], resultsSeed2223[0]);

            Assert.Equal(2223, parseResult2223?.Seed);
            Assert.Equal(2224, parseResult2224?.Seed);
        }

        [Fact]
        public void ParsingSeed()
        {
            for (var index = 0; index < 12; index++)
            {
                SpiceNetlistReaderResult parseResult2223 = null;
                SpiceNetlistReaderResult parseResult2224 = null;

                var resultsSeed2223 = new List<double>();
                var resultsSeed2224 = new List<double>();

                int n = 10;

                for (var i = 0; i < n; i++)
                {
                    parseResult2223 = ParseNetlist(
                        2223,
                        "Random - Seed test circuit",
                        "V1 0 1 100",
                        "R1 1 0 {R+N}",
                        ".OP",
                        ".SAVE i(R1) @R1[resistance]",
                        ".PARAM N=0",
                        ".PARAM R={random() * 1000}",
                        ".STEP PARAM N LIST 2 3",
                        ".END");

                    var exports = RunSimulationsAndReturnExports(parseResult2223);
                    resultsSeed2223.Add((double)exports[0]);
                }

                for (var i = 0; i < n; i++)
                {
                    parseResult2224 = ParseNetlist(
                        2224,
                        "Random - Seed test circuit",
                        "V1 0 1 100",
                        "R1 1 0 {R+N}",
                        ".OP",
                        ".SAVE i(R1) @R1[resistance]",
                        ".PARAM N=0",
                        ".PARAM R={random() * 1000}",
                        ".STEP PARAM N LIST 2 3",
                        ".END");

                    var exports = RunSimulationsAndReturnExports(parseResult2224);
                    resultsSeed2224.Add((double)exports[0]);
                }

                for (var i = 0; i < n; i++)
                {
                    Assert.Equal(resultsSeed2223[0], resultsSeed2223[i]);
                }

                for (var i = 0; i < n; i++)
                {
                    Assert.Equal(resultsSeed2224[0], resultsSeed2224[i]);
                }

                Assert.NotEqual(resultsSeed2224[0], resultsSeed2223[0]);

                Assert.Equal(2223, parseResult2223?.Seed);
                Assert.Equal(2224, parseResult2224?.Seed);
            }
        }
    }
}