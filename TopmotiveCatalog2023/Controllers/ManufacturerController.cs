using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using TopmotiveCatalog2023.Models;
using MySql.Data.MySqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Data.Entity;
using System.Xml;
using TopmotiveCatalog2023.DAL;
using Pomelo.EntityFrameworkCore.MySql;
using Microsoft.EntityFrameworkCore;
using System.Data.Entity.Core.EntityClient;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Diagnostics;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Math.EC;
using MySqlX.XDevAPI.Common;

namespace TopmotiveCatalog2023.Controllers
{

    internal class ManufacturerController : Controller, IFiller 
    {
        public new string? ModelName = "Manufacturer";
        public new string? ModelType = "ManufacturerModel";

        private IEnumerable<System.String> ManufTypes = new[] { "defaultManufacturers", "defaultPieceManufacturers" };
        List<System.String> manufacturers = new() { "Audi", "Ford", "BMW", "Toyota", "Mercedes",
                                        "Kia", "Hyundai", "Honda", "Mini", "Mazda", "Volkswagen" };
        public ManufacturerController() { }

        public ManufacturerController(Action<System.String>? log) {
            if (log != null)
            {
                ConfigController.Log = log;
                Log = log;
            }
        }

        public Dictionary<System.Guid,System.String> ListAll ()
        {
            List<ManufacturerModel> mfs = new List<ManufacturerModel>();
            try
            {
                using (PiecesContext context = new PiecesContext(Log))
                {
                    var repository = new ManufacturerRepository(context);
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    Dictionary<System.Guid, System.String> result = repository.GetByExclType("Assembly only").ToList().Select(m => new { m.Id, m.Description }).ToDictionary(x => x.Id, x => x.Description);
                    s.Stop();
                    Log?.Invoke($"Info:  {DateTime.Now.ToString()} Got a list of car manufacturers in {s.ElapsedMilliseconds.ToString()}ms.");
                    Console.WriteLine($"Got a list of car manufacturers in {s.ElapsedMilliseconds.ToString()}ms");
                    return result;
                }
            }catch(Exception ex)
            {
                Log?.Invoke($"Error: {DateTime.Now.ToString()} Error in listing {ex.Message}!");
                Console.WriteLine(ex.Message);
                return new Dictionary<Guid, string>();
            }
        }

        public object DoGenerate()
        {
            try
            {
                Random rnd = new Random();
                var number = rnd.Next(5, 10);
                List<ManufacturerModel> mfs = new Faker<ManufacturerModel>()
                                            .RuleFor(u => u.Id, f => Guid.NewGuid())
                                            .RuleFor(u => u.Type, f => "Both car and assemblies")
                                            .Generate(number);
                var f = new Faker();
                List<System.String>? defmans = ConfigController.getValuesFrom(ManufTypes.ElementAt(0));
                if (defmans != null) manufacturers = defmans;
                var descs = f.PickRandom(manufacturers, number).Distinct();
                for (int i=0;i<mfs.Count;++i)
                {
                    mfs[i].Description = descs.ElementAt(i)?? f.PickRandom(manufacturers);
                }
                fillWithData(mfs);
                Console.WriteLine("Generated Car types of manufacturers");
                manufacturers = new List<System.String>();
                defmans = ConfigController.getValuesFrom(ManufTypes.ElementAt(1));
                if (defmans != null) manufacturers = defmans;
                Console.WriteLine($"{defmans?.Count} are by default");
                mfs = new Faker<ManufacturerModel>()
                        .RuleFor(u => u.Id, f => Guid.NewGuid())
                        .RuleFor(u => u.Type, f => "Assembly only")
                        .Generate(5);
                for (int i = 0; i < mfs.Count; ++i)
                {
                    mfs[i].Description = descs.ElementAt(i) ?? f.PickRandom(manufacturers);
                }
                fillWithData(mfs);
                Console.WriteLine("Generated Assembly types of manufacturers");
                return new { Status = 200, Message = "OK" };
            } catch(Exception ex)
            {
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                Console.WriteLine($" Exception caught: {0}", ex.Message);
                return ex;
            }
        }

        private void fillWithData(List<ManufacturerModel> mfs)
        {
            try
            {
                var options = new DbContextOptionsBuilder<PiecesContext>()
                    .UseMySql(ConfigController.getConnectionString(), new MySqlServerVersion(new Version(8, 0, 31)))
                    .Options;

                using (PiecesContext context = new PiecesContext(Log))
                {
                    var repository = new ManufacturerRepository(context);
                    foreach (var mf in mfs)
                    {
                        try
                        {
                            ManufacturerModel? nmf = switchIfExists(mf);
                            
                            if (nmf != null)
                            {
                                Console.WriteLine($"Inserting {nmf.Description}.");
                                Stopwatch s = Stopwatch.StartNew();
                                Log?.Invoke($"{DateTime.Now.ToString()} Inserting {nmf.Description} to table {ModelName}!");
                                repository.Insert(nmf);
                                repository.Save();
                                s.Stop();
                                Console.WriteLine($"Inserting elem to {ModelName} took {s.ElapsedMilliseconds}ms");
                                Log?.Invoke($"{DateTime.Now.ToString()} Finished inserting {nmf.Description} to table {ModelName} in {s.Elapsed.TotalMilliseconds}ms!");
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{ex.Message}");
                            Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database context error: {ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
            }
        }

        private ManufacturerModel? switchIfExists(ManufacturerModel? mf)
        {
            try
            {
                using (PiecesContext context = new PiecesContext(Log))
                {
                    Stopwatch s = new Stopwatch();
                    var dpl = new ManufacturerModel();
                    if (mf != null) {
                        s.Start();
                           dpl = context.Manufacturers.Where(m => m.Description == mf.Description).FirstOrDefault();
                        s.Stop();
                        Log?.Invoke($"Info: {DateTime.Now.ToString()} Found {dpl.Description} manufacturer in {s.ElapsedMilliseconds.ToString()}ms");

                    } else
                    {
                        return mf;
                    }
                    Console.WriteLine($"Found {dpl?.Description}");
                    List<System.String> exceptedManufacturers = new List<System.String>();
                    int i = 1;
                    while (dpl?.Description != null && i<15)
                    {
                        if (!exceptedManufacturers.Contains(dpl.Description))
                        {
                            exceptedManufacturers.Add(dpl.Description);
                        }
                        if (exceptedManufacturers.Count < manufacturers.Count)
                        {
                            var f = new Faker<ManufacturerModel>()
                                    .RuleFor(m => m.Description, (f, x) => f.PickRandom(manufacturers.Except(exceptedManufacturers).ToArray()))
                                    .Generate();
                            if(mf!=null) mf.Description = f.Description;
                            Console.WriteLine($"Generated {mf?.Description} in iteration {i}");
                            s.Restart();
                            if(mf!=null) dpl = context.Manufacturers.Where(b => b.Description == mf.Description).First();
                            s.Stop();
                            Log?.Invoke($"Info: {DateTime.Now.ToString()} Found {dpl.Description} manufacturer in {s.ElapsedMilliseconds.ToString()}ms");
                            Console.WriteLine($"FOund {dpl.Description} afterwards directly!");
                        }
                        ++i;
                    }
                    if (i > 14)
                        return null;
                    return mf;
                }
            } catch(Exception ex)
            {
                Log?.Invoke($"{DateTime.Now.ToString()} A switchError: {ex.Message}!");
                Console.WriteLine($"A switchError: {ex.Message}");
                return null;
            }
                   
        }

        public System.Boolean SeeIfEmpty()
        {
            try
            {
                using (PiecesContext context = new PiecesContext())
                {
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    System.Boolean result = context.Manufacturers.Any();
                    s.Stop();
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Verifying manufacturers table emptiness took {s.ElapsedMilliseconds.ToString()}ms");
                    if (!result)
                    {
                        return true;
                    }
                    return false;
                }
            }catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} Checking empty table {ex.Message}");
                return false;
            }
                    
        }

        public Dictionary<Guid, System.String>? GetOption(List<System.String?>? options = null)
        {
            try
            {
                if (options?.Count >= 1)
                {
                    try
                    {
                        using (PiecesContext context = new PiecesContext(Log))
                        {
                            var repository = new ManufacturerRepository(context);
                            Dictionary<Guid, System.String>? results = new Dictionary<Guid, string>();
                            Stopwatch s = new Stopwatch();
                            s.Start();
                            results = repository.GetByExclType("Assembly only").ToList().Select(m => new { m.Id, m.Description }).ToDictionary(x => x.Id, x => x.Description);
                            s.Stop();
                            Log?.Invoke($"Info:  {DateTime.Now.ToString()} Got a list of car manufacturers in {s.ElapsedMilliseconds.ToString()}ms.");
                            Console.WriteLine($"Got a list of car manufacturers in {s.ElapsedMilliseconds.ToString()}ms");
                            return results;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log?.Invoke($"{DateTime.Now.ToString()} Error in listing {ex.Message}!");
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    using (PiecesContext context = new PiecesContext(Log))
                    {
                        var repository = new ManufacturerRepository(context);
                        Stopwatch s = new Stopwatch();s.Start();
                        Dictionary<Guid, string> res = repository.GetByExclType("Assembly only").ToList().Select(m => new { m.Id, m.Description }).ToDictionary(x => x.Id, x => x.Description);
                        s.Stop();
                        Log?.Invoke($"Info: {DateTime.Now.ToString()} Found {res.Count} car manufacturers in {s.ElapsedMilliseconds.ToString()}ms!");
                        return res;
                    }
                }
                return null;
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                return null;
            }

        }

        public void AddNewFromConsole(List<object?> options)
        {
            throw new NotImplementedException();
        }

        public void UpdateExistingFromConsole(List<object>? options, IModel? model)
        {
            throw new NotImplementedException();
        }

        public void DeleteExisitngFromConsole(List<object>? options = null, IModel? model = null)
        {
            throw new NotImplementedException();
        }

        public void DeleteExistingFromConsole(List<object>? options = null, IModel? model = null)
        {
            throw new NotImplementedException();
        }
    }
}
