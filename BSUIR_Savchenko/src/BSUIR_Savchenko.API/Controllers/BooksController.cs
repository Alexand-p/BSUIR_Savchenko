using BSUIR_Savchenko.API.Data;
using BSUIR_Savchenko.API.Models;
using BSUIR_Savchenko.API.Services;
using BSUIR_Savchenko.Domain.Entities;
using BSUIR_Savchenko.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BSUIR_Savchenko.API.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController(AppDbContext db, ImageStorage images) : ControllerBase
{
    private const int PageSize = 3;

    [HttpGet]
    public async Task<ActionResult<ResponseData<ListModel<Book>>>> GetList(
        [FromQuery] string? categoryNormalizedName, [FromQuery] int pageNo = 1)
    {
        if (pageNo < 1) return BadRequest(ResponseData<ListModel<Book>>.Error("Номер страницы должен быть положительным."));

        var query = db.Books.AsNoTracking().Include(book => book.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(categoryNormalizedName))
        {
            var normalized = categoryNormalizedName.Trim().ToLowerInvariant();
            query = query.Where(book => book.Category != null && book.Category.NormalizedName == normalized);
        }

        var count = await query.CountAsync();
        var totalPages = Math.Max(1, (count + PageSize - 1) / PageSize);
        var currentPage = Math.Min(pageNo, totalPages);
        var items = await query.OrderBy(book => book.Id)
            .Skip((currentPage - 1) * PageSize).Take(PageSize).ToListAsync();

        return Ok(ResponseData<ListModel<Book>>.Success(new ListModel<Book>
        {
            Items = items,
            CurrentPage = currentPage,
            TotalPages = totalPages
        }));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ResponseData<Book>>> GetById(int id)
    {
        var item = await db.Books.AsNoTracking().Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound(ResponseData<Book>.Error("Книга не найдена."));
        return Ok(ResponseData<Book>.Success(item));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ResponseData<Book>>> Create([FromForm] BookForm form)
    {
        if (!await db.Categories.AnyAsync(x => x.Id == form.CategoryId))
            return BadRequest(ResponseData<Book>.Error("Категория не найдена."));

        (string Path, string Mime)? uploaded;
        try { uploaded = await images.SaveAsync(form.ImageFile); }
        catch (InvalidDataException ex) { return BadRequest(ResponseData<Book>.Error(ex.Message)); }

        var book = new Book
        {
            Name = form.Name.Trim(), Description = form.Description.Trim(),
            CategoryId = form.CategoryId, Price = form.Price,
            Image = uploaded?.Path, ImageMimeType = uploaded?.Mime
        };
        try
        {
            db.Books.Add(book);
            await db.SaveChangesAsync();
        }
        catch
        {
            images.Delete(uploaded?.Path);
            throw;
        }
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, ResponseData<Book>.Success(book));
    }

    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ResponseData<Book>>> Update(int id, [FromForm] BookForm form)
    {
        var book = await db.Books.FindAsync(id);
        if (book is null) return NotFound(ResponseData<Book>.Error("Книга не найдена."));
        if (!await db.Categories.AnyAsync(x => x.Id == form.CategoryId))
            return BadRequest(ResponseData<Book>.Error("Категория не найдена."));

        (string Path, string Mime)? uploaded;
        try { uploaded = await images.SaveAsync(form.ImageFile); }
        catch (InvalidDataException ex) { return BadRequest(ResponseData<Book>.Error(ex.Message)); }

        var oldImage = book.Image;
        book.Name = form.Name.Trim();
        book.Description = form.Description.Trim();
        book.CategoryId = form.CategoryId;
        book.Price = form.Price;
        if (uploaded is not null)
        {
            book.Image = uploaded.Value.Path;
            book.ImageMimeType = uploaded.Value.Mime;
        }
        try { await db.SaveChangesAsync(); }
        catch
        {
            images.Delete(uploaded?.Path);
            throw;
        }
        if (uploaded is not null) images.Delete(oldImage);
        return Ok(ResponseData<Book>.Success(book));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ResponseData<bool>>> Delete(int id)
    {
        var book = await db.Books.FindAsync(id);
        if (book is null) return NotFound(ResponseData<bool>.Error("Книга не найдена."));
        db.Books.Remove(book);
        await db.SaveChangesAsync();
        images.Delete(book.Image);
        return Ok(ResponseData<bool>.Success(true));
    }
}
