using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopmotiveCatalog2023.Models;

namespace TopmotiveCatalog2023.DAL
{
    internal class VehicleTypesRepository: Repository<VehicleTypesModel>
    {
        public VehicleTypesRepository(PiecesContext context) : base(context)
        {

        }

        public IEnumerable<VehicleTypesModel> GetByName(String name)
        {
            try
            {
                return _dbSet.Where(x => x.Description == name).ToList();
            }catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return Enumerable.Empty<VehicleTypesModel>();
            }
        }

        public VehicleTypesModel? GetById(Guid id)
        {
            try
            {
                return _dbSet.Where(x => x.Id == id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return null;
            }
        }

        public int Delete(Guid id)
        {

            try
            {
                return _dbSet.Where(x => x.Id == id).ExecuteDelete();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return -1;
            }
        }

        public IEnumerable<VehicleTypesModel> GetByVehicleModel(Guid id)
        {
            try
            {
                return _dbSet.Where(x => x.VehicleModelId == id).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return Enumerable.Empty<VehicleTypesModel>();
            }
        }


    }
}
