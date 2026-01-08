using System.Text.Json;

namespace RWS.RobotWebServices
{
    public partial class RWSClient
    {
        /// <summary>
        /// Получение списка файлов и директорий
        /// </summary>

        public async Task<List<FileSystemItem>> GetFileSystemItemsAsync(string path = "/")
        {
            var response = await GetAsync($"/fileservice/{path.TrimStart('/')}");
            if (!response.IsSuccessStatusCode)
                return new List<FileSystemItem>();

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            if (jsonDoc.RootElement.TryGetProperty("_embedded", out var embedded) &&
                embedded.TryGetProperty("_state", out var stateArray))
            {
                return JsonSerializer.Deserialize<List<FileSystemItem>>(stateArray.ToString());
            }

            return new List<FileSystemItem>();
        }
    }


}
