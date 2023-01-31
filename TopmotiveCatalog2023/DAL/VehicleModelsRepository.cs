using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopmotiveCatalog2023.Models;

namespace TopmotiveCatalog2023.DAL
{
    internal class VehicleModelsRepository : Repository<VehicleModelsModel>
    {
        public VehicleModelsRepository(PiecesContext context) : base(context)
        {

        }

        public IEnumerable<VehicleModelsModel> GetByName(String name)
        {
            try
            {
                return _dbSet.Where(x => x.Description == name).ToList();
            }catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return Enumerable.Empty<VehicleModelsModel>();
            }
        }

        public IEnumerable<VehicleModelsModel> GetByManufacturer(Guid id)
        {
            try
            {
                return _dbSet.Where(x => x.ManufacturerId == id).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return Enumerable.Empty<VehicleModelsModel>();
            }
        }

        public IEnumerable<Guid> GetAllIds()
        {
            try
            {
                return _dbSet.Select(x => x.Id).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return Enumerable.Empty<Guid>();
            }
        }
    }
}
