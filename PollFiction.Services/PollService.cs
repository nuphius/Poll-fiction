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

            //récupération de l'id user dans le cookie
            var idCookie = _httpContext.User.Claims.FirstOrDefault(u => u.Type.Equals("id"));

            //récupération des infos user dans l'instance pour y avoir accès de partout
            if (idCookie != null)
            {
                _userId = Convert.ToInt32(idCookie.Value);
                _user = _ctx.Users.FirstOrDefault<User>(u => u.UserId == _userId);
            }

        }

        /// <summary>
        /// Chargement de infos des sondages pour le tableau de bord
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

            //récupère le poll avec les données liées des autres table, puis trie par date
            var polls = await _ctx.Polls
                                .Include(p => p.PollGuests)
                                .Include(p => p.Choices)
                                .OrderByDescending(p => p.Polldate)
                                .ToListAsync();

            //création des infos pour la vue
            DashboardViewModel model = new DashboardViewModel();

            foreach (var poll in polls)
            {
                string voted = string.Empty;

                var pollVote = _ctx.Choices
                                .Include(p => p.GuestChoices)
                                .Where(x => x.GuestChoices.Any(y => y.GuestId == guestId) && x.PollId == poll.PollId)
                                .ToList();

                //regarde si il a déjà voté
                if (pollVote.Count != 0)
                {
                    voted = "(voté !)";
                }

                //création des polls en fonction de si il est créateur ou non
                ListPollViewModel pollForDashboard = new ListPollViewModel
                {
                    PollCreator = poll.UserId == _userId ? poll : null,
                    PollCreatorVote = voted,
                    PollGuest = poll.PollGuests.FirstOrDefault(x => x.GuestId.Equals(guestId)) != null && poll.UserId != _userId ? poll : null,
                    PollGuestVote = voted
                };

                //Ajoute dans la viewModel
                model.listPollViewModels.Add(pollForDashboard);
            }

            return model;
        }
        #endregion

        /// <summary>
        /// Enregristement de sondage crée dans la BDD et de GUID
        /// </summary>
        /// <param name="poll"></param>
        /// <returns></returns>
        #region SaveCreatePollAsync
        public async Task<LinksPollViewModel> SaveCreatePollAsync(CreatePollViewModel poll)
        {
            //var mail = _ctx.Users.Where(u => u.UserId.Equals(_userId)).Select(s => s.UserMail);

            if (!string.IsNullOrEmpty(poll.Titre) && !string.IsNullOrEmpty(poll.Description) && poll.Choices.Count > 1)
            {
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

                //ajout et sauvegarde dans la BDD du poll crée ci-dessus
                await _ctx.AddAsync(pollDb);
                await _ctx.SaveChangesAsync();


                //création un VewModel pour les 3 liens à afficher
                LinksPollViewModel links = new LinksPollViewModel
                {
                    LinkDelete = "https://" + _httpContext.Request.Host.Value + "/Poll/Vote?code=" + pollDb.PollLinkDisable,
                    LinkPoll = "https://" + _httpContext.Request.Host.Value + "/Poll/Vote?code=" + pollDb.PollLinkAccess,
                    LinkStat = "https://" + _httpContext.Request.Host.Value + "/Poll/Stats?code=" + pollDb.PollLinkStat,
                    PollId = pollDb.PollId
                };

                //envoi vers la vue de suite après la création du sondage
                return links;
            }
            else
            {
                return null;
            }
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
            //récupère le code de participation du poll
            string linkPoll = _ctx.Polls.Where(x => x.PollId.Equals(mailGuest.PollId)).Select(y => y.PollLinkAccess).FirstOrDefault();

            //récupère tous les Guest existant
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

                foreach (var mail in mailGuest.GuestMails)
                {
                    //verifier si se mail est déja existant dans la table des Guest
                    issetMailInBdd = guests.Where(x => x.GuestMail.Equals(mail)).FirstOrDefault();

                    //si existe pas on le crée
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

                        //envoi des mails d'invitation
                        SendMail(linkPoll, mail);
                    }
                    else
                    {
                        var invitations = _ctx.PollGuests.Select(x => new Tuple<int, int>(x.GuestId, x.PollId)).ToList();

                        if (!invitations.Contains(new Tuple<int, int>(issetMailInBdd.GuestId, mailGuest.PollId)))
                        {
                            await _ctx.AddAsync(new PollGuest
                            {
                                PollId = mailGuest.PollId,
                                GuestId = issetMailInBdd.GuestId
                            });

                            //envoi des mails d'invitation
                            SendMail(linkPoll, mail);
                        }
                    }
                }
                await _ctx.SaveChangesAsync();
            }
        }
        #endregion

        /// <summary>
        /// Récupération du sondage par son code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        #region SearchPollByCodeAsync
        public async Task<(Poll, string, int)> SearchPollByCodeAsync(string code)
        {
            code = code.Trim();
            //on récupere le Poll qui correspond au code ainsi que toutes les infos des tables liaisons
            Poll poll = await _ctx.Polls
                                .Include(p => p.Choices)
                                .ThenInclude(c => c.GuestChoices)
                                .Include(p => p.User)
                                .Include(p => p.PollGuests)
                                .Where(p => p.PollLinkAccess.Equals(code) ||
                                              p.PollLinkDisable.Equals(code) ||
                                              p.PollLinkStat.Equals(code)).FirstOrDefaultAsync();

            //on verifie si c'est un code de stat et si le code existe
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


            //on verifie que la personne soit connecté
            if (_httpContext.User.Claims.FirstOrDefault(u => u.Type.Equals("name")) == null)
            {
                return (null, null, 0);
            }

            //on verifie que l'utilisateur  est un GuestId
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
        #endregion

        /// <summary>
        /// Fonction tous les choix d'un sondage et leurs réponses
        /// </summary>
        /// <param name="pollid"></param>
        /// <param name="guestId"></param>
        /// <returns></returns>
        #region SearchChoiceAsync
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
        #endregion


        /// <summary>
        /// Fonction de sauvagarde et de mise à jour des votes
        /// </summary>
        /// <param name="votePoll"></param>
        /// <returns></returns>
        #region SaveChoiceVoteAsync
        public async Task<string> SaveChoiceVoteAsync(VotePollViewModel votePoll)
        {
            //récupération des choix et vote de la BDD
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

            //si different de 0 alors on met a jour les votes sinon on enregistre pour la premiere fois
            if (choices.Count != 0)
            {
                //verifie si checkBox ou radioButton
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
                //enregistrement si c'est des checkBox
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
                //enregistrement si c'est des radio
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

            //save dans la BDD
            await _ctx.SaveChangesAsync();

            //retourne le lien de stat de ce sondage
            return await _ctx.Polls.Where(x => x.PollId.Equals(votePoll.PollId)).Select(x => x.PollLinkStat).FirstOrDefaultAsync();
        }
        #endregion


        /// <summary>
        /// Fonction pour afficher les liens des sondages
        /// </summary>
        /// <param name="pollid"></param>
        /// <returns></returns>
        #region DisplayLinksPollAsync
        public async Task<LinksPollViewModel> DisplayLinksPollAsync(int pollid)
        {
            LinksPollViewModel linksPoll = await _ctx.Polls
                                               .Where(p => p.PollId == pollid && p.UserId.Equals(_userId))
                                               .Select(p => new LinksPollViewModel
                                               {
                                                   LinkDelete = "https://" + _httpContext.Request.Host.Value + "/Poll/Vote?code=" + p.PollLinkDisable,
                                                   LinkPoll = "https://" + _httpContext.Request.Host.Value + "/Poll/Vote?code=" + p.PollLinkAccess,
                                                   LinkStat = "https://" + _httpContext.Request.Host.Value + "/Poll/Stats?code=" + p.PollLinkStat,
                                                   PollId = pollid
                                               }).FirstOrDefaultAsync();
            return linksPoll;
        }
        #endregion


        /// <summary>
        /// Fonction pour desactiver les sondages
        /// </summary>
        /// <param name="poll"></param>
        /// <returns></returns>
        #region DisablePollAsync
        public async Task DisablePollAsync(Poll poll)
        {
            poll.PollDisable = true;
            //_ctx.Update(poll);

            await _ctx.SaveChangesAsync();
        }
        #endregion


        /// <summary>
        /// Fonction pour afficher les stats du sondage via son code stats
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        #region StatOfPollAsync
        public async Task<StatViewModel> StatOfPollAsync(string code)
        {
            //récupere toues les infos en fonction du liens
            var stat = await _ctx.Polls
                            .Include(p => p.Choices)
                            .ThenInclude(c => c.GuestChoices)
                            .Include(p => p.PollGuests)
                            .Include(p => p.User)
                            .Where(p => p.PollLinkStat == code)
                            .FirstOrDefaultAsync();

            //si code existe on construit le ViewModel
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

                //création d'une site des réponses
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

                //trie des réponse
                statViewModel.statChoices = tempStat.OrderByDescending(x => x.ScoreChoice).ToList();

                return statViewModel;
            }

            return null;
        }
        #endregion

        /// <summary>
        /// Fonction d'envoi de mail
        /// </summary>
        /// <param name="linkPoll"></param>
        /// <param name="mail"></param>
        #region SendMail
        public void SendMail(string linkPoll, string mail)
        {

            string to = mail; //To address    
            string from = "alsc-adaitp21-bmi@ccicampus.fr"; //From address    
            MailMessage message = new MailMessage(from, to);

            string mailbody = "Merci de participer au sondage \n lien du sondage :" +
                "<a href=\"https://" + _httpContext.Request.Host.Value + "/Poll/Vote?code=" + linkPoll + "\" title=\"Aller au sonadge\"/> " +
                "https://" + _httpContext.Request.Host.Value + "/Poll/Vote?code=" + linkPoll + " </a> " +
                "<p>Pour accéder au sondage vous devez avoir un compte créé avec le mail : " + mail + "</p>";
            message.Subject = "BRAVO ! Vous venez d'être invité a un sondage";
            message.Body = mailbody;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            SmtpClient client = new SmtpClient("smtp.office365.com", 587); //Gmail smtp    
            System.Net.NetworkCredential basicCredential1 = new
            System.Net.NetworkCredential("alsc-adaitp21-bmi@ccicampus.fr", "PASSWORDDDDDDD");
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
        #endregion
    }
}
