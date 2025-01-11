using Microsoft.AspNetCore.Mvc;

namespace MvcBean.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataApiController : ControllerBase
    {
        private readonly Services.BeanService _beanService;

        public DataApiController(Services.BeanService beanService)
        {
            _beanService = beanService ?? throw new ArgumentNullException(nameof(beanService));
        }

        [HttpGet("BeanOfTheDay")]
        public async Task<IActionResult> GetBeanOfTheDay()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            // Retrieve the bean for today's date
            var bean = await _beanService.GetBeanBySaleDateAsync(today);

            if (bean == null)
            {
                return NotFound(new { Message = "No beans available for today's date." });
            }

            var imageUrl = $"{Request.Scheme}://{Request.Host}{bean.ImagePath}";

            return Ok(new
            {
                bean.Id,
                bean.Name,
                bean.ColourHex,
                bean.Aroma,
                bean.PricePer100g,
                bean.SaleDate,
                Image = imageUrl
            });
        }
    }
}
