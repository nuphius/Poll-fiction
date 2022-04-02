using PollFiction.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Services.Models
{
    public class VotePollViewModel
    {
        public int ChoiceId { get; set; }
        public Poll Poll { get; set; }
        public List<Choice> Choices { get; set; }
    }
}
