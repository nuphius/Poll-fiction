using PollFiction.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Services.Models
{
    public class StatViewModel
    {
        public Poll Poll { get; set; }
        public List<Choice> Choices { get; set; }
        public List<GuestChoice> GuestChoices { get; set; }

        public List<StatChoice> statChoices { get; set; } = new List<StatChoice>();
    }

    public class StatChoice
    {
        public string StringChoice { get; set; }
        public int ScoreChoice { get; set; }
    }
}
