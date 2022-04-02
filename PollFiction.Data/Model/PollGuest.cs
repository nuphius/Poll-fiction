using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Data.Model
{
    public class PollGuest
    {
        [Key]
        public int PollGuestId { get; set; }
        public int PollId { get; set; }
        public Poll Poll { get; set; }
        public int GuestId { get; set; }
        public Guest Guest { get; set; }
    }
}
