using TestImpactAnalyzer.Lib;

namespace TestImpactAnalyzer.Console
{
    class Program
    {
        static void Main()
        {
            var finder = new ReferenceFinder();
            var solution = "C:\\Projects\\unity3d\\tia\\TestImpactAnalyzer.sln";
            finder.Find(solution, "GetInt");
        }
    }
}
