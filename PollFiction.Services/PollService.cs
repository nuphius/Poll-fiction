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
        private readonly User _user;

        
        public PollService(AppDbContext ctx, IHttpContextAccessor contextAccessor)
        {
            _ctx = ctx;
            _httpContext = contextAccessor.HttpContext;

            var idCookie = _httpContext.User.Claims.FirstOrDefault(u => u.Type.Equals("id"));

            if (idCookie != null)
            {
                _userId = Convert.ToInt32(idCookie.Value);
                _user = _ctx.Users.FirstOrDefault<User>(u => u.UserId == _userId);
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
            //invitation du créateur du sondage afin qu'il puisse voter
            //mailGuest.GuestMails.Add(_user.UserMail);

            //Guest issetMailInBdd = new Guest();

            //foreach (var mail in mailGuest.GuestMails)
            //{
            //   var mailInBdd = _ctx.Guests.FirstOrDefault<Guest>(g => g.GuestMail == mail);

            //    PollGuest pollGuest = new PollGuest
            //    {
            //        PollId = mailGuest.PollId,
            //        Guest = new Guest
            //        {
            //            GuestMail = mail
            //        }
            //    };

            //    await _ctx.AddAsync(pollGuest);
            //}

            //await _ctx.SaveChangesAsync();

            //invitation du créateur du sondage afin qu'il puisse voter
            mailGuest.GuestMails.Add(_user.UserMail);

            // List<PollGuest> pollGuests = new List<PollGuest>();
            Guest issetMailInBdd;

            foreach (var mail in mailGuest.GuestMails)
            {
                issetMailInBdd = _ctx.Guests.FirstOrDefault<Guest>(g => g.GuestMail.Equals(mail));

                if (issetMailInBdd == null)
                {
                    PollGuest pollGuest = new PollGuest
                    {
                        PollId = mailGuest.PollId,
                        Guest = new Guest
                        {
                            GuestMail = mail
                        }
                    };
                    _ctx.Add(pollGuest);
                }
                else
                {
                    PollGuest pollGuest = new PollGuest
                    {
                        PollId = mailGuest.PollId,
                        GuestId = issetMailInBdd.GuestId
                    };
                    await _ctx.AddAsync(pollGuest);
                }
            }
            await _ctx.SaveChangesAsync();
        }
        #endregion

        public async Task<(Poll,string, int)> SearchPollByCodeAsync(string code)
        {
            //on récupere le Poll qui correspond au code
            Poll poll = await _ctx.Polls.Where(p => p.PollLinkAccess.Equals(code) ||
                                              p.PollLinkDisable.Equals(code) ||
                                              p.PollLinkStat.Equals(code)).FirstOrDefaultAsync();
            if (poll.PollLinkStat == code)
            {
                return (poll, "Stat", 0);
            }


            //on verifie que l'utilisateur  est un GuestId
            var guestId = await _ctx.Guests.Where(u => u.GuestMail.Equals(_user.UserMail)).Select( u => u.GuestId).FirstOrDefaultAsync();

            //on verifie que le Guest soit invité a ce sondage
            if (guestId != 0)
            {
                var isGuest = await _ctx.PollGuests.Where(g => g.PollId.Equals(poll.PollId) && g.GuestId.Equals(guestId)).FirstOrDefaultAsync();

                //on verifie que la personne est invité et quel type de code c'est
                if (isGuest != null)
                {
                    if (poll.PollLinkAccess == code)
                        return (poll, "Vote", guestId);
                    else
                    {
                        return (poll, null, 0);    //sinon c'est un code de désactivation  
                    }
                        
                }
                else
                    return (null, null, 0);
            }
            else
                return (null, null, 0);
        }

        public async Task<List<Choice>> SearchChoiceAsync(int pollid)
        {
            return await _ctx.Choices.Where(choice => choice.PollId == pollid).ToListAsync();
        }

        public async Task<bool> SaveChoiceVoteAsync(VotePollViewModel votePoll)
        {

            var choices = _ctx.Choices
                                .Include(p => p.GuestChoices)
                                .Where(x => x.GuestChoices.Any(y => y.GuestId == votePoll.GuestId))
                                .ToList();

            if(choices != null)
            {

                foreach(var choice in choices)
                {
                    choice.GuestChoices[0].ChoiceId = votePoll.ChoiceId;

                    _ctx.Update(choice);
                }
            }

            await _ctx.SaveChangesAsync();
            return true;
        }
    }
}
