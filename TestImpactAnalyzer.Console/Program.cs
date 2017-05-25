namespace TestImpactAnalyzer.Console
{
    class Program
    {
        static void Main()
        {
            var finder = new ReferenceFinder();
            var solution = "C:\\Users\\Dmitry\\Documents\\Visual Studio 2015\\Projects\\TestImpactAnalyzer\\TestImpactAnalyzer.sln";
            finder.Find(solution, "GetInt");
        }
    }
}
