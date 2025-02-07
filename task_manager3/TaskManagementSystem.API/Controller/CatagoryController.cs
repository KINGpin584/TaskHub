using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Core.DTOs;
using TaskManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.API.Data;
using System.Threading.Tasks;




namespace TaskManagementSystem.API{

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly TaskManagementContext _context;

    public CategoriesController(TaskManagementContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponseDTO>> CreateCategory(CreateCategoryDTO createDto)
    {
        var category = new Category { Name = createDto.Name };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, 
            new CategoryResponseDTO 
            { 
                Id = category.Id, 
                Name = category.Name,
                TaskCount = 0
            });
    }
     [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponseDTO>>> GetCategories()
    {
        try
        {
            var categories = await _context.Categories
                .Include(c => c.Tasks)
                .Select(c => new CategoryResponseDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    TaskCount = c.Tasks.Count
                })
                .ToListAsync();

            return Ok(categories);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryResponseDTO>> GetCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Tasks)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound();

        return new CategoryResponseDTO
        {
            Id = category.Id,
            Name = category.Name,
            TaskCount = category.Tasks.Count
        };
    }
}

}