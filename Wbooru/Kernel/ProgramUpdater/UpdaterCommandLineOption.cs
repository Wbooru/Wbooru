using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Kernel.ProgramUpdater
{
    public class UpdaterCommandLineOption
    {
        [Option(nameof(UpdateTargetPath))]
        public string UpdateTargetPath { get; set; }
    }
}
