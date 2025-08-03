using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Watchlog_Websocket_NET_CORE_8.Classes.Entityies;

namespace Watchlog_Websocket_NET_CORE_8.Classes.Services
{
    public class WebSocketApiService : IDisposable
    {
        private ClientWebSocket _webSocket;
        private readonly string _websocketUrl;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<WebSocketApiResponse>> _pendingRequests;
        private bool _isConnected;
        private Task _listeningTask;

        public WebSocketApiService()
        {
            _websocketUrl = "wss://savolanode.fabricademo.com";
            _cancellationTokenSource = new CancellationTokenSource();
            _pendingRequests = new ConcurrentDictionary<string, TaskCompletionSource<WebSocketApiResponse>>();
            _isConnected = false;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (_isConnected && _webSocket?.State == WebSocketState.Open)
                {
                    return true;
                }

                _webSocket?.Dispose();
                _webSocket = new ClientWebSocket();
                
                Console.WriteLine($"{DateTime.Now} WebSocket API bağlantısı kuruluyor...");
                await _webSocket.ConnectAsync(new Uri(_websocketUrl), _cancellationTokenSource.Token);
                
                _isConnected = true;
                Console.WriteLine($"{DateTime.Now} WebSocket API bağlantısı başarılı!");

                _listeningTask = Task.Run(async () => await ListenForResponses(), _cancellationTokenSource.Token);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} WebSocket API bağlantı hatası: {ex.Message}");
                _isConnected = false;
                return false;
            }
        }

        private async Task ListenForResponses()
        {
            var buffer = new byte[1024 * 8];
            
            while (_webSocket?.State == WebSocketState.Open && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
                    
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await HandleIncomingMessage(message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine($"{DateTime.Now} WebSocket API connection closed by server");
                        _isConnected = false;
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now} WebSocket API receive error: {ex.Message}");
                    _isConnected = false;
                    break;
                }
            }
        }

        private async Task HandleIncomingMessage(string message)
        {
            try
            {
                var response = JsonSerializer.Deserialize<WebSocketMessage>(message);
                
               
                if (response?.type == "api_response" && !string.IsNullOrEmpty(response.requestId))
                {
                    if (_pendingRequests.TryRemove(response.requestId, out var tcs))
                    {
                        tcs.SetResult(response.result);
                    }
                }
               
                else if (response?.type == "ping")
                {
                    await SendMessage(new { type = "pong" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} WebSocket API message handling error: {ex.Message}");
            }
        }

        private async Task<WebSocketApiResponse> SendApiRequestAsync(string requestType, object data = null, int timeoutMs = 10000)
        {
           
            if (!_isConnected || _webSocket?.State != WebSocketState.Open)
            {
                if (!await ConnectAsync())
                {
                    return new WebSocketApiResponse 
                    { 
                        success = false, 
                        error = "WebSocket connection failed" 
                    };
                }
            }

            var requestId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<WebSocketApiResponse>();
            _pendingRequests[requestId] = tcs;

            try
            {
                var request = new
                {
                    type = requestType,
                    data = new 
                    { 
                        requestId = requestId,
                        ipAddress = data?.ToString()
                    }
                };

                await SendMessage(request);

                
                using var timeoutCts = new CancellationTokenSource(timeoutMs);
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    _cancellationTokenSource.Token, timeoutCts.Token);

                try
                {
                    return await tcs.Task.WaitAsync(combinedCts.Token);
                }
                catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
                {
                    _pendingRequests.TryRemove(requestId, out _);
                    return new WebSocketApiResponse 
                    { 
                        success = false, 
                        error = "Request timeout" 
                    };
                }
            }
            catch (Exception ex)
            {
                _pendingRequests.TryRemove(requestId, out _);
                return new WebSocketApiResponse 
                { 
                    success = false, 
                    error = $"Request failed: {ex.Message}" 
                };
            }
        }

        private async Task SendMessage(object message)
        {
            if (_webSocket?.State != WebSocketState.Open)
                return;

            var json = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(json);
            
            await _webSocket.SendAsync(
                new ArraySegment<byte>(buffer), 
                WebSocketMessageType.Text, 
                true, 
                _cancellationTokenSource.Token);
        }

        
        public async Task<List<RobotIP>> GetActiveRobotsAsync()
        {
            try
            {
                var response = await SendApiRequestAsync("api_getRobots");
                
                if (response.success && response.data != null)
                {
                    var robotsJson = response.data.ToString();
                    var robots = JsonSerializer.Deserialize<List<RobotIP>>(robotsJson);
                    return robots ?? new List<RobotIP>();
                }
                
                Console.WriteLine($"{DateTime.Now} GetActiveRobotsAsync failed: {response.error}");
                return new List<RobotIP>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} GetActiveRobotsAsync exception: {ex.Message}");
                return new List<RobotIP>();
            }
        }

        public async Task<List<Alarm_Value>> GetAlarmsByIpAsync(string ipAddress)
        {
            try
            {
                var response = await SendApiRequestAsync("api_getAlarms", ipAddress);
                
                if (response.success && response.data != null)
                {
                    var alarmsJson = response.data.ToString();
                    var alarms = JsonSerializer.Deserialize<List<Alarm_Value>>(alarmsJson);
                    return alarms ?? new List<Alarm_Value>();
                }
                
                Console.WriteLine($"{DateTime.Now} GetAlarmsByIpAsync failed for {ipAddress}: {response.error}");
                return new List<Alarm_Value>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} GetAlarmsByIpAsync exception for {ipAddress}: {ex.Message}");
                return new List<Alarm_Value>();
            }
        }

        public async Task<List<RobotStatusValue>> GetStatusByIpAsync(string ipAddress)
        {
            try
            {
                var response = await SendApiRequestAsync("api_getStatus", ipAddress);
                
                if (response.success && response.data != null)
                {
                    var statusJson = response.data.ToString();
                    var status = JsonSerializer.Deserialize<List<RobotStatusValue>>(statusJson);
                    return status ?? new List<RobotStatusValue>();
                }
                
                Console.WriteLine($"{DateTime.Now} GetStatusByIpAsync failed for {ipAddress}: {response.error}");
                return new List<RobotStatusValue>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} GetStatusByIpAsync exception for {ipAddress}: {ex.Message}");
                return new List<RobotStatusValue>();
            }
        }

        public async Task<Utilization_Value> GetUtilizationByIpAsync(string ipAddress)
        {
            try
            {
                var response = await SendApiRequestAsync("api_getUtilization", ipAddress);
                
                if (response.success && response.data != null)
                {
                    var utilizationJson = response.data.ToString();
                    var utilization = JsonSerializer.Deserialize<Utilization_Value>(utilizationJson);
                    return utilization ?? new Utilization_Value();
                }
                
                Console.WriteLine($"{DateTime.Now} GetUtilizationByIpAsync failed for {ipAddress}: {response.error}");
                return new Utilization_Value();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} GetUtilizationByIpAsync exception for {ipAddress}: {ex.Message}");
                return new Utilization_Value();
            }
        }

        public async Task<List<BackupSchedules>> GetBackupSchedulesAsync()
        {
            try
            {
                var response = await SendApiRequestAsync("api_getBackupSchedules");
                
                if (response.success && response.data != null)
                {
                    var schedulesJson = response.data.ToString();
                    var schedules = JsonSerializer.Deserialize<List<BackupSchedules>>(schedulesJson);
                    return schedules ?? new List<BackupSchedules>();
                }
                
                Console.WriteLine($"{DateTime.Now} GetBackupSchedulesAsync failed: {response.error}");
                return new List<BackupSchedules>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} GetBackupSchedulesAsync exception: {ex.Message}");
                return new List<BackupSchedules>();
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            
            try
            {
                _webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None)
                    .GetAwaiter().GetResult();
            }
            catch { }
            
            _webSocket?.Dispose();
            _cancellationTokenSource?.Dispose();
            _isConnected = false;
        }
    }

   
    public class WebSocketMessage
    {
        public string type { get; set; }
        public string requestType { get; set; }
        public string requestId { get; set; }
        public WebSocketApiResponse result { get; set; }
    }

    public class WebSocketApiResponse
    {
        public bool success { get; set; }
        public object data { get; set; }
        public string error { get; set; }
    }
} 