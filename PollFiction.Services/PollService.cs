using Microsoft.AspNetCore.Http;
using PollFiction.Data;
using PollFiction.Data.Model;
using PollFiction.Services.Interfaces;
using PollFiction.Services.Models;
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

        public async Task<bool> SaveCreatePollAsync(CreatePollViewModel poll)
        {
            Poll pollDb = new Poll
            {
                PollTitle = poll.Titre,
                Polldate = DateTime.Now,
                PollMultiple = poll.Multiple,
                UserId = 5,
                PollDescription = poll.Description,
                PollDisable = false,
                PollLinkAccess = Guid.NewGuid().ToString().Replace("-","").ToUpper(),
                PollLinkDisable = Guid.NewGuid().ToString().Replace("-","").ToUpper(),
                PollLinkStat = Guid.NewGuid().ToString().Replace("-", "").ToUpper()
            };

            await _ctx.AddAsync(pollDb);
            await _ctx.SaveChangesAsync();

            List<Choice> listChoices = new List<Choice>();

            foreach (var item in poll.Choices)
            {
                Choice choiceDb = new Choice
                {
                    PollId = pollDb.PollId,
                    ChoiceText = item, 
                    Poll=pollDb
                };

                listChoices.Add(choiceDb);
            }

            await _ctx.AddRangeAsync(listChoices);
            await _ctx.SaveChangesAsync();

            return true;
        }
    }
}
