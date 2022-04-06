using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollFiction.Services.Models
{
    public class DashboardViewModel
    {
        public List<ListPollViewModel> listPollViewModels { get; set; } = new List<ListPollViewModel>();
        public string Error { get; set; }
    }
}
