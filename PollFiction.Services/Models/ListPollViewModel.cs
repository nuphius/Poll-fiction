using PollFiction.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Services.Models
{
    public class ListPollViewModel
    {
        public Poll PollCreator { get; set; }
        public Poll PollGuest { get; set; }
        public string PollCreatorVote { get; set; }
        public string PollGuestVote { get; set; }
    }
}
