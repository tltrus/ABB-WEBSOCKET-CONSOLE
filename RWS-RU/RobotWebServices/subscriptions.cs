using System.Text;

namespace RWS.RobotWebServices
{
    public partial class RWSClient
    {
        /// <summary>
        /// Создание подписки на изменения ресурсов
        /// </summary>
        public async Task<string> CreateSubscriptionAsync(Dictionary<string, string> resources)
        {
            var formData = new List<string>();
            int i = 1;

            foreach (var resource in resources)
            {
                formData.Add($"resources={i}");
                formData.Add($"{i}={resource.Key}");
                formData.Add($"{i}-p={resource.Value}");
                i++;
            }

            var content = new StringContent(string.Join("&", formData), Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await PostAsync("/subscription", content);

            if (response.IsSuccessStatusCode && response.Headers.TryGetValues("Location", out var locations))
            {
                return locations.First();
            }

            return null;
        }
    }

}
