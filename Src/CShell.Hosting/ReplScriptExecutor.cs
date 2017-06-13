﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CShell.Completion;
using CShell.Framework.Services;
using ScriptCs;
using ScriptCs.Contracts;

namespace CShell.Hosting
{
    public class ReplScriptExecutor : ScriptExecutor, IReplScriptExecutor
    {
        private readonly IReplOutput _replOutput;
        private readonly IObjectSerializer _serializer;
        private readonly IDefaultReferences _defaultReferences;

        public ReplScriptExecutor(
            IReplOutput replOutput,
            IObjectSerializer serializer,
            IFileSystem fileSystem,
            IFilePreProcessor filePreProcessor,
            IScriptEngine scriptEngine,
            ILogProvider logProvider,
            IEnumerable<IReplCommand> replCommands,
            IDefaultReferences defaultReferences)
            : base(fileSystem, filePreProcessor, scriptEngine, logProvider)
        {
            this._replOutput = replOutput;
            this._serializer = serializer;
            this._defaultReferences = defaultReferences;
            Commands = replCommands != null ? replCommands
                .Where(x => x.GetType().Namespace.StartsWith("CShell")) //hack to only include CShell commands for now
                .Where(x => x.CommandName != null)
                .ToDictionary(x => x.CommandName, x => x)
                : new Dictionary<string, IReplCommand>();

            _replCompletion = new CSharpCompletion(true);
            _replCompletion.AddReferences(GetReferencesAsPaths());
            //since it's quite expensive to initialize the "System." references we clone the REPL code completion
            _documentCompletion = _replCompletion.Clone();

            AddDefaultReferencesAndNamespaces();
        }

        public string WorkspaceDirectory => base.FileSystem.CurrentDirectory;

        public event EventHandler<EventArgs> AssemblyReferencesChanged;
        protected virtual void OnAssemblyReferencesChanged() => AssemblyReferencesChanged?.Invoke(this, EventArgs.Empty);

        private readonly ICompletion _replCompletion;
        private readonly ICompletion _documentCompletion;

        public ICompletion ReplCompletion => _replCompletion;

        public ICompletion DocumentCompletion => _documentCompletion;


        public string Buffer { get; private set; }

        public Dictionary<string, IReplCommand> Commands { get; private set; }

        public override void Initialize(IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks, params string[] scriptArgs)
        {
            base.Initialize(paths, scriptPacks, scriptArgs);
            ExecuteReferencesScript();
            ExecuteConfigScript();
        }

        public override ScriptResult Execute(string script, params string[] scriptArgs)
        {
            ScriptResult result = null;
            try
            {
                _replOutput.EvaluateStarted(script, null);

                if (script.StartsWith(":"))
                {
                    var tokens = script.Split(' ');
                    if (tokens[0].Length > 1)
                    {
                        if (Commands.ContainsKey(tokens[0].Substring(1)))
                        {
                            var command = Commands[tokens[0].Substring(1)];
                            var argsToPass = new List<object>();
                            foreach (var argument in tokens.Skip(1))
                            {
                                var argumentResult = ScriptEngine.Execute(argument, scriptArgs, References, Namespaces, ScriptPackSession);

                                if (argumentResult == null)
                                {
                                    argsToPass.Add(argument);
                                    continue;
                                }

                                if (argumentResult.CompileExceptionInfo != null)
                                {
                                    throw new Exception(
                                        GetInvalidCommandArgumentMessage(argument),
                                        argumentResult.CompileExceptionInfo.SourceException);
                                }

                                if (argumentResult.ExecuteExceptionInfo != null)
                                {
                                    throw new Exception(
                                        GetInvalidCommandArgumentMessage(argument),
                                        argumentResult.ExecuteExceptionInfo.SourceException);
                                }

                                if (!argumentResult.IsCompleteSubmission)
                                {
                                    throw new Exception(GetInvalidCommandArgumentMessage(argument));
                                }

                                argsToPass.Add(argumentResult.ReturnValue);
                            }

                            var commandResult = command.Execute(this, argsToPass.ToArray());
                            if (commandResult is ScriptResult)
                                result = commandResult as ScriptResult;
                            else
                                result = new ScriptResult(commandResult);
                        }
                        else
                        {
                            throw new Exception("Command not found: " + tokens[0].Substring(1));
                        }
                    }
                }
                else
                {
                    var preProcessResult = FilePreProcessor.ProcessScript(script);

                    ImportNamespaces(preProcessResult.Namespaces.ToArray());

                    foreach (var reference in preProcessResult.References)
                    {
                        var referencePath = FileSystem.GetFullPath(Path.Combine(FileSystem.BinFolder, reference));
                        AddReferences(FileSystem.FileExists(referencePath) ? referencePath : reference);
                    }

                    InjectScriptLibraries(FileSystem.CurrentDirectory, preProcessResult, ScriptPackSession.State);

                    Buffer = (Buffer == null)
                        ? preProcessResult.Code
                        : Buffer + Environment.NewLine + preProcessResult.Code;

                    var namespaces = Namespaces.Union(preProcessResult.Namespaces).ToList();
                    var references = References.Union(preProcessResult.References);

                    if (preProcessResult.References != null && preProcessResult.References.Count > 0)
                    {
                        OnAssemblyReferencesChanged();
                    }

                    result = ScriptEngine.Execute(Buffer, scriptArgs, references, namespaces, ScriptPackSession);

                    if (result == null)
                    {
                        result = ScriptResult.Empty;
                    }
                    else
                    {
                        if (result.InvalidNamespaces.Any())
                        {
                            RemoveNamespaces(result.InvalidNamespaces.ToArray());
                        }

                        if (result.IsCompleteSubmission)
                        {
                            Buffer = null;
                        }
                    }
                }
            }
            catch (FileNotFoundException fileEx)
            {
                RemoveReferences(fileEx.FileName);
                result = new ScriptResult(compilationException:fileEx);
            }
            catch (Exception ex)
            {
                result = new ScriptResult(executionException:ex);
            }
            finally
            {
                _replOutput.EvaluateCompleted(result);
            }
            return result ?? ScriptResult.Empty;
        }


        private static string GetInvalidCommandArgumentMessage(string argument) => string.Format(CultureInfo.InvariantCulture, "Argument is not a valid expression: {0}", argument);

        private void AddDefaultReferencesAndNamespaces()
        {
            AddReferences(typeof(Shell).Assembly);
            ImportNamespaces(typeof(Shell).Namespace);
            AddReferences(this._defaultReferences.Assemblies.Distinct().ToArray());
            AddReferences(this._defaultReferences.AssemblyPaths.Distinct().ToArray());
            ImportNamespaces(this._defaultReferences.Namespaces.Distinct().ToArray());
        }

        public override void AddReferences(params Assembly[] references)
        {
            base.AddReferences(references);
            _replCompletion.AddReferences(references);
            _documentCompletion.AddReferences(references);
            OnAssemblyReferencesChanged();
        }

        public override void RemoveReferences(params Assembly[] references)
        {
            base.RemoveReferences(references);
            _replCompletion.RemoveReferences(references);
            _documentCompletion.RemoveReferences(references);
            OnAssemblyReferencesChanged();
        }

        public override void AddReferences(params string[] references)
        {
            base.AddReferences(references);
            _replCompletion.AddReferences(references);
            _documentCompletion.AddReferences(references);
            OnAssemblyReferencesChanged();
        }

        public override void RemoveReferences(params string[] references)
        {
            base.RemoveReferences(references);
            _replCompletion.RemoveReferences(references);
            _documentCompletion.RemoveReferences(references);
            OnAssemblyReferencesChanged();
        }

        public string[] GetReferencesAsPaths()
        {
            var paths = new List<string>();
            paths.AddRange(References.Paths);
            paths.AddRange(References.Assemblies.Select(a=>a.GetName().Name));
            return paths.ToArray();
        }

        public string[] GetNamespaces() => Namespaces.ToArray();

        public override void Reset()
        {
            base.Reset();
            AddDefaultReferencesAndNamespaces();
            _replOutput.Clear();
            ExecuteReferencesScript();
        }

        public string[] GetVariables()
        {
            var replEngine = ScriptEngine as IReplEngine;
            if (replEngine != null)
            {
                var varsArray = replEngine.GetLocalVariables(ScriptPackSession)
                    .Where(x => !x.StartsWith("submission", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                return varsArray;
            }
            return new string[0];
        }


        public void ExecuteConfigScript()
        {
            var configPath = Path.Combine(WorkspaceDirectory, CShell.Constants.ConfigFile);
            if (File.Exists(configPath))
            {
                var configScript = File.ReadAllText(configPath);
                Execute(configScript);
            }
        }

        public void ExecuteReferencesScript()
        {
            var refPath = Path.Combine(WorkspaceDirectory, CShell.Constants.ReferencesFile);
            if (File.Exists(refPath))
            {
                var configScript = File.ReadAllText(refPath);
                Execute(configScript);
            }

            var binPath = Path.Combine(WorkspaceDirectory, CShell.Constants.BinFolder);
            if (Directory.Exists(binPath))
            {
                var binFiles = Directory.EnumerateFiles(binPath, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                AddReferences(binFiles);
            }
        }
    }
}
