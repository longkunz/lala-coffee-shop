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
                RollbackBestEffort(transaction);
            }
            finally
            {
                transaction.Dispose();
            }
        }

        // Rollback là best-effort, dùng chung cho mọi chỗ cần rollback thủ công
        // (UnitOfWork và ManageUserController.ResetPassword): nếu Commit() thất
        // bại thì transaction phía SQL Server thường đã hoàn tất/bị rollback, gọi
        // Rollback() lúc này có thể ném InvalidOperationException (che mất lỗi
        // gốc); tương tự, Rollback() mà ném (mất kết nối) sẽ biến lỗi validation
        // lẽ ra hiển thị lại trong View thành 500. Bỏ qua lỗi của Rollback() để
        // lỗi gốc / thông báo lỗi luôn tới được người dùng.
        internal static void RollbackBestEffort(DbContextTransaction transaction)
        {
            try
            {
                transaction.Rollback();
            }
            catch (InvalidOperationException)
            {
                // Transaction đã commit/rollback, dispose, hoặc hoàn tất trước đó.
                // (ObjectDisposedException kế thừa InvalidOperationException nên
                // đã được nhánh này bao phủ; không bắt riêng để tránh CS0160.)
            }
            catch (DbException)
            {
                // Mất kết nối giữa chừng, transaction bị server abort, ...:
                // rollback không thực hiện được, nhưng lỗi gốc / thông báo lỗi
                // mới là thứ cần tới được người dùng.
            }
            catch (Exception)
            {
                // Rollback là best-effort: mọi lỗi rollback khác cũng không được
                // biến lỗi validation lẽ ra hiển thị lại trong View thành 500
                // hay che mất exception gốc.
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