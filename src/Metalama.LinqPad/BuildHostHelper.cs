// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.IO;

namespace Metalama.LinqPad;

internal static class BuildHostHelper
{
    public static void EnsureBuildHostCopied()
    {
        var libDirectory = Path.GetDirectoryName( typeof(MSBuildProjectLoader).Assembly.Location );
        var sourceBuildHostDirectory = Path.Combine( libDirectory, "..\\..\\contentFiles\\any\\any\\BuildHost-netcore" );
        var targetBuildHostDirectory = Path.Combine( libDirectory, "BuildHost-netcore" );
        var touchFile = Path.Combine( targetBuildHostDirectory, ".completed" );

        if ( !File.Exists( touchFile ) )
        {
            CopyFilesRecursively( sourceBuildHostDirectory, targetBuildHostDirectory );
            File.WriteAllText( touchFile, "Completed" );
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
            File.Copy( newPath, newPath.Replace( sourcePath, targetPath, StringComparison.OrdinalIgnoreCase ), true );
        }
    }
}