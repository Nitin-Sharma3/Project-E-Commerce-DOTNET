using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProductAPI.DTOs;
using ProductAPI.Models;
using ProductAPI.Services;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;
    private readonly IMapper _mapper;

    public ProductController(IProductService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _service.GetAll();
        return Ok(_mapper.Map<IEnumerable<ProductDTO>>(products));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var product = await _service.GetById(id);

        if (product == null)
            return NotFound();

        return Ok(_mapper.Map<ProductDTO>(product));
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductDTO dto)
    {
        var product = _mapper.Map<Product>(dto);

        var created = await _service.Add(product);

        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ProductDTO dto)
    {
        var existing = await _service.GetById(id);

        if (existing == null)
            return NotFound();

        _mapper.Map(dto, existing);

        var updated = await _service.Update(existing);

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.Delete(id);

        if (!deleted)
            return NotFound();

        return Ok();
    }
}