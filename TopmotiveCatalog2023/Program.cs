using Bogus;
using System.Collections.Generic;
using System.Text;
using TopmotiveCatalog2023;
using TopmotiveCatalog2023.Models;
using TopmotiveCatalog2023.Controllers;
using static System.Net.Mime.MediaTypeNames;
using System.Xml;
using Mysqlx.Datatypes;
using System.Reflection;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Numerics;
using System.Linq;
using Org.BouncyCastle.Asn1.Cmp;
using Microsoft.Extensions.Options;
using Mysqlx.Notice;
using Google.Protobuf.WellKnownTypes;
using TopmotiveCatalog2023.DAL;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

delegate void menuDelegate();
internal static class Program
{
    public static Action<String>? Log = WriteToLog;
    static int ind = 0;
    static Boolean working = true;
    static Dictionary<int, IFiller> ctrlDict = new Dictionary<int, IFiller>();
    static List<String?> options = new List<String?>();
    static Dictionary<System.Guid, System.String>? lst = new Dictionary<System.Guid, System.String>();
    static List<Guid> selElems = new List<Guid>();
    private static void Main(string[] args)
    {
        try
        {
            initLog();
            Console.WriteLine($"Do you wish to initialize?(Y/N)");
            ConsoleKeyInfo r = Console.ReadKey();
            if (r.Key == ConsoleKey.Y)
            {
                init();
            }
            Console.WriteLine($"Here is a list of manufacturers below.");
            Console.WriteLine($"Please choose the number you want to see the models of, then press Enter");
            Console.WriteLine($"Or choose X to exit.");
            runMenu();
        }catch(Exception e)
        {
            Console.WriteLine($"{e.Message}");
        }
    }

    private static void init()
    {
        try
        {
            DBFiller f = new DBFiller();
            HashSet<dynamic> ctrlSet = new HashSet<dynamic>();
            ctrlSet.Add(new ProductGroupController(Log));
            ctrlSet.Add(new ManufacturerController(Log));
            ctrlSet.Add(new ModelController(Log));
            ctrlSet.Add(new VehicleTypeController(Log));
            ctrlSet.Add(new ArticleController(Log));

            for (int i = 0; i < ctrlSet.Count; ++i)
            {
                f.SetFiller(ctrlSet.ElementAt(i));
                Log?.Invoke($"Info: {DateTime.Now.ToString()} Started generating objects for {ctrlSet.ElementAt(i).ModelName}!");
                f.fillGeneric();
                Log?.Invoke($"Info: {DateTime.Now.ToString()} Finished generating objects for {ctrlSet.ElementAt(i).ModelName}!");
            }
            Console.WriteLine();
        }catch(Exception ex)
        {
            Console.WriteLine($"Initialization Error: {ex.Message}");
            Log?.Invoke($"Error: {DateTime.Now.ToString()} Initialization error: {ex.Message}!");
        }
    }
    


    private static void runMenu()
    {
        try
        {
            ctrlDict.Add(-1, new ManufacturerController(Log));
            ctrlDict.Add(0, new ModelController(Log));
            ctrlDict.Add(1, new VehicleTypeController(Log));
            ctrlDict.Add(2, new ArticleController(Log));
            ctrlDict.Add(3, new ProductGroupController(Log));
            lst = ctrlDict.ElementAt(ind).Value.ListAll();
            options.Add("");
            selElems = new List<Guid>();
            if (lst != null)
            {
                Log?.Invoke($"Info: {DateTime.Now.ToString()} The starting list is of {lst.Count} size");
                processMenu(ctrlDict);
            }
            else
            {
                Console.WriteLine("There are no manufacturers registered!");
                Log?.Invoke($"Warning: {DateTime.Now.ToString()} There are no manufacturers registered!");
            }
        } catch(Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Log?.Invoke($"Error: {DateTime.Now.ToString()} Run menu error: {ex.Message}!");
        }
    }

    private static void initLog(String? filename=null)
    {
        try
        {
            if (Log != null)
            {
                Log?.Invoke($"{Environment.NewLine}{Environment.NewLine} Info: {DateTime.Now.ToString()} Started application!");
            }
        }catch(Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static void printOptions(Dictionary<System.Guid, System.String>? lst, int ind)
    { 
        try {
            if (lst != null && lst.Count>0)
            {
                for (int i = 0; i < lst.Count; i++)
                {
                    Console.WriteLine($"{i + 1}) {lst.ElementAt(i).Value}");
                }

            } else
            {
                Console.WriteLine("There are no options!");
            }
            if(ind>1)
            {
                Console.WriteLine("For this list you can perform the following:");
                Console.WriteLine("N - Add new element");
                Console.WriteLine("U - Update element");
                Console.WriteLine("D - Delete element");
                Console.WriteLine("To choose these options select the letter, then press Enter.");
            }
            if (ind < 3)
            {
                Console.WriteLine($"Please choose the number of your option, then Enter.");
            }
            Console.WriteLine($"Or choose X to go a level higher in the menu.");
        }catch(Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            Log?.Invoke($"Error: {DateTime.Now.ToString()} Menu print options error: {ex.Message}!");
        }
    }

    private static void processStepBack(Dictionary<int, IFiller> ctrlDict)
    {
        try { 
            Console.Clear();
            if (selElems.Count > 0)
            {
                selElems.RemoveAt(selElems.Count - 1);
            }
        
            ind -= 1;
            if (selElems.Count > 0)
            {
                options[0] = selElems.ElementAt(selElems.Count - 1).ToString();
            } else
            {
                options[0] = String.Empty;
            }
            Log?.Invoke($"Info: {DateTime.Now.ToString()} Navigated backwards!");
            lst = ctrlDict.ElementAt(ind).Value.GetOption(options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            Log?.Invoke($"Error: {DateTime.Now.ToString()} Menu step back error: {ex.Message}!");
        }

    }

    private static void processStepForward(Dictionary<int, IFiller> ctrlDict, int uchoice)
    {
        try
        {
            Console.Clear();
            options[0] = lst?.ElementAt(Convert.ToInt16(uchoice) - 1).Key.ToString();
            if (lst != null)
            {
                selElems.Add(lst.ElementAt(Convert.ToInt16(uchoice) - 1).Key);
            }
            ind += 1;
            Log?.Invoke($"Info: {DateTime.Now.ToString()} Navigated forward!");
            lst = ctrlDict.ElementAt(ind).Value.GetOption(options);
        }catch(Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            Log?.Invoke($"Error: {DateTime.Now.ToString()} Menu step forward error: {ex.Message}!");
        }
    }

    private static void processMenu(Dictionary<int, IFiller> ctrlDict)
    {
        char resc;
        menuDelegate mp;
        Dictionary <string, menuDelegate> ctrlMenuOptions = new Dictionary<string, menuDelegate>
        {
            { "x", stepBackInApp },
            { "n", createNew },
            { "d", deleteElem },
            { "u", updateElem },
            { "c", chooseElem }
        };
        List<Char> choices = new List<Char> { 'x', 'n', 'd', 'u' };
        try
        {
            while (working)
            {
                printOptions(lst, ind);
                string? tmp = Console.ReadLine();
                String uchoice = (tmp == null) ? "0" : tmp.ToString().ToLower();
                if (ushort.TryParse(uchoice, out var choice) && ind<3)
                {
                    options.Add(uchoice);
                    mp = new menuDelegate(ctrlMenuOptions["c"]);
                    mp();

                } else if(char.TryParse(uchoice.ToLower(), out resc) && choices.Contains(resc)) {
                    mp = new menuDelegate(ctrlMenuOptions[uchoice.ToLower()]);
                    mp();
                    if (uchoice.ToLower() != "x")
                    {
                        lst = ctrlDict.ElementAt(ind).Value.GetOption(options);
                    } 
                } else 
                {
                    Console.WriteLine("Please select a valid option!");
                }
            }
        } catch(Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            Log?.Invoke($"Error: {DateTime.Now.ToString()} Menu error: {ex.Message}!");
        }
    }

    private static void stepBackInApp()
    {
        try
        {
            if (ind == 0)
            {
                Log?.Invoke($"Info: Exited app.{Environment.NewLine}");
                Console.WriteLine("Exiting...");
                Program.working = false;
            }
            else
            {
                processStepBack(ctrlDict);
            }
        }catch(Exception ex) {
            Console.WriteLine($"{ex.Message}");
            Log?.Invoke($"Error: {DateTime.Now.ToString()} Menu stepper ERR: {ex.Message}!");
        }
    }

    private static void createNew()
    {
        try
        {
            if (ind < 2)
            {
                Console.WriteLine("You can't add for this type of enity. Please choose from the list provided!");
            }
            else
            {
                if (options?.Count >= 1)
                {
                    List<object?> opts = new List<object?>
                    {
                        options[0]
                    };
                    if (options?.Count > 1)
                        opts.Add(options[1]);
                    
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Creating new {ctrlDict.ElementAt(ind).Value.ModelName}!");
                    ctrlDict.ElementAt(ind).Value.AddNewFromConsole(opts);
                }
            }
        }catch(Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            Log?.Invoke($"Error: {DateTime.Now.ToString()} Create elem ERR: {ex.Message}!");
        }
    }

    private static void updateElem()
    {
        try
        {
            if (ind < 2)
            {
                Console.WriteLine("You can't update this type of enity. Please choose from the list provided!");
            }
            else
            {
                Console.WriteLine("Please choose a number form the list provided!");
                string? readline = Console.ReadLine(); String u2choice = "0";
                if (readline != null)
                {
                    u2choice = readline.ToString();
                }
                if (int.TryParse(u2choice, out var choice))
                {
                    List<object> opts = new List<object>();
                    if (lst != null)
                    {
                        opts.Add(lst.ElementAt(choice - 1).Key.ToString());
                    }
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Updating a(n) {ctrlDict.ElementAt(ind).Value.ModelName}!");
                    ctrlDict.ElementAt(ind).Value.UpdateExistingFromConsole(opts);
                }
            }
        }catch(Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            Log?.Invoke($"Error: {DateTime.Now.ToString()} Update elem ERR: {ex.Message}!");
        }
    }

    private static void deleteElem()
    {
        try
        {
            if (ind < 2)
            {
                Console.WriteLine("You can't delete this type of enity. Please choose from the list provided!");
            }
            else
            {
                Console.WriteLine("Please choose a number form the list provided!");
                string? readline = Console.ReadLine();
                string u2choice = (readline == null) ? "0" : readline.ToString();
                if (int.TryParse(u2choice, out var choice))
                {
                    List<object> opts = new List<object>();
                    if (lst != null)
                    {
                        opts.Add(lst.ElementAt(choice - 1).Key.ToString());
                        Log?.Invoke($"Info: {DateTime.Now.ToString()} Deleting a(n) {ctrlDict.ElementAt(ind).Value.ModelName}!");
                        ctrlDict.ElementAt(ind).Value.DeleteExisitngFromConsole(opts);
                    }
                }
            }
        }catch(Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            Log?.Invoke($"Error: {DateTime.Now.ToString()} Delete elem ERR: {ex.Message}!");
        }
    }

    private static void chooseElem()
    {
        try
        {
            String? uchoice = options.Last();
            options.Remove(uchoice);
            if (int.TryParse(uchoice, out int chuint) && lst != null && Convert.ToInt16(uchoice) <= lst.Count)
            {
                if (ind > 1)
                {
                    Console.WriteLine("Please choose the product group option too!");
                    List<Guid>? pgids = printProductGroups((ProductGroupController)ctrlDict.ElementAt(ctrlDict.Count - 1).Value);
                    string? strtmp = Console.ReadLine();
                    String u2choice = (strtmp == null) ? "0" : strtmp.ToString();
                    if (u2choice.ToLower() != "x")
                    {
                        if (int.TryParse(u2choice, out int result))
                        {
                            int ix = int.Parse(u2choice) - 1;
                            Console.WriteLine($"{ix.ToString()}");
                            if (pgids != null && pgids.Count >= ix)
                            {
                                if (options.Count == 1)
                                {
                                    options.Add(pgids[ix].ToString());
                                }
                            }
                        }
                    }
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Listing {ctrlDict.ElementAt(ind).Value.ModelName}!");
                }
                processStepForward(ctrlDict, chuint);
            }
        }catch(Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            Log?.Invoke($"Error: {DateTime.Now.ToString()} Elem choice ERR: {ex.Message}!");
        }
    }

    private static List<Guid>? printProductGroups(ProductGroupController pgc)
    {
        try
        {
            List<ProductGroupModel>? pgs = pgc.ListAll();
            List<Guid>? guids = new List<Guid>();
            if (pgs != null)
            {
                for (int i = 0; i < pgs.Count; ++i)
                {
                    guids.Add(pgs[i].Id);
                    Console.WriteLine($"{(i + 1).ToString()}) {pgs[i].Description}");
                }
            }
            Log?.Invoke($"Info: {DateTime.Now.ToString()} Gotten a list of product groups!");
            return guids;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            Log?.Invoke($"Error: {DateTime.Now.ToString()} Print product ERR: {ex.Message}!");
            return null;
        }
    }

    private static void WriteToLog(string logMessage)
    {
        try
        {
            String filename = "apprun.log";
            var logFile = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Config\\" + filename));
            if (!File.Exists(logFile))
            {
                File.Create(logFile);
            }
            using (StreamWriter writer = new StreamWriter(logFile, true))
            {
                writer.WriteLine(logMessage);
            }
        }catch(Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
        }
    }
}