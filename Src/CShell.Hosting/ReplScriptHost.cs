using CShell.Framework.Services;
using ScriptCs;
using ScriptCs.Contracts;

namespace CShell.Hosting
{
    public class ReplScriptHost : ScriptHost
    {
        public ReplScriptHost(IScriptPackManager scriptPackManager,  string[] scriptArgs)
            : base(scriptPackManager, new ScriptEnvironment(scriptArgs, null, null))
        {
        }
    }

    public class ReplScriptHostFactory : IScriptHostFactory
    {
        public IScriptHost CreateScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs) 
            => new ReplScriptHost(scriptPackManager, scriptArgs);
    }
}
