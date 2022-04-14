using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PollFiction.Data.Model;
using PollFiction.Services.Interfaces;
using PollFiction.Services.Models;
using PollFiction.Web.Models;
using System;
using System.Collections.Generic;
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
        public async Task<IActionResult> Dashboard(string error)
        {
            DashboardViewModel model = await _pollService.LoadDashboardAsync();

            if (!string.IsNullOrEmpty(error))
                model.Error=error;

            return View(model);
        }

        [Authorize,HttpGet]
        public IActionResult CreatePoll()
        {
            return View();
        }

        [Authorize,HttpPost]
        public async Task<IActionResult> CreatePoll(CreatePollViewModel model)
        {
            //bool rst = await _pollService.SaveCreatePollAsync(model);
            Poll rst = await _pollService.SaveCreatePollAsync(model);

            if (rst != null)
            {
                LinksPollViewModel links = new LinksPollViewModel
                {
                    LinkDelete = "https://"+ Request.Host.Value + @"/Poll/Vote?code="+rst.PollLinkDisable,
                    LinkPoll = "https://" + Request.Host.Value + @"/Poll/Vote?code=" + rst.PollLinkAccess,
                    LinkStat = "https://" + Request.Host.Value + @"/Poll/Vote?code=" + rst.PollLinkStat,
                    PollId = rst.PollId                   
                };

                return View("LinksPoll",links);
            }

            return View(model);
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> LinksPoll(int pollId)
        {
            LinksPollViewModel model = await _pollService.DisplayLinksPollAsync(pollId);

            if (model != null)
                return View(model);
            else
                return RedirectToAction(nameof(Dashboard), new {error = "Vous n'étes pas le créateur de ce sondage !" });
        }

        [Authorize, HttpPost]
        public IActionResult LinksPoll(LinksPollViewModel model)
        {
            _pollService.SaveGuestPollAsync(model);

            return RedirectToAction(nameof(Dashboard), new { error = "Votre sondage est créé, invitations envoyées"});
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> Vote(string code)
        {
            (Poll poll, string view, int guestId) = await _pollService.SearchPollByCodeAsync(code);

            if (poll != null && view != null)
            {
                List<Choice> choices = await _pollService.SearchChoiceAsync(poll.PollId, guestId);
                VotePollViewModel model = new VotePollViewModel
                {
                    Choices = choices,
                    Poll = poll,
                    GuestId = guestId
                };

                if (view == "Stats")
                {
                    return RedirectToAction(nameof(Stats), new { codeStat = code }); 
                }
                else if(view == "disable")
                {
                    return RedirectToAction(nameof(Dashboard));
                }

                return View(model);
            }
            else if (view == null)
            {
                //message Erreur si le code du sondage est faux ou que la personne n'est pas invité

                    return RedirectToAction(nameof(Dashboard), new { error = "Merci de vérifier votre code sondage !" }); 
            } 
            else
                return View(nameof(Error));
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> Vote(VotePollViewModel model)
        {
            if (TempData.ContainsKey("poll"))
                model.PollId = Convert.ToInt32(TempData["poll"]);
            if (TempData.ContainsKey("guestId"))
                model.GuestId = Convert.ToInt32(TempData["guestId"].ToString());

            var link = await _pollService.SaveChoiceVoteAsync(model);

            return RedirectToAction(nameof(Stats), new { codeStat = link} );
        }

        public async Task<IActionResult> Stats(string codeStat)
        {
            StatViewModel model = await _pollService.StatOfPollAsync(codeStat);

            return View(model);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
