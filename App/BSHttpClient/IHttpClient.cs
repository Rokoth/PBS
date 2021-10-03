using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBranchSelector.BSHttpClient
{
    public interface IBSHttpClient<TS> where TS : IHttpClientSettings
    {
        bool IsConnected { get; }
        void ConnectToServer(string server, Action<bool, bool, string> onResult);
        Task<(int, IEnumerable<T>)> Get<T>(string param, Type apiType = null) where T : class;
        Task<T> Get<T>(Guid id) where T : class;
        Task<TResp> Post<TReq, TResp>(TReq entity) where TResp : class;
        Task<TResp> Put<TReq, TResp>(TReq entity) where TResp : class;
        Task<T> Delete<T>(Guid id) where T : class;
        event EventHandler OnConnect;
    }

    
}