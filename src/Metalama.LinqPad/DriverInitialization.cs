// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;
using Metalama.Backstage.Extensibility;
using Metalama.Framework.Engine.Utilities.Diagnostics;

namespace Metalama.LinqPad;

internal static class DriverInitialization
{
    public static void Initialize()
    {
        if ( !BackstageServiceFactoryInitializer.IsInitialized )
        {
            // Don't enforce licensing in workspaces.

            BackstageServiceFactoryInitializer.Initialize( new BackstageInitializationOptions( new LinqPadApplicationInfo() ) { AddSupportServices = true } );
        }
    }

    private class LinqPadApplicationInfo : ApplicationInfoBase
    {
        public LinqPadApplicationInfo() : base( typeof(LinqPadApplicationInfo).Assembly ) { }

        public override string Name => "Metalama.LinqPad";
    }
}