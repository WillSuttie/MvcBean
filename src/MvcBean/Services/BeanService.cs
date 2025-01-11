using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MvcBean.Data;
using MvcBean.Models;
using MvcBean.Utilities;

namespace MvcBean.Services;

public class BeanService
{
    private readonly MvcBeanContext _context;
    private readonly IWebHostEnvironment _environment;

    public BeanService(MvcBeanContext context, IWebHostEnvironment environment)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    // Get all beans
    public async Task<List<Bean>> GetAllBeansAsync()
    {
        return await _context.Beans.ToListAsync();
    }

    // Get paginated beans
    public async Task<List<Bean>> GetPaginatedBeansAsync(int page, int pageSize)
    {
        return await _context.Beans
            .OrderBy(b => b.Name) // Adjust sorting as needed
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    // Get total bean count
    public async Task<int> GetTotalBeanCountAsync()
    {
        return await _context.Beans.CountAsync();
    }

    // Get a bean by ID
    public async Task<Bean?> GetBeanByIdAsync(int? id)
    {
        if (id == null) return null;
        return await _context.Beans.FirstOrDefaultAsync(b => b.Id == id);
    }

    // Get a bean by SaleDate
    public async Task<Bean?> GetBeanBySaleDateAsync(DateOnly saleDate)
    {
        return await _context.Beans.FirstOrDefaultAsync(b => b.SaleDate == saleDate);
    }

    // Validate a bean
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateBeanAsync(Bean bean, int? excludeId = null)
    {
        // 1. Check for duplicate sale dates
        var (isDuplicate, conflictingBeanName) = await ValidateSaleDateAsync(bean, excludeId);
        if (isDuplicate)
        {
            return (false, $"The Sale Date conflicts with another bean: '{conflictingBeanName}'. Please choose a different date.");
        }

        // 2. Validate Name
        if (string.IsNullOrWhiteSpace(bean.Name))
        {
            return (false, "A name is required.");
        }
        if (!Regex.IsMatch(bean.Name, @"^[a-zA-Z0-9\s&'()#!?,.-]{3,60}$"))
        {
            return (false, "The Name field contains invalid characters or is not between 3 and 60 characters.");
        }

        // 3. Validate Aroma (if provided)
        if (!string.IsNullOrEmpty(bean.Aroma) &&
            !Regex.IsMatch(bean.Aroma, @"^[a-zA-Z0-9\s&'()#!?,.-]{3,60}$"))
        {
            return (false, "The Aroma field contains invalid characters or is not between 3 and 60 characters.");
        }

        // 4. Validate ColourHex (if provided)
        if (!string.IsNullOrEmpty(bean.ColourHex) &&
            !Regex.IsMatch(bean.ColourHex, @"^#[0-9A-Fa-f]{6}$"))
        {
            return (false, "The Colour field must be a valid hexadecimal colour code (e.g., #FFFFFF).");
        }

        // 5. Validate PricePer100g
        if (bean.PricePer100g < 0.01m || bean.PricePer100g > 10000m)
        {
            return (false, "Price must be between £0.01 and £10,000.00 per 100g.");
        }

        // 6. Validate ImagePath (if provided)
        if (!string.IsNullOrEmpty(bean.ImagePath) && !ImageUtilities.IsValidFileExtension(bean.ImagePath))
        {
            return (false, $"The image file must be one of the following types: {ImageUtilities.GetAllowedExtensionsMessage()}.");
        }

        // All checks passed
        return (true, null);
    }

    // Validate sale date
    public async Task<(bool IsDuplicate, string? ConflictingBeanName)> ValidateSaleDateAsync(Bean bean, int? excludeId = null)
    {
        var conflictingBean = await _context.Beans
            .Where(b => b.SaleDate == bean.SaleDate && (!excludeId.HasValue || b.Id != excludeId))
            .FirstOrDefaultAsync();

        return (conflictingBean != null, conflictingBean?.Name);
    }

    // Add or update a bean
    public async Task AddOrUpdateBeanAsync(Bean bean, IFormFile? image)
    {
        if (bean.Id == 0)
        {
            // Add new bean
            bean.ImagePath = await SaveImageOrPlaceholderAsync(image, bean.SaleDate);
            _context.Beans.Add(bean);
        }
        else
        {
            // Update existing bean
            await UpdateBeanImageAsync(bean, image);
            _context.Beans.Update(bean);
        }

        await _context.SaveChangesAsync();
    }

    // Save a bean
    public async Task<(bool IsSuccess, string? ErrorMessage)> SaveBeanAsync(Bean bean, IFormFile? image, bool isNew)
    {
        var (isValid, errorMessage) = await ValidateBeanAsync(bean);

        if (!isValid)
        {
            return (false, errorMessage ?? "Validation failed without a specific error message.");
        }

        try
        {
            if (image != null)
            {
                bean.ImagePath = await SaveImageAsync(image, bean.SaleDate);
            }
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message);
        }

        if (isNew)
        {
            _context.Beans.Add(bean);
        }
        else
        {
            _context.Beans.Update(bean);
        }

        await _context.SaveChangesAsync();
        return (true, null);
    }

    // Remove a bean's image
    public async Task<bool> RemoveBeanImageAsync(int id)
    {
        var bean = await GetBeanByIdAsync(id);
        if (bean == null) return false;

        await RemoveImageAsync(bean);
        return true;
    }

    // Delete a bean
    public async Task<bool> DeleteBeanAsync(int id)
    {
        var bean = await GetBeanByIdAsync(id);
        if (bean == null) return false;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await RemoveImageAsync(bean);
            _context.Beans.Remove(bean);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Helper methods for image handling
    private async Task<string> SaveImageOrPlaceholderAsync(IFormFile? image, DateOnly saleDate)
    {
        return image != null && image.Length > 0
            ? await SaveImageAsync(image, saleDate)
            : Bean.PlaceholderImagePath;
    }

    private async Task UpdateBeanImageAsync(Bean bean, IFormFile? image)
    {
        if (image != null && image.Length > 0)
        {
            DeleteImageIfExists(bean.ImagePath);
            bean.ImagePath = await SaveImageAsync(image, bean.SaleDate);
        }
        else if (string.IsNullOrEmpty(bean.ImagePath) || bean.ImagePath == Bean.PlaceholderImagePath)
        {
            bean.ImagePath = Bean.PlaceholderImagePath;
        }
    }

    private async Task RemoveImageAsync(Bean bean)
    {
        if (string.IsNullOrEmpty(bean.ImagePath) || bean.ImagePath == Bean.PlaceholderImagePath) return;

        DeleteImageIfExists(bean.ImagePath);
        bean.ImagePath = Bean.PlaceholderImagePath;
        _context.Beans.Update(bean);
        await _context.SaveChangesAsync();
    }

    private async Task<string> SaveImageAsync(IFormFile image, DateOnly saleDate)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(image.FileName).ToLower();

        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new InvalidOperationException("Invalid file type. Only JPG, PNG, and GIF files are allowed.");
        }

        var formattedDate = saleDate.ToString("yyyy-MM-dd");
        var newFileName = $"{formattedDate}_{Path.GetFileNameWithoutExtension(image.FileName)}{fileExtension}";
        var filePath = Path.Combine(_environment.WebRootPath, "images", newFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        return $"/images/{newFileName}";
    }

    private void DeleteImageIfExists(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath) || imagePath == Bean.PlaceholderImagePath) return;

        var relativePath = imagePath.StartsWith("/") ? imagePath.Substring(1) : imagePath;
        var filePath = Path.Combine(_environment.WebRootPath, relativePath);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
