using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OrderAppWeb.API.Context;
using OrderAppWeb.API.Models.Dtos;
using OrderAppWeb.API.Models.Entities;
using OrderAppWeb.API.Models.Results;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;

namespace OrderAppWeb.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly OrderDbContext _context;
        private readonly IMapper mapper;

        public OrderController(IMemoryCache memoryCache, OrderDbContext context, IMapper mapper)
        {
            _memoryCache = memoryCache;
            _context = context;
            this.mapper = mapper;
        }

        // #region Memory Cache ile Cache işlemi
        // [HttpGet]
        //public async Task<IActionResult> Get(string? category)
        // {

        //     // Memory Cache sadece proje açıkken işlem görür.

        //     var result = new List<Product>();

        //     if (category is null)
        //     {
        //         result = _memoryCache.Get("products") as List<Product>;
        //         if(result is null)
        //         {
        //             result = await _context.Products.ToListAsync();
        //             _memoryCache.Set("products", result, TimeSpan.FromMinutes(10));
        //         }
        //     }
        //     else
        //     {
        //         result = _memoryCache.Get($"products-{category}") as List<Product>;
        //         if (result is null)
        //         {
        //             result = await _context.Products.Where(x => x.Category == category).ToListAsync();
        //             _memoryCache.Set($"products-{category}", result, TimeSpan.FromMinutes(10));
        //         }
        //     }

        //     var productDtos=mapper.Map<List<Product>,List<ProductDto>>(result);


        //     return Ok(new ApiResponse<List<ProductDto>>(StatusType.Success,productDtos));
        // }

        // #endregion

        #region Redis ile Cache işlemi
        [HttpGet]
        public async Task<IActionResult> Get(string? category)
        {
            var redisClient = new RedisClient("localhost", 6379);
            IRedisTypedClient<List<Product>> redisProducts = redisClient.As<List<Product>>();

            var result = new List<Product>();
            if (category is null)
            {
                result = redisClient.Get<List<Product>>("products");
                if (result is null)
                {
                    result = await _context.Products.ToListAsync();
                    redisClient.Set("products", result, TimeSpan.FromMinutes(10));
                }
            }
            else
            {
                result = redisClient.Get<List<Product>>($"products-{category}");
                if (result is null)
                {
                    result = await _context.Products.Where(x => x.Category == category).ToListAsync();
                    redisClient.Set($"products-{category}", result, TimeSpan.FromMinutes(10));
                }
            }

            var productDtos = mapper.Map<List<Product>, List<ProductDto>>(result);


            return Ok(new ApiResponse<List<ProductDto>>(StatusType.Success, productDtos));
        }
        #endregion

        [HttpPost] 
        public async Task<IActionResult> CreateOrder(CreateOrderRequest createOrderRequest)
        {
            Order order=mapper.Map<Order>(createOrderRequest);
            List<OrderDetail> orderDetails = mapper.Map<List<ProductDetailDto>, List<OrderDetail>>(createOrderRequest.ProductDetails);
            order.TotalAmount = createOrderRequest.ProductDetails.Sum(x => x.Amount);
            order.OrderDetails= orderDetails;
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<int>(StatusType.Success,order.Id));
        }


        #region 1000 tane fake data için
        //[HttpPost]

        //public async Task<IActionResult> Post()
        //{
        //    for(int i=0;i<1000;i++)
        //    {
        //        Product product = new()
        //        {
        //            Category = $"Kategori {i}",
        //            CreateDate = DateTime.Now,
        //            Description = $"Açıklama {i}",
        //            Status = true,
        //            Unit = i,
        //            UnitPrice = i * 10
        //        };
        //        await _context.Products.AddAsync(product);
        //        await _context.SaveChangesAsync();
        //    }
        //    return Ok("Ürünler başarıyla oluşturuldu");
        //}
        #endregion
    }
}
