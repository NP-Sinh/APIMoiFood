using APIMoiFood.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace APIMoiFood.Services.Statistics.StatisticsService
{
    public interface IStatisticsService
    {
        // Thống kê doanh thu theo ngày, tuần, tháng, năm
        Task<dynamic> GetRevenueAsync(DateTime? fromDate, DateTime? toDate, string groupBy);
        // Thống kê số lượng đơn hàng theo ngày, tuần, tháng, năm
        Task<dynamic> GetOrderCountAsync(DateTime? fromDate, DateTime? toDate, string groupBy);
        // Thống kê món ăn được đặt nhiều nhất, ít nhất
        Task<dynamic> GetFoodOrderStatsAsync(int top, DateTime? fromDate, DateTime? toDate);
        // Thống kê chi tiêu của người dùng theo ngày, tuần, tháng, năm
        Task<dynamic> GetUserSpendingAsync(int userId, DateTime? fromDate, DateTime? toDate, string groupBy);
    }
    public class StatisticsService : IStatisticsService
    {
        private readonly MoiFoodDBContext _context;
        public StatisticsService(MoiFoodDBContext context)
        {
            _context = context;
        }
        public async Task<dynamic> GetRevenueAsync(DateTime? fromDate, DateTime? toDate, string groupBy)
        {
            var start = fromDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = (toDate?.Date.AddDays(1).AddTicks(-1)) ?? DateTime.UtcNow;


            var query = await _context.Orders
                .Where(o => o.PaymentStatus == "Paid" &&
                            o.CreatedAt >= start &&
                            o.CreatedAt <= end)
                .ToListAsync();

            object data;

            switch (groupBy.ToLower())
            {
                case "day":
                    data = query
                        .GroupBy(o => o.CreatedAt!.Value.Date)
                        .Select(g => new
                        {
                            Period = g.Key.ToString("dd-MM-yyyy"),
                            TotalRevenue = g.Sum(x => x.TotalAmount)
                        })
                        .OrderBy(x => x.Period)
                        .ToList();
                    break;

                case "week":
                    data = query
                        .GroupBy(o =>
                            System.Globalization.CultureInfo.InvariantCulture.Calendar
                                .GetWeekOfYear(o.CreatedAt!.Value,
                                    System.Globalization.CalendarWeekRule.FirstDay,
                                    DayOfWeek.Monday))
                        .Select(g => new
                        {
                            Period = $"Week {g.Key}",
                            TotalRevenue = g.Sum(x => x.TotalAmount)
                        })
                        .OrderBy(x => x.Period)
                        .ToList();
                    break;

                case "month":
                    data = query
                        .GroupBy(o => new { o.CreatedAt!.Value.Year, o.CreatedAt!.Value.Month })
                        .Select(g => new
                        {
                            Period = $"{g.Key.Month:D2}-{g.Key.Year}",
                            TotalRevenue = g.Sum(x => x.TotalAmount)
                        })
                        .OrderBy(x => x.Period)

                        .ToList();
                    break;

                case "year":
                    data = query
                        .GroupBy(o => o.CreatedAt!.Value.Year)
                        .Select(g => new
                        {
                            Period = g.Key.ToString(),
                            TotalRevenue = g.Sum(x => x.TotalAmount)
                        })
                        .OrderBy(x => x.Period)
                        .ToList();
                    break;

                default:
                    throw new ArgumentException("Invalid groupBy value. Use 'day', 'week', 'month', or 'year'.");
            }

            var grandTotal = ((IEnumerable<dynamic>)data).Sum(x => (decimal)x.TotalRevenue);

            return new
            {
                GroupBy = groupBy,
                GrandTotal = grandTotal,
                Data = data
            };
        }
        public async Task<dynamic> GetOrderCountAsync(DateTime? fromDate, DateTime? toDate, string groupBy)
        {
            var start = fromDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = (toDate?.Date.AddDays(1).AddTicks(-1)) ?? DateTime.UtcNow;

            var query = await _context.Orders
                .Where(o => o.CreatedAt >= start &&
                            o.CreatedAt <= end)
                .ToListAsync();

            object data;
            switch (groupBy.ToLower())
            {
                case "day":
                    data = query
                        .GroupBy(o => o.CreatedAt!.Value.Date)
                        .Select(g => new
                        {
                            PeriodicTimer = g.Key.ToString("dd-MM-yyyy"),
                            OrderCount = g.Count()
                        })
                        .OrderBy(x => x.PeriodicTimer)
                        .ToList();
                    break;

                case "week":
                    data = query
                        .GroupBy(o =>
                            System.Globalization.CultureInfo.InvariantCulture.Calendar
                                .GetWeekOfYear(o.CreatedAt!.Value,
                                    System.Globalization.CalendarWeekRule.FirstDay,
                                    DayOfWeek.Monday))
                        .Select(g => new
                        {
                            PeriodicTimer = $"Week {g.Key}",
                            OrderCount = g.Count()
                        })
                        .OrderBy(x => x.PeriodicTimer)
                        .ToList();
                    break;

                case "month":
                    data = query
                        .GroupBy(o => new { o.CreatedAt!.Value.Year, o.CreatedAt!.Value.Month })
                        .Select(g => new
                        {
                            PeriodicTimer = $"{g.Key.Month:D2}-{g.Key.Year}",
                            OrderCount = g.Count()
                        })
                        .OrderBy(x => x.PeriodicTimer)
                        .ToList();
                    break;

                case "year":
                    data = query
                        .GroupBy(o => o.CreatedAt!.Value.Year)
                        .Select(g => new
                        {
                            PeriodicTimer = g.Key.ToString(),
                            OrderCount = g.Count()
                        })
                        .OrderBy(x => x.PeriodicTimer)
                        .ToList();
                    break;

                default:
                    throw new ArgumentException("Invalid groupBy value. Use 'day', 'week', 'month', or 'year'.");

            }
            var totalOrders = ((IEnumerable<dynamic>)data).Sum(x => (int)x.OrderCount);
            return new
            {
                GroupBy = groupBy,
                TotalOrders = totalOrders,
                Data = data
            };

        }
        public async Task<dynamic> GetFoodOrderStatsAsync(int top, DateTime? fromDate, DateTime? toDate)
        {
            var start = fromDate ?? DateTime.MinValue;
            var end = toDate ?? DateTime.MaxValue;

            var query = _context.OrderItems
                .Where(oi => oi.Order.PaymentStatus == "Paid" &&
                             oi.Order.CreatedAt >= start &&
                             oi.Order.CreatedAt <= end);

           var grouped = await query
                .GroupBy(oi => new 
                {
                    oi.FoodId, 
                    oi.Food!.Name
                })
                .Select(g => new
                {
                    g.Key.FoodId,
                    FoodName = g.Key.Name,
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalOrders = g.Select(oi => oi.OrderId).Distinct().Count(),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.Price)
                })
                .ToListAsync();

            var mostOrdered = grouped
                .OrderByDescending(x => x.TotalQuantity)
                .Take(top)
                .ToList();

            var leastOrdered = grouped
                .OrderBy(x => x.TotalQuantity)
                .Take(top)
                .ToList();

            return new
            {
                FromDate = start,
                ToDate = end,
                Top = top,
                MostOrdered = mostOrdered,
                LeastOrdered = leastOrdered
            };    
        }
        public async Task<dynamic> GetUserSpendingAsync(int userId, DateTime? fromDate, DateTime? toDate, string groupBy)
        {
            var start = fromDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = (toDate?.Date.AddDays(1).AddTicks(-1)) ?? DateTime.UtcNow;
            var query = await _context.Orders
                .Where(o => o.UserId == userId &&
                            o.PaymentStatus == "Paid" &&
                            o.CreatedAt >= start &&
                            o.CreatedAt <= end)
                .ToListAsync();
            object data;
            switch (groupBy.ToLower())
            {
                case "day":
                    data = query
                        .GroupBy(o => o.CreatedAt!.Value.Date)
                        .Select(g => new
                        {
                            Period = g.Key.ToString("dd-MM-yyyy"),
                            TotalSpending = g.Sum(x => x.TotalAmount)
                        })
                        .OrderBy(x => x.Period)
                        .ToList();
                    break;
                case "week":
                    data = query
                        .GroupBy(o =>
                            System.Globalization.CultureInfo.InvariantCulture.Calendar
                                .GetWeekOfYear(o.CreatedAt!.Value,
                                    System.Globalization.CalendarWeekRule.FirstDay,
                                    DayOfWeek.Monday))
                        .Select(g => new
                        {
                            Period = $"Week {g.Key}",
                            TotalSpending = g.Sum(x => x.TotalAmount)
                        })
                        .OrderBy(x => x.Period)
                        .ToList();
                    break;
                case "month":
                    data = query
                        .GroupBy(o => new { o.CreatedAt!.Value.Year, o.CreatedAt!.Value.Month })
                        .Select(g => new
                        {
                            Period = $"{g.Key.Month:D2}-{g.Key.Year}",
                            TotalSpending = g.Sum(x => x.TotalAmount)
                        })
                        .OrderBy(x => x.Period)
                        .ToList();
                    break;
                case "year":
                    data = query
                        .GroupBy(o => o.CreatedAt!.Value.Year)
                        .Select(g => new
                        {
                            Period = g.Key.ToString(),
                            TotalSpending = g.Sum(x => x.TotalAmount)
                        })
                        .OrderBy(x => x.Period)
                        .ToList();
                    break;
                default:
                    throw new ArgumentException("Invalid groupBy value. Use 'day', 'week', 'month', or 'year'.");

            }
            var totalSpending = ((IEnumerable<dynamic>)data).Sum(x => (decimal)x.TotalSpending);

            return new
            {
                UserId = userId,
                GroupBy = groupBy,
                TotalSpending = totalSpending,
                Data = data

            };
        }
    }
}
