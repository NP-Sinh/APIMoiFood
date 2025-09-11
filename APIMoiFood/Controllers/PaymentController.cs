using APIMoiFood.Models.DTOs.Payment;
using APIMoiFood.Models.Entities;
using APIMoiFood.Services.PaymentMethodService;
using APIMoiFood.Services.PaymentService;
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
        public PaymentController(IPaymentService paymentService, IPaymentMethodService paymentMethodService)
        {
            _paymentService = paymentService;
            _paymentMethodService = paymentMethodService;
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

    }
}

