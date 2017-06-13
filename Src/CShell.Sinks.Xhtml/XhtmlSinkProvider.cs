﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CShell.Framework.Services;

namespace CShell.Sinks.Xhtml
{
    [Export(typeof(ISinkProvider))]
    public class XhtmlSinkProvider : ISinkProvider
    {
        public bool Handles(Uri uri) => uri.Scheme == "sink" && uri.Host == "cshell";

        /// <summary>
        /// Creates a CShell sink.
        /// The CShell sink URI is arranged like this:
        ///   sink://cshell/SinkType/SinkName
        /// for example, following uri would create a XHTML window named "Hi"
        ///   sink://cshell/xhtml/Hi
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>If the URI was correct a sink, otherwise null.</returns
        public Framework.ISink Create(Uri uri)
        {
            var pathParts = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length > 0)
            {
                //the first part of the uri is the sink type
                var sinkType = pathParts[0].ToLower();

                if (sinkType == "xhtml")
                {
                    return new XhtmlSinkViewModel(uri);
                }

            }
            return null;
        }
    }
}
