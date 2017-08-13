using CShell.Framework.Services;
using ScriptCs;
using ScriptCs.Contracts;

namespace CShell.Hosting
{
    public class ReplScriptExecutorFactory : IReplScriptExecutorFactory
    {
        private readonly IReplOutput _replOutput;
        private readonly IDefaultReferences _defaultReferences;
        private readonly ScriptServices _scriptServices;
        private readonly IScriptInfo _scriptInfo;

        public ReplScriptExecutorFactory(ScriptServices scriptServices, IReplOutput replOutput, IDefaultReferences defaultReferences, IScriptInfo scriptInfo)
        {
            _scriptServices = scriptServices;
            _replOutput = replOutput;
            _defaultReferences = defaultReferences;
            _scriptInfo = scriptInfo;
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
                _defaultReferences, 
                _scriptInfo
                );

            var assemblies = _scriptServices.AssemblyResolver.GetAssemblyPaths(_scriptServices.FileSystem.CurrentDirectory);
            var scriptPacks = _scriptServices.ScriptPackResolver.GetPacks();

            replExecutor.Initialize(assemblies, scriptPacks);
            _replOutput.Initialize(replExecutor);

            return replExecutor;
        }
    }
}
