using APIMoiFood.Models.DTOs.Payment;
using APIMoiFood.Models.Entities;
using APIMoiFood.Models.Mapping;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace APIMoiFood.Services.PaymentMethodService
{
    public interface IPaymentMethodService
    {
        Task<dynamic> GetAllPaymentMethods();
        Task<dynamic> GetPaymentMethodById(int methodId);
        Task<dynamic> Modify(int id, PaymentMethodRequest request);
        Task<dynamic> delete(int id);
    }
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly MoiFoodDBContext _context;
        private readonly IMapper _mapper;
        public PaymentMethodService(MoiFoodDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<dynamic> GetAllPaymentMethods()
        {
            var methods = await _context.PaymentMethods
                .OrderBy(m => m.MethodId)
                .Select(m => new
                {
                    m.MethodId,
                    m.Name
                })
                .ToListAsync();
            return methods;
        }
        public async Task<dynamic> GetPaymentMethodById(int methodId)
        {
            var method = await _context.PaymentMethods
                .Where(m => m.MethodId == methodId)
                .Select(m => new
                {
                    m.MethodId,
                    m.Name
                })
                .FirstOrDefaultAsync();
            return method;
        }
        public async Task<dynamic> Modify(int id, PaymentMethodRequest request)
        {
            try
            {
                var data = await _context.PaymentMethods.FindAsync(id);
                if(data != null)
                {
                    data.Name = request.Name;
                    await _context.SaveChangesAsync();
                    return data;
                }   
                else
                {
                    var newMethod = _mapper.Map<PaymentMethod>(request);
                    _context.PaymentMethods.Add(newMethod);
                    await _context.SaveChangesAsync();
                    return newMethod;
                }
            }
            catch (Exception ex)
            {
                return new { Message = ex.Message };
            }
        }
        public async Task<dynamic> delete(int id)
        {
            var query = await _context.PaymentMethods.FindAsync(id); 

            _context.Remove(query!);
            await _context.SaveChangesAsync();

            return new
            {  
                StatusCode = 200,
                Message = "Xoá thành công",
            };
        }
    }
}
