using System.Collections.Generic;
using System.Linq;

namespace LalaCoffee.Data
{
	public interface IRepository<T> where T : class
	{
		void Add(T entity);
		void Update(T entity);
		void Remove(T entity);
		T GetById(int id);
		IEnumerable<T> GetAll();
		IQueryable<T> GetQueryable();
	}

}
