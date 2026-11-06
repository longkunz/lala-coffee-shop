using System;
using System.Collections.Generic;

namespace LalaCoffee.Models
{
    public static class OrderStatus
    {
        // Canonical Business Statuses
        public const string PendingConfirmation = "Chờ xác nhận";
        public const string Preparing = "Đang chuẩn bị";
        public const string Delivering = "Đang giao";
        public const string Completed = "Hoàn tất";
        public const string Cancelled = "Đã hủy";

        public static readonly IReadOnlyList<string> All = new List<string>
        {
            PendingConfirmation,
            Preparing,
            Delivering,
            Completed,
            Cancelled
        };

        // Payment status constants
        public const int Unpaid = 0;
        public const int Paid = 1;

        public const string UnpaidText = "Chưa thanh toán";
        public const string PaidText = "Đã thanh toán";

        public static string NormalizeStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return PendingConfirmation;
            }

            var trimmed = status.Trim();

            if (trimmed.Equals(PendingConfirmation, StringComparison.OrdinalIgnoreCase)) return PendingConfirmation;
            if (trimmed.Equals(Preparing, StringComparison.OrdinalIgnoreCase)) return Preparing;
            if (trimmed.Equals(Delivering, StringComparison.OrdinalIgnoreCase)) return Delivering;
            if (trimmed.Equals(Completed, StringComparison.OrdinalIgnoreCase)) return Completed;
            if (trimmed.Equals(Cancelled, StringComparison.OrdinalIgnoreCase)) return Cancelled;

            var lower = trimmed.ToLowerInvariant();
            switch (lower)
            {
                case "pending":
                case "pendingconfirmation":
                case "cho xac nhan":
                case "choxacnhan":
                case "chờ duyệt":
                case "confirmed":
                case "1":
                    return PendingConfirmation;

                case "preparing":
                case "processing":
                case "dang chuan bi":
                case "dangchuanbi":
                case "pha che":
                case "đang pha chế":
                    return Preparing;

                case "delivering":
                case "shipping":
                case "dang giao":
                case "danggiao":
                case "dang van chuyen":
                    return Delivering;

                case "completed":
                case "finished":
                case "done":
                case "hoan tat":
                case "hoantat":
                case "da giao":
                case "đã giao":
                case "paid":
                    return Completed;

                case "cancelled":
                case "canceled":
                case "da huy":
                case "dahuy":
                case "huy":
                case "hủy":
                case "0":
                    return Cancelled;

                default:
                    return trimmed;
            }
        }

        public static string GetStatusBadge(string status)
        {
            var normalized = NormalizeStatus(status);
            switch (normalized)
            {
                case PendingConfirmation:
                    return "<span class='badge badge-warning text-dark p-2' style='font-size:12px;'><i class='fas fa-clock mr-1'></i> Chờ xác nhận</span>";
                case Preparing:
                    return "<span class='badge badge-info p-2' style='font-size:12px;'><i class='fas fa-coffee mr-1'></i> Đang chuẩn bị</span>";
                case Delivering:
                    return "<span class='badge badge-primary p-2' style='font-size:12px;'><i class='fas fa-motorcycle mr-1'></i> Đang giao</span>";
                case Completed:
                    return "<span class='badge badge-success p-2' style='font-size:12px;'><i class='fas fa-check-circle mr-1'></i> Hoàn tất</span>";
                case Cancelled:
                    return "<span class='badge badge-secondary p-2' style='font-size:12px;'><i class='fas fa-times-circle mr-1'></i> Đã hủy</span>";
                default:
                    return $"<span class='badge badge-light p-2' style='font-size:12px;'>{normalized}</span>";
            }
        }

        public static string GetPaidBadge(int paid)
        {
            if (paid == Paid)
            {
                return "<span class='badge badge-success p-2' style='font-size:12px;'><i class='fas fa-check-circle mr-1'></i> " + PaidText + "</span>";
            }
            return "<span class='badge badge-danger p-2' style='font-size:12px;'><i class='fas fa-exclamation-circle mr-1'></i> " + UnpaidText + "</span>";
        }
    }
}
