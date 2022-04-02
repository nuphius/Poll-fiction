﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Data.Model
{
    public class Choice
    {   
        [Key]
        public int ChoiceId { get; set; }
        [Required]
        public string ChoiceText { get; set; }
        public int NumberVote { get; set; } = 0;


        //relation 1 - n avec la table Poll
        [Required]
        public Poll Poll { get; set; } = new Poll();
        [Required]
        public int PollId { get; set; }


        //relation n - n avec la table Guest
        public IList<GuestChoice> GuestChoices { get; set; }
    }
}
