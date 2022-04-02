using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Services.Models
{
    public class LinksPollViewModel
    {
        public int PollId { get; set; }
        [Display(Name ="Lien d'acces au sondage")]
        public string LinkPoll { get; set; }
        [Display(Name = "Lien d'acces aux résultats su sondage")]
        public string LinkStat { get; set; }
        [Display(Name = "Lien pour désactiver le sondage")]
        public string LinkDelete { get; set; }
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Adresse Mail")]
        [RegularExpression(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "L'E-mail saisie n'est pas valide")]
        public string GuestMail { get; set; }
        public List<string> GuestMails { get; set; }

    }
}
