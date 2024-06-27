// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.IO;

namespace Metalama.LinqPad;

internal static class BuildHostHelper
{
    private static readonly ILogger _logger = BackstageServiceFactory.ServiceProvider.GetLoggerFactory().GetLogger( nameof(BuildHostHelper) );
    
    public static void EnsureBuildHostCopied()
    {
        _logger.Trace?.Log( "EnsureBuildHostCopied" );
        
        var libDirectory = Path.GetDirectoryName( typeof(MSBuildProjectLoader).Assembly.Location );
        var sourceContentDirectory = Path.Combine( libDirectory, "..\\..\\contentFiles\\any\\any" );
        var touchFile = Path.Combine( libDirectory, ".contentFilesCopied" );

        if ( !File.Exists( touchFile ) )
        {
            _logger.Trace?.Log( $"'{touchFile}' does not exist." );
            CopyFilesRecursively( sourceContentDirectory, libDirectory );
            File.WriteAllText( touchFile, "Completed" );
        }
        else
        {
            _logger.Trace?.Log( $"'{touchFile}' exists." );
        }
    }

    private static void CopyFilesRecursively( string sourcePath, string targetPath )
    {
        //Now Create all of the directories
        foreach ( var dirPath in Directory.GetDirectories( sourcePath, "*", SearchOption.AllDirectories ) )
        {
            Directory.CreateDirectory( dirPath.Replace( sourcePath, targetPath, StringComparison.OrdinalIgnoreCase ) );
        }

        //Copy all the files & Replaces any files with the same name
        foreach ( var newPath in Directory.GetFiles( sourcePath, "*.*", SearchOption.AllDirectories ) )
        {
            var destFileName = newPath.Replace( sourcePath, targetPath, StringComparison.OrdinalIgnoreCase );
            
            _logger.Trace?.Log( $"Copying '{newPath}' -> '{destFileName}' " );

            File.Copy( newPath, destFileName, true );
        }
    }
}