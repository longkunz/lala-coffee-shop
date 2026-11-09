using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LalaCoffee.Data
{
	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly DbContext _dbContext;
		private readonly DbSet<T> _dbSet;

		public Repository(DbContext dbContext)
		{
			_dbContext = dbContext;
			_dbSet = dbContext.Set<T>();
		}
		public void Add(T entity)
		{
			_dbSet.Add(entity);
		}
		public void Update(T entity)
		{
			_dbSet.Attach(entity);
			_dbContext.Entry(entity).State = EntityState.Modified;

		}
		public void Remove(T entity)
		{
			_dbSet.Attach(entity);
			_dbContext.Entry(entity).State = EntityState.Deleted;
		}
		public T GetById(int id)
		{
			return _dbContext.Set<T>().Find(id);
		}
		public IEnumerable<T> GetAll()
		{
			return _dbSet.ToList();
		}
		public IQueryable<T> GetQueryable()
		{
			return _dbSet.AsQueryable();
		}
	}

}