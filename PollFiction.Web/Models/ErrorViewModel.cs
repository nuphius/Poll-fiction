using System;

namespace PollFiction.Web.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public string error { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
