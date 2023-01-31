using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TopmotiveCatalog2023.Models;

namespace TopmotiveCatalog2023.DAL
{
    internal class ManufacturerRepository : Repository<ManufacturerModel>
    {
        public ManufacturerRepository(PiecesContext context) : base(context) { 
           
        }

        public IEnumerable<ManufacturerModel> GetByName(String name)
        {
            try
            {
                return _dbSet.Where(x => x.Description == name).ToList();
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Enumerable.Empty<ManufacturerModel>();
            }
        }

        public IEnumerable<ManufacturerModel> GetByType(String type)
        {
            try
            {
                return _dbSet.Where(x => x.Type == type).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Enumerable.Empty<ManufacturerModel>();
            }
        }

        public IEnumerable<ManufacturerModel> GetByExclType(String type)
        {
            try
            {
                return _dbSet.Where(x => x.Type != type).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Enumerable.Empty<ManufacturerModel>();
            }
        }

        public IEnumerable<Guid> GetAllIds(String typeex)
        {
            try
            {
                return _dbSet.Where(x => x.Type != typeex).Select(x => x.Id).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Enumerable.Empty<Guid>();
            }
        }
    }
}
