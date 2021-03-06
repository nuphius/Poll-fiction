using PollFiction.Data.Model;
using PollFiction.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Services.Interfaces
{
    public interface IPollService
    {
        Task<DashboardViewModel> LoadDashboardAsync();

        Task<LinksPollViewModel> SaveCreatePollAsync(CreatePollViewModel poll);

        Task SaveGuestPollAsync(LinksPollViewModel mailGuest);

        Task<(Poll, string, int)> SearchPollByCodeAsync(string code);

        Task<List<Choice>> SearchChoiceAsync(int pollid, int guestId);

        Task<string> SaveChoiceVoteAsync(VotePollViewModel votePoll);

        Task<LinksPollViewModel> DisplayLinksPollAsync(int pollid);

        Task DisablePollAsync(Poll poll);

        Task<StatViewModel> StatOfPollAsync(string code);

        void SendMail(string poll, string mail);
    }
}
