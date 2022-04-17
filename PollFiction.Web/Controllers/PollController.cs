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

        /// <summary>
        /// injection des services
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="userService"></param>
        /// <param name="pollService"></param>
        public PollController(ILogger<HomeController> logger, IUserService userService, IPollService pollService)
        {
            _logger = logger;
            _userService = userService;
            _pollService = pollService;
        }

        /// <summary>
        /// Affichage du tableau de bord
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Dashboard(string error)
        {
            //chargment des information dans le tableau de bord
            DashboardViewModel model = await _pollService.LoadDashboardAsync();

            //test si le message d'erreur est vide
            if (!string.IsNullOrEmpty(error))
                model.Error=error;

            return View(model);
        }

        /// <summary>
        /// Affichage de la page création de sondage
        /// </summary>
        /// <returns></returns>
        [Authorize,HttpGet]
        public IActionResult CreatePoll()
        {
            return View();
        }

        /// <summary>
        /// Traitement des infos saisies pour la création du sondage
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize,HttpPost]
        public async Task<IActionResult> CreatePoll(CreatePollViewModel model)
        {
            //envoi des infos du sondage pour traiment et enregistement puis on les récupèrent
            Poll rst = await _pollService.SaveCreatePollAsync(model);

            if (rst != null)
            {
                //si la création est OK on crée un VewModel pour les 3 liens à afficher
                LinksPollViewModel links = new LinksPollViewModel
                {
                    LinkDelete = "https://"+ Request.Host.Value + @"/Poll/Vote?code="+rst.PollLinkDisable,
                    LinkPoll = "https://" + Request.Host.Value + @"/Poll/Vote?code=" + rst.PollLinkAccess,
                    LinkStat = "https://" + Request.Host.Value + @"/Poll/Vote?code=" + rst.PollLinkStat,
                    PollId = rst.PollId                   
                };

                //envoi vers la vue de suite après la création du sondage
                return View("LinksPoll",links);
            }

            //sinon retour sur la page de création de sondage
            return View(model);
        }

        /// <summary>
        /// Affichage de la page avec les 3 liens et controle de l'accès depuis le Dashboard
        /// </summary>
        /// <param name="pollId"></param>
        /// <returns></returns>
        [Authorize, HttpGet]
        public async Task<IActionResult> LinksPoll(int pollId)
        {
            //on récupere et met en forme les 3 liens, retourne null si pas le droit d'accès
            LinksPollViewModel model = await _pollService.DisplayLinksPollAsync(pollId);

            if (model != null)
                return View(model);
            else
                return RedirectToAction(nameof(Dashboard), new {error = "Vous n'étes pas le créateur de ce sondage !" });
        }

        /// <summary>
        /// Sauvegarde des mails invitation
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize, HttpPost]
        public IActionResult LinksPoll(LinksPollViewModel model)
        {
            //sauvegarde des mail invitation
            _pollService.SaveGuestPollAsync(model);

            return RedirectToAction(nameof(Dashboard), new { error = "Votre sondage est créé, invitations envoyées"});
        }

        /// <summary>
        /// Gestion des l'acces aux pages de vote ou des stat en fonction des autorisations et du code envoyé
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [Authorize, HttpGet]
        public async Task<IActionResult> Vote(string code)
        {
            //récupère les infos du sondage
            (Poll poll, string view, int guestId) = await _pollService.SearchPollByCodeAsync(code);

            if (poll != null && view != null)
            {
                //si existe on récupre les choix et les votes
                List<Choice> choices = await _pollService.SearchChoiceAsync(poll.PollId, guestId);
                VotePollViewModel model = new VotePollViewModel
                {
                    Choices = choices,
                    Poll = poll,
                    GuestId = guestId
                };

                //envoi ver la view en fonction du code saisie
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

        /// <summary>
        /// enregistrement des votes
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize, HttpPost]
        public async Task<IActionResult> Vote(VotePollViewModel model)
        {
            //récupération d'information complémentaire depuis la view
            if (TempData.ContainsKey("poll"))
                model.PollId = Convert.ToInt32(TempData["poll"]);
            if (TempData.ContainsKey("guestId"))
                model.GuestId = Convert.ToInt32(TempData["guestId"].ToString());

            var link = await _pollService.SaveChoiceVoteAsync(model);

            return RedirectToAction(nameof(Stats), new { codeStat = link} );
        }

        //page stats
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
