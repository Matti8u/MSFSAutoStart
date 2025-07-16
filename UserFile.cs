using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSFSAutoStart
{
    public class UserFile
    {
        public string FilePath { get; set; }    // Full path to the file
        public string CmdLineArg { get; set; } = "";
        public bool AutoStartEnabled { get; set; }
        public bool AutoStopEnabled { get; set; }

        // Constructor
        public UserFile(string filePath, bool autoStartEnabled = true, bool autoStopEnabled = false)
        {
            FilePath = filePath;
            AutoStartEnabled = autoStartEnabled;
            AutoStopEnabled = autoStopEnabled;
        }
    }
}
