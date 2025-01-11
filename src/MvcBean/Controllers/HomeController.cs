using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcBean.Data;
using MvcBean.Models;

namespace MvcBean.Controllers
{
    public class HomeController(
        MvcBeanContext context
    ) : Controller
    {
        private readonly MvcBeanContext _context = context;

        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            // Check if there's a bean with today's sale date
            var beanOfTheDay = await _context.Beans
                .FirstOrDefaultAsync(b => b.SaleDate == today);

            if (beanOfTheDay != null)
            {
                ViewData["BeanOfTheDay"] = beanOfTheDay;  // Passing it to the view
            }

            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
