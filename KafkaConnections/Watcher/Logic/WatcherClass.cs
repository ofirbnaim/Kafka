using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher.Config;

namespace Watcher.Logic
{
    public class WatcherClass:IWatcherClass
    {
        private readonly ILogger<WatcherClass> _logger;
        private readonly ConfigDM _configDM;

        public WatcherClass() { }

        public WatcherClass(ILogger<WatcherClass> logger, ConfigDM configDM)
        {
            _logger = logger;
            _configDM = configDM;
        }

        public void Watch(FileSystemWatcher watcher, string scanPath)
        {
            // Determine the Path to Watch
            //using var watcher = new FileSystemWatcher(scanPath);

            watcher = new FileSystemWatcher(scanPath)
            {
                // Determine the type of changes to notify
                NotifyFilter = NotifyFilters.Attributes
                               | NotifyFilters.FileName
                               | NotifyFilters.LastWrite
                               | NotifyFilters.LastAccess
                               | NotifyFilters.CreationTime
                               | NotifyFilters.Security,

                // Determine wich files to monitored in the directory
                Filter = "*.json",

                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
            };

            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;
            
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            
          //_logger.LogInformation($"Changed: {e.FullPath}");
          Console.WriteLine($"Changed: {e.FullPath}");
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
           //_logger.LogInformation($"Created: {e.FullPath}");

           string value = $"Created: {e.FullPath}";
           Console.WriteLine(value);
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            //_logger.LogInformation($"Deleted: {e.FullPath}");

            string value = $"Deleted: {e.FullPath}";
            Console.WriteLine(value);
        }

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"Renamed:");
            Console.WriteLine($"Old Name: {e.OldFullPath}");
            Console.WriteLine($"Old Name: {e.FullPath}");

            //_logger.LogInformation($"File renamed from: {e.OldFullPath} to {e.FullPath}");
        }

        private static void OnError(object sender, ErrorEventArgs e)
        {
            PrintException(e.GetException());
        }

        private static void PrintException(Exception ex)
        {
            if(ex != null)
            {
                //_logger.LogError($"{ex.Message},{ ex.StackTrace}, {ex.InnerException}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine("Stacktrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                PrintException(ex.InnerException);
            }
        }
    }
}
