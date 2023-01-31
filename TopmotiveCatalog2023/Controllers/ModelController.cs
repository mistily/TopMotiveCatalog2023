using Bogus;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using TopmotiveCatalog2023.DAL;
using TopmotiveCatalog2023.Models;

namespace TopmotiveCatalog2023.Controllers
{
    internal class ModelController: Controller, IFiller
    {
        public String? modelName = "Vehicle Model";
        public String modeltype = "Vehicle Models Model";

        public ModelController() { }

        public ModelController(Action<String>? log)
        {
            if (log != null)
            {
                Log = log;
                ConfigController.Log = log;
            }
        }
        public new String? ModelType { get => ModelType; set => ModelType=value; }
        public new String? ModelName { get => modelName; set => ModelName = value; }

        public object DoGenerate()
        {
            try
            {
                List<Guid> manufs = LoadPresentManufs().ToList();
                Console.WriteLine($"Found {manufs.Count} manufacturers.");
                Random rnd = new Random();
                foreach (Guid manuf in manufs)
                {
                    var number = rnd.Next(5, 10);
                    Console.WriteLine($"Generating {number} models for {manuf.ToString()}.");
                    List<VehicleModelsModel> vms = new Faker<VehicleModelsModel>()
                                                .RuleFor(u => u.Id, f => Guid.NewGuid())
                                                .RuleFor(u => u.ManufacturerId, f => manuf)
                                                .Generate(number);
                    var f = new Faker();
                    List<System.String>? defmdls = ConfigController.getValuesFrom("defaultModels");
                    if (defmdls != null)
                    {
                        var descs = f.PickRandom(defmdls, number).Distinct();
                        for (int i = 0; i < vms.Count; ++i)
                        {
                            vms[i].Description = descs.ElementAt(i) ?? f.PickRandom(defmdls);

                        }
                    }
                    fillWithData(vms, defmdls);
                }
                return new { Status = 200, Message = "OK" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Exception caught: {0}", ex.Message);
                return ex;
            }
        }

        public Dictionary<Guid, string>? GetOption(List<string?>? options = null)
        {
            try
            {
                if (options != null && options.Count >= 1)
                {
                    string? strtmp = options[0]?.ToString(); Guid key = Guid.Empty;
                    if (strtmp != null) key = Guid.Parse(strtmp);
                    try
                    {
                        using (PiecesContext context = new PiecesContext(Log))
                        {
                            var repository = new VehicleModelsRepository(context);
                            Dictionary<Guid, string> results= new Dictionary<Guid, string>();
                            Stopwatch s = new Stopwatch();
                            s.Start();
                            results = repository.GetByManufacturer(key).ToList().Select(m => new { m.Id, m.Description }).ToDictionary(x => x.Id, x => x.Description);
                            s.Stop();
                            Console.WriteLine($"Got a list of vehicle models in {s.ElapsedMilliseconds.ToString()}ms");
                            Log?.Invoke($"Info {DateTime.Now.ToString()} Got a list of vehicle models in {s.ElapsedMilliseconds.ToString()}ms!");
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
                    return null;
                }
                return null;
            }catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return null;
            }
        }

        private void fillWithData(List<VehicleModelsModel> vms, List<System.String>? defmdls)
        {
            try
            {
                using (PiecesContext context = new PiecesContext(Log))
                {
                    var repository = new VehicleModelsRepository(context);
                    foreach (var mf in vms)
                    {
                        try
                        {
                            VehicleModelsModel? vmm = switchIfExists(mf, defmdls);

                            if (vmm != null)
                            {
                                Console.WriteLine($"Inserting {vmm.Description}.");
                                Stopwatch s = Stopwatch.StartNew();
                                Log?.Invoke($"{DateTime.Now.ToString()} Inserting {vmm.Description} to table {ModelName}!");
                                repository.Insert(vmm);
                                repository.Save();
                                s.Stop();
                                Console.WriteLine($"Inserting elem to {ModelName} took {s.ElapsedMilliseconds}ms");
                                Log?.Invoke($"{DateTime.Now.ToString()} Finished inserting {vmm.Description} to table {ModelName} in {s.Elapsed.TotalMilliseconds}ms!");
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

        private VehicleModelsModel? switchIfExists(VehicleModelsModel? vmm, List<System.String>? defmdls)
        {
            try
            {
                using (PiecesContext context = new PiecesContext(Log))
                {
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    var vm = (vmm!=null) ? context.VehicleModels.Where(m => m.Description == vmm.Description && m.ManufacturerId== vmm.ManufacturerId).FirstOrDefault(): null;
                    s.Stop( );
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Found vehicle model {vm?.Description} in {s.ElapsedMilliseconds.ToString()}ms.");
                    if (vm == null)
                    {
                        return vmm;
                    }
                    Console.WriteLine($"Found {vm.Description}");
                    List<System.String> exceptedVMs = new List<System.String>();
                    int i = 1;
                    while (vm.Description != null && i < 15)
                    {
                        if (!exceptedVMs.Contains(vm.Description))
                        {
                            exceptedVMs.Add(vm.Description);
                        }
                        if (defmdls!=null && exceptedVMs.Count < defmdls.Count)
                        {
                            var f = new Faker<VehicleModelsModel>()
                                    .RuleFor(m => m.Description, (f, x) => f.PickRandom(defmdls.Except(exceptedVMs).ToArray()))
                                    .Generate();
                            vm.Description = f.Description;
                            Console.WriteLine($"Generated {vm.Description} in iteration {i}");
                            s.Restart();
                            vm = context.VehicleModels.Where(b => b.Description == vm.Description).First();
                            s.Stop();
                            Log?.Invoke($"Info: {DateTime.Now.ToString()} Found vehicle model {vm?.Description} in {s.ElapsedMilliseconds.ToString()}ms.");
                            Console.WriteLine($"Found {vm.Description} afterwards!");
                        }
                        ++i;
                    }
                    if (i > 14)
                        return null;
                    return vm;
                }
            }
            catch (Exception ex)
            {
                Log?.Invoke($"Error: {DateTime.Now.ToString()} A switchError: {ex.Message}!");
                Console.WriteLine($"A switchError: {ex.Message}");
                return null;
            }
        }

        private IEnumerable<Guid> LoadPresentManufs()
        {
            List<Guid> manufs = new List<Guid>();
            try
            {
                using (PiecesContext context = new PiecesContext(Log))
                {
                    var repository = new ManufacturerRepository(context);
                    Stopwatch s = new Stopwatch();s.Start();
                    IEnumerable<Guid> res = repository.GetAllIds("Assembly only");
                    s.Stop();
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Got vehicle part brands in {s.ElapsedMilliseconds.ToString()}ms!");
                    return res;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}!");
            }
            return manufs;
        }

        public System.Boolean SeeIfEmpty()
        {
            try
            {
                using (PiecesContext context = new PiecesContext())
                {
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    Boolean result = context.VehicleModels.Any();
                    s.Stop();
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Verifying vehicle model table emptiness took {s.ElapsedMilliseconds.ToString()}ms");
                    if (!result)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Err: {DateTime.Now.ToString()} Checking empty table: {ex.Message}");
                return true;
            }

        }

        Dictionary<Guid, string> IFiller.ListAll()
        {
            throw new NotImplementedException();
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
