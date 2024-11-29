using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace Infrastructures.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        void AddRange(ICollection<T> entity);
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entity);
        void Commit();

        IEnumerable<T> Get(Expression<Func<T, object>>[]? include = null, Expression<Func<T, bool>>? where = null, bool tracked = true);

        public T? GetOne(Expression<Func<T, object>>[]? include = null, Expression<Func<T, bool>>? where = null, bool tracked = true);

        IQueryable<T> ThenInclude<TProperty, TThenProperty>(Expression<Func<T, TProperty>> includeExpression,
                Expression<Func<TProperty, TThenProperty>> thenIncludeExpression);


        void UpdateImage(T entity, IFormFile imageFile, string currentImagePath, string imageFolder, string imageUrlProperty);
        void CreateWithImage(T entity, IFormFile imageFile, string imageFolder, string imageUrlProperty);
        void DeleteWithImage(T entity, string imageFolder, string imageProperty);


    }
}
