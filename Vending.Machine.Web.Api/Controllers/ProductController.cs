using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vending.Machine.Domain.Core;
using Vending.Machine.Domain.Core.Repository;
using Vending.Machine.Web.Api.ViewModels;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Vending.Machine.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Seller")]
    public class ProductController : ControllerBase
    {
        private const string ProductNotFound = "Product not found";
        
        private IVendingMachineRepository _repository;
        public ProductController(IVendingMachineRepository repository)
        {
            _repository = repository;
        }

        // GET api/Product/5
        [HttpGet("{id}")]
        public ActionResult<Product>  GetProduct(string id)
        {
            var product = _repository.GetProduct(id);
            if(product == null)
            {
                return NotFound(ProductNotFound);
            }
            return Ok(product);
        }

        // POST api/product
        [HttpPost]
        public async Task<IActionResult> PostProduct(ProductDto newProduct)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try {
                var product = newProduct.ToProduct(sellerId);
                await _repository.CreateProduct(product);
                return CreatedAtAction("GetProduct", new { id = product.Id }, product);
            }catch(InvalidOperationException ex)
            {
                return Problem(ex.Message, statusCode: 403);
            }
        }

        // PUT api/<ProductController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, ProductDto productDto)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var oldProduct = _repository.GetProduct(id);
            if (sellerId != oldProduct.SellerId)
            {
                return BadRequest();
            }
            try
            {
                var product = productDto.ToProduct();
                
                product.Id = id;
                await _repository.UpdateProduct(product);
            }
            catch when (!ProductExists(id))
            {
                return NotFound(ProductNotFound);
            }

            return NoContent();
        }

        // DELETE api/Product/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var oldProduct = _repository.GetProduct(id);
            if (sellerId != oldProduct.SellerId)
            {
                return BadRequest();
            }

            var product = await _repository.RemoveProduct(id);
            if (product == null)
            {
                return NotFound(ProductNotFound);
            }
            return NoContent();
        }
        private bool ProductExists(string id) => _repository.ContainsProduct(id);

    }
}
