using Asp.Versioning;
using AutoMapper;
using e_commerce.Model;
using e_commerce.Model.Dtos;
using e_commerce.Model.Dtos.Responses;
using e_commerce.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace e_commerce.Controllers
{
   
    [Authorize(Roles = "Admin")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersionNeutral]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }
        
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            var products = _productRepository.GetProducts();
            var productDtos = _mapper.Map<List<ProductDto>>(products);
            
            return Ok(productDtos);
        }

        [AllowAnonymous]
        [HttpGet("{productId:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProduct(int productId)
        {
            var product = _productRepository.GetProduct(productId);

            if (product == null) return NotFound($"El producto con el id {productId} no existe");

            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }
        
        [AllowAnonymous]
        [HttpGet("Paged", Name = "GetProductsInPage")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsInPage([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1) return BadRequest("Los parametros de paginacion no son validos");
            
            var totalProducts = _productRepository.GetTotalProducts();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            
            if (pageNumber > totalPages) return NotFound("No hay mas paginas disponibles");
            
            var products = _productRepository.GetProductsInPages(pageNumber, pageSize);
            var productDto = _mapper.Map<List<ProductDto>>(products);
            var paginationResponse = new PaginationResponse<ProductDto>()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                Items = productDto
            };
            return Ok(paginationResponse);
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            if (createProductDto == null) return BadRequest(ModelState);

            if (_productRepository.ProductExists(createProductDto.Name))
            {
                ModelState.AddModelError("CustomError", $"El producto ya existe");
                return BadRequest(ModelState);
            }
            
            if (!_categoryRepository.CategoryExists(createProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", $"La categoria con el id {createProductDto.CategoryId} no existe");
                return BadRequest(ModelState);
            }
            
            var product = _mapper.Map<Product>(createProductDto);
            
            //Agregando Imagen

            if (string.IsNullOrEmpty(createProductDto.ImageUrl))
            {
                UploadProductImage(createProductDto, product); 
            }
            else
            {
                product.ImageUrl = "https://placehold.co/600x400";
            }

            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Algo salio mal al guardar el producto {product.Name}");
                return StatusCode(500, ModelState);
            }

            var createdProduct = _productRepository.GetProduct(product.ProductId);
            var productDto = _mapper.Map<ProductDto>(createdProduct);
            
            return CreatedAtRoute("GetProduct", new { productId = product.ProductId }, productDto);
        }
        
        [HttpGet("searchByCategory/{categoryId:int}", Name = "GetProductsForCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsForCategory(int categoryId)
        {
            if (categoryId == null) return BadRequest("La categoria no existe");
            
            var products = _productRepository.GetProductsForCategory(categoryId);

            if (products.Count == 0) return NotFound($"Los productos con la categoria {categoryId} no existen");

            var productDto = _mapper.Map<List<ProductDto>>(products);
            return Ok(productDto);
        }
        
        [HttpGet("searchProductByNameDescription/{searchTerm}", Name = "SearchProducts")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult SearchProducts(string searchTerm)
        {
            if (searchTerm.Length <= 0) return BadRequest("Se necesita una descripcion");
            
            var products = _productRepository.SearchProducts(searchTerm);

            if (products.Count == 0) return NotFound($"Los productos con el nombre {searchTerm} no existen");

            var productDto = _mapper.Map<List<ProductDto>>(products);
            return Ok(productDto);
        
        }
        
        [HttpPatch("buyProduct/{name}/{quantity:int}", Name = "BuyProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult BuyProduct(string name, int quantity)
        {
            if (string.IsNullOrWhiteSpace(name) || quantity <= 0)
                return BadRequest("El nombre del producto o la cantidad no son validos");

            var foundProduct = _productRepository.ProductExists(name);

            if (!foundProduct) return NotFound("El producto no existe");

            if (!_productRepository.BuyProduct(name, quantity))
            {
                ModelState.AddModelError(
                    "CustomError", 
                    $"No se pudo buy el producto {name} o la cantidad solicitada es mayor al stock disponible"
                );
                
                return BadRequest(ModelState);
            }
            
            var units = quantity == 1 ? "unidad" : "unidades";
            
            return Ok($"Se compro {quantity} {units} del producto '{name}'");
        }
        
        [HttpPut("{productId:int}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateProduct(int productId,[FromForm] UpdateProductDto updateProductDto)
        {
            if (updateProductDto == null) return BadRequest(ModelState);

            if (!_productRepository.ProductExists(productId))
            {
                ModelState.AddModelError("CustomError", $"El producto no existe  ");
                return BadRequest(ModelState);
            }
            
            if (!_categoryRepository.CategoryExists(updateProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", $"La categoria con el id {updateProductDto.CategoryId} no existe");
                return BadRequest(ModelState);
            }
            
            var product = _mapper.Map<Product>(updateProductDto);
            product.ProductId = productId;
            
            //Agregando Imagen
            if (!string.IsNullOrEmpty(updateProductDto.ImageUrlLocal))
            {
                FileInfo file = new FileInfo(updateProductDto.ImageUrlLocal!);
                
                Console.WriteLine($"Existe el archivo?: {file.Exists}");
                
                if (file.Exists) file.Delete();
            }

            if (string.IsNullOrEmpty(updateProductDto.ImageUrl))
                UploadProductImage(updateProductDto, product); 
            else
                product.ImageUrl = "https://placehold.co/600x400";

            if (!_productRepository.UpdateProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Algo salio mal al actualizar el producto {product.Name}");
                return StatusCode(500, ModelState);
            }
            
            return NoContent();
        }

        private void UploadProductImage(dynamic productDto, Product product)
        {
                string fileName = product.ProductId + Guid.NewGuid().ToString() + Path.GetExtension(productDto.ImageUrl);
                var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductsImages");

                if (!Directory.Exists(imageFolder)) Directory.CreateDirectory(imageFolder);
                
                var filePath = Path.Combine(imageFolder, fileName);
                
                FileInfo file = new FileInfo(filePath);
                
                if (file.Exists) file.Delete();

                using var fileStream = new FileStream(filePath, FileMode.Create);
                productDto.Image.CopyTo(fileStream);
                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                product.ImageUrl = $"{baseUrl}/ProductsImages/{fileName}";
                product.ImageUrlLocal = filePath;
        }

        [HttpDelete("{productId:int}", Name = "RemoveProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult RemoveProduct(int productId)
        {
            if (productId == 0) return BadRequest("El producto no existe");
            
            var product = _productRepository.GetProduct(productId);

            if (!_productRepository.DeleteProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Algo salio mal al eliminar el producto {product.Name}");
                return StatusCode(500, ModelState);
            }
            
            return NoContent();
        }
    }
}
