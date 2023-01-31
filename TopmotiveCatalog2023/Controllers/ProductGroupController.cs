using Bogus;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopmotiveCatalog2023.DAL;
using TopmotiveCatalog2023.Models;

namespace TopmotiveCatalog2023.Controllers
{
    internal class ProductGroupController : Controller, IFiller
    {
        private string? modelName = "Product Group";
        private string? modelType = "Product Group Model";

        public new string? ModelName { get => modelName; set => modelName = value; }
        public new string? ModelType { get => modelType; set => modelType = value; }

        public ProductGroupController()
        {
            
        }

        public ProductGroupController(Action<String>? log) {
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
                List<System.String>? pgs = ConfigController.getValuesFrom("defaultProductGroups");
                Console.WriteLine($"Found default {pgs?.Count} product groups to insert.");
                ProductGroupModel onepg = new ProductGroupModel();
                Stopwatch s = new Stopwatch();
                if (pgs != null)
                {
                    foreach (String pg in pgs)
                    {
                        using (PiecesContext context = new PiecesContext(Log))
                        {
                            var repository = new ProductGroupRepository(context);
                            onepg.Id = Guid.NewGuid();
                            onepg.Description = pg;
                            s.Restart();
                            repository.Insert(onepg);
                            repository.Save();
                            s.Stop();
                            Console.WriteLine($"Inserted product group {onepg.Description} in {s.ElapsedMilliseconds.ToString()}ms");
                            Log?.Invoke($"Inserted product group {onepg.Description} in {s.ElapsedMilliseconds.ToString()}ms");
                        }
                    }
                    return new { Status = 200, Message = "OK" };
                }
                return new { Status = 404, Message = "There are not default values to pick from." };
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Exception caught: {0}", ex.Message);
                return ex;
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
                    Boolean result = context.ProductGroups.Any();
                    s.Stop();
                    Log?.Invoke($"Info {DateTime.Now.ToString()} Verifying product group table emptiness took {s.ElapsedMilliseconds.ToString()}ms");
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

        public List<ProductGroupModel>? ListAll()
        {
            try
            {
                using (PiecesContext context = new PiecesContext())
                {
                    var repo = new ProductGroupRepository(context);
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    List<ProductGroupModel> all = repo.GetAll().ToList();
                    s.Stop();
                    Log?.Invoke($"Getting all the product groups took {s.ElapsedMilliseconds.ToString()}ms");
                    return all;
                }
            } catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return null;
            }
        }

        Dictionary<Guid, string> IFiller.ListAll()
        {
            throw new NotImplementedException();
        }

        public Dictionary<Guid, string>? GetOption(List<string?>? options = null)
        {
            throw new NotImplementedException();
        }

        public void AddNewFromConsole(List<object?> options)
        {
            throw new NotImplementedException();
        }

        public void DeleteExisitngFromConsole(List<object>? options = null, IModel? model = null)
        {
            throw new NotImplementedException();
        }

        public void UpdateExistingFromConsole(List<object>? options = null, IModel? model = null)
        {
            throw new NotImplementedException();
        }

        public void DeleteExistingFromConsole(List<object>? options = null, IModel? model = null)
        {
            throw new NotImplementedException();
        }
    }
}
