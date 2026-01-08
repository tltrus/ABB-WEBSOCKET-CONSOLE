using System.Text.Json;

namespace RWS.RobotWebServices
{
    public partial class RWSClient
    {
        /// <summary>
        /// Получение состояния RAPID задачи
        /// </summary>
        public async Task<RAPIDExecutionState> GetRAPIDExecutionStateAsync()
        {
            var list = new List<RAPIDExecutionState>();
            var response = await GetAsync($"/rw/rapid/execution?json=1");
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            if (jsonDoc.RootElement.TryGetProperty("_embedded", out var embedded) &&
                embedded.TryGetProperty("_state", out var stateArray))
            {
                var batch = JsonSerializer.Deserialize<List<RAPIDExecutionState>>(stateArray.ToString());
                list.AddRange(batch);
            }

            return list[0];
        }

        /// <summary>
        /// Получение состояния TASK
        /// </summary>
        public async Task<RAPIDTaskState> GetTaskStateAsync(string task = "T_ROB1")
        {
            var list = new List<RAPIDTaskState>();
            var response = await GetAsync($"/rw/rapid/tasks/{task}?json=1");
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            if (jsonDoc.RootElement.TryGetProperty("_embedded", out var embedded) &&
                embedded.TryGetProperty("_state", out var stateArray))
            {
                var batch = JsonSerializer.Deserialize<List<RAPIDTaskState>>(stateArray.ToString());
                list.AddRange(batch);
            }

            return list[0];
        }

        /// <summary>
        /// Получение информации о RAPID задачах
        /// </summary>
        public async Task<List<RAPIDTask>> GetRAPIDTasksAsync()
        {
            var response = await GetAsync("/rw/rapid/tasks?json=1");
            if (!response.IsSuccessStatusCode)
                return new List<RAPIDTask>();

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            if (jsonDoc.RootElement.TryGetProperty("_embedded", out var embedded) &&
                embedded.TryGetProperty("_state", out var stateArray))
            {
                return JsonSerializer.Deserialize<List<RAPIDTask>>(stateArray.ToString());
            }

            return new List<RAPIDTask>();
        }

        /// <summary>
        /// Получение значения RAPID переменной
        /// </summary>
        public async Task<RAPIDVariable> GetRAPIDVariableAsync(string task, string module, string variable)
        {
            var list = new List<RAPIDVariable>();
            var response = await GetAsync($"/rw/rapid/symbol/data/RAPID/{task}/{module}/{variable}?json=1");
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            if (jsonDoc.RootElement.TryGetProperty("_embedded", out var embedded) &&
                embedded.TryGetProperty("_state", out var stateArray))
            {
                var batch = JsonSerializer.Deserialize<List<RAPIDVariable>>(stateArray.ToString());
                list.AddRange(batch);
            }

            return list[0];
        }

        /// <summary>
        /// Установка значения RAPID переменной
        /// </summary>
        public async Task<bool> SetRAPIDVariableAsync(string task, string module, string variable, string value)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("value", value)
            });

            var response = await PostAsync($"/rw/rapid/symbol/data/RAPID/{task}/{module}/{variable}?action=set", content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Запуск RAPID программы
        /// </summary>
        public async Task<bool> StartRAPIDProgramAsync()
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("regain", "continue"),
                new KeyValuePair<string, string>("execmode", "continue"),
                new KeyValuePair<string, string>("cycle", "forever"),
                new KeyValuePair<string, string>("condition", "none"),
                new KeyValuePair<string, string>("stopatbp", "disabled"),
                new KeyValuePair<string, string>("alltaskbytsp", "false")
            });

            var response = await PostAsync($"/rw/rapid/execution?action=start", content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Остановка RAPID программы
        /// </summary>
        public async Task<bool> StopRAPIDProgramAsync(string task = "T_ROB1")
        {
            var response = await PostAsync($"/rw/rapid/tasks/{task}/execution?action=stop", new FormUrlEncodedContent(new Dictionary<string, string>()));
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Сброс RAPID программы
        /// </summary>
        public async Task<bool> ResetRAPIDProgramAsync(string task = "T_ROB1")
        {
            var response = await PostAsync($"/rw/rapid/tasks/{task}/execution?action=resetpp", new FormUrlEncodedContent(new Dictionary<string, string>()));
            return response.IsSuccessStatusCode;
        }
    }
}
