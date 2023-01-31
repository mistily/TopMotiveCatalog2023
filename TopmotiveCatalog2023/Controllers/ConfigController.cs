using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TopmotiveCatalog2023.Controllers
{
    internal static class ConfigController
    {
        private static readonly string logFile = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Config\\apprun.log"));
        public static Action<String>? Log = new Action<string>(s => File.AppendAllText(logFile, s + Environment.NewLine));
        private static String? filepath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Config\\AppConfig.xml"));

        
        public static void SetConfig(String? fp = null)
        {
            if (fp != null)
            {
                filepath = fp;
            }
        }
        public static String? getConnectionString()
        {
            String? cns = null;
            try
            {
                XmlTextReader reader = new XmlTextReader(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Config\\AppConfig.xml")));
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "connectionStrings")
                    {
                        reader.Read(); reader.Read();
                        string? strtmp = reader.GetAttribute("connectionString");
                        if (strtmp != null) {
                            cns = strtmp.ToString().Trim();
                            Log?.Invoke($"Info: {DateTime.Now.ToString()} Config: Found connection string: {cns}.");
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} Config: {ex.Message}.");
            }
            return cns;
        }

        public static List<String>? getValuesFrom(String tag)
        {
            List<String>? options = new List<String>();
            Console.WriteLine($"Getting tag {tag} from Config");
            Log?.Invoke($"Info: {DateTime.Now.ToString()} Config: getting {tag} value from file.");
            try
            {
                XmlTextReader reader = new XmlTextReader(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Config\\AppConfig.xml")));
                while (reader.Read())
                {
                    if (reader.IsStartElement() && reader.Name == tag)
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement() && reader.Name == "option") {
                                string? strtmp = reader.GetAttribute("value");
                                if (strtmp != null && strtmp.Trim()!=String.Empty)
                                {
                                    options.Add(strtmp.Trim());
                                }
                            } else if(reader.NodeType == XmlNodeType.EndElement && reader.Name == tag)
                            {
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} Config: {ex.Message}.");
            }
            finally {
                Console.WriteLine($"Finished constructing options list. {options.Count}");
            }
            return options;
        }
    }
}
