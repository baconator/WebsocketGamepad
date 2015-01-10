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
        protected static ConcurrentDictionary<string, string> ConnectionIdToClientMap = new ConcurrentDictionary<string, string>();
        protected static SemaphoreSlim AvailableGamepads;

        private Task<Gamepad> GetUniqueGamepad(string id)
        {
            ConnectionIdToClientMap.AddOrUpdate(Context.ConnectionId, id, (a, b) => id);
            return Gamepads.GetOrAdd(id, new Lazy<Task<Gamepad>>(Gamepad.Construct, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        private async Task ReturnClientGamepad() {
            string userId;
            Lazy<Task<Gamepad>> gamepadWrapper;
            if (ConnectionIdToClientMap.TryGetValue(Context.ConnectionId, out userId) && Gamepads.TryRemove(userId, out gamepadWrapper)) {
                var gamepad = await gamepadWrapper.Value;
                gamepad.Dispose();
            }
        }
    }
}
