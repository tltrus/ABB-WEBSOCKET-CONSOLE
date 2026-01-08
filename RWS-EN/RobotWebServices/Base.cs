using System.Net.Http;

namespace RWS_EN.RobotWebServices
{
    public partial class RWSClient
    {
        /// <summary>
        /// Execute HTTP GET request
        /// </summary>
        private async Task<HttpResponseMessage> GetAsync(string endpoint)
        {
            return await _httpClient.GetAsync($"{_baseUrl}{endpoint}");
        }

        /// <summary>
        /// Execute HTTP POST request
        /// </summary>
        private async Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent content)
        {
            return await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);
        }

        /// <summary>
        /// Execute HTTP PUT request
        /// </summary>
        private async Task<HttpResponseMessage> PutAsync(string endpoint, HttpContent content)
        {
            return await _httpClient.PutAsync($"{_baseUrl}{endpoint}", content);
        }

        /// <summary>
        /// Execute HTTP DELETE request
        /// </summary>
        private async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            return await _httpClient.DeleteAsync($"{_baseUrl}{endpoint}");
        }
    }
}