using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopmotiveCatalog2023.Models;

namespace TopmotiveCatalog2023.DAL
{
    internal class ProductGroupRepository : Repository<ProductGroupModel>
    {
        public ProductGroupRepository(PiecesContext context) : base(context)
        {

        }

        public IEnumerable<ProductGroupModel> GetByName(String name)
        {
            try
            {
                return _dbSet.Where(x => x.Description == name).ToList();
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Enumerable.Empty<ProductGroupModel>();
            }
        }

        public IEnumerable<Guid>? GetIds()
        {
            try
            {
                return _dbSet.Select(x => x.Id).ToList();
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

    }
}
