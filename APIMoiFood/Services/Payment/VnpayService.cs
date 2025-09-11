using APIMoiFood.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace APIMoiFood.Services.PaymentService.VnpayService
{
    public interface IVnPayService
    {
        Task<string> CreatePaymentUrlAsync(int orderId, decimal amount, string orderInfo);
        Task<dynamic> HandleIpnAsync(Dictionary<string, string> queryParams);
    }

    public class VnpayService : IVnPayService
    {
        private readonly IConfiguration _config;
        private readonly MoiFoodDBContext _context;

        public VnpayService(IConfiguration config, MoiFoodDBContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task<string> CreatePaymentUrlAsync(int orderId, decimal amount, string orderInfo)
        {
            string vnp_TmnCode = _config["VnPay:TmnCode"];
            string vnp_HashSecret = _config["VnPay:HashSecret"];
            string vnp_Url = _config["VnPay:Url"];
            string vnp_ReturnUrl = _config["VnPay:ReturnUrl"];

            string vnp_TxnRef = orderId.ToString();
            string vnp_Amount = (amount * 100).ToString("F0");
            string vnp_OrderInfo = orderInfo;
            string vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            string vnp_IpAddr = "127.0.0.1";

            var query = new Dictionary<string, string>
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", vnp_TmnCode },
            { "vnp_Amount", vnp_Amount },
            { "vnp_CurrCode", "VND" },
            { "vnp_TxnRef", vnp_TxnRef },
            { "vnp_OrderInfo", vnp_OrderInfo },
            { "vnp_Locale", "vn" },
            { "vnp_ReturnUrl", vnp_ReturnUrl },
            { "vnp_CreateDate", vnp_CreateDate },
            { "vnp_IpAddr", vnp_IpAddr }
        };

            // Sắp xếp & tạo hash
            var sorted = query.OrderBy(k => k.Key);
            var signData = string.Join("&", sorted.Select(k => $"{k.Key}={k.Value}"));
            string vnp_SecureHash;
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(vnp_HashSecret)))
            {
                vnp_SecureHash = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(signData))).Replace("-", "").ToLower();
            }

            string paymentUrl = $"{vnp_Url}?{string.Join("&", sorted.Select(k => $"{k.Key}={k.Value}"))}&vnp_SecureHash={vnp_SecureHash}";

            // Tạo Payment duy nhất
            var method = await _context.PaymentMethods.FirstOrDefaultAsync(m => m.Name == "VNPAY");
            var payment = new Payment
            {
                OrderId = orderId,
                MethodId = method.MethodId,
                Amount = amount,
                PaymentStatus = "Pending",
                TransactionId = vnp_TxnRef,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return paymentUrl;
        }

        public async Task<dynamic> HandleIpnAsync(Dictionary<string, string> queryParams)
        {
            string txnRef = queryParams["vnp_TxnRef"];
            string vnp_ResponseCode = queryParams["vnp_ResponseCode"];

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.TransactionId == txnRef && p.PaymentStatus == "Pending");
            if (payment == null)
                return new { status = "error", message = "Payment not found or already processed" };

            payment.PaymentStatus = vnp_ResponseCode == "00" ? "Paid" : "Failed";
            payment.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return new { status = "success" };
        }
    }

}
