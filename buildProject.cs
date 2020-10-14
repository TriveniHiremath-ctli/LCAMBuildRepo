using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCAMBuild
{
    public class buildProject
    {
        public string category;
        public string projectName;
        public string zipPrefix;
        public string zipPostfix;
        public List<string> fileTypes;
        public List<string> folderExcludes;
        public string sourceDirectory;
        public string targetDirectory;
        public string zipProjectName;
    }
}
