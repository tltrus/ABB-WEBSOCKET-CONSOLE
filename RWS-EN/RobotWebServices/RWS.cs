using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace RWS_EN.RobotWebServices
{
    /// <summary>
    /// Main class for working with ABB Robot Web Services
    /// </summary>
    public partial class RWSClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;
        private CookieContainer _cookieContainer;
        private HttpClientHandler _handler;
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsConnected { get; private set; }
        public bool IsWebSocketConnected { get; private set; }

        /// <summary>
        /// RWS client constructor
        /// </summary>
        /// <param name="baseUrl">Controller base URL (e.g., "http://192.168.125.1")</param>
        /// <param name="username">Username (default "Default User")</param>
        /// <param name="password">Password (default "robotics")</param>
        public RWSClient(string baseUrl, string username = "Default User", string password = "robotics")
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _username = username;
            _password = password;

            _cookieContainer = new CookieContainer();
            _handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                UseCookies = true,
                PreAuthenticate = true,
                Credentials = new System.Net.NetworkCredential(username, password)
            };

            _httpClient = new HttpClient(_handler);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Authenticate in the system
        /// </summary>
        public async Task<bool> AuthenticateAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/rw");
                IsConnected = response.IsSuccessStatusCode;
                return IsConnected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication error: {ex.Message}");
                IsConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Connect to WebSocket for receiving events
        /// </summary>
        public async Task<bool> ConnectWebSocketAsync(string webSocketUrl)
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _webSocket = new ClientWebSocket();

                // Add cookies to headers
                var cookies = _cookieContainer.GetCookies(new Uri(_baseUrl));
                var cookieHeader = string.Join("; ", cookies.Cast<System.Net.Cookie>().Select(c => $"{c.Name}={c.Value}"));

                _webSocket.Options.SetRequestHeader("Cookie", cookieHeader);
                _webSocket.Options.AddSubProtocol("robapi2_subscription");

                await _webSocket.ConnectAsync(new Uri(webSocketUrl), _cancellationTokenSource.Token);

                IsWebSocketConnected = _webSocket.State == WebSocketState.Open;
                return IsWebSocketConnected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket connection error: {ex.Message}");
                IsWebSocketConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Receive events from WebSocket
        /// </summary>
        public async Task<string> ReceiveWebSocketEventAsync(CancellationToken cancellationToken)
        {
            if (!IsWebSocketConnected || _webSocket == null)
                return null;

            try
            {
                var buffer = new byte[4096];
                var result = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

                return Encoding.UTF8.GetString(buffer, 0, result.Count);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Keep old method for backward compatibility:
        public async Task<string> ReceiveWebSocketEventAsync()
        {
            return await ReceiveWebSocketEventAsync(_cancellationTokenSource?.Token ?? CancellationToken.None);
        }

        /// <summary>
        /// Logout from the system
        /// </summary>
        public async Task LogoutAsync()
        {
            await GetAsync("/logout");
            IsConnected = false;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _handler?.Dispose();

            if (_webSocket != null)
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None).Wait();
                }
                _webSocket.Dispose();
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}