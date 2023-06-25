// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Build.Solutions;
using PostSharp.Engineering.BuildTools.Dependencies.Definitions;
using Spectre.Console.Cli;
using MetalamaDependencies = PostSharp.Engineering.BuildTools.Dependencies.Definitions.MetalamaDependencies.V2023_2;

var product = new Product( MetalamaDependencies.MetalamaLinqPad )
{
    Solutions = new Solution[]
    {
        new DotNetSolution( "Metalama.LinqPad.sln" ) { CanFormatCode = true }
    },
    PublicArtifacts = Pattern.Create( "Metalama.LinqPad.$(PackageVersion).nupkg" ),
    Dependencies = new[] { DevelopmentDependencies.PostSharpEngineering, MetalamaDependencies.Metalama },
    MainVersionDependency = MetalamaDependencies.Metalama,
    
    // This is set temporarily to investigate hanging tests.
    // After removing, don't forget to run `b generate-scripts`. 
    BuildTimeOutThreshold = TimeSpan.FromMinutes( 25 )
};

var commandApp = new CommandApp();

commandApp.AddProductCommands( product );

return commandApp.Run( args );