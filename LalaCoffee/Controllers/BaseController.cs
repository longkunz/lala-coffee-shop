using LalaCoffee.Data;
using LalaCoffee.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace LalaCoffee.Controllers
{
	public class BaseController<T> : Controller where T : class
	{
		public readonly ApplicationDbContext Context;
		public readonly IUnitOfWork UnitOfWork;
		public readonly IRepository<T> Repository;
		public UserManager<ApplicationUser> UserManager;
		public RoleManager<IdentityRole> RoleManager;
		public BaseController()
		{
			Context = new ApplicationDbContext();
			UnitOfWork = new UnitOfWork(Context);
			Repository = new Repository<T>(Context);
			UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>());
			RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>());
		}

		public void Add(T entity)
		{
			Repository.Add(entity);
			UnitOfWork.BeginTransaction();
			try
			{
				UnitOfWork.Commit();
			}
			catch
			{
				UnitOfWork.Rollback();
				throw;
			}
		}
		public IEnumerable<T> GetAll()
		{
			return Repository.GetAll();
		}

		public IQueryable<T> GetQueryable()
		{
			return Repository.GetQueryable();
		}

		public T GetById(int Id)
		{
			return Repository.GetById(Id);
		}

		public void Update(T entity)
		{
			Repository.Update(entity);
			UnitOfWork.BeginTransaction();
			try
			{
				UnitOfWork.Commit();
			}
			catch
			{
				UnitOfWork.Rollback();
				throw;
			}
		}

		public void Remove(T entity)
		{
			Repository.Remove(entity);
			UnitOfWork.BeginTransaction();
			try
			{
				UnitOfWork.Commit();
			}
			catch
			{
				UnitOfWork.Rollback();
				throw;
			}
		}

		private static bool IsForeignKeyViolation(Exception ex)
		{
			for (var inner = ex; inner != null; inner = inner.InnerException)
			{
				var sqlException = inner as SqlException;
				if (sqlException != null && sqlException.Number == 547)
				{
					return true;
				}
			}
			return false;
		}

		protected JsonResult TryRemove(T entity, string successMessage, string relatedDataMessage)
		{
			try
			{
				Remove(entity);
			}
			catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
			{
				Trace.TraceError("TryRemove failed (related data): " + ex);
				return Json(new { success = false, message = relatedDataMessage });
			}
			catch (Exception ex)
			{
				Trace.TraceError("TryRemove failed: " + ex);
				return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa. Vui lòng thử lại sau." });
			}
			return Json(new { success = true, message = successMessage });
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Context?.Dispose();
				UnitOfWork?.Dispose();
				UserManager?.Dispose();
				RoleManager?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
