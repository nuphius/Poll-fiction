using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Data.Model
{
    public class GuestChoice
    {
        public int ChoiceId { get; set; }
        public Choice Choice { get; set; }

        public int NumberVote { get; set; } = 0;

        public int GuestId { get; set; }
        public Guest Guest { get; set; }
    }
}
