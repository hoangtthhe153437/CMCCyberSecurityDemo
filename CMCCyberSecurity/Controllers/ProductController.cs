using CMCCyberSecurity.DTO;
using CMCCyberSecurity.Helpers;
using CMCCyberSecurity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CMCCyberSecurity.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly CMCCyberSecurityContext _context;
        private readonly IMemoryCache _cache;

        public ProductController(CMCCyberSecurityContext context, IMemoryCache cache)
        {
            _cache = cache;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            const string cacheKey = "productList";
            if (!_cache.TryGetValue(cacheKey, out List<Product> products))
            {
                products = await _context.Products.ToListAsync();
                _cache.Set(cacheKey, products, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }
            return products;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id) ?? throw new HttpStatusException("Product not found", EnumHelper.ECode.NotFound);
            return product;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutProduct(int id, ProductUpdateDTO product)
        {
            var productCheck = await _context.Products.FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new HttpStatusException("product is not exist", EnumHelper.ECode.BadRequest);

            productCheck.Name = product.Name;
            productCheck.Price = product.Price;
            productCheck.Description = product.Description;

            try
            {
                const string cacheKey = "productList";
                _cache.Remove(cacheKey);
                _context.Update(productCheck);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(ex.Message, EnumHelper.ECode.InternalServerError);
            }

            return NoContent();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            try
            {
                const string cacheKey = "productList";
                _cache.Remove(cacheKey);
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(ex.Message, EnumHelper.ECode.InternalServerError);
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id) ?? throw new HttpStatusException("product is not exist", EnumHelper.ECode.BadRequest);
            try
            {
                const string cacheKey = "productList";
                _cache.Remove(cacheKey);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(ex.Message, EnumHelper.ECode.InternalServerError);
            }

            return NoContent();
        }
    }
}
