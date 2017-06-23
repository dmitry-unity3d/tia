using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Http;
using TestImpactAnalyzer.Web.Properties;

namespace TestImpactAnalyzer.Web.Controllers
{
    public class AffectedTestsController : ApiController
    {
        // GET api/affectedtests/75019e622dbc
        public AffectedTests Get(string id)
        {
            //var workingFolder = "C:\\Projects\\unity3d\\unity";
            //var solutionPath = $"{workingFolder}\\Projects\\CSharp\\Unity.CSharpProjects.gen.sln";
            //var mercurialClient = new MercurialClient(workingFolder);
            //var changes = mercurialClient.GetChangedFiles(id);
            //var classPath = changes[0];
            //var className = Path.GetFileNameWithoutExtension(classPath);
            //var finder = new ReferenceFinder(solutionPath);
            //var locations = finder.FindAffectedTests(classPath, className);
            var locations = GetAffectedTests(id);
            for (var i = 0; i < locations.Length; i++)
            {
                locations[i] = locations[i].Replace(@"\\", @"\");
            }
            var affectedTests = new AffectedTests { TestPaths = locations };
            return affectedTests;
        }

        private static string[] GetAffectedTests(string revision)
        {
            var runTestCommandProcess = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = Settings.Default.ImpactsAnalyzerToolPath,
                    Arguments = $"--revision {revision} --working-folder {Settings.Default.WorkingFolder} --solution-path {Settings.Default.SolutionPath}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            runTestCommandProcess.Start();
            var affectedTestsOutput = new List<string>();
            while (!runTestCommandProcess.StandardOutput.EndOfStream) {
                var line = runTestCommandProcess.StandardOutput.ReadLine();
                affectedTestsOutput.Add(line);
            }
            return affectedTestsOutput.ToArray();
        }
    }

    public class AffectedTests
    {
        public string[] TestPaths { get; set; }
    }
}
