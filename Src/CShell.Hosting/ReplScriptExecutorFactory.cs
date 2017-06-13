using CShell.Framework.Services;
using ScriptCs;

namespace CShell.Hosting
{
    public class ReplScriptExecutorFactory : IReplScriptExecutorFactory
    {
        private readonly IReplOutput _replOutput;
        private readonly IDefaultReferences _defaultReferences;
        private readonly ScriptServices _scriptServices;

        public ReplScriptExecutorFactory(ScriptServices scriptServices, IReplOutput replOutput, IDefaultReferences defaultReferences)
        {
            this._scriptServices = scriptServices;
            this._replOutput = replOutput;
            this._defaultReferences = defaultReferences;
        }

        public IReplScriptExecutor Create(string workspaceDirectory)
        {
            _scriptServices.FileSystem.CurrentDirectory = workspaceDirectory;
            _scriptServices.InstallationProvider.Initialize();

            var replExecutor = new ReplScriptExecutor(
                _replOutput, 
                _scriptServices.ObjectSerializer, 
                _scriptServices.FileSystem, 
                _scriptServices.FilePreProcessor,
                _scriptServices.Engine,
                _scriptServices.LogProvider,
                _scriptServices.ReplCommands,
                _defaultReferences
                );

            var assemblies = _scriptServices.AssemblyResolver.GetAssemblyPaths(_scriptServices.FileSystem.CurrentDirectory);
            var scriptPacks = _scriptServices.ScriptPackResolver.GetPacks();

            replExecutor.Initialize(assemblies, scriptPacks);
            _replOutput.Initialize(replExecutor);

            return replExecutor;
        }
    }
}
