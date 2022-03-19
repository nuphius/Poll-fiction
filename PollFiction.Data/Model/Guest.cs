using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Data.Model
{
    public class Guest
    {
        [Key]
        public int GuestId { get; set; }
        [Required]
        public string GuestMail { get; set; }

        //relation n - n avec la table Poll
        public IList<PollGuest> PollGuests { get; set; }

        //relation n - n avec la table Choice
        public IList<GuestChoice> GuestChoices { get; set; }
    }
}
