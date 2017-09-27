using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinesOfCodeCounter
{
    // https://github.com/gsscoder/commandline
    class Options
    {
        [Option('p', "path", Required = true, HelpText = "Path to root folder where the source code is located - e.g. C:\\temp\\source\\project")]
        public string Path { get; set; }
    }
}
