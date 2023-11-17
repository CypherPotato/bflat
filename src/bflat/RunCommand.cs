using Internal.TypeSystem;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 8509
#nullable enable

internal class RunCommand : CommandBase
{
    internal static string? RunOutputFilename = null;
    internal static bool? RunVerboseOption = null;

    public static Command Create()
    {
        var command = new Command("run", "Compiles and runs the specified C# source files")
        {
            CommonOptions.InputFilesArgument,
            CommonOptions.DefinedSymbolsOption,
            CommonOptions.ReferencesOption
        };
        command.Handler = new RunCommand();

        return command;
    }

    public override Int32 Handle(ParseResult result)
    {
        var c = new BuildCommand();
        string outputFile;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            outputFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".exe");
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            outputFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        else
            throw new NotImplementedException();

        RunOutputFilename = outputFile;
        RunVerboseOption = false;

        int n = c.Handle(result);

        ProcessStartInfo pstart = new ProcessStartInfo()
        {
            UseShellExecute = false,
            FileName = outputFile
        };

        Process? outputProcess = Process.Start(pstart);
        if (outputProcess == null)
            return 51;

        outputProcess.WaitForExit();

        string debugFile;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            debugFile = outputFile.Substring(0, outputFile.Length - 4) + ".pdb";
        else
            debugFile = outputFile + ".pdb";

        File.Delete(outputFile);
        File.Delete(debugFile);

        return outputProcess.ExitCode;
    }
}
