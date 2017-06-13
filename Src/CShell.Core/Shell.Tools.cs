﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CShell.Framework;
using CShell.Framework.Services;
using Caliburn.Micro;

namespace CShell
{
    public static partial class Shell
    {
        /// <summary>
        /// Gets a specific tool based on the URI. 
        /// If the tool URI exists or can be created the tool is opened.
        /// </summary>
        /// <param name="uri">The tool URI.</param>
        public static ITool GetTool(Uri uri) => GetTool(uri, false);

        /// <summary>
        /// Gets a specific tool based on the uri.
        /// </summary>
        /// <param name="uri">The tool URI.</param>
        /// <param name="suppressOpen">If set to <c>true</c> tool will not be opened, but just created.</param>
        /// <returns></returns>
        public static ITool GetTool(Uri uri, bool suppressOpen)
        {
            var tools = Ui.Tools.ToArray();
            var tool = tools.FirstOrDefault(t => t.Uri == uri);
            if (tool == null)
            {
                tool = IoC.GetAllInstances(typeof(ITool))
                    .Cast<ITool>()
                    .FirstOrDefault(t=>t.Uri == uri);

                if (tool != null && !suppressOpen)
                    Ui.ShowTool(tool);
            }
            return tool;
        }
    }
}
