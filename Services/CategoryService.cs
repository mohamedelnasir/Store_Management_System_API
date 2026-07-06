using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Data;
using StoreManagementSystem.DTOs.Categories;
using StoreManagementSystem.Interfaces;
using StoreManagementSystem.Middleware;
using StoreManagementSystem.Models;

namespace StoreManagementSystem.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ProductCount = c.Products.Count
            })
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ProductCount = c.Products.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var nameExists = await _context.Categories.AnyAsync(c => c.Name == dto.Name);
        if (nameExists)
        {
            throw new ConflictException("A category with this name already exists.");
        }

        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return new CategoryDto { Id = category.Id, Name = category.Name, Description = category.Description, ProductCount = 0 };
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
        {
            return null;
        }

        var nameTaken = await _context.Categories.AnyAsync(c => c.Name == dto.Name && c.Id != id);
        if (nameTaken)
        {
            throw new ConflictException("Another category already uses this name.");
        }

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ProductCount = category.Products.Count
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
        {
            return false;
        }

        if (category.Products.Any())
        {
            throw new ConflictException("Cannot delete a category that still has products assigned to it.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}
