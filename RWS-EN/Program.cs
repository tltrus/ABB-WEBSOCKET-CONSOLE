using RWS_EN.RobotWebServices;
using System.Net.WebSockets;
using System.Xml.Linq;

namespace RWS_EN
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

                // 1. Create client and authenticate
                var client = new RWSClient("http://127.0.0.1");

                Console.WriteLine("Authenticating...");
                var authenticated = await client.AuthenticateAsync();

                if (!authenticated)
                {
                    Console.WriteLine("Authentication error!");
                    return;
                }

                Console.WriteLine("Authentication successful!");

                // 2. Get controller information
                Console.WriteLine("\nController information:");
                var controllerInfo = await client.GetControllerInfoAsync();
                if (controllerInfo != null && controllerInfo.Count > 1)
                {
                    Console.WriteLine($"\tName: {controllerInfo[1].Name}");
                }

                // 3. Work with IO signals
                Console.WriteLine("\nWorking with IO signals:");

                // Read signal
                var signal = await client.GetIOSignalAsync("DI1");
                if (signal != null)
                {
                    Console.WriteLine($"\tSignal {signal.Name}: Value={signal.Value}, State={signal.State}");
                }

                // Set signal
                bool setResult = await client.SetIOSignalAsync("DO1", 1);
                Console.WriteLine($"\tSet signal DO1 to 1: {(setResult ? "Success" : "Error")}");

                // Get all signals
                var allSignals = await client.GetAllIOSignalsAsync();
                Console.WriteLine($"\tTotal signals: {allSignals.Count}");

                // 4. Work with RAPID
                Console.WriteLine("\nWorking with RAPID:");

                // Get Task state
                var taskName = "T_ROB1";
                var taskState = await client.GetTaskStateAsync(taskName);
                if (taskState != null)
                {
                    Console.WriteLine($"\tState of {taskName}: {taskState.Taskstate}");
                }

                // Get execution state
                var rapidState = await client.GetRAPIDExecutionStateAsync();
                if (rapidState != null)
                {
                    Console.WriteLine($"\tExecution state: {rapidState.ControllerState}");
                }

                // Get list of tasks
                var tasks = await client.GetRAPIDTasksAsync();
                Console.WriteLine($"\tFound tasks: {tasks.Count}");
                foreach (var task in tasks.Take(3))
                {
                    Console.WriteLine($"\tTask: {task.Name}, State: {task.State}, Active: {task.Active}");
                }

                // Read RAPID variable
                var variable = await client.GetRAPIDVariableAsync("T_ROB1", "MainModule", "nCounter");
                if (variable != null)
                {
                    Console.WriteLine($"\tVariable nCounter: {variable.Value}, Type: {variable.Type}");
                }

                // Set RAPID variable
                bool varSetResult = await client.SetRAPIDVariableAsync("T_ROB1", "MainModule", "nCounter", "100");
                Console.WriteLine($"\tSet variable: {(varSetResult ? "Success" : "Error")}");

                // Start program
                bool startResult = await client.StartRAPIDProgramAsync();
                Console.WriteLine($"\tStart program: {(startResult ? "Success" : "Error")}");

                // 5. Work with file system
                Console.WriteLine("\nFile system:");
                var files = await client.GetFileSystemItemsAsync("/$home?json=1");
                Console.WriteLine($"\tFiles in HOME: {files.Count}");
                foreach (var file in files.Take(5))
                {
                    Console.WriteLine($"\t{file.Title} ({file.Type}, {file.Size} bytes)");
                }

                // 6. START SUBSCRIPTION IN BACKGROUND MODE
                Console.WriteLine("\nStarting subscription in background mode...");
                var subscriptionResult = await StartWebSocketSubscriptionAsync(client);

                if (subscriptionResult)
                {
                    Console.WriteLine("Subscription started in background mode.");
                }

                // 8. Get system properties
                Console.WriteLine("\nSystem properties:");
                var systemProps = await client.GetSystemPropertiesAsync();
                if (systemProps != null)
                {
                    var system = systemProps.FirstOrDefault(a => a.Title == "system");
                    if (system != null)
                    {
                        Console.WriteLine($"\tSystem name: {system.Name}");
                        Console.WriteLine($"\tRobotWare version: {system.RWVersion}");
                    }

                    var options = systemProps.FirstOrDefault(a => a.Title == "options");
                    if (options != null && options.Options != null)
                    {
                        Console.WriteLine($"\tSystem options (first 5):");
                        foreach (var option in options.Options.Take(5))
                        {
                            Console.WriteLine($"\t     {option.Option}");
                        }
                    }
                }

                Console.ReadKey();

                // 9. STOP BACKGROUND SUBSCRIPTION BEFORE EXIT
                Console.WriteLine("\nStarting program shutdown...");
                await StopWebSocketSubscriptionAsync();

                // Give time for proper shutdown
                await Task.Delay(1000);

                // 10. Logout from system
                Console.WriteLine("\nLogging out...");
                await client.LogoutAsync();
                client.Dispose();

                Console.WriteLine("\nAll operations completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                await StopWebSocketSubscriptionAsync();
            }
        }

        /// <summary>
        /// Start WebSocket subscription in background mode
        /// </summary>
        private static async Task<bool> StartWebSocketSubscriptionAsync(RWSClient client)
        {
            try
            {
                Console.WriteLine("Setting up subscription...");
                var resources = new Dictionary<string, string>
                {
                    { "/rw/iosystem/signals/DO1;state", "1" },
                    { "/rw/iosystem/signals/DI1;state", "1" }
                };

                var webSocketUrl = await client.CreateSubscriptionAsync(resources);
                if (string.IsNullOrEmpty(webSocketUrl))
                {
                    Console.WriteLine("Failed to create subscription.");
                    return false;
                }

                Console.WriteLine($"WebSocket URL: {webSocketUrl}");

                var wsConnected = await client.ConnectWebSocketAsync(webSocketUrl);
                if (!wsConnected)
                {
                    Console.WriteLine("Failed to connect to WebSocket.");
                    return false;
                }

                Console.WriteLine("WebSocket connected. Starting background event processing thread...");

                _webSocketRunning = true;
                _eventProcessingTask = Task.Run(() =>
                    ProcessWebSocketEventsAsync(client, _cancellationTokenSource.Token));

                await Task.Delay(500);

                Console.WriteLine("Background subscription thread successfully started.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting subscription: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Method for processing WebSocket events in background thread
        /// </summary>
        private static async Task ProcessWebSocketEventsAsync(RWSClient client, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"[Background subscription] Event processing thread started.");

                while (!cancellationToken.IsCancellationRequested && _webSocketRunning)
                {
                    try
                    {
                        // Use CancellationToken for interrupt capability
                        var eventData = await client.ReceiveWebSocketEventAsync(cancellationToken);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            Console.WriteLine($"[Background subscription] Cancellation requested.");
                            break;
                        }

                        if (!string.IsNullOrEmpty(eventData))
                        {
                            ProcessEventData(eventData);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        Console.WriteLine($"[Background subscription] Task canceled.");
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine($"[Background subscription] Operation canceled.");
                        break;
                    }
                    catch (WebSocketException wsEx)
                    {
                        if (wsEx.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely ||
                            wsEx.Message.Contains("closed"))
                        {
                            Console.WriteLine($"[Background subscription] WebSocket connection closed.");
                        }
                        else
                        {
                            Console.WriteLine($"[Background subscription] WebSocket error: {wsEx.Message}");
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("closed") || ex.Message.Contains("aborted"))
                        {
                            Console.WriteLine($"[Background subscription] Connection interrupted.");
                            break;
                        }

                        Console.WriteLine($"[Background subscription] Error receiving event: {ex.Message}");

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

                Console.WriteLine($"[Background subscription] Event processing thread completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Background subscription] Critical error: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop WebSocket subscription
        /// </summary>
        private static async Task StopWebSocketSubscriptionAsync()
        {
            try
            {
                if (!_webSocketRunning)
                    return;

                Console.WriteLine("\nStopping background subscription...");
                _webSocketRunning = false;

                // Send cancellation signal
                _cancellationTokenSource?.Cancel();

                Console.WriteLine("Cancellation signal sent. Waiting for background task completion...");

                // Give task time for proper completion
                if (_eventProcessingTask != null && !_eventProcessingTask.IsCompleted)
                {
                    // Wait for completion with timeout
                    var completedTask = await Task.WhenAny(
                        _eventProcessingTask,
                        Task.Delay(3000) // Timeout 3 seconds
                    );

                    if (completedTask != _eventProcessingTask)
                    {
                        Console.WriteLine("Timeout waiting for background task completion. Forced stop.");
                    }
                    else
                    {
                        Console.WriteLine("Background task completed successfully.");
                    }
                }

                Console.WriteLine("Background subscription stopped.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping subscription: {ex.Message}");
            }
        }

        /// <summary>
        /// Process received event data
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
                    Console.Write($"[Event {DateTime.Now:HH:mm:ss}] ");
                    Console.ResetColor();
                    Console.WriteLine($"{title}: value={lvalue}, state={lstate}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"[Event {DateTime.Now:HH:mm:ss}] Received unknown event");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"[Event] Processing error: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}