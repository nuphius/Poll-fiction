using Microsoft.AspNetCore.Http;
using PollFiction.Data;
using PollFiction.Data.Model;
using PollFiction.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Services
{
    public class PollService : IPollService
    {
        private readonly AppDbContext _ctx;
        private readonly HttpContext _httpContext;
        public PollService(AppDbContext ctx, IHttpContextAccessor contextAccessor)
        {
            _ctx = ctx;
            _httpContext = contextAccessor.HttpContext;
        }
        public User LoadDashboardAsync()
        {
            var a = _httpContext.User.Claims.ToList();

            User user = new User
            {
                UserPseudo = a[0].Value,
                UserName = a[1].Value,
                UserId = Convert.ToInt32(a[2].Value)
            };

            return user;
        }
    }
}
