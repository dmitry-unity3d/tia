using System;
using System.Linq;

namespace TestImpactAnalyzer.Lib.Utils
{
    public enum FormattingType
    {
        Json = 0,
        Text
    }

    public class CommandLineParameters
    {
        private const string ParameterPrefix = "--";
        private const string RunTestsParameter = "run-tests";
        private const string SolutionPathParameter = "solution-path";
        private const string RevisionParameter = "revision";
        private const string WorkingFolderParameter = "working-folder";
        private const string OutputFormattingParameter = "formatting";

        public string Revision { get; private set; }

        public string SolutionPath { get; private set; }

        public bool RunTests { get;  private set; }

        public string WorkingFolder { get; private set; }

        public FormattingType OutputFormatting { get; private set; } = FormattingType.Json;

        public static CommandLineParameters Parse(string[] args)
        {
            var parsedParameters = new CommandLineParameters();
            parsedParameters.RunTests = args.Contains(GetFormattedParameter(RunTestsParameter));
            parsedParameters.SolutionPath = GetParameterValue(args, SolutionPathParameter);
            parsedParameters.Revision = GetParameterValue(args, RevisionParameter);
            parsedParameters.WorkingFolder = GetParameterValue(args, WorkingFolderParameter);
            var formatting = GetParameterValue(args, OutputFormattingParameter);
            if (!string.IsNullOrEmpty(formatting))
            {
                formatting = formatting[0].ToString().ToUpper() + formatting.Substring(1);
                parsedParameters.OutputFormatting = (FormattingType)Enum.Parse(typeof(FormattingType), formatting);    
            }
            return parsedParameters;
        }

        public static bool TryParse(string[] args, out CommandLineParameters parameters)
        {
            var parsedSuccessfully = true;
            parameters = null;
            try
            {
                parameters = Parse(args);
            }
            catch
            {
                parsedSuccessfully = false;
            }
            return parsedSuccessfully;
        }

        private static string GetParameterValue(string[] args, string parameterName)
        {
            var indexParameter = Array.IndexOf(args, GetFormattedParameter(parameterName));
            if (indexParameter == -1)
            {
                return string.Empty;
            }
            if (indexParameter == args.Length - 1)
            {
                throw new ArgumentException($"Parameter {parameterName} is not passed properly", parameterName);
            }
            var parameterValue = args.ElementAtOrDefault(indexParameter + 1);
            if (IsParameterName(parameterValue))
            {
                throw new ArgumentException($"Parameter {parameterName} is not passed properly", parameterName);
            }
            return parameterValue;
        }

        private static string GetFormattedParameter(string parameterName)
        {
            return $"{ParameterPrefix}{parameterName}";
        }

        private static bool IsParameterName(string parameterToken)
        {
            return parameterToken.StartsWith(ParameterPrefix);
        }
    }
}
