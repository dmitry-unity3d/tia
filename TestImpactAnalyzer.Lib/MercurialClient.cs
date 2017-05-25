using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TestImpactAnalyzer.Lib
{
    public class MercurialClient
    {
        private readonly string _workingFolder;

        public MercurialClient(string workingFolder)
        {
            _workingFolder = workingFolder;
        }

        public string[] GetChangedFiles()
        {
            var changedFiles = new List<string>();
            var statusCommandProcess = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "hg",
                    Arguments = "status",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = _workingFolder
                }
            };
            statusCommandProcess.Start();
            while (!statusCommandProcess.StandardOutput.EndOfStream) {
                var line = statusCommandProcess.StandardOutput.ReadLine();
                var changedFilePath = line.Split(' ')[1];
                var changedFileFullPath = Path.Combine(_workingFolder, changedFilePath);
                if (Path.GetExtension(changedFileFullPath) != ".cs")
                {
                    continue;
                }
                changedFiles.Add(changedFileFullPath);
            }
            return changedFiles.ToArray();
        }
    }
}
