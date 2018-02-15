using System;
using MarginTrading.OrderbookAggregator.Models.Settings;
using MarginTrading.OrderbookAggregator.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.OrderbookAggregator.Controllers
{
    [Route("api/[controller]")]
    public class SettingsRootController : Controller
    {
        private readonly ISettingsRootService _settingsRootService;

        public SettingsRootController(ISettingsRootService settingsRootService)
        {
            _settingsRootService = settingsRootService;
        }

        [HttpGet]
        public SettingsRoot Get()
        {
            return _settingsRootService.Get();
        }
        
        [HttpPut]
        public IActionResult Set([FromBody] SettingsRoot settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            _settingsRootService.Set(settings);
            return Ok(new {success = true});
        }
    }
}