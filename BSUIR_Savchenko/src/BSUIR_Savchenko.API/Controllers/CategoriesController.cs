using BSUIR_Savchenko.API.Data;
using BSUIR_Savchenko.Domain.Entities;
using BSUIR_Savchenko.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BSUIR_Savchenko.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ResponseData<List<Category>>>> Get()
    {
        var categories = await db.Categories.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
        return Ok(ResponseData<List<Category>>.Success(categories));
    }
}
