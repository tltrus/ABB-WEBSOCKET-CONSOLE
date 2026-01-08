using System.Text.Json;

namespace RWS_EN.RobotWebServices
{
    public partial class RWSClient
    {
        /// <summary>
        /// Get system properties
        /// </summary>
        public async Task<List<SystemProperties>> GetSystemPropertiesAsync()
        {
            var list = new List<SystemProperties>();
            var response = await GetAsync("/rw/system?json=1");
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            if (jsonDoc.RootElement.TryGetProperty("_embedded", out var embedded) &&
                embedded.TryGetProperty("_state", out var stateArray))
            {
                var batch = JsonSerializer.Deserialize<List<SystemProperties>>(stateArray.ToString());
                list.AddRange(batch);
            }

            return list;
        }
    }
}