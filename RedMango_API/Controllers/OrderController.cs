using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RedMango_API.Data;
using RedMango_API.Models;
using RedMango_API.Models.Dto;
using RedMango_API.Utility;
using System.Net;

namespace RedMango_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        #region ctor
        private readonly ApplicationDbContext _db;
        private ApiResponse _response;
        public OrderController(ApplicationDbContext db)
        {
            _db = db;
            _response = new();
        }
        #endregion

        #region GetOrders
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> GetOrders(string? userId,
            string searchString, string status)
        {
            try
            {
                IEnumerable<OrderHeader> orderHeaders = _db.OrderHeaders.Include(x => x.OrderDetails)
                    .ThenInclude(x => x.MenuItem)
                    .OrderByDescending(x => x.OrderHeaderId);
                if (!string.IsNullOrEmpty(userId))
                {
                    orderHeaders = orderHeaders.Where(x => x.ApplicationUserId == userId);
                }

                if (!searchString.IsNullOrEmpty())
                {
                    orderHeaders = orderHeaders.Where(x =>
                        x.PickUpPhoneNumber.ToLower().Contains(searchString.ToLower())
                        || x.PickUpEmail.ToLower().Contains(searchString.ToLower())
                        || x.PickUpName.ToLower().Contains(searchString.ToLower()));
                }

                if (!status.IsNullOrEmpty())
                {
                    orderHeaders = orderHeaders
                        .Where(x => x.Status.ToLower() == status.ToLower());
                }

                _response.Result = orderHeaders;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };

            }
            return _response;
        }

        #endregion

        #region GetOrder
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse>> GetOrder(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Order was not found");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var orderHeaders = _db.OrderHeaders.Include(x => x.OrderDetails)
                    .ThenInclude(x => x.MenuItem)
                    .Where(x => x.OrderHeaderId == id);

                if (orderHeaders == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("Order was not found");
                    return NotFound(_response);
                }
                _response.Result = orderHeaders;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };

            }
            return _response;
        }
        #endregion

        #region CreateOrder
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateOrder([FromBody] OrderHeaderCreateDto orderHeaderCreateDto)
        {
            try
            {
                OrderHeader order = new()
                {
                    ApplicationUserId = orderHeaderCreateDto.ApplicationUserId,
                    PickUpEmail = orderHeaderCreateDto.PickUpEmail,
                    PickUpName = orderHeaderCreateDto.PickUpName,
                    PickUpPhoneNumber = orderHeaderCreateDto.PickUpPhoneNumber,
                    OrderTotal = orderHeaderCreateDto.OrderTotal,
                    OrderDate = DateTime.Now,
                    StripePaymentIntentId = orderHeaderCreateDto.StripePaymentIntentId,
                    TotalItems = orderHeaderCreateDto.TotalItems,
                    Status = String.IsNullOrEmpty(orderHeaderCreateDto.Status) ? SD.status_pending : orderHeaderCreateDto.Status
                };

                if (ModelState.IsValid)
                {
                    _db.OrderHeaders.Add(order);
                    _db.SaveChanges();

                    foreach (var orderDetailDto in orderHeaderCreateDto.OrderDetailsDto)
                    {
                        OrderDetails orderDetails = new()
                        {
                            OrderHeaderId = order.OrderHeaderId,
                            ItemName = orderDetailDto.ItemName,
                            MenuItemId = orderDetailDto.MenuItemId,
                            Price = orderDetailDto.Price,
                            Quantity = orderDetailDto.Quantity,
                        };
                        _db.OrderDetails.Add(orderDetails);
                    }
                    _db.SaveChanges();
                    _response.Result = order;
                    order.OrderDetails = null;
                    _response.StatusCode = HttpStatusCode.Created;
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }
        #endregion

        #region UpdateOrderHeader
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse>> UpdateOrderHeader(int id, [FromBody] OrderHeaderUpdateDto orderHeaderUpdateDto)
        {
            try
            {
                if (orderHeaderUpdateDto == null || id != orderHeaderUpdateDto.OrderHeaderId)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Order id doesn't match provided id");
                    return BadRequest();
                }
                OrderHeader orderHeaderFromDb = _db.OrderHeaders.FirstOrDefault(x => x.OrderHeaderId == id);
                if (orderHeaderFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Order not found");
                    return BadRequest();
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDto.PickUpName))
                {
                    orderHeaderFromDb.PickUpName = orderHeaderUpdateDto.PickUpName;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDto.PickUpPhoneNumber))
                {
                    orderHeaderFromDb.PickUpPhoneNumber = orderHeaderUpdateDto.PickUpPhoneNumber;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDto.Status))
                {
                    orderHeaderFromDb.Status = orderHeaderUpdateDto.Status;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDto.PickUpEmail))
                {
                    orderHeaderFromDb.PickUpEmail = orderHeaderUpdateDto.PickUpEmail;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDto.StripePaymentIntentId))
                {
                    orderHeaderFromDb.StripePaymentIntentId = orderHeaderUpdateDto.StripePaymentIntentId;
                }

                _db.SaveChanges();
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                return BadRequest();
            }
        }
        #endregion
    }
}
