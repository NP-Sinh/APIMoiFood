using APIMoiFood.Models.DTOs.Payment;
using APIMoiFood.Models.Entities;
using APIMoiFood.Services.PaymentService.MomoService;
using APIMoiFood.Services.PaymentService.VnpayService;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace APIMoiFood.Services.PaymentService
{
    public interface IPaymentService
    {
        Task<PaymentResult> CreatePaymentAsync(PaymentRequest request);
        Task HandleMomoIpnAsync(MomoIpnRequest request);
        Task<dynamic> HandleVNPAYReturnAsync(IQueryCollection query);
    }

    public class PaymentService : IPaymentService
    {
        private readonly MoiFoodDBContext _context;
        private readonly IMapper _mapper;
        private readonly IMomoService _momo;
        private readonly IVnpayService _vnpay;


        public PaymentService(MoiFoodDBContext context, IMapper mapper, IMomoService momo, IVnpayService vnpayService)
        {
            _context = context;
            _mapper = mapper;
            _momo = momo;
            _vnpay = vnpayService;
        }

        public async Task<PaymentResult> CreatePaymentAsync(PaymentRequest request)
        {
            var payment = _mapper.Map<Payment>(request);

            // Nếu là MoMo
            if (request.MethodId == 1)
            {
                var payUrl = await _momo.CreatePayment(
                    amount: request.Amount,
                    orderInfo: $"ThanhToanDon{request.OrderId}",
                    returnUrl: request.ReturnUrl,
                    notifyUrl: request.NotifyUrl
                );

                return new PaymentResult
                {
                    PaymentId = payment.PaymentId,
                    PayUrl = payUrl
                };
            }
            // VNPAY
            if (request.MethodId == 3)
            {
                var payUrl = _vnpay.PaymentVNPAY(new VnPaymentRequest
                {
                    Amount = request.Amount,
                    OrderInfo = $"ThanhToanDon{request.OrderId}",
                    OrderId = request.OrderId,
                    IpAddress = "127.0.0.1",
                    ReturnUrl = request.ReturnUrl
                });

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
                    payment.PaymentStatus = "Success";
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<dynamic> HandleVNPAYReturnAsync(IQueryCollection query)
        {
            if (!_vnpay.ValidateSignature(query))
                throw new Exception("Chữ ký không hợp lệ");

            var resp = _vnpay.ParseResponse(query);

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == int.Parse(resp.TxnRef));
            if (payment != null)
            {
                if (resp.IsSuccess)
                    payment.PaymentStatus = "Success";
                else
                    payment.PaymentStatus = "Failed";

                payment.TransactionId = resp.TransactionNo;
                payment.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Update Order
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == payment.OrderId);
                if (order != null)
                {
                    order.PaymentStatus = resp.IsSuccess ? "Paid" : "Failed";
                    order.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            return resp;
        }
    }

    public class PaymentResult
    {
        public int PaymentId { get; set; }
        public string PayUrl { get; set; }  // null nếu COD
    }
    public class VnpayReturnResult
    {
        public bool IsSuccess { get; set; }
        public string OrderId { get; set; } = "";
        public string TransactionId { get; set; } = "";
        public string Message { get; set; } = "";
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
