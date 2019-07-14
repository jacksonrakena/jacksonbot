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
#pragma warning disable IDE0067 // Dispose objects before losing scope
            var newProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    FileName = "dotnet",
                    Arguments = "run --project ../../../Abyss.Console.csproj -c Release",
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                }
            };
#pragma warning restore IDE0067 // Dispose objects before losing scope
            newProcess.Start();
            Environment.Exit(0);
            return Task.CompletedTask;
        }
    }
}
