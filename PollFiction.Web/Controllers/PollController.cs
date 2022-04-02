using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PollFiction.Data.Model;
using PollFiction.Services.Interfaces;
using PollFiction.Services.Models;
using PollFiction.Web.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PollFiction.Web.Controllers
{
    public class PollController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserService _userService;
        private readonly IPollService _pollService;

        public PollController(ILogger<HomeController> logger, IUserService userService, IPollService pollService)
        {
            _logger = logger;
            _userService = userService;
            _pollService = pollService;
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var model = await _pollService.LoadDashboardAsync();

            return View(model);
        }

        [HttpGet]
        public IActionResult CreatePoll()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePoll(CreatePollViewModel model)
        {
            //bool rst = await _pollService.SaveCreatePollAsync(model);
            Poll rst = await _pollService.SaveCreatePollAsync(model);

            if (rst != null)
            {
                LinksPollViewModel links = new LinksPollViewModel
                {
                    LinkDelete = "https://"+ Request.Host.Value + @"/Poll/Sondage?code="+rst.PollLinkDisable,
                    LinkPoll = "https://" + Request.Host.Value + @"/Poll/Sondage?code=" + rst.PollLinkAccess,
                    LinkStat = "https://" + Request.Host.Value + @"/Poll/Sondage?code=" + rst.PollLinkStat,
                    PollId = rst.PollId                   
                };

                return View("LinksPoll",links);
                //return RedirectToAction(nameof(Dashboard)); //changer pour linlksppoll
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult LinksPoll(LinksPollViewModel model)
        {
            _pollService.SaveGuestPollAsync(model);

            return RedirectToAction(nameof(Dashboard));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
