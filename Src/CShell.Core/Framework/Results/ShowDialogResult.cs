using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace CShell.Framework.Results
{
    public class ShowDialogResult : ResultBase
    {
        private readonly Type _dialogViewModelType;
        private object _dialogViewModel;

        public IDictionary<string, object> Settings { get; set; } 

        public ShowDialogResult(object dialogViewModel)
        {
            this._dialogViewModel = dialogViewModel;
        }

        public ShowDialogResult(Type dialogViewModelType)
        {
            this._dialogViewModelType = dialogViewModelType;
        }

        public override void Execute(CoroutineExecutionContext context)
        {
            var windowManager = IoC.Get<IWindowManager>();
            if(_dialogViewModel == null)
            {
                _dialogViewModel = IoC.GetInstance(_dialogViewModelType, null);
            }
            var result = windowManager.ShowDialog(_dialogViewModel, settings:Settings);
            OnCompleted(null, result.HasValue && !result.Value);
        }
    }
}
