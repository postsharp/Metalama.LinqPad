// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Workspaces;
using System;

namespace Metalama.LinqPad
{
    /// <summary>
    /// The base class for all queries created with <see cref="MetalamaDriver"/>.
    /// </summary>
    [PublicAPI]
    public class MetalamaDataContext
    {
        static MetalamaDataContext()
        {
            DriverInitialization.Initialize();
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBePrivate.Global
#pragma warning disable SA1401, IDE1006
        protected readonly Workspace workspace;
#pragma warning restore SA1401, IDE1006

        public MetalamaDataContext( string path, bool ignoreWorkspaceErrors )
        {
            WorkspaceCollection.Default.IgnoreLoadErrors = ignoreWorkspaceErrors;
            this.workspace = WorkspaceCollection.Default.Load( path );

            foreach ( var diagnostic in this.workspace.WorkspaceDiagnostics )
            {
                Console.WriteLine( diagnostic.FormatAsBuildDiagnostic() );
            }
            
            DiagnosticReporter.ClearCounters();
        }
    }
}