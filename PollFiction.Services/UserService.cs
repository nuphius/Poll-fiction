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
            if (user.Password != user.ConfirmPwd)
            {
                return "Les mots de passe ne sont pas identiques !";
            }

            User checkUserIsExiste = await _ctx.Users
                                               .Select(u => new User
                                               {
                                                   UserPseudo = u.UserPseudo,
                                                   UserMail = u.UserMail
                                               })
                                               .Where(u => u.UserPseudo == user.Pseudo || u.UserMail == user.Mail)
                                               .FirstOrDefaultAsync();

            Task<string> CryptPwd = Task.Factory.StartNew(() => 
            {
                byte[] pwd = Encoding.UTF8.GetBytes(user.Password);
                byte[] hash = SHA512.HashData(pwd);

                return Convert.ToBase64String(hash).ToString();
            });

            var a = CryptPwd.Result.ToString();

            if (checkUserIsExiste != null)
            {
                return "Pseudo ou mail déjà utilisé";
            }
            else
            {
                return "";
            }
        }
    }
}
