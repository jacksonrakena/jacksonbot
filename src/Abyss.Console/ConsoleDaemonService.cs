using Abyss.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Console
{
    public class ConsoleDaemonService : IDaemonService
    {
        public Task RestartApplicationAsync()
        {
            System.Console.WriteLine(Directory.GetCurrentDirectory());
            var newProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    FileName = "dotnet",
                    Arguments = "run --project ../../../Abyss.Console.csproj -c Release",
                    WorkingDirectory = Directory.GetCurrentDirectory()
                }
            };
            newProcess.Start();
            Environment.Exit(0);
            return Task.CompletedTask;
        }
    }
}
