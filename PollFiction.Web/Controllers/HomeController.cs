using Microsoft.AspNetCore.Authorization;
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
        private readonly IPollService _pollService;

        public HomeController(ILogger<HomeController> logger, IUserService userService, IPollService pollService)
        {
            _logger = logger;
            _userService = userService;
            _pollService = pollService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register(string returnUrl)
        {
            RegisterViewModel model = new RegisterViewModel();

            if (returnUrl != null)
                model.returnUrl = returnUrl;

            model.Error = "";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            string rst = await _userService.RegisterUserAsync(model);

            if (String.IsNullOrEmpty(rst))
            {
                return RedirectToAction(nameof(Login), new { returnUrl = model.returnUrl});
            }
            else
            {
                model.Error = rst;
                return View(model);
            } 
        }
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            LoginViewModel model = new LoginViewModel();
            model.ReturnUrl = returnUrl;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            bool login = await _userService.ConnectUserAsync(model.Pseudo, model.Password, model.RememberMe);

            if (login)
            {
                model.Error = "";
                if (!string.IsNullOrEmpty(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    return RedirectToAction(nameof(Dashboard), "Poll");
                }
            }
            else
            {
                model.Error = "Login ou mot de passe incorrect !";
                return View(model);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _userService.DisconnectAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            var model = _pollService.LoadDashboardAsync();

            return View(model);
        }

        [Authorize]
        public IActionResult CreatePoll()
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
