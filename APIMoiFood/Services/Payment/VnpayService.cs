using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace APIMoiFood.Services.PaymentService.VnpayService
{
    public interface IVnpayService
    {
        string PaymentVNPAY(VnPaymentRequest req);
        bool ValidateSignature(IQueryCollection query);
        VnPaymentResponse ParseResponse(IQueryCollection query);
    }

    public class VnpayService : IVnpayService
    {
        private readonly VnpaySettings _settings;
        private readonly ILogger<VnpayService> _logger;
        public VnpayService(IOptions<VnpaySettings> options, ILogger<VnpayService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }
        public string PaymentVNPAY(VnPaymentRequest req)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);

            query["vnp_Version"] = "2.1.0";
            query["vnp_Command"] = "pay";
            query["vnp_TmnCode"] = _settings.TmnCode;
            query["vnp_Amount"] = (req.Amount * 100).ToString();
            query["vnp_CurrCode"] = "VND";
            query["vnp_TxnRef"] = req.OrderId.ToString();
            query["vnp_OrderInfo"] = req.OrderInfo;
            query["vnp_Locale"] = string.IsNullOrEmpty(req.Locale) ? "vn" : req.Locale;
            query["vnp_ReturnUrl"] = string.IsNullOrEmpty(req.ReturnUrl) ? _settings.ReturnUrl : req.ReturnUrl;
            if (!string.IsNullOrEmpty(req.NotifyUrl))
            {
                query["vnp_NotifyUrl"] = req.NotifyUrl;
            }    
            query["vnp_CreateDate"] = req.CreateDate.ToString("yyyyMMddHHmmss");
            query["vnp_IpAddr"] = string.IsNullOrEmpty(req.IpAddress) ? "127.0.0.1" : req.IpAddress;
            query["vnp_OrderType"] = string.IsNullOrEmpty(req.OrderType) ? "other" : req.OrderType;

            // Sắp xếp theo key và tạo chuỗi dữ liệu để ký
            var sorted = query.AllKeys
                    .OrderBy(k => k)
                    .Select(k => $"{k}={Uri.EscapeDataString(query[k])}");
            var signData = string.Join("&", sorted);

            // Tạo chữ ký HMAC-SHA512
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_settings.HashSecret));
            var hash = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(signData)))
                                 .Replace("-", "").ToLower();

            return $"{_settings.Url}?{signData}&vnp_SecureHash={hash}";
        }

        public bool ValidateSignature(IQueryCollection query)
        {
            if (!query.ContainsKey("vnp_SecureHash"))
                return false;

            var receivedHash = query["vnp_SecureHash"].ToString().ToLower();

            var dataToSign = query
                .Where(x => x.Key.StartsWith("vnp_") && x.Key != "vnp_SecureHash" && x.Key != "vnp_SecureHashType")
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}={x.Value}");

            var rawData = string.Join("&", dataToSign);

            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_settings.HashSecret));
            var myHash = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData)))
                                .Replace("-", "").ToLower();

            return myHash == receivedHash;
        }

        public VnPaymentResponse ParseResponse(IQueryCollection query)
        {
            return new VnPaymentResponse
            {
                TransactionNo = query["vnp_TransactionNo"],
                TxnRef = query["vnp_TxnRef"],
                ResponseCode = query["vnp_ResponseCode"],
                SecureHash = query["vnp_SecureHash"],
                BankCode = query["vnp_BankCode"],
                CardType = query["vnp_CardType"],
                Amount = long.TryParse(query["vnp_Amount"], out var amt) ? amt : 0,
                OrderInfo = query["vnp_OrderInfo"],
                PayDate = query["vnp_PayDate"]
            };
        }
    }
    public class VnpaySettings
    {
        public string TmnCode { get; set; } = null!;
        public string HashSecret { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string ReturnUrl { get; set; } = null!;
    }
    public class VnPaymentRequest
    {
        public long Amount { get; set; }               // Số tiền
        public string OrderInfo { get; set; } = "";    // Thông tin đơn hàng
        public int OrderId { get; set; }               // Mã đơn hàng nội bộ
        public string IpAddress { get; set; } = "";    // Địa chỉ IP của khách
        public string Locale { get; set; } = "vn";     // Ngôn ngữ hiển thị
        public string OrderType { get; set; } = "other";
        public string ReturnUrl { get; set; } = "";    // URL trả kết quả về
        public string NotifyUrl { get; set; } = "";
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
    public class VnPaymentResponse
    {
        public string TransactionNo { get; set; } = "";     // vnp_TransactionNo
        public string TxnRef { get; set; } = "";            // vnp_TxnRef (mã đơn hàng)
        public string ResponseCode { get; set; } = "";      // vnp_ResponseCode
        public string SecureHash { get; set; } = "";        // vnp_SecureHash
        public string BankCode { get; set; } = "";
        public string CardType { get; set; } = "";
        public long Amount { get; set; }                    // vnp_Amount
        public string OrderInfo { get; set; } = "";
        public string PayDate { get; set; } = "";
        public bool IsSuccess => ResponseCode == "00";
    }
}

