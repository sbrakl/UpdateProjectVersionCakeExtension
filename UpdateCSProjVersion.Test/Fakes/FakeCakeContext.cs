﻿using System;
using Cake.Core.IO;
using Cake.Core;
using Cake.Core.Tooling;
using Cake.Testing;

namespace UpdateCSProjVersion.Test.Fakes
{
    public class FakeCakeContext
    {
        readonly ICakeContext context;
        readonly FakeLog log;
        readonly DirectoryPath testsDir;

        public FakeCakeContext ()
        {
            testsDir = new DirectoryPath (
                System.IO.Path.GetFullPath (AppDomain.CurrentDomain.BaseDirectory));

            var fileSystem = new FileSystem ();
            log = new FakeLog();
            var runtime = new CakeRuntime();
            var platform = new FakePlatform(PlatformFamily.Windows);
            var environment = new CakeEnvironment (platform, runtime, log);
            var globber = new Globber (fileSystem, environment);
            
            var args = new FakeCakeArguments ();
            var processRunner = new ProcessRunner (environment, log);
            var registry = new WindowsRegistry ();

            var toolRepository = new ToolRepository(environment);
            var toolResolutionStrategy = new ToolResolutionStrategy(fileSystem, environment, globber, new FakeConfiguration());
            IToolLocator tools = new ToolLocator(environment, toolRepository, toolResolutionStrategy);
            context = new CakeContext (fileSystem, environment, globber, log, args, processRunner, registry, tools);
            context.Environment.WorkingDirectory = testsDir;
        }

        public DirectoryPath WorkingDirectory {
            get { return testsDir; }
        }

        public ICakeContext CakeContext {
            get { return context; }
        }

        public string GetLogs ()
        {
            return string.Join(Environment.NewLine, log.Messages);
        }

        public void DumpLogs ()
        {
            foreach (var m in log.Messages)
                Console.WriteLine (m);
        }
    }
}

