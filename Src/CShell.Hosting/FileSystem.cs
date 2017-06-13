using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CShell.Hosting
{
    public class FileSystem : ScriptCs.FileSystem
    {
        public override string BinFolder => Constants.BinFolder;

        public override string DllCacheFolder => Constants.DllCacheFolder;

        public override string PackagesFile => Constants.PackagesFile;

        public override string PackagesFolder => Constants.PackagesFolder;

        public override string NugetFile => Constants.NugetFile;
    }
}
