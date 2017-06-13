﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace CShell.Framework.Services
{
	public class InputBindingTrigger : TriggerBase<FrameworkElement>, ICommand
	{
		public static readonly DependencyProperty InputBindingProperty =
			DependencyProperty.Register("InputBinding", typeof(InputBinding), 
			typeof(InputBindingTrigger), new UIPropertyMetadata(null));

		public InputBinding InputBinding
		{
			get { return (InputBinding)GetValue(InputBindingProperty); }
			set { SetValue(InputBindingProperty, value); }
		}

		protected override void OnAttached()
		{
			if (InputBinding != null)
			{
				InputBinding.Command = this;
				AssociatedObject.InputBindings.Add(InputBinding);
			}
			base.OnAttached();
		}

		#region ICommand Members
		public bool CanExecute(object parameter) => true;
	    public event EventHandler CanExecuteChanged = delegate { };

		public void Execute(object parameter) => InvokeActions(parameter);

	    #endregion
	}
}