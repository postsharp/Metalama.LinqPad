﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Build.Publishers;
using PostSharp.Engineering.BuildTools.Build.Solutions;
using PostSharp.Engineering.BuildTools.Dependencies.Model;
using Spectre.Console.Cli;
using System.Linq;

var temporaryDependencyDefinition = new DependencyDefinition( "Metalama.LinqPad", VcsProvider.GitHub, "Metalama" );

var product = new Product( temporaryDependencyDefinition )
{
    Solutions = new Solution[]
    {
        new DotNetSolution( "Metalama.LinqPad.sln" )
    },
    PublicArtifacts = Pattern.Create( "Metalama.LinqPad.$(PackageVersion).nupkg" ),
    Dependencies = new[] { Dependencies.PostSharpEngineering, Dependencies.Metalama },
    MainVersionDependency = Dependencies.Metalama,
    Configurations = Product.DefaultConfigurations
        .WithValue(
        BuildConfiguration.Public, Product.DefaultConfigurations.Public with { 
            PublicPublishers = Product.DefaultPublicPublishers.Add( new MergePublisher() ).ToArray() } )
};

var commandApp = new CommandApp();

commandApp.AddProductCommands( product );

return commandApp.Run( args );