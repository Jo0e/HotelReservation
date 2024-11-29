using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;
using Infrastructures.Data;
using Microsoft.AspNetCore.Http;
using Infrastructures.Repository.IRepository;

namespace Infrastructures.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected ApplicationDbContext context;
        protected DbSet<T> dbSet;

        public Repository(ApplicationDbContext context)
        {
            this.context = context;
            dbSet = context.Set<T>();
        }


        // Cruds
        public IEnumerable<T> Get(Expression<Func<T, object>>[]? include = null, Expression<Func<T, bool>>? where = null, bool tracked = true)
        {
            IQueryable<T> query = dbSet;

            if (where != null)
            {
                query = query.Where(where);
            }

            if (include != null)
            {
                foreach (var prop in include)
                {
                    query = query.Include(prop);
                }
            }

            if (!tracked)
            {
                query = query.AsNoTracking();
            }

            return query.ToList();
        }

        public T? GetOne(Expression<Func<T, object>>[]? include = null, Expression<Func<T, bool>>? where = null, bool tracked = true)
        {
            return Get(include, where, tracked).FirstOrDefault();
        }

        public IQueryable<T> ThenInclude<TProperty, TThenProperty>(Expression<Func<T, TProperty>> include,
            Expression<Func<TProperty, TThenProperty>> thenInclude)
        {
            return dbSet.Include(include).ThenInclude(thenInclude);
        }


        public void Create(T entity)
        {
            dbSet.Add(entity);
        }

        public void AddRange(ICollection<T> entity)
        {
            dbSet.AddRange(entity);
        }


        public void Update(T entity)
        {
            dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            dbSet.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }


        public void Commit()
        {
            context.SaveChanges();
        }



        // Img Crud
        // CreateWithImage(Hotel, Formfile, "Main imgs", "CoverImg");
        public void CreateWithImage(T entity, IFormFile imageFile, string imageFolder, string imageUrlProperty)
        {
            if (imageFile != null && imageFile.Length > 0)
            {

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\images\\{imageFolder}");

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                var property = typeof(T).GetProperty(imageUrlProperty);
                if (property != null)
                {
                    property.SetValue(entity, fileName);
                }
            }

            dbSet.Add(entity);
            context.SaveChanges();
        }

        public void UpdateImage(T entity, IFormFile imageFile, string currentImagePath, string imageFolder, string imageUrlProperty)
        {
            var entityId = (int)typeof(T).GetProperty("Id").GetValue(entity);
            var oldEntity = dbSet.AsNoTracking().AsEnumerable().FirstOrDefault(e => (int)typeof(T).GetProperty("Id").GetValue(e) == entityId);

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\images\\{imageFolder}", fileName);
                var oldFilePath = oldEntity != null ? Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\images\\{imageFolder}", (string)typeof(T).GetProperty(imageUrlProperty).GetValue(oldEntity)) : "";

                using (var stream = File.Create(filePath))
                {
                    imageFile.CopyTo(stream);
                }

                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }

                var property = typeof(T).GetProperty(imageUrlProperty);
                if (property != null)
                {
                    property.SetValue(entity, fileName);
                }
            }
            else
            {
                var property = typeof(T).GetProperty(imageUrlProperty);
                if (property != null)
                {
                    property.SetValue(entity, oldEntity != null ? (string)property.GetValue(oldEntity) : currentImagePath);
                }
            }

            var trackedEntity = dbSet.Local.FirstOrDefault(e => (int)typeof(T).GetProperty("Id").GetValue(e) == entityId);
            if (trackedEntity != null)
            {
                context.Entry(trackedEntity).State = EntityState.Detached;
            }

            dbSet.Update(entity);
            context.SaveChanges();

        }

        public void DeleteWithImage(T entity, string imageFolder, string imageProperty)
        {
            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\images\\{imageFolder}\\{imageProperty}");
            if (File.Exists(oldFilePath))
            {
                File.Delete(oldFilePath);
            }
            dbSet.Remove(entity);

        }




    }
}
