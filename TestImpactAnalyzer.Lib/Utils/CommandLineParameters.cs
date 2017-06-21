using System;
using System.Linq;

namespace TestImpactAnalyzer.Lib.Utils
{
    public class CommandLineParameters
    {
        public string SolutionPath { get; set; }

        public bool RunTests { get; set; }

        public static CommandLineParameters Parse(string[] args)
        {
            var parsedParameters = new CommandLineParameters();
            parsedParameters.RunTests = args.Contains("--run-tests");
            return parsedParameters;
        }
    }
}
