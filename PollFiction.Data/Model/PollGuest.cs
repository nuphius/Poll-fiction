using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Data.Model
{
    public class PollGuest
    {
        public int PollId { get; set; }
        public Poll Poll { get; set; }

        public int GuestId { get; set; }
        public Guest Guest { get; set; }
    }
}
