using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PollFiction.Services.Interfaces;
using PollFiction.Services.Models;
using PollFiction.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PollFiction.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserService _userService;

        public HomeController(ILogger<HomeController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            RegisterViewModel model = new RegisterViewModel();
            model.Error = "";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            string rst = await _userService.RegisterUserAsync(model);

            if (String.IsNullOrEmpty(rst))
            {
                return RedirectToAction(nameof(Login));
            }
            else
            {
                model.Error = rst;
                return View(model);
            } 
        }

        public IActionResult Login()
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
