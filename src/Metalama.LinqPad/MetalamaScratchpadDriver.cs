// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using LINQPad;
using LINQPad.Extensibility.DataContext;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Utilities;
using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Introspection;
using Metalama.Framework.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.LinqPad
{
    /// <summary>
    /// A LinqPad driver that does not include a workspace in the context.
    /// </summary>
    [UsedImplicitly]
    public class MetalamaScratchpadDriver : DynamicDataContextDriver
    {
        protected static ILogger? Logger { get; private set; }

        internal static void Initialize()
        {
            // We don't start initialization in the static constructor because it causes LinqPad to generate to timeout exception
            // when the debugger UI is active.
            if ( Logger == null )
            {
                DriverInitialization.Initialize();
                Logger = BackstageServiceFactory.ServiceProvider.GetLoggerFactory().GetLogger( "LinqPad" );
            }
        }

        public override bool AreRepositoriesEquivalent( IConnectionInfo c1, IConnectionInfo c2 ) => true;

        public override void OnQueryFinishing( IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager )
            => DiagnosticReporter.ClearCounters();

        public override string Name => "Metalama Scratchpad";

        public override string Author => "PostSharp Technologies";

        private readonly FacadeObjectFactory _facadeObjectFactory = new();

        public override string GetConnectionDescription( IConnectionInfo cxInfo ) => this.Name;

        public override bool ShowConnectionDialog( IConnectionInfo cxInfo, ConnectionDialogOptions dialogOptions )
        {
            Initialize();

            return true;
        }

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(
            IConnectionInfo cxInfo,
            AssemblyName assemblyToBuild,
            ref string nameSpace,
            ref string typeName )
        {
            Initialize();

            try
            {
                var source = $@"
using System;
using System.Collections.Generic;
using Metalama.LinqPad;
using Metalama.Framework.Workspaces;

namespace {nameSpace}
{{
    // The main typed data class. The user's queries subclass this, so they have easy access to all its members.
	public class {typeName} : {nameof(MetalamaScratchpadDataContext)}
	{{
	           
	}}	
}}";

#pragma warning disable SYSLIB0044
                Compile( source, assemblyToBuild.CodeBase!, cxInfo );
#pragma warning restore SYSLIB0044

                var schemaFactory = new SchemaFactory( FormatTypeName );
                var projectSchema = schemaFactory.GetSchema( "Workspace.Load(\"MySolution.sln\")" );

                return projectSchema;
            }
            catch ( Exception e )
            {
                Logger?.Error?.Log( e.ToString() );

                throw;
            }
        }

        public override IEnumerable<string> GetNamespacesToAdd( IConnectionInfo cxInfo )
            => new[]
            {
                "Metalama.Framework.Workspaces",
                "Metalama.Framework.Code",
                "Metalama.Framework.Code.Collections",
                "Metalama.Framework.Introspection",
                "Metalama.Framework.Diagnostics",
                "Metalama.LinqPad"
            };

        private static IReadOnlyList<string> GetAssembliesToAdd( bool addReferenceAssemblies, IConnectionInfo connectionInfo )
        {
            List<string> assembliesToReference = [];

            if ( addReferenceAssemblies )
            {
                assembliesToReference.AddRange( GetCoreFxReferenceAssemblies( connectionInfo ) );
            }

            // Metalama.LinqPad
            assembliesToReference.Add( typeof(MetalamaWorkspaceDriver).Assembly.Location );

            // Metalama.Framework
            assembliesToReference.Add( typeof(IDeclaration).Assembly.Location );

            // Metalama.Framework.Workspaces
            assembliesToReference.Add( typeof(Workspace).Assembly.Location );

            // Metalama.Framework.Inspection
            assembliesToReference.Add( typeof(IIntrospectionAspectInstance).Assembly.Location );

            // Metalama.Framework.Engine
            assembliesToReference.Add( typeof(AspectPipeline).Assembly.Location );

            return assembliesToReference;
        }

        public override IEnumerable<string> GetAssembliesToAdd( IConnectionInfo cxInfo ) => GetAssembliesToAdd( false, cxInfo );

        protected static void Compile( string cSharpSourceCode, string outputFile, IConnectionInfo connectionInfo )
        {
            var assembliesToReference = GetAssembliesToAdd( true, connectionInfo );

            // CompileSource is a static helper method to compile C# source code using LINQPad's built-in Roslyn libraries.
            // If you prefer, you can add a NuGet reference to the Roslyn libraries and use them directly.
            var compileResult = CompileSource(
                new CompilationInput { FilePathsToReference = assembliesToReference.ToArray(), OutputPath = outputFile, SourceCode = [cSharpSourceCode] } );

            if ( compileResult.Errors.Length > 0 )
            {
                throw new AssertionFailedException( "Cannot compile typed context: " + compileResult.Errors[0] );
            }
        }

        public override ICustomMemberProvider? GetCustomDisplayMemberProvider( object objectToWrite ) => this._facadeObjectFactory.GetFacade( objectToWrite );

        public override void InitializeContext( IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager )
        {
            Util.HtmlHead.AddStyles( "a.error { color: red !important; } span.null, .empty { color: #888 !important; }" );

            base.InitializeContext( cxInfo, context, executionManager );
        }

        public override void OverrideDriverDependencies( DriverDependencyInfo dependencyInfo )
        {
            base.OverrideDriverDependencies( dependencyInfo );

            var packageName = "Microsoft.CodeAnalysis.Workspaces.MSBuild";
            var packageVersion = AssemblyMetadataReader.GetInstance( this.GetType().Assembly ).GetPackageVersion( packageName );
            
            dependencyInfo.AddNuGetPackages ([(packageName, packageVersion)]);
        }

        public override bool AlwaysCopyLocal( IConnectionInfo cxInfo ) => true;
    }
}