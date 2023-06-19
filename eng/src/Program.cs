// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Build.Solutions;
using PostSharp.Engineering.BuildTools.Dependencies.Model;
using PostSharp.Engineering.BuildTools.Utilities;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

var product = new Product( Dependencies.MetalamaLinqPad )
{
    Solutions = new Solution[]
    {
        new DumpCapturingSolution( "Metalama.LinqPad.sln" ) { CanFormatCode = true }
    },
    PublicArtifacts = Pattern.Create( "Metalama.LinqPad.$(PackageVersion).nupkg" ),
    Dependencies = new[] { Dependencies.PostSharpEngineering, Dependencies.Metalama },
    MainVersionDependency = Dependencies.Metalama,

    // MergePublisher disabled for 2023.1.
    // Configurations = Product.DefaultConfigurations
    //     .WithValue(
    //     BuildConfiguration.Public, Product.DefaultConfigurations.Public with { 
    //         PublicPublishers = Product.DefaultPublicPublishers.Add( new MergePublisher() ).ToArray() } )
    Configurations = Product.DefaultConfigurations
        .WithValue(
            BuildConfiguration.Debug,
            Product.DefaultConfigurations.Debug with
            {
                AdditionalArtifactRules = new[]
                {
                    $@"+:%system.teamcity.build.tempDir%/Metalama/EngineeringDumps/**/*=>engdumps",
                }
            } )
};

var commandApp = new CommandApp();

commandApp.AddProductCommands( product );

return commandApp.Run( args );

// TODO: This is a prototype that should go to the main engineering later.
public class DumpCapturingSolution : DotNetSolution
{
    public DumpCapturingSolution( string solutionPath ) : base( solutionPath )
    {
    }

    public override bool Test( BuildContext context, BuildSettings settings )
    {
        var cancellationSource = new CancellationTokenSource();
        var monitoringTask = Task.Run( async () => await MonitorTimeout( context, cancellationSource.Token ) );

        try
        {
            return base.Test( context, settings );
        }
        finally
        {
            cancellationSource.Cancel();
        }
    }

    private static async Task MonitorTimeout( BuildContext context, CancellationToken token )
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        // Wait for 15 minutes (if the timeout is not met, this gets cancelled).
        await Task.Delay( 900_000, token );

        // Dump all child processes.

        var currentProcess = Process.GetCurrentProcess();

        var directory = $"{Path.GetTempPath()}\\Metalama\\EngineeringDumps";

        Directory.CreateDirectory( directory );

        var processesToCapture = new List<ManagementObject>();

        var mos = new ManagementObjectSearcher( $"Select * From Win32_Process Where ParentProcessID={Environment.ProcessId}" );

        foreach ( ManagementObject mo in mos.Get() )
        {
            ProcessChildProcesses( mo );
        }

        foreach( var processToCapture in processesToCapture)
        {
            var process = Process.GetProcessById( Convert.ToInt32( processToCapture["ProcessID"], CultureInfo.InvariantCulture ) );

            var dumpFileName = $"{directory}\\Dump_{process.Id}_{Path.GetFileName( processToCapture["ExecutablePath"].ToString() )}.dmp";

            context.Console.WriteImportantMessage( $"Capturing dump of '{processToCapture["ExecutablePath"]}:{process.Id}' into '{dumpFileName}'. Command line: {processToCapture["CommandLine"]}" );

            if ( !ToolInvocationHelper.InvokeTool(
                context.Console,
                "dotnet",
                $"dump collect -p {process.Id} -o \"{dumpFileName}\" --type Full",
                null ) )
            {
                context.Console.WriteImportantMessage( "Failed." );
            }
            else
            {
                context.Console.WriteImportantMessage( "Success." );
            }
        }

        void ProcessChildProcesses( ManagementObject processObject )
        {
            if ( FilterProcess( processObject ) )
            {
                context.Console.WriteImportantMessage( $"Will capture dump of '{processObject["ExecutablePath"]}:{processObject["ProcessId"]}'. Command line: {processObject["CommandLine"]}" );
                processesToCapture.Add( processObject );
            }
            else
            {
                context.Console.WriteImportantMessage( $"Skipping dump of '{processObject["ExecutablePath"]}:{processObject["ProcessId"]}'. Command line: {processObject["CommandLine"]}" );
            }

            var cmos = new ManagementObjectSearcher( $"Select * From Win32_Process Where ParentProcessID={processObject["ProcessId"]}" );

            foreach ( ManagementObject cmo in cmos.Get() )
            {
                ProcessChildProcesses( cmo );
            }
        }
    }

    private static bool FilterProcess( ManagementObject processObject )
    {
        if ( !RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
        {
            return false;
        }

        return processObject["ExecutablePath"].ToString().Contains( "dotnet", StringComparison.OrdinalIgnoreCase );
    }
}