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
        User LoadDashboardAsync();

        Task<bool> SaveCreatePollAsync(CreatePollViewModel poll);
    }
}
