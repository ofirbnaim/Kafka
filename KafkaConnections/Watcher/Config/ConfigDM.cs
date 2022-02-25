using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watcher.Config
{
    public class ConfigDM
    {
        public WatcherSection WatcherConfig { get; set; }
        public class WatcherSection
        {
            public string ScanPath { get; set; }
        }
    }
}
