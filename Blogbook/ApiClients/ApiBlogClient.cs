using Blogbook.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Blogbook.ApiClients
{
    public static class ApiBlogClient
    {
        public static async Task<List<Post>> GetPostsAsync()
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync("https://sq1-api-test.herokuapp.com/posts");
            string apiResponse = await response.Content.ReadAsStringAsync();

            return JObject.Parse(apiResponse).Value<JArray>("data").ToObject<List<Post>>();
        }
    }
}
