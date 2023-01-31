using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopmotiveCatalog2023.DAL;
using TopmotiveCatalog2023.Models;

namespace TopmotiveCatalog2023.Controllers
{
    internal abstract class Controller
    {
        public Action<String>? Log { get; set; }

        public Controller()
        {
          initLog();
        }
        public Controller(Action<String>? log)
        {
            if (log != null) { 
                Log = log;
            } else
            {
                initLog();
            }
        }

        public void initLog()
        {
            try
            {
                var logFile = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Config\\apprun.log"));
                if (!File.Exists(logFile))
                {
                    File.Create(logFile);
                }
                var log = new Action<string>(s => File.AppendAllText(logFile, s + Environment.NewLine));
                Log = log;
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log?.Invoke($"Error: {DateTime.Now.ToString()} Controller: {ex.Message}.");
            }
        }

        public String? ModelName { get; set; }
        public String? ModelType { get; set; }

    }
}
