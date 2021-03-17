using Microsoft.Extensions.Options;

namespace ProjectBranchSelector.Common
{
    public enum ClientMode
    { 
      ThickPriority = 1,
      ThinPriority = 2,
      ThickOnly = 3,
      ThinOnly = 4
    }

    public class CommonOptions 
    {
        public string ConnectionString { get; set; }
        public ClientMode ClientMode { get; set; }
    }
}
