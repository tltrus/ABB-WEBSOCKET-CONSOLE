using RWS.RobotWebServices;
using System.Net.WebSockets;
using System.Xml.Linq;

namespace RWS
{
    class Program
    {
        private static CancellationTokenSource _cancellationTokenSource;
        private static bool _webSocketRunning = false;
        private static Task? _eventProcessingTask;

        static async Task Main(string[] args)
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();

                // 1. Создание клиента и аутентификация
                var client = new RWSClient("http://127.0.0.1");

                Console.WriteLine("Аутентификация...");
                var authenticated = await client.AuthenticateAsync();

                if (!authenticated)
                {
                    Console.WriteLine("Ошибка аутентификации!");
                    return;
                }

                Console.WriteLine("Аутентификация успешна!");

                // 2. Получение информации о контроллере
                Console.WriteLine("\nИнформация о контроллере:");
                var controllerInfo = await client.GetControllerInfoAsync();
                if (controllerInfo != null && controllerInfo.Count > 1)
                {
                    Console.WriteLine($"Имя: {controllerInfo[1].Name}");
                }

                // 3. Работа с IO сигналами
                Console.WriteLine("\nРабота с IO сигналами:");

                // Чтение сигнала
                var signal = await client.GetIOSignalAsync("DI1");
                if (signal != null)
                {
                    Console.WriteLine($"Сигнал {signal.Name}: Значение={signal.Value}, Состояние={signal.State}");
                }

                // Установка сигнала
                bool setResult = await client.SetIOSignalAsync("DO1", 1);
                Console.WriteLine($"Установка сигнала DO1 в 1: {(setResult ? "Успешно" : "Ошибка")}");

                // Получение всех сигналов
                var allSignals = await client.GetAllIOSignalsAsync();
                Console.WriteLine($"Всего сигналов: {allSignals.Count}");

                // 4. Работа с RAPID
                Console.WriteLine("\nРабота с RAPID:");

                // Получение состояния Task
                var taskName = "T_ROB1";
                var taskState = await client.GetTaskStateAsync(taskName);
                if (taskState != null)
                {
                    Console.WriteLine($"Состояние {taskName}: {taskState.Taskstate}");
                }

                // Получение состояния выполнения
                var rapidState = await client.GetRAPIDExecutionStateAsync();
                if (rapidState != null)
                {
                    Console.WriteLine($"Состояние выполнения: {rapidState.ControllerState}");
                }

                // Получение списка задач
                var tasks = await client.GetRAPIDTasksAsync();
                Console.WriteLine($"Найдено задач: {tasks.Count}");
                foreach (var task in tasks.Take(3))
                {
                    Console.WriteLine($"  Задача: {task.Name}, Состояние: {task.State}, Активна: {task.Active}");
                }

                // Чтение переменной RAPID
                var variable = await client.GetRAPIDVariableAsync("T_ROB1", "MainModule", "nCounter");
                if (variable != null)
                {
                    Console.WriteLine($"Переменная nCounter: {variable.Value}, Тип: {variable.Type}");
                }

                // Установка переменной RAPID
                bool varSetResult = await client.SetRAPIDVariableAsync("T_ROB1", "MainModule", "nCounter", "100");
                Console.WriteLine($"Установка переменной: {(varSetResult ? "Успешно" : "Ошибка")}");

                // Запуск программы
                bool startResult = await client.StartRAPIDProgramAsync();
                Console.WriteLine($"Запуск программы: {(startResult ? "Успешно" : "Ошибка")}");

                // 5. Работа с файловой системой
                Console.WriteLine("\nФайловая система:");
                var files = await client.GetFileSystemItemsAsync("/$home?json=1");
                Console.WriteLine($"Файлов в HOME: {files.Count}");
                foreach (var file in files.Take(5))
                {
                    Console.WriteLine($"  {file.Title} ({file.Type}, {file.Size} байт)");
                }

                // 6. ЗАПУСК ПОДПИСКИ В ФОНОВОМ РЕЖИМЕ
                Console.WriteLine("\nЗапуск подписки в фоновом режиме...");
                var subscriptionResult = await StartWebSocketSubscriptionAsync(client);

                if (subscriptionResult)
                {
                    Console.WriteLine("Подписка запущена в фоновом режиме.");
                }

                // 8. Получение системных свойств
                Console.WriteLine("\nСистемные свойства:");
                var systemProps = await client.GetSystemPropertiesAsync();
                if (systemProps != null)
                {
                    var system = systemProps.FirstOrDefault(a => a.Title == "system");
                    if (system != null)
                    {
                        Console.WriteLine($"Имя системы: {system.Name}");
                        Console.WriteLine($"Версия RobotWare: {system.RWVersion}");
                    }

                    var options = systemProps.FirstOrDefault(a => a.Title == "options");
                    if (options != null && options.Options != null)
                    {
                        Console.WriteLine($"Опции системы (первые 5):");
                        foreach (var option in options.Options.Take(5))
                        {
                            Console.WriteLine($"\t{option.Option}");
                        }
                    }
                }

                Console.ReadKey();

                // 9. ОСТАНОВКА ФОНОВОЙ ПОДПИСКИ ПЕРЕД ВЫХОДОМ
                Console.WriteLine("\nНачинаем остановку программы...");
                await StopWebSocketSubscriptionAsync();

                // Даем время на корректное завершение
                await Task.Delay(1000);

                // 10. Выход из системы
                Console.WriteLine("\nВыход из системы...");
                await client.LogoutAsync();
                client.Dispose();

                Console.WriteLine("\nВсе операции завершены успешно!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                await StopWebSocketSubscriptionAsync();
            }
        }

        /// <summary>
        /// Запуск WebSocket подписки в фоновом режиме
        /// </summary>
        private static async Task<bool> StartWebSocketSubscriptionAsync(RWSClient client)
        {
            try
            {
                Console.WriteLine("Настройка подписки...");
                var resources = new Dictionary<string, string>
                {
                    { "/rw/iosystem/signals/DO1;state", "1" },
                    { "/rw/iosystem/signals/DI1;state", "1" }
                };

                var webSocketUrl = await client.CreateSubscriptionAsync(resources);
                if (string.IsNullOrEmpty(webSocketUrl))
                {
                    Console.WriteLine("Не удалось создать подписку.");
                    return false;
                }

                Console.WriteLine($"WebSocket URL: {webSocketUrl}");

                var wsConnected = await client.ConnectWebSocketAsync(webSocketUrl);
                if (!wsConnected)
                {
                    Console.WriteLine("Не удалось подключиться к WebSocket.");
                    return false;
                }

                Console.WriteLine("WebSocket подключен. Запуск фонового потока обработки событий...");

                _webSocketRunning = true;
                _eventProcessingTask = Task.Run(() =>
                    ProcessWebSocketEventsAsync(client, _cancellationTokenSource.Token));

                await Task.Delay(500);

                Console.WriteLine("Фоновый поток подписки успешно запущен.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запуске подписки: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Метод для обработки событий WebSocket в фоновом потоке
        /// </summary>
        private static async Task ProcessWebSocketEventsAsync(RWSClient client, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"[Фоновая подписка] Поток обработки событий запущен.");

                while (!cancellationToken.IsCancellationRequested && _webSocketRunning)
                {
                    try
                    {
                        // Используем CancellationToken для возможности прерывания
                        var eventData = await client.ReceiveWebSocketEventAsync(cancellationToken);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            Console.WriteLine($"[Фоновая подписка] Получен запрос на отмену.");
                            break;
                        }

                        if (!string.IsNullOrEmpty(eventData))
                        {
                            ProcessEventData(eventData);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        Console.WriteLine($"[Фоновая подписка] Задача отменена.");
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine($"[Фоновая подписка] Операция отменена.");
                        break;
                    }
                    catch (WebSocketException wsEx)
                    {
                        if (wsEx.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely ||
                            wsEx.Message.Contains("closed"))
                        {
                            Console.WriteLine($"[Фоновая подписка] WebSocket соединение закрыто.");
                        }
                        else
                        {
                            Console.WriteLine($"[Фоновая подписка] Ошибка WebSocket: {wsEx.Message}");
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("closed") || ex.Message.Contains("aborted"))
                        {
                            Console.WriteLine($"[Фоновая подписка] Соединение прервано.");
                            break;
                        }

                        Console.WriteLine($"[Фоновая подписка] Ошибка при получении события: {ex.Message}");

                        if (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(1000, cancellationToken);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                Console.WriteLine($"[Фоновая подписка] Поток обработки событий завершен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Фоновая подписка] Критическая ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Остановка WebSocket подписки
        /// </summary>
        private static async Task StopWebSocketSubscriptionAsync()
        {
            try
            {
                if (!_webSocketRunning)
                    return;

                Console.WriteLine("\nОстановка фоновой подписки...");
                _webSocketRunning = false;

                // Отправляем сигнал отмены
                _cancellationTokenSource?.Cancel();

                Console.WriteLine("Сигнал отмены отправлен. Ожидание завершения фоновой задачи...");

                // Даем задаче время на корректное завершение
                if (_eventProcessingTask != null && !_eventProcessingTask.IsCompleted)
                {
                    // Ждем завершения с таймаутом
                    var completedTask = await Task.WhenAny(
                        _eventProcessingTask,
                        Task.Delay(3000) // Таймаут 3 секунды
                    );

                    if (completedTask != _eventProcessingTask)
                    {
                        Console.WriteLine("Таймаут ожидания завершения фоновой задачи. Принудительная остановка.");
                    }
                    else
                    {
                        Console.WriteLine("Фоновая задача успешно завершена.");
                    }
                }

                Console.WriteLine("Фоновая подписка остановлена.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при остановке подписки: {ex.Message}");
            }
        }
        /// <summary>
        /// Обработка полученных данных события
        /// </summary>
        private static void ProcessEventData(string eventData)
        {
            try
            {
                XDocument doc = XDocument.Parse(eventData);
                XNamespace x = "http://www.w3.org/1999/xhtml";

                var li = doc.Descendants(x + "li")
                            .FirstOrDefault(e => e.Attribute("class")?.Value == "ios-signalstate-ev");

                if (li != null)
                {
                    string title = li.Attribute("title")?.Value ?? "N/A";
                    string lvalue = li.Elements(x + "span")
                                      .FirstOrDefault(e => e.Attribute("class")?.Value == "lvalue")
                                      ?.Value ?? "N/A";
                    string lstate = li.Elements(x + "span")
                                      .FirstOrDefault(e => e.Attribute("class")?.Value == "lstate")
                                      ?.Value ?? "N/A";

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"[Событие {DateTime.Now:HH:mm:ss}] ");
                    Console.ResetColor();
                    Console.WriteLine($"{title}: значение={lvalue}, состояние={lstate}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"[Событие {DateTime.Now:HH:mm:ss}] Получено неизвестное событие");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"[Событие] Ошибка обработки: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}