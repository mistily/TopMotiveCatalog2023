using Bogus;
using Bogus.DataSets;
using Bogus.Extensions.Italy;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using TopmotiveCatalog2023.DAL;
using TopmotiveCatalog2023.Models;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace TopmotiveCatalog2023.Controllers
{
    delegate Task AsyncDelegate();
    internal class VehicleTypeController : Controller, IFiller
    {
        private string? modelType = "Vehicle Type";
        private string? modelName = "Vehicle Type Model";

        public new string? ModelName { get => modelName; set => modelName = value; }
        public new string? ModelType { get => modelType; set => modelType = value; }

        public VehicleTypeController()
        {
            ModelName = "Vehicle Type";
            ModelType = "Vehicle Type Model";
            
        }

        public VehicleTypeController(Action<String>? log)
        {
            if (log != null)
            {
                Log = log;
                ConfigController.Log = log;
            }
        }

        public object DoGenerate()
        {
            try
            {
                List<Guid> vmodels = LoadPresentModels().ToList();
                Console.WriteLine($"Found {vmodels.Count} vehicle models.");
                Random rnd = new Random();
                foreach (Guid vmodel in vmodels)
                {
                    var number = rnd.Next(10, 20);
                    Console.WriteLine($"Generating {number} vehicle types for {vmodel.ToString()}.");
                    List<ushort> dcs = new List<ushort>() { 2, 3, 4, 5, 6, 7, 9 };
                    List<String> fts = new List<String>() { "Petrol", "Diesel", "Bio diesel",
                        "Liquid petroleum gas", "Ethanol or methanol", "Electric", "Hydrogen" };
                    ushort ccm = (ushort)rnd.Next(1000, 2200);
                    ushort kw = (ushort)rnd.Next(4, 1500);
                    List<String>? bdts = ConfigController.getValuesFrom("defaultModels");
                    Log?.Invoke($"Info:  {DateTime.Now.ToString()} There are {bdts?.Count} default models!");
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    List<VehicleTypesModel> vtps = new Faker<VehicleTypesModel>()
                                                .RuleFor(u => u.Id, f => Guid.NewGuid())
                                                .RuleFor(u => u.CCM, f => ccm)
                                                .RuleFor(u => u.KW, f => kw)
                                                .RuleFor(u => u.FuelType, (f, u) => f.PickRandom(fts))
                                                .RuleFor(u => u.BodyType, (f, u) => f.PickRandom(bdts))
                                                .RuleFor(u => u.DoorCount, (f, u) => f.PickRandom(dcs))
                                                .RuleFor(u => u.VehicleModelId, f => vmodel)
                                                .Generate(number);
                    s.Stop();
                    Log?.Invoke($"Info:  {DateTime.Now.ToString()} Generated {number} vehivle types in {s.ElapsedMilliseconds.ToString()}ms.");
                    var f = new Faker(); 
                    List<System.String>? deftypes = ConfigController.getValuesFrom("defaultTypes");
                    Log?.Invoke($"Info:  {DateTime.Now.ToString()} Found {deftypes.Count} vehicle types.");
                    if (deftypes != null)
                    {
                        var descs = f.PickRandom(deftypes, number).Distinct();
                        for (int i = 0; i < vtps.Count; ++i)
                        {
                            vtps[i].Description = descs.ElementAt(i) ?? f.PickRandom(deftypes);

                        }
                        fillWithData(vtps, deftypes);
                    }

                }
                return new { Status = 200, Message = "OK" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Exception caught: {0}", ex.Message);
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                return ex;
            }
        }

        private void fillWithData(List<VehicleTypesModel> vtps, List<string>? deftypes)
        {
            try
            {
                using (PiecesContext context = new PiecesContext(Log))
                {
                    var repository = new VehicleTypesRepository(context);
                    foreach (var vt in vtps)
                    {
                        try
                        {
                            VehicleTypesModel? vtm = switchIfExists(vt, deftypes);

                            if (vtm != null)
                            {
                                Console.WriteLine($"Inserting {vtm.Description}.");
                                Stopwatch s = Stopwatch.StartNew();
                                Log?.Invoke($"{DateTime.Now.ToString()} Inserting {vtm.Description} to table {ModelName}!");
                                repository.Insert(vtm);
                                repository.Save();
                                s.Stop();
                                Log?.Invoke($"{DateTime.Now.ToString()} Finished inserting {vtm.Description} to table {ModelName} in {s.Elapsed.TotalMilliseconds}ms!");
                                Console.WriteLine($"Inserting elem to {ModelName} took {s.ElapsedMilliseconds.ToString()}ms");
                                fillRelation(vtm, context);
                                Log?.Invoke($"{DateTime.Now.ToString()} Added its relationship too.");
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Database insertion exception: {ex.Message}");
                            break;
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

        private void fillRelation(VehicleTypesModel vtm, PiecesContext context)
        {
            try
            {
                Random rand = new Random();
                int nrofgroups = rand.Next(3, 5);
                for (int i = 0; i < nrofgroups; i++)
                {
                    int skip = rand.Next(0, context.ProductGroups.Count());
                    var rndProdGrp = context.ProductGroups.OrderBy(x => Guid.NewGuid()).Skip(skip).FirstOrDefault();
                    if (rndProdGrp != null)
                    {
                        ProductGroupToVehicleTypeModel pgtvtm = new ProductGroupToVehicleTypeModel
                        {
                            ProductGroup = rndProdGrp,
                            VehicleType = vtm
                        };

                        Console.WriteLine($"Inserting {vtm.Description} to {rndProdGrp.Description} group.");
                        Stopwatch s = Stopwatch.StartNew();
                        Log?.Invoke($"Info: {DateTime.Now.ToString()}Inserting {vtm.Description} to {rndProdGrp.Description} group in relation table !");
                        context.ProductGroupToVehicleTypes.Add(pgtvtm);
                        context.SaveChanges();
                        s.Stop();
                        Console.WriteLine($"Inserting elem to {ModelName} took {s.ElapsedMilliseconds.ToString()}ms");
                        Log?.Invoke($"Info: {DateTime.Now.ToString()}Inserting {vtm.Description} to {rndProdGrp.Description} took {s.ElapsedMilliseconds.ToString()}ms !");
                    }
                }
            } catch(Exception ex)
            {
                Console.WriteLine($"ERR Product Type Product Group relationship problem: {ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
            }
        }

        private VehicleTypesModel? switchIfExists(VehicleTypesModel mf, List<String>? deftypes)
        {
            try
            {
                using (PiecesContext context = new PiecesContext(Log))
                {
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    int nr = context.VehicleTypes.Count();
                    s.Stop();
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Counting rows in table vehcle types took {s.ElapsedMilliseconds.ToString()}ms !");
                    
                    if (nr == 0 )
                    {
                        return mf;
                    }
                    s.Restart();
                    var nmf = context.VehicleTypes.Where(m => m.Description == mf.Description && m.VehicleModelId == mf.VehicleModelId).FirstOrDefault();
                    s.Stop();
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Getting a vehicle type took {s.ElapsedMilliseconds.ToString()}ms !");
                    if (nmf == null)
                    {
                        return mf;
                    }
                    Console.WriteLine($"Found {nmf.Description}");
                    List<System.String> exceptedVTs = new List<System.String>();
                    int i = 1;
                    while (nmf.Description != null && i < 15)
                    {
                        if (!exceptedVTs.Contains(nmf.Description))
                        {
                            exceptedVTs.Add(nmf.Description);
                        }
                        if (exceptedVTs.Count < deftypes?.Count)
                        {
                            var f = new Faker<VehicleTypesModel>()
                                    .RuleFor(m => m.Description, (f, x) => f.PickRandom(deftypes.Except(exceptedVTs).ToArray()))
                                    .Generate();
                            nmf.Description = f.Description;
                            Console.WriteLine($"Generated {nmf.Description} in iteration {i}");
                            s.Restart();
                            nmf = context.VehicleTypes.Where(b => b.Description == nmf.Description).First();
                            s.Stop();
                            Log?.Invoke($"Info: {DateTime.Now.ToString()} Getting a vehicle type from db took {s.ElapsedMilliseconds.ToString()}ms !");
                            Console.WriteLine($"FOund {nmf.Description} afterwards directly!");
                        }
                        ++i;
                    }
                    if (i > 14)
                        return null;
                    return nmf;
                }
            }
            catch (Exception ex)
            {
                Log?.Invoke($"{DateTime.Now.ToString()} A switchError: {ex.Message}!");
                Console.WriteLine($"A switchError: {ex.Message}");
                return null;
            }
        }

        public bool SeeIfEmpty()
        {
            try
            {
                using (PiecesContext context = new PiecesContext())
                {
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    Boolean result = context.VehicleTypes.Any();
                    s.Stop();
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Verifying vehicle type table emptiness took {s.ElapsedMilliseconds.ToString()}ms");
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
                Log?.Invoke($"Error: {DateTime.Now.ToString()} Checking empty table {ex.Message}");
                return true;
            }
        }

        private IEnumerable<Guid> LoadPresentModels()
        {
            List<Guid> manufs = new List<Guid>();
            try
            {
                using (PiecesContext context = new PiecesContext(Log))
                {
                    var repository = new VehicleModelsRepository(context);
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    List<Guid> res = repository.GetAllIds().ToList();
                    s.Stop();
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Got a list of vehicle ids in {s.ElapsedMilliseconds.ToString()}ms !");
                    return res;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
            }
            return manufs;
        }

        public Dictionary<Guid, string>? GetOption(List<string?>? options = null)
        {
            try
            {
                if (options?.Count >= 1)
                {
                    string? strtmp = options[0]; Guid key = Guid.Empty;
                    if (strtmp != null)
                    {
                        key = Guid.Parse(strtmp);
                    }
                    try
                    {
                        using (PiecesContext context = new PiecesContext(Log))
                        {
                            var repository = new VehicleTypesRepository(context);
                            Stopwatch s = new Stopwatch();s.Start();
                            Dictionary<Guid, string> vmopts = repository.GetByVehicleModel(key).ToList().Select(m => new { m.Id, v = getMyDescription(m.Id, m.Description) }).ToDictionary(x => x.Id, x => x.v);
                            s.Stop();
                            Log?.Invoke($"Info:  {DateTime.Now.ToString()} Got a list of vehicle types in {s.ElapsedMilliseconds.ToString()}ms.");
                            Console.WriteLine($"Got a list of vehicle types in {s.ElapsedMilliseconds.ToString()}ms");
                            return vmopts;
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
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                return null;
            }
        }

        public Dictionary<Guid, string> ListAll()
        {
            throw new NotImplementedException();
        }

        private String getMyDescription(Guid id, string description)
        {
            try
            {
                StringBuilder s = new StringBuilder();
                s.Append(description);
                s.Append(Environment.NewLine);
                s.Append("Part of the following groups");
                s.Append(Environment.NewLine);
                using (PiecesContext context = new PiecesContext(Log))
                {
                    var repository = new ProductGroupRepository(context);
                    Stopwatch stopw = new Stopwatch();
                    stopw.Start();
                    var query = from pg in context.ProductGroups
                                join pgvt in context.ProductGroupToVehicleTypes on pg.Id equals pgvt.ProductGroupId
                                join vt in context.VehicleTypes on pgvt.VehicleTypeId equals vt.Id
                                where vt.Id == id
                                orderby pg.Description descending
                                select new { pg.Id, pg.Description };
                    List<string> res = query.ToList().Select((elem, index) => "    " + (index + 1).ToString() + ") " + elem.Description).ToList();
                    stopw.Stop();
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Got a product group list in {stopw.ElapsedMilliseconds.ToString()}ms");
                    if (res.Count > 0)
                    {
                        foreach (var item in res)
                        {
                            s.Append(item.ToString());
                            s.Append(Environment.NewLine);
                        }
                    }
                    else
                    {
                        s.Append("None");
                        s.Append(Environment.NewLine);
                    }
                    return s.ToString();
                }
            }catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                return String.Empty;
            }
        }
        public void AddNewFromConsole(List<object?> options)
        {
            try
            {
                VehicleTypesModel vtm = new VehicleTypesModel();
                vtm.Id= VerifyID(vtm.Id);
                string? tmpval = options[0]?.ToString();
                if (tmpval != null) { vtm.VehicleModelId = Guid.Parse(tmpval); }
                Console.Write($"{Environment.NewLine} Please type in a vehicle name:");
                string? readline = Console.ReadLine();
                if (readline != null) {
                    vtm.Description = readline.ToString();
                }
                while(!VerifyName(vtm.Description))
                {
                    Console.Write($"{Environment.NewLine} This name already exists. Please type in another one.");
                    readline = Console.ReadLine();
                    if (readline != null)
                    {
                        if (readline.ToLower() == "x") return;
                        vtm.Description = readline.ToString();
                    }
                }
                vtm.CCM = VerifyAndGetCCM();
                vtm.FuelType = VerifyAndGetFuelType(vtm.FuelTypes);
                vtm.KW = VerifyAndGetKW();
                vtm.BodyType = VerifyAndGetBodyType();
                vtm.DoorCount = VefiryAndGetDoorCount();
                SaveElem(vtm,'i');
            }
            catch(Exception ex)
            {
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                Console.WriteLine($"{ex.Message}");
            }
        }

        private Guid VerifyID(Guid id)
        {
            try
            {
                Guid nid = id;
                using (PiecesContext context = new PiecesContext())
                {
                    VehicleTypesRepository repository = new VehicleTypesRepository(context);
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    VehicleTypesModel? elem = repository.GetById(nid);
                    s.Stop();
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Checked one vehicle type by ID took {s.ElapsedMilliseconds.ToString()}ms");
                    while (elem!=null)
                    {
                        nid = Guid.NewGuid();
                        s.Reset();s.Restart();
                        elem = repository.GetById(nid);
                        s.Stop();
                        Log?.Invoke($"Info: {DateTime.Now.ToString()} Checked one vehicle type by ID took {s.ElapsedMilliseconds.ToString()}ms");
                    }
                    return nid;
                }
            }catch(Exception ex) {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                return id;
            }
        }

        private Boolean VerifyName(String name)
        {
            try
            {
                String nn = name.Trim();
                if(nn==String.Empty)
                {
                    return true;
                }
                using (PiecesContext context = new PiecesContext())
                {
                    VehicleTypesRepository repository = new VehicleTypesRepository(context);
                    Stopwatch s = new Stopwatch();s.Start();
                    List<VehicleTypesModel> isThere = (List<VehicleTypesModel>)repository.GetByName(nn);
                    s.Stop();
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Checked one vehicle type by name in {s.ElapsedMilliseconds.ToString()}ms");
                    if (isThere==null)
                    {
                        return false;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                return true;
            }
        }

        private ushort VerifyAndGetCCM()
        {
            try
            {
                Console.Write($"{Environment.NewLine} Please type in the cubic capacity of engine:");
                string? tmpstrval = Console.ReadLine();
                ushort tmp = 1;
                while (!ushort.TryParse(tmpstrval, out tmp))
                {
                    Console.Write($"{Environment.NewLine} This is not the accepted type of value. Please type in another one.");
                    tmpstrval = Console.ReadLine();
                }
                return tmp;
            } catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                return 0;
            }
        }

        private String? VerifyAndGetFuelType(List<String> vtm)
        {
            try
            {
                Console.Write($"{Environment.NewLine} Please type in the fuel type name ({String.Join(" ", vtm)}):");
                String? tmpval = Console.ReadLine();
                while (tmpval!=null && !vtm.Contains(tmpval))
                {
                    Console.Write($"{Environment.NewLine} This is not the accepted value. Please type in another one.");
                    tmpval = Console.ReadLine();
                }
                return tmpval;
            }catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                return null;
            }
        }

        private ushort? VerifyAndGetKW()
        {
            try
            {
                Console.Write($"{Environment.NewLine} Please type in the power of the engine in KW:");
                String? tmpval = Console.ReadLine(); ushort tmp = 0;
                while (!ushort.TryParse(tmpval, out tmp))
                {
                    Console.Write($"{Environment.NewLine} This is not the accepted type of value. Please type in another one.");
                    tmpval = Console.ReadLine();
                }
                return tmp;
            }catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                return null;
            }
        }

        private String? VerifyAndGetBodyType()
        {
            try
            {
                Console.Write($"{Environment.NewLine} Please type in the body type of the vehicle type(max chars: 100):");
                string? readline = Console.ReadLine();
                if (readline != null)
                {
                    if (readline.ToLower() == "x") return null;
                    while (readline?.Length > 100)
                    {
                        Console.Write($"{Environment.NewLine} This is not the accepted length. Please type in another one.");
                        readline = Console.ReadLine();
                    }
                }
                return readline;
            } catch(Exception ex) {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                return null;
            }
        }

        private ushort? VefiryAndGetDoorCount()
        {
            try
            {
                Console.Write($"{Environment.NewLine} Please type in the door count of the vehicle type:");
                string? tmpval = Console.ReadLine(); ushort tmp = 0;
                while (!ushort.TryParse(tmpval, out tmp) && tmp > 17)
                {
                    Console.Write($"{Environment.NewLine} This is not the accepted type of value. Please type in another one.");
                    tmpval = Console.ReadLine();
                }
                return tmp;
            }catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                return null;
            }
        }

        private void SaveElem(VehicleTypesModel vtm, Char optype='i')
        {
            try
            {
                using(PiecesContext context = new PiecesContext())
                {
                    VehicleTypesRepository repository = new VehicleTypesRepository(context);
                    Stopwatch s = new Stopwatch();
                    if (optype == 'i')
                    {
                        s.Reset();
                        s.Start();
                        Console.WriteLine($"{DateTime.Now.ToString()} Inserting new elem..");
                        repository.Insert(vtm);
                        repository.Save();
                        s.Stop();
                        Console.WriteLine($"{DateTime.Now.ToString()} Inserted new elem in {s.ElapsedMilliseconds.ToString()}ms");
                        Log?.Invoke($"{DateTime.Now.ToString()} Inserted new vehicle type {vtm.Description} in {s.ElapsedMilliseconds.ToString()}ms");
                    } else if(optype=='u')
                    {
                        s.Reset();
                        s.Start();
                        Console.WriteLine($"{DateTime.Now.ToString()} Updating elem..");
                        repository.Update(vtm);
                        repository.Save();
                        s.Stop();
                        Console.WriteLine($"{DateTime.Now.ToString()} Updated vehicle type in {s.ElapsedMilliseconds.ToString()}ms");
                        Log?.Invoke($"{DateTime.Now.ToString()} Updated vehicle type {vtm.Description} in {s.ElapsedMilliseconds.ToString()}ms");
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
            }
        }

        public void UpdateExistingFromConsole(List<object>? options=null, IModel? vtma=null)
        {
            try {
                VehicleTypesModel? vtm = (VehicleTypesModel?)vtma;
                if (options == null && vtma == null) {
                    return;
                }
                if (vtma == null)
                {
                    string? str = options?[0].ToString();
                    if(str!=null) {
                        vtm = GetOneVehicleType(Guid.Parse(str));
                    }
                }
                Console.WriteLine($"You have chosen to update the vehicle type {vtm?.Description}");
                Console.WriteLine($"{Environment.NewLine}Please choose another name:");
                string? readline = Console.ReadLine();
                if (readline != null && vtm != null)
                {
                    vtm.Description = readline.ToString();
                }
                if (vtm != null)
                {
                    while (!VerifyName(vtm.Description))
                    {
                        Console.Write($"{Environment.NewLine} This name already exists. Please type in another one.");
                        readline = Console.ReadLine();
                        if (readline != null) {
                            vtm.Description = readline;
                        }
                    }
                }
                Console.WriteLine($"{Environment.NewLine} The previous value of the cubic capacity metric is {vtm?.CCM.ToString()}");
                if(vtm!=null) { vtm.CCM = VerifyAndGetCCM(); };
                Console.WriteLine($"{Environment.NewLine} The previous value of the fuel type is {vtm?.FuelType}");
                if (vtm != null) { vtm.FuelType = VerifyAndGetFuelType(vtm.FuelTypes); }
                Console.WriteLine($"{Environment.NewLine} The previous value of the KiloWatt capacity is {vtm?.KW.ToString()}");
                if (vtm != null)
                {
                    vtm.KW = VerifyAndGetKW();
                }
                Console.WriteLine($"{Environment.NewLine} The previous value of the body type is {vtm?.BodyType}");
                if (vtm != null) { vtm.BodyType = VerifyAndGetBodyType(); }
                Console.WriteLine($"{Environment.NewLine} The previous value of door count is {vtm?.DoorCount.ToString()}");
                if (vtm != null)
                {
                    vtm.DoorCount = VefiryAndGetDoorCount();
                    SaveElem(vtm, 'u');
                }
                
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
            }
        }

        public void DeleteExisitngFromConsole(List<object>? options = null, IModel? model = null)
        {
            try
            {
                VehicleTypesModel? vtm = (VehicleTypesModel?)model;
                if (options != null || model != null)
                {
                    string? str = options[0].ToString();
                    if (str != null)
                    {
                        vtm = GetOneVehicleType(Guid.Parse(str));
                    }
                    Console.WriteLine($"You have chosen to delete the vehicle type {vtm?.Description}. Are you sure?(Y/N)");
                    ConsoleKeyInfo sure = Console.ReadKey();
                    if (sure.KeyChar.ToString().ToLower() == "y")
                    {
                        Console.WriteLine("Deletion Started");
                        if (vtm != null)
                        {
                            DeleteOneVehicleType(vtm.Id);
                            Console.WriteLine("Deletion Completed");
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
            }
        }

        private VehicleTypesModel? GetOneVehicleType(Guid gid)
        {
            try
            {
                using(PiecesContext context = new PiecesContext())
                {
                    Stopwatch s = new Stopwatch();
                    VehicleTypesRepository repository = new VehicleTypesRepository(context);
                    s.Start();
                    VehicleTypesModel? vtm = repository.GetById(gid);
                    s.Stop();
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Got a vehicle type from the database in {s.ElapsedMilliseconds.ToString()}ms");
                    return vtm;
                }
            }catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                return null;
            }
        }

       public void DeleteOneVehicleType(Guid gid)
        {
            try
            {
                using (PiecesContext context = new PiecesContext())
                {
                    VehicleTypesRepository repository = new VehicleTypesRepository(context);
                    Stopwatch s = new Stopwatch();
                    Console.WriteLine($"{DateTime.Now.ToString()} started deleting vehicle type {gid}");
                    s.Start();
                    VehicleTypesModel? vtm = repository.GetById(gid);
                    s.Stop();
                    Log?.Invoke($"Info {DateTime.Now.ToString()} vehicle type getting took {s.ElapsedMilliseconds.ToString()}ms");
                    s.Reset();
                    if (vtm != null)
                    {
                        s.Restart();
                        context.Remove(vtm);
                        context.SaveChanges();
                        s.Stop();
                        Console.WriteLine($"{DateTime.Now.ToString()} deleting took {s.ElapsedMilliseconds.ToString()}ms");
                        Log?.Invoke($"Info {DateTime.Now.ToString()} Deleted a vehicle type from the database in {s.ElapsedMilliseconds.ToString()}ms");
                    } else
                    {
                        Console.WriteLine("Element not found! Not deleted.");
                        Log?.Invoke($"Info: {DateTime.Now.ToString()} vehicle type was not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} Deleting vehile type {ex.Message}");
            }
        }

        
    }
}
