using System.IO;
using System.Web.Http;
using Newtonsoft.Json;
using TestImpactAnalyzer.Lib;

namespace TestImpactAnalyzer.Web.Controllers
{
    public class AffectedTestsController : ApiController
    {
        // GET api/affected-tests/5
        public string Get(string id)
        {
            var workingFolder = "C:\\Projects\\unity3d\\unity";
            var solutionPath = $"{workingFolder}\\Projects\\CSharp\\Unity.CSharpProjects.gen.sln";
            var mercurialClient = new MercurialClient(workingFolder);
            var changes = mercurialClient.GetChangedFiles(id);
            var classPath = changes[0];
            var className = Path.GetFileNameWithoutExtension(classPath);
            var finder = new ReferenceFinder(solutionPath);
            var locations = finder.FindAffectedTests(classPath, className);
            //var locations = new[] { @"Tests\Unity.PureCSharpTests\FileMirroringTests" };
            return JsonConvert.SerializeObject(locations);
        }
    }
}
