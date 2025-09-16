using APIMoiFood.Models.DTOs.Payment;
using APIMoiFood.Models.Entities;
using APIMoiFood.Services.PaymentService.MomoService;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace APIMoiFood.Services.PaymentService
{
    public interface IPaymentService
    {
        Task<PaymentResult> CreatePaymentAsync(PaymentRequest request);
        Task HandleMomoIpnAsync(MomoIpnRequest request);
    }

    public class PaymentService : IPaymentService
    {
        private readonly MoiFoodDBContext _context;
        private readonly IMapper _mapper;
        private readonly IMomoService _momo;

        public PaymentService(MoiFoodDBContext context, IMapper mapper, IMomoService momo)
        {
            _context = context;
            _mapper = mapper;
            _momo = momo;
        }

        public async Task<PaymentResult> CreatePaymentAsync(PaymentRequest request)
        {
            var payment = new Payment
            {
                OrderId = request.OrderId,
                MethodId = request.MethodId,
                Amount = request.Amount,
                PaymentStatus = "pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Nếu là MoMo
            if (request.MethodId == 1)
            {
                var payUrl = await _momo.CreatePayment(
                    amount: request.Amount,
                    orderInfo: $"Thanh toán đơn {request.OrderId}",
                    returnUrl: request.ReturnUrl,
                    notifyUrl: request.NotifyUrl
                );

                return new PaymentResult
                {
                    PaymentId = payment.PaymentId,
                    PayUrl = payUrl
                };
            }

            // COD thì không cần payUrl
            return new PaymentResult
            {
                PaymentId = payment.PaymentId
            };
        }

        // Xử lý IPN MoMo để cập nhật TransactionId + trạng thái
        public async Task HandleMomoIpnAsync(MomoIpnRequest req)
        {
            if (req.ResultCode == 0)
            {
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.OrderId == int.Parse(req.OrderId));

                if (payment != null)
                {
                    payment.TransactionId = req.TransId;
                    payment.PaymentStatus = "paid";
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }

    public class PaymentResult
    {
        public int PaymentId { get; set; }
        public string PayUrl { get; set; }  // null nếu COD
    }
    public class MomoIpnRequest
    {
        public string PartnerCode { get; set; }     // Mã đối tác
        public string OrderId { get; set; }         // Mã đơn hàng của bạn
        public string RequestId { get; set; }       // ID request gửi lên MoMo
        public long Amount { get; set; }            // Số tiền thanh toán
        public string OrderInfo { get; set; }       // Thông tin đơn
        public string OrderType { get; set; }       // Loại đơn
        public string TransId { get; set; }         // Mã giao dịch MoMo
        public int ResultCode { get; set; }         // 0 = thành công
        public string Message { get; set; }         // Mô tả
        public string PayType { get; set; }         // Loại thanh toán
        public long ResponseTime { get; set; }      // Unix ms
        public string ExtraData { get; set; }       // Dữ liệu thêm
        public string Signature { get; set; }       // Chữ ký để verify
    }
}
