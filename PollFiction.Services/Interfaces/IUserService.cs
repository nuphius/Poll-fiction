using PollFiction.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Services.Interfaces
{
    public interface IUserService
    {
        Task<string> RegisterUserAsync(RegisterViewModel user);

        Task<bool> ConnectUserAsync(string pseudo, string password, bool RemenberMe);

        Task DisconnectAsync();
    }
}
