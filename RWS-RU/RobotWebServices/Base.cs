
namespace RWS.RobotWebServices
{
    public partial class RWSClient
    {
        /// <summary>
        /// Выполнение HTTP GET запроса
        /// </summary>
        private async Task<HttpResponseMessage> GetAsync(string endpoint)
        {
            return await _httpClient.GetAsync($"{_baseUrl}{endpoint}");
        }

        /// <summary>
        /// Выполнение HTTP POST запроса
        /// </summary>
        private async Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent content)
        {
            return await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);
        }

        /// <summary>
        /// Выполнение HTTP PUT запроса
        /// </summary>
        private async Task<HttpResponseMessage> PutAsync(string endpoint, HttpContent content)
        {
            return await _httpClient.PutAsync($"{_baseUrl}{endpoint}", content);
        }

        /// <summary>
        /// Выполнение HTTP DELETE запроса
        /// </summary>
        private async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            return await _httpClient.DeleteAsync($"{_baseUrl}{endpoint}");
        }
    }


}
