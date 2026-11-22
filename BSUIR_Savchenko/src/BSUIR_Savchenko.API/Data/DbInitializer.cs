using BSUIR_Savchenko.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BSUIR_Savchenko.API.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (await db.Categories.AnyAsync()) return;

        var fiction = new Category { Name = "Художественная литература", NormalizedName = "fiction-books" };
        var science = new Category { Name = "Наука", NormalizedName = "science-books" };
        var it = new Category { Name = "Программирование", NormalizedName = "programming-books" };
        db.Categories.AddRange(fiction, science, it);
        await db.SaveChangesAsync();

        db.Books.AddRange(
            new Book { Name = "Мастер и Маргарита", Description = "Роман о Москве, искусстве и выборе.", Price = 28.50m, CategoryId = fiction.Id, Image = "/images/fiction.svg", ImageMimeType = "image/svg+xml" },
            new Book { Name = "Преступление и наказание", Description = "Классический роман о совести и ответственности.", Price = 24.00m, CategoryId = fiction.Id, Image = "/images/fiction.svg", ImageMimeType = "image/svg+xml" },
            new Book { Name = "Три товарища", Description = "История дружбы и любви в непростое время.", Price = 29.00m, CategoryId = fiction.Id, Image = "/images/fiction.svg", ImageMimeType = "image/svg+xml" },
            new Book { Name = "Краткая история времени", Description = "Популярная книга о пространстве, времени и Вселенной.", Price = 35.90m, CategoryId = science.Id, Image = "/images/science.svg", ImageMimeType = "image/svg+xml" },
            new Book { Name = "Эгоистичный ген", Description = "Введение в эволюционное мышление.", Price = 31.50m, CategoryId = science.Id, Image = "/images/science.svg", ImageMimeType = "image/svg+xml" },
            new Book { Name = "Чистый код", Description = "Практики написания понятных программ.", Price = 47.00m, CategoryId = it.Id, Image = "/images/programming.svg", ImageMimeType = "image/svg+xml" },
            new Book { Name = "Совершенный код", Description = "Подходы к проектированию и сопровождению кода.", Price = 56.00m, CategoryId = it.Id, Image = "/images/programming.svg", ImageMimeType = "image/svg+xml" },
            new Book { Name = "Грокаем алгоритмы", Description = "Иллюстрированное введение в алгоритмы.", Price = 42.40m, CategoryId = it.Id, Image = "/images/programming.svg", ImageMimeType = "image/svg+xml" },
            new Book { Name = "Рефакторинг", Description = "Приёмы улучшения структуры существующего кода.", Price = 52.00m, CategoryId = it.Id, Image = "/images/programming.svg", ImageMimeType = "image/svg+xml" }
        );
        await db.SaveChangesAsync();
    }
}
