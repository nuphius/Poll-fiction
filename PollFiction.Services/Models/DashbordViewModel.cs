using PollFiction.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Services.Models
{
    public class DashbordViewModel
    {
        public User user { get; set; }
        public List<Poll> Polls { get; set; }
    }
}
