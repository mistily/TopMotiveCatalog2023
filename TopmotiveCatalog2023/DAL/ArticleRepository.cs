using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopmotiveCatalog2023.Models;

namespace TopmotiveCatalog2023.DAL
{
    internal class ArticleRepository : Repository<ArticleModel>
    {
        public ArticleRepository(PiecesContext context) : base(context)
        {

        }

        public IEnumerable<ArticleModel> GetByName(String name)
        {
            try
            {
                return _dbSet.Where(x => x.Description == name).ToList();
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Enumerable.Empty<ArticleModel>();
            }
        }

        public ArticleModel? GetByGuid(Guid name)
        {
            try
            {
                return _dbSet.Where(x => x.Id == name).FirstOrDefault();
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
}

        public ArticleModel? GetByNameExclId(String name, Guid? gid) {
            try { 
            return _dbSet.Where(x => x.Id != gid && x.Description==name).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public int Delete(Guid id)
        {
            try { 
                return _dbSet.Where(x => x.Id == id).ExecuteDelete();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }


    }
}
