using Microsoft.EntityFrameworkCore;
using PollFiction.Data;
using PollFiction.Data.Model;
using PollFiction.Services.Interfaces;
using PollFiction.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PollFiction.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _ctx;
        public UserService(AppDbContext ctx)
        {
            _ctx = ctx;
        }
        public async Task<string> RegisterUserAsync(RegisterViewModel user)
        {
            //Expression régulière de verification du mail et du mot de passe
            Regex regexMail = new Regex(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
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
                string newName = randomName[rand.Next(0, nbRandomName)] + rand.Next(1,999).ToString();
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
    }
}
