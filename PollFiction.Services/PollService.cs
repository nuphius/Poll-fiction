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
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;

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
        public async Task<DashboardViewModel> LoadDashboardAsync()
        {
            //ont récupère le guestId
            int guestId = await _ctx.Guests.Where(g => g.GuestMail.Equals(_user.UserMail))
                                           .Select(g => g.GuestId)
                                           .FirstOrDefaultAsync();

            var polls = await _ctx.Polls
                                .Include(p => p.PollGuests)
                                .Include(p => p.Choices)
                                .OrderByDescending(p => p.Polldate)
                                .ToListAsync();

            DashboardViewModel model = new DashboardViewModel();

            foreach (var poll in polls)
            {
                string voted = string.Empty;

                var pollVote = _ctx.Choices
                                .Include(p => p.GuestChoices)
                                .Where(x => x.GuestChoices.Any(y => y.GuestId == guestId) && x.PollId == poll.PollId)
                                .ToList();

                if (pollVote.Count != 0)
                {
                    voted = "(voté !)";
                }

                ListPollViewModel pollForDashboard = new ListPollViewModel
                {
                    PollCreator = poll.UserId == _userId ? poll : null,
                    PollCreatorVote = voted,
                    PollGuest = poll.PollGuests.FirstOrDefault(x => x.GuestId.Equals(guestId)) != null && poll.UserId != _userId ? poll : null,
                    PollGuestVote = voted
                };

                model.listPollViewModels.Add(pollForDashboard);
            }

            return model;
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
            List<Guest> guests = _ctx.Guests.Select(x => new Guest
            {
                GuestId = x.GuestId,
                GuestMail = x.GuestMail
            }).ToList();

            Guest issetMailInBdd;
            //invitation du créateur du sondage afin qu'il puisse voter
            if (mailGuest.GuestMails != null)
            {
                mailGuest.GuestMails.Add(_user.UserMail);

                // List<PollGuest> pollGuests = new List<PollGuest>();

                foreach (var mail in mailGuest.GuestMails)
                {
                    //issetMailInBdd = _ctx.Guests.FirstOrDefault<Guest>(g => g.GuestMail.Equals(mail));
                    issetMailInBdd = guests.Where(x => x.GuestMail.Equals(mail)).FirstOrDefault();


                    if (issetMailInBdd == null)
                    {
                        _ctx.Add(new PollGuest
                        {
                            PollId = mailGuest.PollId,
                            Guest = new Guest
                            {
                                GuestMail = mail
                            }
                        });
                        //_ctx.Add(pollGuest);
                        //await _ctx.SaveChangesAsync();

                        string linkPoll = _ctx.Polls.Where(x => x.PollId.Equals(mailGuest.PollId)).Select(y => y.PollLinkAccess).FirstOrDefault();

                        string to = mail; //To address    
                        string from = "alsc-adaitp21-bmi@ccicampus.fr"; //From address    
                        MailMessage message = new MailMessage(from, to);

                        string mailbody = "Merci de participer au sondage bande de noob !!!!!\n lien du sondage :" +
                            "<a href=\"https://" + _httpContext.Request.Host.Value + "/Poll/Vote?code=" + linkPoll + "\" title=\"Aller au sonadge\"/> " +
                            "https://" + _httpContext.Request.Host.Value + "/Poll/Vote?code=" + linkPoll + " </a> " +
                            "<p>Pour accéder au sondage vous devez avoir un compte créé avec le mail : " + mail + "</p>";
                        message.Subject = "BRAVO ! Vous venez d'être invité a un sondage";
                        message.Body = mailbody;
                        message.BodyEncoding = Encoding.UTF8;
                        message.IsBodyHtml = true;
                        SmtpClient client = new SmtpClient("smtp.office365.com", 587); //Gmail smtp    
                        System.Net.NetworkCredential basicCredential1 = new
                        System.Net.NetworkCredential("alsc-adaitp21-bmi@ccicampus.fr", "Irzv4885");
                        client.EnableSsl = true;
                        client.UseDefaultCredentials = false;
                        client.Credentials = basicCredential1;
                        try
                        {
                            client.Send(message);
                        }

                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                    else
                    {
                        await _ctx.AddAsync(new PollGuest
                        {
                            PollId = mailGuest.PollId,
                            GuestId = issetMailInBdd.GuestId
                        });
                    }
                }
                await _ctx.SaveChangesAsync();
            }
        }
        #endregion

        public async Task<(Poll, string, int)> SearchPollByCodeAsync(string code)
        {
            //on récupere le Poll qui correspond au code
            Poll poll = await _ctx.Polls
                                .Include(p => p.Choices)
                                .ThenInclude(c => c.GuestChoices)
                                .Include(p => p.User)
                                .Include(p => p.PollGuests)
                                .Where(p => p.PollLinkAccess.Equals(code) ||
                                              p.PollLinkDisable.Equals(code) ||
                                              p.PollLinkStat.Equals(code)).FirstOrDefaultAsync();
            if (poll != null)
            {
                if (poll.PollLinkStat == code)
                {
                    return (poll, "Stats", 0);
                }
            }
            else
            {
                return (null, null, 0);
            }


            //on verifie que l'utilisateur  est un GuestId
            if (_httpContext.User.Claims.FirstOrDefault(u => u.Type.Equals("name")) == null)
            {
                return (null, null, 0);
            }

            var guestId = await _ctx.Guests
                .Where(u => u.GuestMail.Equals(_user.UserMail))
                .Select(u => u.GuestId)
                .FirstOrDefaultAsync();



            //on verifie que le Guest soit invité a ce sondage
            if (guestId != 0 && !poll.PollDisable)
            {
                var isGuest = await _ctx.PollGuests.Where(g => g.PollId.Equals(poll.PollId) && g.GuestId.Equals(guestId))
                                                   .FirstOrDefaultAsync();

                //on verifie que la personne est invité et quel type de code c'est
                if (isGuest != null)
                {
                    if (poll.PollLinkAccess == code)
                        return (poll, "Vote", guestId);
                    else
                    {
                        await DisablePollAsync(poll);
                        return (poll, "disable", 0);
                    }

                }
                else
                    return (null, null, 0);
            }
            else
                return (null, null, 0);
        }

        public async Task<List<Choice>> SearchChoiceAsync(int pollid, int guestId)
        {
            return await _ctx.Choices
                .Include(p => p.GuestChoices)
                .Where(c => c.PollId == pollid)
                .Select(c => new Choice
                {
                    ChoiceId = c.ChoiceId,
                    ChoiceText = c.ChoiceText,
                    PollId = c.PollId,
                    GuestChoices = c.GuestChoices.Where(x => x.GuestId == guestId).ToList()
                }).ToListAsync();
        }

        public async Task<string> SaveChoiceVoteAsync(VotePollViewModel votePoll)
        {
            var choices = _ctx.Choices
                            .Include(p => p.GuestChoices)
                            .Where(x => x.GuestChoices.Any(y => y.GuestId == votePoll.GuestId) && x.PollId == votePoll.PollId)
                            .Select(c => new Choice
                            {
                                ChoiceId = c.ChoiceId,
                                ChoiceText = c.ChoiceText,
                                PollId = c.PollId,
                                GuestChoices = c.GuestChoices.Where(x => x.GuestId == votePoll.GuestId).ToList()
                            }).ToList();

            if (choices.Count != 0)
            {

                if (votePoll.CheckChoice == null)
                {
                    choices[0].GuestChoices[0].ChoiceId = votePoll.ChoiceId;
                    _ctx.Update(choices[0].GuestChoices[0]);
                }
                else
                {
                    foreach (var choice in choices)
                    {
                        _ctx.Remove(choice.GuestChoices[0]);
                    }

                    foreach (var newChoice in votePoll.CheckChoice)
                    {
                        var newAddVote = new GuestChoice
                        {
                            ChoiceId = newChoice,
                            GuestId = votePoll.GuestId
                        };
                        await _ctx.AddAsync(newAddVote);
                    }
                }
            }
            else
            {
                if (votePoll.CheckChoice != null)
                {
                    foreach (var item in votePoll.CheckChoice)
                    {
                        var newAddVote = new GuestChoice
                        {
                            ChoiceId = item,
                            GuestId = votePoll.GuestId
                        };
                        _ctx.Add(newAddVote);
                    }
                }
                else
                {
                    var newAddVote = new GuestChoice
                    {
                        ChoiceId = votePoll.ChoiceId,
                        GuestId = votePoll.GuestId
                    };
                    _ctx.Add(newAddVote);
                }
            }

            await _ctx.SaveChangesAsync();

            return await _ctx.Polls.Where(x => x.PollId.Equals(votePoll.PollId)).Select(x => x.PollLinkStat).FirstOrDefaultAsync();
        }

        public async Task<LinksPollViewModel> DisplayLinksPollAsync(int pollid)
        {
            LinksPollViewModel linksPoll = await _ctx.Polls
                                               .Where(p => p.PollId == pollid && p.UserId.Equals(_userId))
                                               .Select(p => new LinksPollViewModel
                                               {
                                                   LinkDelete = "https://" + _httpContext.Request.Host.Value + @"/Poll/Vote?code=" + p.PollLinkDisable,
                                                   LinkPoll = "https://" + _httpContext.Request.Host.Value + @"/Poll/Vote?code=" + p.PollLinkAccess,
                                                   LinkStat = "https://" + _httpContext.Request.Host.Value + @"/Poll/Vote?code=" + p.PollLinkStat,
                                                   PollId = pollid
                                               }).FirstOrDefaultAsync();
            return linksPoll;
        }

        public async Task DisablePollAsync(Poll poll)
        {
            poll.PollDisable = true;
            //_ctx.Update(poll);

            await _ctx.SaveChangesAsync();
        }

        public async Task<StatViewModel> StatOfPollAsync(string code)
        {
            var stat = await _ctx.Polls
                            .Include(p => p.Choices)
                            .ThenInclude(c => c.GuestChoices)
                            .Include(p => p.PollGuests)
                            .Include(p => p.User)
                            .Where(p => p.PollLinkStat == code)
                            .FirstOrDefaultAsync();

            if (stat != null)
            {
                StatViewModel statViewModel = new StatViewModel
                {
                    Poll = stat,
                    Choices = stat.Choices.ToList(),
                    GuestChoices = stat.Choices.SelectMany(x => x.GuestChoices, (a, b) => new GuestChoice
                    {
                        ChoiceId = b.ChoiceId,
                        GuestId = b.GuestId,
                        GuestChoiceId = b.GuestChoiceId
                    }).ToList()
                };


                List<StatChoice> tempStat = new List<StatChoice>();

                foreach (var choice in stat.Choices)
                {
                    StatChoice statChoice = new StatChoice
                    {
                        StringChoice = choice.ChoiceText,
                        ScoreChoice = choice.GuestChoices.Count
                    };

                    tempStat.Add(statChoice);
                }

                statViewModel.statChoices = tempStat.OrderByDescending(x => x.ScoreChoice).ToList();

                return statViewModel;
            }

            return null;
        }
    }
}
