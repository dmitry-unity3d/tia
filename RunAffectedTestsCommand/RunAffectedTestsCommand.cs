//------------------------------------------------------------------------------
// <copyright file="RunAffectedTestsCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using TestImpactAnalyzer.Lib;

namespace RunAffectedTestsCommand
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class RunAffectedTestsCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("ea6d6019-4259-4b5d-b1b9-020971d6149f");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="RunAffectedTestsCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private RunAffectedTestsCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(FindAndRunAffectedTests, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        private static void FindAndRunAffectedTests(object sender, EventArgs e)  
        {  
            var workingFolder = "C:\\Projects\\unity3d\\unity";

            var runTestCommandProcess = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "C:\\Projects\\unity3d\\tia\\TestImpactAnalyzer.Console\\bin\\Debug\\TestImpactAnalyzer.Console.exe",
                    Arguments = workingFolder,
                    UseShellExecute = false
                }
            };
            runTestCommandProcess.Start();
        }  

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static RunAffectedTestsCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new RunAffectedTestsCommand(package);
        }
    }
}
