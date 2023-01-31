using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TopmotiveCatalog2023.DAL;
using Pomelo.EntityFrameworkCore.MySql;

namespace TopmotiveCatalog2023.DAL
{ 
    class Repository<T> : IRepository<T> where T : class
    {
      //  private  DbContext _context;
        protected DbSet<T> _dbSet;
        private PiecesContext context;

       /* public Repository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }*/


        public Repository(PiecesContext ncontext)
        {
            try
            {
                context = ncontext;
                _dbSet = context.Set<T>();
            }catch(Exception ex)
            {
                Console.WriteLine($"Init constructor exception: {ex.Message}");
            }
        }


        public IEnumerable<T> GetAll()
        {
            try
            {
                return _dbSet.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Listing: {ex.Message}");
                var innerEx = ex.InnerException;
                while (innerEx?.InnerException != null)
                {
                    innerEx = innerEx.InnerException;
                }
                Console.WriteLine("Error: " + innerEx?.Message);
                return Enumerable.Empty<T>();
            }
        }

        public T GetById(int id)
        {
            try
            {
                return _dbSet.Find(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get one by id(int) error: {ex.Message}");
                var innerEx = ex.InnerException;
                while (innerEx?.InnerException != null)
                {
                    innerEx = innerEx.InnerException;
                }
                Console.WriteLine("Error: " + innerEx?.Message);
                object o = new object();
                return (T)o;
            }
        }

        public void Insert(T entity)
        {
            try
            {
                _dbSet.Add(entity);
            } catch (Exception ex){
                Console.WriteLine($"Insert Exception: {ex.Message}");
                var innerEx = ex.InnerException;
                while (innerEx?.InnerException != null)
                {
                    innerEx = innerEx.InnerException;
                }
                Console.WriteLine("Error: " + innerEx?.Message);
            }
        }

        public void Update(T entity)
        {
            try
            {
                _dbSet.Attach(entity);
                context.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update Exception: {ex.Message}");
                var innerEx = ex.InnerException;
                while (innerEx?.InnerException != null)
                {
                    innerEx = innerEx.InnerException;
                }
                Console.WriteLine("Error: " + innerEx?.Message);
            }
        }

        public void Delete(int id)
        {
            try
            {
                var entity = _dbSet.Find(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete Exception: {ex.Message}");
                var innerEx = ex.InnerException;
                while (innerEx?.InnerException != null)
                {
                    innerEx = innerEx.InnerException;
                }
                Console.WriteLine("Error: " + innerEx?.Message);
            }
        }

        public void Save()
        {
            try { 
             context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save Exception: {ex.Message}");
                var innerEx = ex.InnerException;
                while (innerEx?.InnerException != null)
                {
                    innerEx = innerEx.InnerException;
                }
                Console.WriteLine("Error: " + innerEx?.Message);
            }
        }
    }
}