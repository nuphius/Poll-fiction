using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        private readonly int _userId;

        
        public PollService(AppDbContext ctx, IHttpContextAccessor contextAccessor)
        {
            _ctx = ctx;
            _httpContext = contextAccessor.HttpContext;

            var idCookie = _httpContext.User.Claims.FirstOrDefault(u => u.Type.Equals("id"));

            if (idCookie != null)
            {
                _userId = Convert.ToInt32(idCookie.Value);
            }
            
        }

        /// <summary>
        /// Chargement de infos des sondage pour le tableau de bord
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="contextAccessor"></param>
        #region PollService
        public async Task<List<Poll>> LoadDashboardAsync()
        {
            var poll = await _ctx.Polls.Select(p => new Poll
                                                        {
                                                            UserId = _userId,
                                                            PollId = p.PollId,
                                                            Choices = p.Choices,
                                                            Polldate = DateTime.Now,
                                                            PollDescription = p.PollDescription,
                                                            PollDisable = p.PollDisable,
                                                            PollGuests = p.PollGuests,
                                                            PollLinkAccess = p.PollLinkAccess,
                                                            PollLinkDisable = p.PollLinkDisable,
                                                            PollLinkStat = p.PollLinkStat,
                                                            PollMultiple = p.PollMultiple,
                                                            PollTitle = p.PollTitle,
                                                            User = p.User
                                                        }).Where(p => p.UserId == _userId).ToListAsync();


            return poll;
        }
        #endregion

        /// <summary>
        /// Enregristement de sondage crée dans la BDD
        /// </summary>
        /// <param name="poll"></param>
        /// <returns></returns>
        #region SaveCreatePollAsync
        public async Task<Poll> SaveCreatePollAsync(CreatePollViewModel poll)
        {
            var mail = _ctx.Users.Where(u => u.UserId.Equals(_userId)).Select(s => s.UserMail);

            Poll pollDb = new Poll
            {
                PollTitle = poll.Titre,
                Polldate = DateTime.Now,
                PollMultiple = poll.Multiple,
                UserId = _userId,
                PollDescription = poll.Description,
                PollDisable = false,
                PollLinkAccess = Guid.NewGuid().ToString().Replace("-", "").ToUpper(),
                PollLinkDisable = Guid.NewGuid().ToString().Replace("-", "").ToUpper(),
                PollLinkStat = Guid.NewGuid().ToString().Replace("-", "").ToUpper(),
                Choices = poll.Choices.Select(c => new Choice
                {
                    ChoiceText = c
                }).ToList()
            };

            await _ctx.AddAsync(pollDb);
            await _ctx.SaveChangesAsync();

            //List<Choice> listChoices = new List<Choice>();

            //foreach (var item in poll.Choices)
            //{
            //    Choice choiceDb = new Choice
            //    {
            //        PollId = pollDb.PollId,
            //        ChoiceText = item, 
            //        Poll=pollDb
            //    };

            //    listChoices.Add(choiceDb);
            //}

            //await _ctx.AddRangeAsync(listChoices);
            //await _ctx.SaveChangesAsync();

            return pollDb;
        }
        #endregion

        /// <summary>
        /// Enregistrement dans la BDD des Guest a un sondage
        /// </summary>
        /// <param name="mailGuest"></param>
        /// <returns></returns>
        #region SaveGuestPollAsync
        public async Task SaveGuestPollAsync(LinksPollViewModel mailGuest)
        {
            List<PollGuest> pollGuests = new List<PollGuest>();

            foreach (var mail in mailGuest.GuestMails)
            {
                PollGuest pollGuest = new PollGuest
                {
                    PollId = mailGuest.PollId,
                    Guest = new Guest
                    {
                        GuestMail = mail
                    }
                };

                await _ctx.AddAsync(pollGuest);
            }

            await _ctx.SaveChangesAsync();
        }
        #endregion

        public async Task<(Poll,string)> SearchPollByCodeAsync(string code)
        {
            Poll poll = await _ctx.Polls.FirstOrDefaultAsync<Poll>(p => p.PollLinkAccess.Equals(code));

            if (poll == null)
            {
                poll = await _ctx.Polls.FirstOrDefaultAsync<Poll>(p => p.PollLinkDisable.Equals(code));
                //ici désactiver le sondage
                return (poll, null);
            }   
            else if (poll == null)
            {
                poll = await _ctx.Polls.FirstOrDefaultAsync<Poll>(p => p.PollLinkStat.Equals(code));
                return (poll, "Stat");
            }
            else if (poll == null)
                return (null, null);
            else
                return (poll, "Vote");
        }

        public async Task<List<Choice>> SearchChoiceAsync(int pollid)
        {
            return await _ctx.Choices.Where(choice => choice.PollId == pollid).ToListAsync();
        }
    }
}
