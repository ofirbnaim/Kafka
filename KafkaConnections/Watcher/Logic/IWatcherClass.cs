using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watcher.Logic
{
    public interface IWatcherClass
    {
        public void Watch(FileSystemWatcher watcher, string scanPath);
    }
}
