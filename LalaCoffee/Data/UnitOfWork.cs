using System;
using System.Data.Common;
using System.Data.Entity;

namespace LalaCoffee.Data
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly DbContext _dbContext;
		private DbContextTransaction _transaction;

		public UnitOfWork(DbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public void BeginTransaction()
		{
			_transaction = _dbContext.Database.BeginTransaction();
		}

		public void Commit()
		{
			try
			{
				_dbContext.SaveChanges();
				_transaction?.Commit();
			}
			catch
			{
				RollbackTransaction();
				throw;
			}
			finally
			{
				DisposeTransaction();
			}
		}

		public void Rollback()
		{
			RollbackTransaction();
		}

		public void Dispose()
		{
			DisposeTransaction();
			_dbContext?.Dispose();
		}

		private void RollbackTransaction()
		{
			var transaction = _transaction;
			_transaction = null;
			if (transaction == null)
			{
				return;
			}

			try
			{
				RollbackBestEffort(transaction);
			}
			finally
			{
				transaction.Dispose();
			}
		}

		internal static void RollbackBestEffort(DbContextTransaction transaction)
		{
			try
			{
				transaction.Rollback();
			}
			catch (InvalidOperationException)
			{
			}
			catch (DbException)
			{
			}
			catch (Exception)
			{
			}
		}

		private void DisposeTransaction()
		{
			var transaction = _transaction;
			_transaction = null;
			transaction?.Dispose();
		}
	}

}