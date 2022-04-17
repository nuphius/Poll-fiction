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

        /// <summary>
        /// affichage page index
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// Affichage de la page register avec memorisation de l'url non autorisé
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register(string returnUrl)
        {
            RegisterViewModel model = new RegisterViewModel();

            if (returnUrl != null)
                model.returnUrl = returnUrl;

            model.Error = "";
            return View(model);
        }

        /// <summary>
        /// traitement des données de la page register
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            string rst = await _userService.RegisterUserAsync(model);

            if (String.IsNullOrEmpty(rst))
            {
                //envoi vers la page login si tout et bon
                return RedirectToAction(nameof(Login), new { returnUrl = model.returnUrl});
            }
            else
            {
                //envoi sur la page register avec les infos saisies, si problème
                model.Error = rst;
                return View(model);
            } 
        }

        /// <summary>
        /// affichage de la page login
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            LoginViewModel model = new LoginViewModel();
            model.ReturnUrl = returnUrl;

            return View(model);
        }

        /// <summary>
        /// traitemant des infos pour un connexion au site
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            //on verifie les info de connection
            bool login = await _userService.ConnectUserAsync(model.Pseudo, model.Password, model.RememberMe);

            if (login)
            {
                model.Error = "";
                if (!string.IsNullOrEmpty(model.ReturnUrl))
                {
                    //si connection OK on redirige vers la page demandé au départ
                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // si connection Ok mais pas de page demandé au départ
                    return RedirectToAction("Dashboard", "Poll");
                }
            }
            else
            {
                //erreur dasn les infos de log
                model.Error = "Login ou mot de passe incorrect !";
                return View(model);
            }
        }

        /// <summary>
        /// Fonction pour la deconnection du site
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            await _userService.DisconnectAsync();

            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// affichage du Dashboard, uniquement pour les gens autorisés
        /// </summary>
        /// <returns></returns>
        //[Authorize]
        //public IActionResult Dashboard()
        //{
        //    var model = _pollService.LoadDashboardAsync();

        //    return View(model);
        //}
        
        /// <summary>
        /// Affichage de la page création de sondage (autorisation requise)
        /// </summary>
        /// <returns></returns>
        //[Authorize]
        //public IActionResult CreatePoll()
        //{
        //    return View();
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
