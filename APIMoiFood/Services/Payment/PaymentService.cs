using APIMoiFood.Models.DTOs.Payment;
using APIMoiFood.Models.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace APIMoiFood.Services.PaymentService
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(PaymentRequest request);
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
    }
}
