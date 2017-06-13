using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CShell.Util
{
    /// <summary>
    /// Class with static members to access the content of the global assembly
    /// cache.
    /// </summary>
    public static class GlobalAssemblyCache
    {
        static readonly string CachedGacPathV2 = Fusion.GetGacPath(false);
        static readonly string CachedGacPathV4 = Fusion.GetGacPath(true);

        public static string GacRootPathV2 => CachedGacPathV2;

        public static string GacRootPathV4 => CachedGacPathV4;

        //public static bool IsWithinGac(string assemblyLocation)
        //{
        //    return Core.FileUtility.IsBaseDirectory(GacRootPathV2, assemblyLocation)
        //        || Core.FileUtility.IsBaseDirectory(GacRootPathV4, assemblyLocation);
        //}

        public static List<AssemblyName> GetAssemblyList()
        {
            IApplicationContext applicationContext = null;
            IAssemblyEnum assemblyEnum = null;
            IAssemblyName assemblyName = null;

            List<AssemblyName> l = new List<AssemblyName>();
            Fusion.CreateAssemblyEnum(out assemblyEnum, null, null, 2, 0);
            while (assemblyEnum.GetNextAssembly(out applicationContext, out assemblyName, 0) == 0)
            {
                uint nChars = 0;
                assemblyName.GetDisplayName(null, ref nChars, 0);

                StringBuilder sb = new StringBuilder((int)nChars);
                assemblyName.GetDisplayName(sb, ref nChars, 0);

                l.Add(new AssemblyName(sb.ToString()));
            }
            return l;
        }

        /// <summary>
        /// Gets the full display name of the GAC assembly of the specified short name
        /// </summary>
        public static AssemblyName FindBestMatchingAssemblyName(string name) => FindBestMatchingAssemblyName(new AssemblyName(name));

        public static AssemblyName FindBestMatchingAssemblyName(AssemblyName name)
        {
            string[] info;
            Version requiredVersion = name.Version;
            string publicKey = PublicKeyTokenToString(name);

            IApplicationContext applicationContext = null;
            IAssemblyEnum assemblyEnum = null;
            IAssemblyName assemblyName;
            Fusion.CreateAssemblyNameObject(out assemblyName, name.Name, 0, 0);
            Fusion.CreateAssemblyEnum(out assemblyEnum, null, assemblyName, 2, 0);
            List<string> names = new List<string>();

            while (assemblyEnum.GetNextAssembly(out applicationContext, out assemblyName, 0) == 0)
            {
                uint nChars = 0;
                assemblyName.GetDisplayName(null, ref nChars, 0);

                StringBuilder sb = new StringBuilder((int)nChars);
                assemblyName.GetDisplayName(sb, ref nChars, 0);

                string fullName = sb.ToString();
                if (!string.IsNullOrEmpty(publicKey))
                {
                    info = fullName.Split(',');
                    if (publicKey != info[3].Substring(info[3].LastIndexOf('=') + 1))
                    {
                        // Assembly has wrong public key
                        continue;
                    }
                }
                names.Add(fullName);
            }
            if (names.Count == 0)
                return null;
            string best = null;
            Version bestVersion = null;
            Version currentVersion;
            if (requiredVersion != null)
            {
                // use assembly with lowest version higher or equal to required version
                for (int i = 0; i < names.Count; i++)
                {
                    info = names[i].Split(',');
                    currentVersion = new Version(info[1].Substring(info[1].LastIndexOf('=') + 1));
                    if (currentVersion.CompareTo(requiredVersion) < 0)
                        continue; // version not good enough
                    if (best == null || currentVersion.CompareTo(bestVersion) < 0)
                    {
                        bestVersion = currentVersion;
                        best = names[i];
                    }
                }
                if (best != null)
                    return new AssemblyName(best);
            }
            // use assembly with highest version
            best = names[0];
            info = names[0].Split(',');
            bestVersion = new Version(info[1].Substring(info[1].LastIndexOf('=') + 1));
            for (int i = 1; i < names.Count; i++)
            {
                info = names[i].Split(',');
                currentVersion = new Version(info[1].Substring(info[1].LastIndexOf('=') + 1));
                if (currentVersion.CompareTo(bestVersion) > 0)
                {
                    bestVersion = currentVersion;
                    best = names[i];
                }
            }
            return new AssemblyName(best);
        }

        #region FindAssemblyInGac
        // This region is based on code from Mono.Cecil:

        // Author:
        //   Jb Evain (jbevain@gmail.com)
        //
        // Copyright (c) 2008 - 2010 Jb Evain
        //
        // Permission is hereby granted, free of charge, to any person obtaining
        // a copy of this software and associated documentation files (the
        // "Software"), to deal in the Software without restriction, including
        // without limitation the rights to use, copy, modify, merge, publish,
        // distribute, sublicense, and/or sell copies of the Software, and to
        // permit persons to whom the Software is furnished to do so, subject to
        // the following conditions:
        //
        // The above copyright notice and this permission notice shall be
        // included in all copies or substantial portions of the Software.
        //
        // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
        // EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
        // MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
        // NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
        // LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
        // OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
        // WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
        //

        static readonly string[] GacPaths = { GacRootPathV2, GacRootPathV4 };
        static readonly string[] Gacs = { "GAC_MSIL", "GAC_32", "GAC" };
        static readonly string[] Prefixes = { string.Empty, "v4.0_" };

        /// <summary>
        /// Gets the file name for an assembly stored in the GAC.
        /// </summary>
        public static string FindAssemblyInNetGac(AssemblyName reference)
        {
            // without public key, it can't be in the GAC
            if (reference.GetPublicKeyToken() == null || reference.GetPublicKeyToken().Length == 0)
                return null;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < Gacs.Length; j++)
                {
                    var gac = Path.Combine(GacPaths[i], Gacs[j]);
                    var file = GetAssemblyFile(reference, Prefixes[i], gac);
                    if (File.Exists(file))
                        return file;
                }
            }

            return null;
        }

        static string GetAssemblyFile(AssemblyName reference, string prefix, string gac)
        {
            var gacFolder = new StringBuilder()
                .Append(prefix)
                .Append(reference.Version)
                .Append("__");

            gacFolder.Append(PublicKeyTokenToString(reference));

            return Path.Combine(
                Path.Combine(
                    Path.Combine(gac, reference.Name), gacFolder.ToString()),
                reference.Name + ".dll");
        }

        //example from here: http://msdn.microsoft.com/en-us/library/system.reflection.assemblyname.getpublickeytoken(v=vs.95).aspx
        private const byte Mask = 15;
        private const string Hex = "0123456789ABCDEF";

        public static string PublicKeyTokenToString(AssemblyName assemblyName)
        {
            var pkt = new System.Text.StringBuilder();
            if (assemblyName.GetPublicKeyToken() == null)
                return string.Empty;

            foreach (byte b in assemblyName.GetPublicKeyToken())
            {
                pkt.Append(Hex[b / 16 & Mask]);
                pkt.Append(Hex[b & Mask]);
            }
            return pkt.ToString();
        }
        #endregion
    }
}
