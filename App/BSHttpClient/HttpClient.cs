using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProjectBranchSelector.BSHttpClient
{
    public class BSHttpClient: IBSHttpClient, IDisposable
    {
        private readonly ILogger<BSHttpClient> _logger;
        private bool isConnected = false;
        private bool isDisposed = false;
        private bool isCheckRun = false;
        private readonly Dictionary<Type, string> _apis;
        private string _server;

        public event EventHandler OnConnect;

        public BSHttpClient(IHttpClientSettings settings, ILogger<BSHttpClient> logger)
        {
            _apis = settings.Apis;            
            _logger = logger;
            if (!string.IsNullOrEmpty(settings.Server))
            {
                _server = settings.Server;
                Task.Factory.StartNew(CheckConnect, TaskCreationOptions.LongRunning);
                isCheckRun = true;
            }
        }

        public void ConnectToServer(string server, Action<bool, bool, string> onResult)
        {            
            CheckConnectOnce(server).ContinueWith(s => {
                if (s.IsFaulted)
                {                    
                    var message = "";
                    if (s.Exception is AggregateException aex)
                    {
                        
                        var stack = "";
                        foreach (var ex in aex.InnerExceptions)
                        {
                            message += ex.Message + "\r\n";
                            stack += ex.StackTrace + "\r\n";
                        }
                        
                        _logger.LogError($"Error in ConnectToServer: {message}; StackTrace: {stack}");
                    }
                    else
                    {
                        message = s.Exception.Message;
                        _logger.LogError($"Error in ConnectToServer: {s.Exception.Message}; StackTrace: {s.Exception.StackTrace}");
                    }
                    onResult?.Invoke(false, true, $"Error in ConnectToServer: {message};");
                }
                else
                {
                    var result = s.Result;
                    onResult?.Invoke(result, false, null);                    
                    if (result)
                    { 
                        _server = server;
                        isConnected = true;
                        if (!isCheckRun)
                        {
                            Task.Factory.StartNew(CheckConnect, TaskCreationOptions.LongRunning);
                            isCheckRun = true;
                        }
                    }
                }
            });            
        }

        public bool IsConnected => isConnected;

        public async Task<(int, IEnumerable<T>)> Get<T>(string param, Type apiType = null) where T : class
        {
            return await Execute(client =>
                client.GetAsync($"{GetApi<T>(apiType)}/{param}"), "Get", s=>s.ParseResponseArray<T>());
        }

        public async Task<T> Get<T>(Guid id) where T : class
        {
            return await Execute(client =>
                client.GetAsync($"{GetApi<T>()}/{id}"), "Get", s => s.ParseResponse<T>());           
        }

        public async Task<TResp> Post<TReq, TResp>(TReq entity)
            where TResp : class
        {
            return await Execute(client =>
                client.PostAsync($"{GetApi<TResp>()}", entity.SerializeRequest()), "Post", s => s.ParseResponse<TResp>());            
        }

        public async Task<TResp> Put<TReq, TResp>(TReq entity) where TResp : class
        {
            return await Execute(client => 
                client.PutAsync($"{GetApi<TResp>()}", entity.SerializeRequest()), "Put", s => s.ParseResponse<TResp>());            
        }

        public async Task<T> Delete<T>(Guid id) where T : class
        {
            return await Execute(client => 
                client.DeleteAsync($"{GetApi<T>()}/{id}"), "Delete", s => s.ParseResponse<T>());
        }
                
        private async Task<T> Execute<T>(
            Func<HttpClient, Task<HttpResponseMessage>> action, 
            string method, 
            Func<HttpResponseMessage, Task<T>> parseMethod)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var result = await action(client);
                    return await parseMethod(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in {method}: {ex.Message}; StackTrace: {ex.StackTrace}");
                    return default;
                }
            }
        }

        private string GetApi<T>(Type apiType = null) where T : class
        {
            return $"{_server}/{_apis[apiType ?? typeof(T)]}";
        }

        private async Task CheckConnect()
        {
            while (!isDisposed)
            {
                var curConnect = isConnected;
                isConnected = await CheckConnectOnce(_server);
                if (isConnected && !curConnect)
                {
                    OnConnect?.Invoke(this, new EventArgs());
                }
                await Task.Delay(1000);
            }
        }

        private async Task<bool> CheckConnectOnce(string server)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var check = await client.GetAsync($"{server}/api/v1/common/ping");
                    var result = check != null && check.IsSuccessStatusCode;
                    _logger.LogInformation($"Ping result: server {server} {(result ? "connected" : "disconnected")}");
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in CheckConnect: {ex.Message}; StackTrace: {ex.StackTrace}");
                    return false;
                }
            }
        }

        public void Dispose()
        {
            isDisposed = true;
        }
    }
}
