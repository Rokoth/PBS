using System;
using System.Collections.Generic;

namespace ProjectBranchSelector.BSHttpClient
{
    public interface IHttpClientSettings
    {
        Dictionary<Type, string> Apis { get; }
        string Server { get; }
    }
}
