using System;
using System.Net.WebSockets;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Portal.Data;
using Portal.Extensions;
using Portal.Models;

namespace Portal.Services
{
    public class WebSocketService
    {
        private static readonly Regex Regex = new Regex("^/api/v1/projects/(?<projectId>[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12})/ws$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly WebSocketConnectionManager _connectionManager;

        public WebSocketService(WebSocketConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task Acceptor(HttpContext httpContext, Func<Task> next)
        {
            var match = Regex.Match(httpContext.Request.Path);
            if (match.Success)
            {
                if (httpContext.WebSockets.IsWebSocketRequest)
                {
                    var projectId = match.Groups["projectId"].Value;
                    var userManager = httpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                    var context = httpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                    var userId = userManager.GetUserId(httpContext.User);
                    var result = await context.CanAccessToProjectWithProjectAsync(userId, projectId);
                    if (!result.canAccess)
                    {
                        httpContext.Response.StatusCode = 404;
                        return;
                    }
                    var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                    var client = new WebSocketClient(webSocket, projectId, userId);
                    _connectionManager.AddClient(projectId, userId, client);
                    await client.ReceiveAsync();
                    await _connectionManager.RemoveClientAsync(client.Id, projectId, userId);
                }
                else
                {
                    httpContext.Response.StatusCode = 400;
                }
            }
            else
            {
                await next();
            }
        }
    }

    public class WebSocketClient : IObservable<string>, IObserver<string>
    {
        private readonly Subject<string> _receiveSubject;
        private readonly WebSocket _socket;

        public bool IsOpen => _socket.State == WebSocketState.Open;
        public string Id { get; } = Guid.NewGuid().ToString();
        public string ProjectId { get; }
        public string UserId { get; }

        public WebSocketClient(WebSocket socket, string projectId, string userId)
        {
            ProjectId = projectId;
            UserId = userId;
            _socket = socket;
            _receiveSubject = new Subject<string>();
        }

        public async Task DisposeAsync()
        {
            _receiveSubject?.Dispose();
            await _socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                statusDescription: "Closed by the WebSocketManager",
                cancellationToken: CancellationToken.None);
            _socket.Dispose();
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            return _receiveSubject.Subscribe(observer);
        }

        public void OnCompleted()
        {
        }

        public async void OnError(Exception error)
        {
            if (_socket.State == WebSocketState.Open)
                await _socket.CloseOutputAsync(WebSocketCloseStatus.InternalServerError, error.Message,
                    CancellationToken.None);
        }

        public async void OnNext(string value)
        {
            if (_socket.State == WebSocketState.Open)
                await _socket.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(value)),
                    WebSocketMessageType.Text,
                    true, CancellationToken.None);
        }

        public async Task ReceiveAsync()
        {
            var buffer = new byte[1024 * 4];
            var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await _socket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await _socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}