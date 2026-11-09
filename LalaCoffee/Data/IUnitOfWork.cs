using System;

namespace LalaCoffee.Data
{
	public interface IUnitOfWork : IDisposable
	{
		void BeginTransaction();
		void Commit();
		void Rollback();
	}
}
