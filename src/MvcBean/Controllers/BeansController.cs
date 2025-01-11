using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcBean.Models;
using MvcBean.Utilities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MvcBean.Data;

[Authorize(Roles = "Admin")]
public class BeansController : Controller
{
    private readonly BeanService _beanService;
    private readonly ILogger<BeansController> _logger;

    public BeansController(BeanService beanService, ILogger<BeansController> logger)
    {
        _beanService = beanService ?? throw new ArgumentNullException(nameof(beanService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("Retrieving bean list for page {Page} with page size {PageSize}.", page, pageSize);

        var (beans, totalBeans) = await _beanService.GetPaginatedBeansWithCountAsync(page, pageSize);

        ViewData["TotalPages"] = (int)Math.Ceiling((double)totalBeans / pageSize);
        ViewData["CurrentPage"] = page;

        return View(beans);
    }

    public IActionResult Create()
    {
        _logger.LogInformation("Accessing Create form for new bean.");
        return PrepareForm("Create", new Bean
        {
            SaleDate = DateOnly.FromDateTime(DateTime.Now)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Bean bean, IFormFile? image)
    {
        return await HandleSaveAsync(bean, image, isNew: true, nameof(Create));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        _logger.LogInformation("Accessing Edit form for bean with ID {BeanId}.", id);

        var bean = await GetBeanOrNotFoundAsync(id);
        return bean == null ? NotFound() : PrepareForm("Edit", bean);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Bean bean, IFormFile? image)
    {
        if (id != bean.Id) return BadRequest();
        return await HandleSaveAsync(bean, image, isNew: false, nameof(Edit));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveImage(int id)
    {
        if (await _beanService.RemoveBeanImageAsync(id))
        {
            _logger.LogInformation("Successfully removed image for bean with ID {BeanId}.", id);
            return Ok();
        }

        _logger.LogWarning("Failed to remove image for bean with ID {BeanId}.", id);
        return BadRequest("Failed to remove image.");
    }

    public async Task<IActionResult> Delete(int? id)
    {
        _logger.LogInformation("Accessing Delete confirmation view for bean with ID {BeanId}.", id);

        var bean = await GetBeanOrNotFoundAsync(id);
        return bean == null ? NotFound() : View(bean);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (await _beanService.DeleteBeanAsync(id))
        {
            _logger.LogInformation("Successfully deleted bean with ID {BeanId}.", id);
            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning("Failed to delete bean with ID {BeanId}.", id);
        return NotFound();
    }

    private async Task<Bean?> GetBeanOrNotFoundAsync(int? id)
    {
        if (id == null) return null;

        var bean = await _beanService.GetBeanByIdAsync(id);

        if (bean == null)
            _logger.LogWarning("Bean with ID {BeanId} was not found.", id);
        else
            _logger.LogInformation("Retrieved bean with ID {BeanId}.", id);

        return bean;
    }

    private IActionResult PrepareForm(string action, Bean bean)
    {
        ViewData["FormAction"] = action;
        return View(bean);
    }

    private async Task<IActionResult> HandleSaveAsync(Bean bean, IFormFile? image, bool isNew, string viewName)
    {
        var (isSuccess, errorMessage) = await _beanService.SaveBeanAsync(bean, image, isNew);

        if (!isSuccess)
        {
            _logger.LogWarning("Failed to {Action} bean with name {BeanName}. Error: {ErrorMessage}", viewName.ToLower(), bean.Name, errorMessage);
            ModelState.AddModelError("SaleDate", errorMessage ?? "An unknown validation error occurred.");
            return View(bean);
        }

        _logger.LogInformation("Successfully {Action}ed bean with name {BeanName}.", viewName.ToLower(), bean.Name);
        return RedirectToAction(nameof(Index));
    }
}

public class BeanService
{
    private readonly MvcBeanContext _context;

    public BeanService(MvcBeanContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<(IEnumerable<Bean> beans, int totalBeans)> GetPaginatedBeansWithCountAsync(int page, int pageSize)
    {
        var beansQuery = _context.Beans.AsQueryable();

        var totalBeans = await beansQuery.CountAsync();
        var beans = await beansQuery
            .OrderBy(b => b.Name) // Adjust ordering as needed
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (beans, totalBeans);
    }

    public async Task<Bean?> GetBeanByIdAsync(int? id)
    {
        return id == null ? null : await _context.Beans.FindAsync(id);
    }

    public async Task<(bool isSuccess, string? errorMessage)> SaveBeanAsync(Bean bean, IFormFile? image, bool isNew)
    {
        try
        {
            if (isNew)
            {
                await _context.Beans.AddAsync(bean);
            }
            else
            {
                _context.Beans.Update(bean);
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<bool> RemoveBeanImageAsync(int id)
    {
        var bean = await _context.Beans.FindAsync(id);
        if (bean == null || string.IsNullOrEmpty(bean.ImagePath)) return false;

        // Logic to remove image from storage can go here
        bean.ImagePath = null;

        _context.Beans.Update(bean);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteBeanAsync(int id)
    {
        var bean = await _context.Beans.FindAsync(id);
        if (bean == null) return false;

        _context.Beans.Remove(bean);
        await _context.SaveChangesAsync();
        return true;
    }
}
