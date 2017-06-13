﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CShell.Util
{
    public static class PathHelper
    {
        public static string ToAbsolutePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(toPath))
                return null;
            if (string.IsNullOrEmpty(fromPath))
                return toPath;

            Uri path2 = new Uri(toPath, UriKind.RelativeOrAbsolute);
            if (path2.IsAbsoluteUri)
                return toPath;
            Uri basePath = new Uri(fromPath + "/", UriKind.Absolute);
            Uri absPath = new Uri(basePath, toPath);
            return absPath.LocalPath;
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string ToRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(toPath))
                return null;
            if (string.IsNullOrEmpty(fromPath))
                return toPath;

            Uri uri1 = new Uri(toPath, UriKind.RelativeOrAbsolute);
            if (uri1.IsAbsoluteUri)
            {
                Uri uri2 = new Uri(fromPath + "/", UriKind.Absolute);
                Uri relativeUri = uri2.MakeRelativeUri(uri1);
                return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
            }
            // else it is already a relative path
            return toPath;
        }
    }
}
