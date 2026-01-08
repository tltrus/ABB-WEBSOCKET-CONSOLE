using System.Text.Json;

namespace RWS_EN.RobotWebServices
{
    public partial class RWSClient
    {
        /// <summary>
        /// Read IO signal state
        /// </summary>
        public async Task<IOSignal> GetIOSignalAsync(string signalPath)
        {
            var list = new List<IOSignal>();

            var response = await GetAsync($"/rw/iosystem/signals/{signalPath}?json=1");
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            if (jsonDoc.RootElement.TryGetProperty("_embedded", out var embedded) &&
                embedded.TryGetProperty("_state", out var stateArray))
            {
                var batch = JsonSerializer.Deserialize<List<IOSignal>>(stateArray.ToString());
                list.AddRange(batch);
            }

            return list[0];
        }

        /// <summary>
        /// Set IO signal value
        /// </summary>
        public async Task<bool> SetIOSignalAsync(string signalPath, int value)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("lvalue", value.ToString())
            });

            var response = await PostAsync($"/rw/iosystem/signals/{signalPath}?action=set", content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Get list of all IO signals
        /// </summary>
        public async Task<List<_IOSignal>> GetAllIOSignalsAsync(int limit = 100, int start = 0)
        {
            var list = new List<_IOSignal>();
            int currentStart = start;
            bool hasMore = true;

            while (hasMore)
            {
                var response = await GetAsync($"/rw/iosystem/signals?json=1&limit={limit}&start={currentStart}");
                if (!response.IsSuccessStatusCode)
                    break;

                var content = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(content);

                if (jsonDoc.RootElement.TryGetProperty("_embedded", out var embedded) &&
                    embedded.TryGetProperty("_state", out var stateArray))
                {
                    var batch = JsonSerializer.Deserialize<List<_IOSignal>>(stateArray.ToString());
                    list.AddRange(batch);

                    // Check if there are more signals
                    hasMore = batch.Count == limit;
                    currentStart += limit;
                }
                else
                {
                    hasMore = false;
                }
            }

            return list;
        }
    }
}