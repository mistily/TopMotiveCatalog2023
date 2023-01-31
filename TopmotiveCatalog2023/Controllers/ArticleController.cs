using Bogus;
using Bogus.DataSets;
using Bogus.Extensions.Italy;
using MediatR.Pipeline;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Nist;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TopmotiveCatalog2023.DAL;
using TopmotiveCatalog2023.Models;

namespace TopmotiveCatalog2023.Controllers
{
    internal class ArticleController : Controller, IFiller
    {
        string? modelName = "Article";
        string? modelType = "Article Model";
        public new string? ModelName { get => modelName; set => modelName=value; }
        public new string? ModelType { get => modelType; set => modelType = value; }

        public delegate string handleInput(string? prevVal);
        public ArticleController() { }

        public ArticleController(Action<System.String>? log)
        {
            if (log != null)
            {
                Log = log;
            }
        }

        public object DoGenerate()
        {
            try
            {
                Random rnd = new();
                int nr = rnd.Next(1000001, 1000500);
                Console.WriteLine($"Prepering to add {nr} articles");
                int batchcount = 0;
                Stopwatch s = new Stopwatch();s.Start();
                List<ProductGroupModel>? productgroups = getProductGroups();
                s.Stop();
                Log?.Invoke($"Info: {DateTime.Now.ToString()} Got a product group list in {s.ElapsedMilliseconds.ToString()}ms!");
                Console.WriteLine("I have gotten a product group list in {s.ElapsedMilliseconds.ToString()}ms!");
                if (productgroups != null)
                {
                    batchcount = (int)Math.Round((decimal)nr / productgroups.Count);
                } else
                { 
                    Log?.Invoke($"Warning: {DateTime.Now.ToString()}There are no products!");
                    Console.WriteLine("Please insert some product groups first!");
                    return new { Status= 404, Message= "Please insert some product groups first!" };
                }
                Console.WriteLine($"Bacth count is {batchcount}");
                foreach (ProductGroupModel productgroup in productgroups)
                {
                    Console.WriteLine($"Generating {batchcount} articles for {productgroup.Description}.");
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Started generating {batchcount} entries for {productgroup.Description}.");
                    Randomizer.Seed = new Random(854565484);
                    if(!generateBatch(batchcount, productgroup))
                    {
                        Log?.Invoke($"Warning: Batch for {productgroup.Description} encountered an exception and was not complete!");
                        Console.WriteLine($"Something went wrong in the generation of a bacth for {productgroup.Description}!");
                    }
                }
                return new { Status = 200, Message = "OK" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Exception caught: {0}", ex.Message);
                Log?.Invoke($"Error:  {DateTime.Now.ToString()} Article generation {ex.Message}!");
                return ex;
            }
        }

        private Boolean generateBatch(int nr, ProductGroupModel pm)
        {
            try
            {
                Boolean ok = true;
                var faker = new Faker("en_GB");
                for (int j = 0; j < nr; j++)
                {
                    Console.WriteLine($"At iteration {j}");
                    Log?.Invoke($"Info:  {DateTime.Now.ToString()} Reached iteration {j} in batch generation!");
                    ArticleModel art = new();
                    Guid ng = Guid.NewGuid();
                    art.Id = switchIfExists(ng);
                    art.Description = getNewDescription(faker, pm.Description) ?? "Not named";
                    art.Price = faker.Random.Float(1, 20000);
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    List<Guid>? mm = getManufacturerIds();
                    s.Stop();
                    Console.WriteLine($"{DateTime.Now.ToString()} Got a brand list in {s.ElapsedMilliseconds.ToString()}ms");
                    Log?.Invoke($"Info:  {DateTime.Now.ToString()}  Got a brand list in {s.ElapsedMilliseconds.ToString()}ms.");
                    art.BrandId = faker.PickRandom(mm);
                    art.ProductGroupId = pm.Id;
                    Console.WriteLine($"Generated new article with name: {art.Description}");
                    Log?.Invoke($"Info:  {DateTime.Now.ToString()} Generated article {art.Description}!");
                    if (!AddToDB(art))
                    {
                        ok = false;
                        Console.WriteLine("A problem occured!");
                        Log?.Invoke($"Warning:  {DateTime.Now.ToString()} batch {j} exited!");
                    }
                }
                return ok;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB ADDing: {ex.Message}");
                Log?.Invoke($"Error:  {DateTime.Now.ToString()} article batch generating {ex.Message}!");
                return false;
            }
        }


        private String? getNewDescription(Faker faker, String desc)
        {
            try
            {
                StringBuilder sb = new(faker.Random.Word(), 255);
                sb.Append(" ");
                sb.Append(desc);
                sb.Append(" ");
                sb.Append(faker.Random.Hash(6).ToUpper());
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error:  {DateTime.Now.ToString()} getting name for article {ex.Message}!");
                return "";
            }
        }

        private Guid switchIfExists(Guid id)
        {
            Guid nid = id;ArticleModel? am = null;
            try
            {
                using (PiecesContext context = new(Log))
                {
                    var repository = new ArticleRepository(context);
                    Console.WriteLine("Repo init");
                    while (am==null)
                    {
                        Stopwatch s = new Stopwatch();
                        s.Start();
                        am = repository.GetByGuid(nid);
                        s.Stop();
                        Console.WriteLine($"Checked the database for existing Guid in {s.ElapsedMilliseconds.ToString()}ms");
                        Log?.Invoke($"Info:  {DateTime.Now.ToString()} Checked the database for existing article Guid in {s.ElapsedMilliseconds.ToString()}ms");
                        if (am!=null)
                        {
                            Console.WriteLine($"generating new Guid, beacuse {nid} was found");
                            Log?.Invoke($"Info:  {DateTime.Now.ToString()} generating new Guid, because previous one was found!");
                            nid = Guid.NewGuid();
                        } else
                        {
                            return nid;
                        }
                    }
                }
                return nid;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error:  {DateTime.Now.ToString()} {ex.Message}!");
                return id;
            }
        }

        private Boolean AddRelationVehicleTypes(ArticleModel art)
        {
            try
            {
                using (PiecesContext context = new(Log))
                {
                    var repository = new VehicleTypesRepository(context);
                    var random = new Random();
                    Stopwatch s = new Stopwatch();
                    List<VehicleTypesModel> vhtps = context.VehicleTypes
                            .AsEnumerable()
                            .OrderBy(x => random.Next())
                            .Take(random.Next(5,10))
                            .ToList();
                    s.Stop();
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Interogated Vehicle Types to get 5-10 random entries in {s.ElapsedMilliseconds.ToString()}ms!");
                    if (vhtps==null)
                    {
                        Log?.Invoke($"Warning: {DateTime.Now.ToString()} There are no vehicle types!");
                    }
                    if (vhtps != null)
                    {
                        foreach (VehicleTypesModel v in vhtps)
                        {
                            VehicleTypesOfArticlesModel vtoam = new();
                            vtoam.Article = art;
                            vtoam.VehicleType = v;
                            try

                            {
                                s.Restart();
                                context.VehicleTypesOfArticles.Add(vtoam);
                                context.SaveChanges();
                                s.Stop();
                                Console.WriteLine($"Added and article and vehicle type entities for articles in {s.ElapsedMilliseconds.ToString()}ms");
                                Log?.Invoke($"Info: {DateTime.Now.ToString()} Added and article and its vehicle type relationship in {s.ElapsedMilliseconds.ToString()}ms.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Save Exception: {ex.Message}");
                                var innerEx = ex.InnerException;
                                if (innerEx != null)
                                {
                                    while (innerEx.InnerException != null)
                                    {
                                        innerEx = innerEx.InnerException;
                                    }
                                    Console.WriteLine("Error: " + innerEx.Message);

                                    Log?.Invoke($"Error: {DateTime.Now.ToString()} Database insert relationship adding error at vehicle type with articles.");
                                }
                            }
                        }
                    }
                }
                return true;
            }catch(Exception ex)
            {
                Console.WriteLine($"Rel: {ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                return false;
            }
        }

        private Boolean AddToDB(ArticleModel art)
        {
            try
            {
                using (PiecesContext context = new(Log))
                {
                    var repository = new ArticleRepository(context);
                    if(!AddRelationVehicleTypes(art))
                    {
                        Console.WriteLine($"Relationship containing insert error!");
                        return false;
                    }
                    return true;
                }
            } catch(Exception ex)
            {
                Console.WriteLine($"DB Adding in Article section: Err: {ex.Message}");
                Log?.Invoke($"Error in DB adding: {DateTime.Now.ToString()} {ex.Message}");
                return false;
            }
        }
        private List<ProductGroupModel>? getProductGroups()
        {
            try
            {
                using (PiecesContext context = new(Log))
                {
                    var repository = new ProductGroupRepository(context);
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    List<ProductGroupModel>? res =  repository.GetAll().ToList();
                    s.Stop();
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Getting a list of {res.Count.ToString()} product groups took {s.ElapsedMilliseconds.ToString()}ms !");
                    return res;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}");
                return null;
            }
        }

        private List<Guid>? getManufacturerIds()
        {
            try
            {
                using (PiecesContext context = new(Log))
                {
                    var repository = new ManufacturerRepository(context);
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    List<Guid>? res = repository.GetAllIds("Car").ToList();
                    s.Stop();
                    Log?.Invoke($"Info {DateTime.Now.ToString()} Got a list of {res.Count} manufacturers in {s.ElapsedMilliseconds.ToString()}ms");
                    return res;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error getting manufacturer ids: {DateTime.Now.ToString()} {ex.Message}");
                return null;
            }
        }

        public bool SeeIfEmpty()
        {
            try
            {
                using (PiecesContext context = new())
                {
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    System.Boolean result = context.Articles.Any();
                    s.Stop();
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Verifying articles table emptiness took {s.ElapsedMilliseconds.ToString()}ms");
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

        public Dictionary<Guid, String> ListAll()
        {
            throw new NotImplementedException();
        }

        public Dictionary<Guid, string>? GetOption(List<string?>? options = null)
        {
            try
            {
                if (options != null && options.Count >= 2)
                {
                    Console.WriteLine($"{options.Count} options: Type: {options[0]} and Group: {options[1]}");
                    Guid key = Guid.Empty; Guid key2 = Guid.Empty;
                    string? strtmp = options[0];
                    if (strtmp != null) key = Guid.Parse(strtmp);
                    strtmp = options[1];
                    if (strtmp != null) key2 = Guid.Parse(strtmp);
                    try
                    {
                        using (PiecesContext context = new(Log))
                        {
                            var repository = new ArticleRepository(context);
                            Stopwatch s = new Stopwatch();
                            Console.WriteLine($"Listing for {key.ToString()} vehicle type and {key2.ToString()} group");
                            s.Start();
                            var query = from a in context.Articles
                                        join avtt in context.VehicleTypesOfArticles on a.Id equals avtt.ArticleId
                                        join vtt in context.VehicleTypes on avtt.VehicleTypeId equals vtt.Id
                                        where vtt.Id == key && a.ProductGroupId == key2
                                        orderby a.Description descending
                                        select new { a.Id, a.Description };
                            var resultlist = query.ToList();
                            Dictionary<Guid, string>? result = query.ToList().Select(elem => new { elem.Id, elem.Description }).ToDictionary(a => a.Id, a => a.Description);
                            s.Stop();
                            Log?.Invoke($"Info:  {DateTime.Now.ToString()} Got a list of {resultlist.Count} articles in {s.ElapsedMilliseconds.ToString()}ms.");
                            Console.WriteLine($"Got a list {resultlist.Count} in {s.ElapsedMilliseconds.ToString()}ms");
                            return result;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log?.Invoke($"{DateTime.Now.ToString()} Error in listing {ex.Message}!");
                        Console.WriteLine($"Article query exception: {ex.Message}");
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("Options are not present!");
                    Log?.Invoke($"{DateTime.Now.ToString()} Options not present!");
                    return null;
                }
            }catch(Exception ex)
            {
                Console.WriteLine($"{ ex.Message }");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}!");
                return null;
            }
        }

       


        public void AddNewFromConsole(List<object?> options)
        {
            try 
            {
                Log?.Invoke($"Info:  {DateTime.Now.ToString()} Adding new article from console!");
                ArticleModel am = new();
                am.Id = VerifyID(am.Id);
                string? strtmp = options[1]?.ToString();
                if (Guid.TryParse(strtmp, out Guid res)) {
                    am.ProductGroupId = res;
                }
                VehicleTypesOfArticlesModel vtoam = new();
                strtmp = options[0]?.ToString();
                if (Guid.TryParse(strtmp, out res))
                {
                    vtoam.ArticleId = am.Id; vtoam.VehicleTypeId = res;
                }
                am.VehicleTypesOfArticles = new List<VehicleTypesOfArticlesModel>();
                am.VehicleTypesOfArticles.Add(vtoam);
                List <handleInput> inputHandles = new List<handleInput>();
                handleInput handler = handlDescription;
                inputHandles.Add(handler);
                handler = handlePrice;
                inputHandles.Add(handler);
                handler = handleBrandId;
                inputHandles.Add(handler);
                for (int i = 0; i < inputHandles.Count; ++i)
                {
                    strtmp = inputHandles[i](null);
                    if (strtmp == "x")
                    {
                        return;
                    }
                    if (i == 0) am.Description = strtmp;
                    if (i == 1) am.Price = float.Parse(strtmp);
                    if (i == 2) am.BrandId = Guid.Parse(strtmp);
                }
                SaveElem(am, 'i');
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}!");
            }
        }

        public void UpdateExistingFromConsole(List<object>? options = null, IModel? amtu = null)
        {
            try
            {
                ArticleModel? am = (ArticleModel?)amtu;
                if (options == null && amtu == null)
                {
                    return;
                }
                string? strtmp = "";
                if (options != null) strtmp = options[0].ToString();
                if (amtu == null && strtmp != null)
                {
                    Guid guid = strtmp!=null ? Guid.Parse(strtmp) : Guid.Empty;
                    am = GetOneArticle(guid); 
                }
                List<handleInput> inputHandles = new List<handleInput>();
                handleInput handler = handlDescription;
                inputHandles.Add(handler);
                handler = handlePrice;
                inputHandles.Add(handler);
                handler = handleBrandId;
                inputHandles.Add(handler);
                for (int i = 0; i < inputHandles.Count; ++i)
                {
                    if (i == 0) strtmp = am?.Description;
                    if (i == 1) strtmp = am?.Price.ToString();
                    if (i == 2) strtmp = am?.BrandId.ToString();
                    strtmp = inputHandles[i](strtmp);
                    if (strtmp == "x")
                    {
                        return;
                    }
                    if (i == 0 && am!=null) am.Description = strtmp;
                    if (i == 1 && am!=null) am.Price = float.Parse(strtmp);
                    if (i == 2 && am!=null) am.BrandId = Guid.Parse(strtmp);
                }
                if (am!=null) SaveElem(am, 'u');
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error: {DateTime.Now.ToString()} {ex.Message}!");
            }
        }


        public void DeleteExisitngFromConsole(List<object>? options = null, IModel? model = null)
        {
            try
            {
                ArticleModel? atm = (ArticleModel?)model;
                if (!(options == null && model == null))
                {
                    if (model == null && options!=null && options[0]!=null)
                    {
                        string? str = options[0].ToString();
                        if (str != null) { atm = GetOneArticle(Guid.Parse(str)); }
                    }
                    Console.WriteLine($"You have chosen to delete the article {atm?.Description}. Are you sure?(Y/N)");
                    ConsoleKeyInfo sure = Console.ReadKey();
                    if (sure.KeyChar.ToString().ToLower() == "y")
                    {
                        Console.WriteLine("Deletion Started");
                        if (atm != null)
                        {
                            int del = DeleteOneArticle(atm.Id);
                            Console.WriteLine("Deletion Completed");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error:  : {DateTime.Now.ToString()} {ex.Message}!");
            }
        }

        


        private Guid VerifyID(Guid id)
        {
            try
            {
                Guid nid = id;
                using (PiecesContext context = new())
                {
                    ArticleRepository repository = new(context);
                    Stopwatch s = new Stopwatch();s.Start();
                    ArticleModel? amn = repository.GetByGuid(nid);
                    s.Stop();
                    Log?.Invoke($"Info:  {DateTime.Now.ToString()} Verified {nid} in {s.ElapsedMilliseconds.ToString()}ms!");
                    while (amn != null)
                    {
                        nid = Guid.NewGuid();
                    }
                    return nid;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error:  {DateTime.Now.ToString()} {ex.Message}!");
                return id;
            }
        }

        private VehicleTypesModel? GetVehicleType(Guid gid)
        {
            try
            {
                using(PiecesContext context = new())
                {
                    VehicleTypesRepository repository= new(context);
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    VehicleTypesModel? vtm = repository.GetById(gid);
                    s.Stop();
                    Log?.Invoke($"Info:  Getting vehicle type {vtm.Description} took {s.ElapsedMilliseconds.ToString()}ms!");
                    return vtm;
                }
            }catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error:  {DateTime.Now.ToString()} Getting vehicle type {ex.Message}!");
                return null;
            }
        }

        private Boolean VerifyName(String name, Guid? gid=null)
        {
            try
            {
                String nn = name.Trim();
                if (nn == String.Empty)
                {
                    return false;
                }
                using (PiecesContext context = new())
                {
                    ArticleRepository repository = new(context);
                    ArticleModel? am = new();
                    if (gid != null)
                    {
                        Stopwatch s = new Stopwatch();
                        s.Start();
                        ArticleModel? amrep = repository.GetByNameExclId(nn, gid);
                        s.Stop();
                        Log?.Invoke($"Info:  {DateTime.Now.ToString()} Found {amrep.Description} article in {s.ElapsedMilliseconds.ToString()}ms!");
                        if (amrep == null)
                        {
                            return true;
                        } else
                        {
                            return false;
                        }
                    } else
                    {
                        Stopwatch s = new Stopwatch();
                        s.Start();
                        List<ArticleModel> res = (List<ArticleModel>)repository.GetByName(nn);
                        s.Stop();
                        Log?.Invoke($"Info:  {DateTime.Now.ToString()} Found {res.Count} articles in {s.ElapsedMilliseconds.ToString()}ms!");
                        if (res.Count == 0)
                       {
                            return true;
                       }
                    }
                    if (am == null)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error:  {DateTime.Now.ToString()} veirfying name {ex.Message}!");
                return false;
            }
        }

        private string VerifyAndGetPrice(String instr)
        {
            try
            {
                float price = 0;
                instr = instr.Trim();
                while(!float.TryParse(instr, out price))
                {
                    Console.WriteLine($"{Environment.NewLine} Please write a readable price!");
                    string? readline = Console.ReadLine();
                    if(readline=="x" || readline=="X")
                    {
                        return "x";
                    }
                    instr = (readline == null)? "0": readline;
                }
                return Math.Abs(price).ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error:  {DateTime.Now.ToString()} getting price {ex.Message}!");
                return "0";
            }
        }


        private List<Guid>? GetModelOptions()
        {
            try
            {
                Log?.Invoke($"Info:  {DateTime.Now.ToString()} getting model options!");
                List<Guid> opts = new List<Guid>();
                using(PiecesContext context = new())
                {
                    ManufacturerRepository repository= new(context);
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    List<ManufacturerModel> res = (List<ManufacturerModel>)repository.GetByExclType("Car");
                    s.Stop();
                    Log?.Invoke($"Info:  {DateTime.Now.ToString()} I have gotten manufacturers brands options {s.ElapsedMilliseconds.ToString()}ms.");
                    for (int i=0;i<res.Count;++i)
                    {
                        opts.Add(res[i].Id);
                        Console.WriteLine($"{i+1}) {res[i].Description}");
                    }
                }
                return opts;
            }catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error:  {DateTime.Now.ToString()} getting model options {ex.Message}!");
                return null;
            }
        }

        private void SaveElem(ArticleModel am, Char optype = 'i')
        {
            try
            {
                using (PiecesContext context = new())
                {
                    ArticleRepository repository = new(context);
                    Stopwatch s = new();
                    if (optype == 'i')
                    {
                        s.Start();
                        Console.WriteLine($"{DateTime.Now.ToString()} Inserting new elem..");
                        repository.Insert(am);
                        repository.Save();
                        s.Stop();
                        Console.WriteLine($"{DateTime.Now.ToString()} Inserted new article in {s.ElapsedMilliseconds.ToString()}ms");
                        Log?.Invoke($"Info:  {DateTime.Now.ToString()} Inserted article {am.Description} took {s.ElapsedMilliseconds.ToString()}ms!");
                    }
                    else if (optype == 'u')
                    {
                        s.Start();
                        Console.WriteLine($"{DateTime.Now.ToString()} Updating elem..");
                        repository.Update(am);
                        repository.Save();
                        s.Stop();
                        Console.WriteLine($"{DateTime.Now.ToString()} Updated elem in {s.ElapsedMilliseconds.ToString()}ms");
                        Log?.Invoke($"Info:  {DateTime.Now.ToString()} Updated article {am.Description} took {s.ElapsedMilliseconds.ToString()}ms!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log?.Invoke($"Error:  {DateTime.Now.ToString()} Saving article {ex.Message}!");
            }
        }

        private ArticleModel? GetOneArticle(Guid gid)
        {
            try
            {
                using (PiecesContext context = new())
                {
                    ArticleRepository repository = new(context);
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    ArticleModel? am = repository.GetByGuid(gid);
                    s.Stop();
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Got one article from database in {s.ElapsedMilliseconds.ToString()}ms");
                    return am;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error:  {DateTime.Now.ToString()} getting an article {ex.Message}!");
                return null;
            }
        }

        private int DeleteOneArticle(Guid gid)
        {
            try
            {
                Log?.Invoke($"Info:  {DateTime.Now.ToString()} deleting one article!");
                using (PiecesContext context = new())
                {
                    ArticleRepository repository = new(context);
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    int res = repository.Delete(gid);
                    s.Stop();
                    Console.WriteLine($"Deletion took {s.ElapsedMilliseconds.ToString()}ms");
                    Log?.Invoke($"Info: {DateTime.Now.ToString()} Deletion of an article took {s.ElapsedMilliseconds.ToString()}ms");
                    return res;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error:  {DateTime.Now.ToString()} deleting one article {ex.Message}!");
                return -1;
            }
        }


        private string handlDescription(string? prevdesc=null)
        {
            try
            {
                if(prevdesc!=null)
                {
                    Console.WriteLine($"You have chosen to update the article {prevdesc}");
                }
                string str = String.Empty;
                Console.Write($"{Environment.NewLine} Please type in an article name:");
                string? strtmp = Console.ReadLine();
                if (strtmp != null)
                {
                    if (strtmp.ToLower() == "x")
                    {
                        return "x";
                    }
                    str = strtmp.ToString().Trim() ?? "Not named";
                }
                while (!VerifyName(str))
                {
                    Console.Write($"{Environment.NewLine} This name already exists. Please type in another one.");
                    strtmp = Console.ReadLine();
                    if (strtmp != null)
                    {
                        str = strtmp.ToString();
                    }
                }
                return str;
            } catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error:  {DateTime.Now.ToString()} handling description {ex.Message}!");
                return String.Empty;
            }
        }


        private string handlePrice(string? prevStr=null)
        {
            try
            {
                if(prevStr!=null)
                {
                    Console.Write($"{Environment.NewLine} The previous price value was {prevStr}");
                }
                Console.Write($"{Environment.NewLine} Please type a price value:");
                string? strtmp = Console.ReadLine();
                if (strtmp == "x" || strtmp == "X")
                {
                    return "x";
                }
                if (strtmp != null)
                {
                    return VerifyAndGetPrice(strtmp.ToString()).ToString();
                }
                return "0";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error:  {DateTime.Now.ToString()} handling price {ex.Message}!");
                return "0";
            }
        }


        private String handleBrandId(string? prevVal=null)
        {
            try
            {
                if(prevVal!=null)
                {
                    Console.Write($"{Environment.NewLine} The previous price value was {prevVal}");
                }
                Console.Write($"{Environment.NewLine} Please select a brand from the list:{Environment.NewLine}");
                List<Guid>? brands = GetModelOptions();
                ushort bkk = 0;
                string? strtmp = Console.ReadLine();
                if(strtmp=="x" || strtmp=="X")
                {
                    return "x";
                }
                if (strtmp != null)
                {
                    String brandkey = strtmp.ToString();
                    int cnr = (brands == null) ? 0 : brands.Count;
                    while (!(ushort.TryParse(brandkey, out bkk) && bkk > 0 && bkk <= cnr))
                    {
                        Console.Write($"{Environment.NewLine} This is not a brand index.");
                        strtmp = Console.ReadLine();
                        if(strtmp=="x" || strtmp=="X")
                        {
                            return "x";
                        }
                        if (strtmp != null)
                        {
                            brandkey = strtmp.ToString();
                        }
                    }
                    if (brands != null)
                    {
                        return brands.ElementAt(bkk - 1).ToString();
                    }
                }
                return String.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Log?.Invoke($"Error:  {DateTime.Now.ToString()} handling brand id {ex.Message}!");
                return String.Empty;
            }
        }

    }
}
