using APIMoiFood.Models.DTOs.Payment;
using APIMoiFood.Models.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace APIMoiFood.Services.PaymentService
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(PaymentRequest request);
        Task<dynamic> ProcessPayment(int paymentId, string? transactionId = null);
    }
    public class PaymentService : IPaymentService
    {
        private readonly MoiFoodDBContext _context;
        private readonly IMapper _mapper;
        public PaymentService(MoiFoodDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<Payment> CreatePaymentAsync(PaymentRequest request)
        {
            var payment = _mapper.Map<Payment>(request);

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<dynamic> ProcessPayment(int paymentId, string? transactionId = null)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .Include(p => p.Method)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null) return new { Message = "Payment not found" };

            if (payment.Method.Name.ToLower() == "cod")
            {
                payment.PaymentStatus = "success";
                payment.TransactionId = Guid.NewGuid().ToString();
                payment.Order.PaymentStatus = "Paid";
                payment.Order.OrderStatus = "Confirmed";
            }
            else
            {
                payment.PaymentStatus = "success";
                payment.TransactionId = transactionId ?? Guid.NewGuid().ToString();
                payment.Order.PaymentStatus = "Paid";
                payment.Order.OrderStatus = "Confirmed";
            }
            payment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return payment;
        }
    }
}
