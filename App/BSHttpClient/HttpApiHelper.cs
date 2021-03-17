using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBranchSelector.BSHttpClient
{
    public static class HttpApiHelper
    {
        public static StringContent SerializeRequest<TReq>(this TReq entity)
        {
            var json = JsonConvert.SerializeObject(entity);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            return data;
        }

        public static async Task<TResp> ParseResponse<TResp>(this HttpResponseMessage result) where TResp : class
        {
            if (result != null && result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();
                return JObject.Parse(response).ToObject<TResp>();
            }
            return null;
        }

        public static async Task<(int, IEnumerable<T>)> ParseResponseArray<T>(this HttpResponseMessage result) where T : class
        {
            if (result != null && result.IsSuccessStatusCode)
            {
                var ret = new List<T>();
                var response = await result.Content.ReadAsStringAsync();
                foreach (var item in JArray.Parse(response))
                {
                    ret.Add(item.ToObject<T>());
                }
                int count = 0;
                if (result.Headers.TryGetValues("x-pages", out IEnumerable<string> pageHeaders))
                {
                    int.TryParse(pageHeaders.FirstOrDefault(), out count);
                }
                return (count, ret);
            }
            return (0, null);
        }
    }
}
