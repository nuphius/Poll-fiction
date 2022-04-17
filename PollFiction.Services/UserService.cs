using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PollFiction.Data;
using PollFiction.Data.Model;
using PollFiction.Services.Interfaces;
using PollFiction.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PollFiction.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _ctx;
        private readonly HttpContext _httpContext;
        public UserService(AppDbContext ctx, IHttpContextAccessor contextAccessor)
        {
            _ctx = ctx;
            _httpContext = contextAccessor.HttpContext;
        }

        public async Task DisconnectAsync()
        {
            await _httpContext.SignOutAsync();
        }

        /// <summary>
        /// Vérification du pseudo et mot de passe, puis connection de 'lutilisateur
        /// avec cookie ou non
        /// </summary>
        /// <param name="pseudo"></param>
        /// <param name="password"></param>
        /// <param name="RemenberMe"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        #region ConnectUserAsync
        public async Task<bool> ConnectUserAsync(string pseudo, string password, bool rememberMe)
        {
            //cryptage
            byte[] pwd = Encoding.UTF8.GetBytes(password);
            byte[] hash = SHA512.HashData(pwd);

            string pwdCrypted = Convert.ToBase64String(hash).ToString();

            User user = await _ctx.Users.FirstOrDefaultAsync(u => u.UserPseudo == pseudo && u.UserPwd == pwdCrypted);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    //new Claim("pseudo", user.UserName),
                    new Claim("name", user.UserName),
                    new Claim("id", user.UserId.ToString())
                };

                //générer avec les claims un objet claimIdentity
                var identity = new ClaimsIdentity(claims, "Cookies");
                //utiliser ce claimsidenty pour faire un claimPrincipal
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                //connecter avec le claimsprincipal
                await _httpContext.SignInAsync(principal, new AuthenticationProperties { IsPersistent = rememberMe });
                //var a = _httpContext.User.Claims.ToList();
                return true;
            }

            return false;
        }
     
        #endregion

        /// <summary>
        /// Fonction de vérification des informations saisie dans le formulaire d'incription
        /// et enregistrement dans la base de donnée
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        #region RegisterUserAsync
        public async Task<string> RegisterUserAsync(RegisterViewModel user)
        {
            //Expression régulière de verification du mail et du mot de passe
            Regex regexMail = new Regex(@"^([\w\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
            Regex regexPwd = new Regex(@"^(?=.{8,}$)(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).*$");

            if (!regexMail.IsMatch(user.Mail))
            {
                return "Merci de saisir une adresse mail valide";
            }
            else if (!regexPwd.IsMatch(user.Password))
            {
                return "Merci de saisir un mot de passe au bon format";
            }
            else if (user.Password != user.ConfirmPwd)
            {
                return "Les mots de passe ne sont pas identiques !";
            }

            //Vérification que le pseudo et mail soient unique (donc pas dans la BDD)
            User checkUserIsExiste = await _ctx.Users
                                               .Select(u => new User
                                               {
                                                   UserPseudo = u.UserPseudo,
                                                   UserMail = u.UserMail
                                               })
                                               .Where(u => u.UserPseudo == user.Pseudo || u.UserMail == user.Mail)
                                               .FirstOrDefaultAsync();
            if (checkUserIsExiste != null)
            {
                return "Pseudo ou mail déjà utilisé";
            }

            //vérification de la saisie d'un nom
            if (string.IsNullOrEmpty(user.Name))
            {
                //création d'une liste de nom
                string[] randomName = new string[] { "Super", "poulet", "Siphon", "Procureur", "Sueur", "United", "Gencives", "Aiguille",
                    "Examen", "Exalté", "Promenade", "Koala", "Toilette", "Myope", "Inondation", "Canari", "Gemme", "Polygone", "Vie", "Monocle", "Treuil"};

                //récuprération de la taille de la liste
                int nbRandomName = randomName.Length - 1;

                Random rand = new Random();
                //création d'un nom aléatoire à partir de la liste et de nombre aléatoire
                string newName = randomName[rand.Next(0, nbRandomName)] + rand.Next(1, 999).ToString();
                user.Name = newName;
            }

            //Cryptage du mot de passe
            Task<string> cryptString = Task.Factory.StartNew(() =>
            {
                byte[] pwd = Encoding.UTF8.GetBytes(user.Password);
                byte[] hash = SHA512.HashData(pwd);

                return Convert.ToBase64String(hash).ToString();
            });

            string pwdCrypt = cryptString.Result.ToString();

            //Création du nouvel utilisateur
            User newUser = new User()
            {
                UserMail = user.Mail,
                UserName = user.Name,
                UserPseudo = user.Pseudo,
                UserPwd = pwdCrypt
            };

            await _ctx.Users.AddAsync(newUser);
            await _ctx.SaveChangesAsync();

            return "";
        }
        #endregion
    }
}
