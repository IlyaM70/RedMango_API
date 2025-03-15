using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMango_API.Data;
using RedMango_API.Models;
using Stripe;
using System.Net;

namespace RedMango_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        #region ctor
        private readonly ApplicationDbContext _db;
        private ApiResponse _response;
        private string secretKey;
        public PaymentController(ApplicationDbContext db, IConfiguration configuration)
        {
            _db = db;
            _response = new ApiResponse();
            secretKey = configuration.GetValue<string>("StripeSetting:SecretKey");
        }
        #endregion

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> MakePayment(string userId)
        {
            ShoppingCart shoppingCart = _db.ShoppingCarts.
                Include(x=>x.CartItems)
                .ThenInclude(x=>x.MenuItem)
                .FirstOrDefault(x=>x.UserId==userId);

            if (shoppingCart == null || shoppingCart.CartItems==null|| shoppingCart.CartItems.Count==0)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Shopping cart was not found or empty");
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);

            }

            #region Create Payment Intent

            StripeConfiguration.ApiKey = secretKey;
            shoppingCart.CartTotal = shoppingCart.CartItems.Sum(x=>x.Quantity*x.MenuItem.Price);
            PaymentIntentCreateOptions options = new()
            {
                Amount = (int)shoppingCart.CartTotal*100,
                Currency = "usd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };
            PaymentIntentService service = new();
            PaymentIntent response = service.Create(options);
            shoppingCart.StripePaymentIntentId = response.Id;
            shoppingCart.ClientSecret = response.ClientSecret;
            #endregion

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = shoppingCart;
            return Ok(_response);
        }
    }
}
