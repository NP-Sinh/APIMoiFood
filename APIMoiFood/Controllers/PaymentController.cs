using APIMoiFood.Models.DTOs.Payment;
using APIMoiFood.Models.Entities;
using APIMoiFood.Services.PaymentMethodService;
using APIMoiFood.Services.PaymentService;
using APIMoiFood.Services.PaymentService.VnpayService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace APIMoiFood.Controllers
{
    [ApiController]
    [Route("moifood/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly IConfiguration _configuration;
        public PaymentController(IPaymentService paymentService, IPaymentMethodService paymentMethodService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _paymentMethodService = paymentMethodService;
            _configuration = configuration;
        }
        [HttpPost("order/{orderId}")]
        public async Task<IActionResult> CreatePayment(int orderId, [FromBody] PaymentRequest request)
        {
            request.OrderId = orderId;
            var result = await _paymentService.CreatePaymentAsync(request);
            return Ok(result);
        }
        [HttpGet("get-payment-method")]
        public async Task<IActionResult> GetPaymentMethods()
        {
            var methods = await _paymentMethodService.GetAllPaymentMethods();
            return Ok(methods);
        }
        [HttpGet("get-payment-method-by-id")]
        public async Task<IActionResult> GetPaymentMethodById(int methodId)
        {
            var method = await _paymentMethodService.GetPaymentMethodById(methodId);
            if(method == null)
                return NotFound("Phương thức thanh toán không tồn tại!");
            return Ok(method);
        }
        [HttpPost("modify-payment-method")]
        public async Task<IActionResult> ModifyPaymentMethod(int id, [FromBody] PaymentMethodRequest request)
        {
            var result = await _paymentMethodService.Modify(id, request);
            if(result == null)
                return NotFound("Phương thức thanh toán không tồn tại!");
            return Ok(result);
        }
        [HttpPost("momo-ipn")]
        public async Task<IActionResult> MomoIpn([FromBody] MomoIpnRequest request)
        {
            await _paymentService.HandleMomoIpnAsync(request);
            return Ok(new { message = "success" });
        }
        [HttpGet("momo-return")]
        public async Task<IActionResult> MomoReturn([FromQuery] MomoIpnRequest request)
        {
            await _paymentService.HandleMomoIpnAsync(request);

            if (request.ResultCode == 0)
                return Ok(new { success = true, orderId = request.OrderId, message = "Thanh toán MoMo thành công" });
            else
                return BadRequest(new { success = false, orderId = request.OrderId, message = request.Message });
        }

        [HttpGet("vnpay-ipn")]
        public async Task<IActionResult> VnPayIpn()
        {
            var result = await _paymentService.HandleVNPAYReturnAsync(Request.Query);
            if (result.IsSuccess)
                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            return BadRequest(new { RspCode = "97", Message = "Invalid signature or data" });
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            var result = await _paymentService.HandleVNPAYReturnAsync(Request.Query);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result);
        }

    }
}

