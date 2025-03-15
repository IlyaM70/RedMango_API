using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMango_API.Data;
using RedMango_API.Models;
using RedMango_API.Services;
using System.Net;

namespace RedMango_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        #region ctor
        private readonly ApplicationDbContext _db;
        private ApiResponse _response;
        public ShoppingCartController(ApplicationDbContext db)
        {
            _db = db;
            _response = new();
        }
        #endregion

        #region GetShoppingCart
        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetShoppingCart(string userId)
        {
            try
            {
                ShoppingCart shoppingCart;
                if (string.IsNullOrEmpty(userId))
                {
                    shoppingCart = new();
                }
                else
                {
                    shoppingCart = _db.ShoppingCarts
                    .Include(x => x.CartItems)
                    .ThenInclude(x => x.MenuItem)
                    .FirstOrDefault(x => x.UserId == userId);
                }

                if (shoppingCart.CartItems != null && shoppingCart.CartItems.Count() > 0)
                {
                    shoppingCart.CartTotal = shoppingCart.CartItems.Sum(x => x.Quantity * x.MenuItem.Price);                
                }

                _response.Result = shoppingCart;
                _response.StatusCode =HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                _response.StatusCode = HttpStatusCode.BadRequest;
            }
            return _response;
        }
        #endregion

        #region AddOrUpdateItemInCart
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> AddOrUpdateItemInCart(string userId, int menuItemId, int updateQuantityBy)
        {
            ShoppingCart shoppingCart = _db.ShoppingCarts.Include(x=>x.CartItems).FirstOrDefault(x => x.UserId == userId);
            MenuItem menuItem = _db.MenuItems.FirstOrDefault(x => x.Id == menuItemId);
            if (menuItem == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            if (shoppingCart == null && updateQuantityBy > 0)
            {
                //create a shopping cart and add item
                ShoppingCart newCart = new() { UserId = userId, };
                _db.ShoppingCarts.Add(newCart);
                _db.SaveChanges();

                CartItem newCartItem = new()
                {
                    MenuItemId = menuItemId,
                    Quantity = updateQuantityBy,
                    ShoppingCartId = newCart.Id,
                    MenuItem = null
                };

                _db.CartItems.Add(newCartItem);
                _db.SaveChanges();
            }
            else 
            {
                //shopping cart exists
                CartItem cartItemInCart = shoppingCart.CartItems.FirstOrDefault(x=>x.MenuItemId == menuItemId);
                if (cartItemInCart == null) 
                {
                    //item does not exist in current cart
                    CartItem newCartItem = new()
                    {
                        MenuItemId = menuItemId,
                        Quantity = updateQuantityBy,
                        ShoppingCartId = shoppingCart.Id,
                        MenuItem = null
                    };
                    _db.CartItems.Add(newCartItem);
                    _db.SaveChanges();

                }
                else
                {
                    //item already exist in the cart and we have to update quantity
                    int newQuantity = cartItemInCart.Quantity+updateQuantityBy;
                    if (updateQuantityBy == 0 || newQuantity <= 0)
                    {
                        //remove item from cart and if it is the only item remove the cart
                        _db.CartItems.Remove(cartItemInCart);
                        if (shoppingCart.CartItems.Count == 1)
                        {
                            _db.ShoppingCarts.Remove(shoppingCart);
                        }
                        _db.SaveChanges();
                    }
                    else
                    {
                        cartItemInCart.Quantity = newQuantity;
                        _db.SaveChanges();
                    }
                }
            }
            return _response;
        }
        #endregion
    }
}
