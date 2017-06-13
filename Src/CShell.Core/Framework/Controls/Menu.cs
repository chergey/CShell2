﻿using System.Windows;

namespace CShell.Framework.Controls
{
	public class Menu : System.Windows.Controls.Menu
	{
		private object _currentItem;

		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			_currentItem = item;
			return base.IsItemItsOwnContainerOverride(item);
		}

		protected override DependencyObject GetContainerForItemOverride() => MenuItem.GetContainer(this, _currentItem);
	}
}