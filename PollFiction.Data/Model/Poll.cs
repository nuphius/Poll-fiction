using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Data.Model
{
    public class Poll
    {   
        [Key]
        public int PollId { get; set; }
        [Required]
        [StringLength(maximumLength:255)]
        public string PollTitle { get; set; }
        [Required]
        public DateTime Polldate { get; set; }
        [DefaultValue(false)]
        public bool PollMultiple { get; set; }
        [DefaultValue(false)]
        public bool PollDisable { get; set; }
        public string PollLinkAccess { get; set; }
        public string PollLinkDisable { get; set; }
        public string PollLinkStat { get; set; }

        // relation 1 - n avec la table User
        public User User { get; set; }
        public int UserId { get; set; }

        //relation 1 - n avec la table Choise
        public ICollection<Choice> Choices { get; set; }

        //relation n - n avec la table Guest
        public IList<PollGuest> PollGuests { get; set; }
    }
}
