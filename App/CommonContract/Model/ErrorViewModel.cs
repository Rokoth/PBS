using System;

namespace ProjectBranchSelector.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class MessageModel
    { 
        public string Message { get; set; }
        public bool IsError { get; set; }
    }
}
