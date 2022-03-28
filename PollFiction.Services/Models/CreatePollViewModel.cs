using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Services.Models
{
    public class CreatePollViewModel
    {
        [Required(ErrorMessage ="Titre obligatoire")]
        public string Titre { get; set; }
        [Required(ErrorMessage ="Description obligatoire")]
        public string Description { get; set; }
        [Display(Name ="Permettre le choix multiple")]
        public bool Multiple { get; set; }
        [Required(ErrorMessage ="Question obligatoire")]
        public List<string> Choices { get; set; }
    }
}
