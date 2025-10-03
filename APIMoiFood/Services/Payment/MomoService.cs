using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace APIMoiFood.Services.PaymentService.MomoService
{
    public interface IMomoService
    {
        Task<dynamic> CreatePayment(MomoRequest request);
    }
    public class MomoService : IMomoService
    {
        private readonly HttpClient _client;
        private readonly MomoSettings _settings;

        public MomoService(HttpClient client, IOptions<MomoSettings> options)
        {
            _client = client;
            _settings = options.Value;
        }

        public async Task<dynamic> CreatePayment(MomoRequest request)
        {
            var raw = $"accessKey={_settings.AccessKey}&amount={request.Amount}&extraData={request.ExtraData}" +
                        $"&ipnUrl={request.IpnUrl}&orderId={request.OrderId}&orderInfo={request.OrderInfo}" +
                        $"&partnerCode={_settings.PartnerCode}&redirectUrl={request.RedirectUrl}" +
                        $"&requestId={request.RequestId}&requestType={request.RequestType}";

            var signature = SignSHA256(raw, _settings.SecretKey);

            var payload = new
            {
                partnerCode = _settings.PartnerCode,
                partnerName = request.PartnerName,
                storeId = request.StoreId,
                requestId = request.RequestId,
                amount = request.Amount,
                orderId = request.OrderId,
                orderInfo = request.OrderInfo,
                redirectUrl = request.RedirectUrl,
                ipnUrl = request.IpnUrl,
                lang = request.Lang,
                requestType = request.RequestType,
                extraData = request.ExtraData,
                autoCapture = request.AutoCapture,
                signature
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _client.PostAsync(_settings.Endpoint, content);
            res.EnsureSuccessStatusCode();

            var respJson = await res.Content.ReadAsStringAsync();
            var momoResp = JsonSerializer.Deserialize<MomoResponse>(respJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return momoResp!;
        }

        private static string SignSHA256(string text, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            return BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(text)))
                               .Replace("-", "").ToLower();
        }
    }
    public class MomoSettings
    {
        public string PartnerCode { get; set; } = null!;
        public string AccessKey { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
        public string Endpoint { get; set; } = null!;
        public string ReturnUrl { get; set; } = null!;
        public string NotifyUrl { get; set; } = null!;
    }
    public class MomoRequest
    {
        public string PartnerCode { get; set; } = null!;
        public string AccessKey { get; set; } = null!;
        public string SecretKey { get; set; } = null!; // không gửi đi, chỉ dùng để ký
        public string OrderId { get; set; } = null!;
        public string OrderInfo { get; set; } = null!;
        public string RedirectUrl { get; set; } = null!;
        public string IpnUrl { get; set; } = null!;
        public long Amount { get; set; }
        public string RequestId { get; set; } = null!;
        public string ExtraData { get; set; } = "";
        public string RequestType { get; set; } = "payWithMethod";
        public bool AutoCapture { get; set; } = true;
        public string Lang { get; set; } = "vi";
        public string PartnerName { get; set; } = "MoMo Payment";
        public string StoreId { get; set; } = "Test Store";
    }
    
    public class MomoResponse
    {
        public int ResultCode { get; set; }
        public string Message { get; set; }
        public string PayUrl { get; set; }
        public string Deeplink { get; set; }
        public string QrCodeUrl { get; set; }
        public string OrderId { get; set; }
        public string RequestId { get; set; }
        public long Amount { get; set; }
    }
}
