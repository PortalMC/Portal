using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using JoshuaKearney.Collections;

namespace Portal.Services
{
    public class WebSocketConnectionManager
    {
        private ConcurrentDictionary<string, WebSocketClient> _sockets = new ConcurrentDictionary<string, WebSocketClient>();

        private ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentSet<string>>> _mapping = new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentSet<string>>>();

        public bool AddClient(string projectId, string userId, WebSocketClient client)
        {
            var id = CreateConnectionId();
            _sockets.TryAdd(id, client);
            var sessionMapping = GetSessionMapping(projectId, userId);
            sessionMapping.Add(id);
            return true;
        }

        public async Task RemoveClientAsync(string clientId, string projectId, string userId)
        {
            var sessionMapping = GetSessionMapping(projectId, userId);
            sessionMapping?.Remove(clientId);
            _sockets.TryRemove(clientId, out var client);
            if (client != null)
            {
                await client.DisposeAsync();
            }
        }

        public void SendMessage(string projectId, string userId, string message)
        {
            var sessionMapping = GetSessionMapping(projectId, userId);
            if (sessionMapping == null)
            {
                return;
            }
            foreach (var sessionId in sessionMapping)
            {
                _sockets.TryGetValue(sessionId, out var client);
                client?.OnNext(message);
            }
        }

        private ConcurrentSet<string> GetSessionMapping(string projectId, string userId)
        {
            ConcurrentDictionary<string, ConcurrentSet<string>> userMapping;
            if (!_mapping.ContainsKey(projectId))
            {
                userMapping = new ConcurrentDictionary<string, ConcurrentSet<string>>();
                _mapping.TryAdd(projectId, userMapping);
            }
            else
            {
                _mapping.TryGetValue(projectId, out userMapping);
            }
            if (userMapping == null)
            {
                return null;
            }
            ConcurrentSet<string> sessionMapping;
            if (!userMapping.ContainsKey(userId))
            {
                sessionMapping = new ConcurrentSet<string>();
                userMapping.TryAdd(userId, sessionMapping);
            }
            else
            {
                userMapping.TryGetValue(userId, out sessionMapping);
            }
            return sessionMapping;
        }

        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}