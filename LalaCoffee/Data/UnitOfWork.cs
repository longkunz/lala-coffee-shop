using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Web;

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
                // Rollback ở đây đã dispose transaction; BaseController.Remove() trong
                // catch còn gọi Rollback() lần nữa — lần thứ hai phải là no-op, không
                // được ném exception mới che mất DbUpdateException gốc (để nhánh FK 547
                // ở TryRemove bắt đúng). RollbackTransaction() lo việc đó.
                RollbackTransaction();
                throw;
            }
            finally
            {
                DisposeTransaction();
            }
            //_transaction.Commit();
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

        // Rollback + dispose transaction rồi null trường, để mọi lần gọi sau đều no-op.
        // Mọi lỗi trong lúc rollback đều được bỏ qua: ngoài transaction đã hoàn tất/
        // dispose, Rollback() còn có thể ném DbException (vd SqlException khi mất kết
        // nối giữa chừng, hoặc transaction bị server abort như deadlock-victim). Lỗi
        // gốc (nếu có) mới là lỗi cần propagate; Rollback() gọi riêng thì không có
        // lỗi gốc. Rollback là best-effort, không bao giờ được che mất lỗi gốc.
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
                transaction.Rollback();
            }
            catch (InvalidOperationException)
            {
                // Transaction đã commit/rollback trước đó.
            }
            catch (ObjectDisposedException)
            {
                // Transaction đã dispose trước đó.
            }
            catch (DbException)
            {
                // Mất kết nối giữa chừng, transaction bị server abort, ...: rollback
                // không thực hiện được, nhưng lỗi gốc (nếu có) mới là lỗi cần báo.
            }
            catch (Exception)
            {
                // Rollback là best-effort: mọi lỗi rollback khác cũng không được che
                // mất DbUpdateException gốc trong Commit() hay làm TryRemove lệch
                // khỏi nhánh FK 547 trong BaseController.Remove().
            }
            finally
            {
                transaction.Dispose();
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