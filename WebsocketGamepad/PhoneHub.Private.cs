using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VJoyWrapper;

namespace WebsocketGamepad
{
    public partial class PhoneHub : Hub
    {
        private double Rescale(double input, Tuple<long, long> destination, Tuple<int, int> source)
        {
            return (input + (destination.Item1 - source.Item1)) * ((destination.Item2 - destination.Item1) / (source.Item2 - source.Item1));
        }

        protected static ConcurrentDictionary<string, Lazy<Task<Gamepad>>> Gamepads = new ConcurrentDictionary<string, Lazy<Task<Gamepad>>>();
        protected static ConcurrentDictionary<string, string> ConnectionIdToClientIdMap = new ConcurrentDictionary<string, string>();
        protected static ConcurrentDictionary<string, string> ConnectionIdToIpMap = new ConcurrentDictionary<string, string>();
        protected static ConcurrentDictionary<string, bool> BannedIps = new ConcurrentDictionary<string, bool>();
        public static IEnumerable<Connection> Connections {
            get {
                return ConnectionIdToIpMap.Select(kvp =>
                {
                    var connectionId = kvp.Key;
                    var address = kvp.Value;
                    string userId;
                    var active = ConnectionIdToClientIdMap.TryGetValue(connectionId, out userId) && Gamepads.ContainsKey(userId);
                    return new Connection() { Address = address, Banned = BannedIps.ContainsKey(address), Active = active, UserId = userId };
                }).Distinct();
            }
        }
        public static void Ban(string ip) {
            BannedIps.AddOrUpdate(ip, true, (a, b) => true);
        }
        public static void Unban(string ip) {
            bool ignored;
            BannedIps.TryRemove(ip, out ignored);
        }

        private string GetIpAddress() {
            object address;
            if (Context.Request.Environment.TryGetValue("server.RemoteIpAddress", out address)) return (string)address;
            return null;
        }

        private void UpdateIpMap() {
            ConnectionIdToIpMap.AddOrUpdate(Context.ConnectionId, GetIpAddress(), (a, b) => GetIpAddress());
        }

        private Task<Gamepad> GetUniqueGamepad(string id)
        {
            UpdateIpMap();
            ConnectionIdToClientIdMap.AddOrUpdate(Context.ConnectionId, id, (a, b) => id);
            return Gamepads.GetOrAdd(id, new Lazy<Task<Gamepad>>(async ()=> {
                var output = await Gamepad.Construct();
                Clients.Caller.updateGamepadNumber(output.Id);
                return output;
            }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        private async Task ReturnClientGamepad() {
            string userId;
            Lazy<Task<Gamepad>> gamepadWrapper;
            if (ConnectionIdToClientIdMap.TryGetValue(Context.ConnectionId, out userId) && Gamepads.TryRemove(userId, out gamepadWrapper)) {
                var gamepad = await gamepadWrapper.Value;
                gamepad.Dispose();
            }
        }
    }
}
