using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using CShell.Framework.Menus;
using CShell.Framework.Services;

namespace CShell.Framework
{
	public abstract class ModuleBase : IModule
	{
		[Import]
		private IShell _shell;

		protected IShell Shell => _shell;

	    protected IMenu MainMenu => _shell.MainMenu;

	    protected IToolBar ToolBar => _shell.ToolBar;

	    public int Order { get; set; }

	    public virtual void Configure()
	    {}

        public abstract void Start();

        public void Dispose() => Dispose(true);

	    protected virtual void Dispose(bool disposing)
        {}

	}
}