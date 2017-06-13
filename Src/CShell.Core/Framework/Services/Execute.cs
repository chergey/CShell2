using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CShell.Framework.Services
{
    public static class Execute
    {
        public static void OnUiThread(this Action action) => Caliburn.Micro.Execute.OnUIThread(action);

        /// <summary>
        /// Executes an action and marshals any exceptions that occur to the calling thread.
        /// </summary>
        /// <param name="action">The action.</param>
        public static void OnUiThreadEx(this Action action)
        {
            Exception exception = null;
            Caliburn.Micro.Execute.OnUIThread(()=>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });
            if (exception != null)
                throw exception;
        }
    }
}
