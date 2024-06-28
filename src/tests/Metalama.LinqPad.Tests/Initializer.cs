// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.Build.Locator;
using System.Runtime.CompilerServices;

namespace Metalama.LinqPad.Tests;

internal static class Initializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        if ( MSBuildLocator.CanRegister )
        {
            MSBuildLocator.RegisterDefaults();
        }
    }
}