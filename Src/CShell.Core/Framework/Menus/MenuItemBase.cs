﻿using System.Collections;
using System.Collections.Generic;
using Caliburn.Micro;

namespace CShell.Framework.Menus
{
	public class MenuItemBase : PropertyChangedBase, IEnumerable<MenuItemBase>
	{
		#region Static stuff

		public static MenuItemBase Separator => new MenuItemSeparator();

	    #endregion

		#region Properties

		public IObservableCollection<MenuItemBase> Children { get; private set; }

		public virtual string Name => "-";

	    #endregion

		#region Constructors

		protected MenuItemBase()
		{
			Children = new BindableCollection<MenuItemBase>();
		}

		#endregion

		public void Add(params MenuItemBase[] menuItems) => menuItems.Apply(Children.Add);

	    public IEnumerator<MenuItemBase> GetEnumerator() => Children.GetEnumerator();

	    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}