using System.Text.Json;

namespace RWS_EN.RobotWebServices
{
    public partial class RWSClient
    {
        /// <summary>
        /// Get controller information
        /// </summary>
        public async Task<List<ControllerInfo>> GetControllerInfoAsync()
        {
            var info = new List<ControllerInfo>();
            var response = await GetAsync("/ctrl?json=1");
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            if (jsonDoc.RootElement.TryGetProperty("_embedded", out var embedded) &&
                embedded.TryGetProperty("_state", out var stateArray))
            {
                var batch = JsonSerializer.Deserialize<List<ControllerInfo>>(stateArray.ToString());
                info.AddRange(batch);
            }

            return info;
        }
    }
}