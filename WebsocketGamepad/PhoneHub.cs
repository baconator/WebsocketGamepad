using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VJoyWrapper;

namespace WebsocketGamepad
{
    public class PhoneHub : Hub
    {
        const int Min = -100;
        const int Max = 100;
        protected static ConcurrentDictionary<string, Lazy<Task<Gamepad>>> Gamepads = new ConcurrentDictionary<string, Lazy<Task<Gamepad>>>();

        private double Rescale(double input, Tuple<long, long> destination, Tuple<int, int> source) {
            return (input + (destination.Item1 - source.Item1)) * ((destination.Item2 - destination.Item1) / (source.Item2 - source.Item1));
        }

        private Task<Gamepad> GetUniqueGamepad(string id) {
            return Gamepads.GetOrAdd(id, new Lazy<Task<Gamepad>>(Gamepad.Construct, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        public async Task UpdateState(string id, string value) {
            try
            {
                var gamepad = await GetUniqueGamepad(Context.ConnectionId);
                if (id == "analog0")
                {
                    var tup = JsonConvert.DeserializeObject<JObject>(value); // TODO: Unsafe? Check to see if this can handle malicious input
                    var x = tup.GetValue("x").Value<double>();
                    var y = tup.GetValue("y").Value<double>();

                    // TODO: Handle multiple connections XD
                    // ... this is a linear transformation. Forgot my matrices, though.
                    var scaledX = Rescale(x, Tuple.Create(gamepad.Axes.X.Min, gamepad.Axes.X.Max), Tuple.Create(Min, Max));
                    var scaledY = Rescale(y, Tuple.Create(gamepad.Axes.Y.Min, gamepad.Axes.Y.Max), Tuple.Create(Min, Max));
                    gamepad.Axes.X.Value = Convert.ToInt64(scaledX);
                    gamepad.Axes.Y.Value = Convert.ToInt64(scaledY);
                } else if (id.Contains("dpad0")) {
                    int buttonIndex = -1;
                    var parsable = int.TryParse(Regex.Match(id, ":(.+)").Groups[1].Value, out buttonIndex);
                    if (parsable) {
                        gamepad.Buttons[buttonIndex] = (value == "down" || value == "move") ? true : false;
                    }
                }
            }
            catch (Exception e)
            {
                // D:
            }
        }
    }
}
