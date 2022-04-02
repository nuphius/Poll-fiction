using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Data.Model
{
    public class GuestChoice
    {
        [Key]
        public int GuestChoiceId { get; set; }
        public int ChoiceId { get; set; }
        public Choice Choice { get; set; }

        public int GuestId { get; set; }
        public Guest Guest { get; set; }
    }
}
