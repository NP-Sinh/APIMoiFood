using Azure.Core;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace APIMoiFood.Services.PaymentService.MomoService
{
    public interface IMomoService
    {
        Task<string> CreatePayment(long amount, string orderId, string orderInfo, string? returnUrl = null, string? notifyUrl = null);
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

        public async Task<string> CreatePayment(long amount, string orderId, string orderInfo,
                                                string? returnUrl = null, string? notifyUrl = null)
        {
            var requestId = Guid.NewGuid().ToString();
            var momoOrderId = $"{orderId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

            var req = new CollectionLinkRequest
            {
                orderInfo = orderInfo,
                partnerCode = _settings.PartnerCode,
                redirectUrl = returnUrl ?? _settings.ReturnUrl,
                ipnUrl = notifyUrl ?? _settings.NotifyUrl,
                amount = amount,
                orderId = momoOrderId,
                requestId = requestId,
                requestType = "payWithMethod",
                partnerName = "MoMo Payment",
                storeId = "Test Store",
                autoCapture = true,
                lang = "vi",
                extraData = ""
            };

            // Tạo chữ ký và gửi request
            var raw = $"accessKey={_settings.AccessKey}&amount={req.amount}&extraData={req.extraData}" +
                      $"&ipnUrl={req.ipnUrl}&orderId={req.orderId}&orderInfo={req.orderInfo}" +
                      $"&partnerCode={req.partnerCode}&redirectUrl={req.redirectUrl}" +
                      $"&requestId={req.requestId}&requestType={req.requestType}";

            req.signature = SignSHA256(raw, _settings.SecretKey);

            var content = new StringContent(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(_settings.Endpoint, content);
            return await response.Content.ReadAsStringAsync();
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

    public class CollectionLinkRequest
    {
        public string orderInfo { get; set; }
        public string partnerCode { get; set; }
        public string redirectUrl { get; set; }
        public string ipnUrl { get; set; }
        public long amount { get; set; }
        public string orderId { get; set; }
        public string requestId { get; set; }
        public string extraData { get; set; }
        public string partnerName { get; set; }
        public string storeId { get; set; }
        public string requestType { get; set; }
        public string orderGroupId { get; set; }
        public bool autoCapture { get; set; }
        public string lang { get; set; }
        public string signature { get; set; }
    }
}
