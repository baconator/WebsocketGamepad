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
    public partial class PhoneHub : Hub
    {
        const int Min = -100;
        const int Max = 100;

        public async Task UpdateState(string userId, string id, string value) {
            try
            {
                var gamepadTask = GetUniqueGamepad(userId);
                if (await Task.WhenAny(gamepadTask, Task.Delay(100)) != gamepadTask) throw new Exception("Timed out requesting a gamepad.");
                var gamepad = await gamepadTask;
                if (id == "analog0")
                {
                    var tup = JsonConvert.DeserializeObject<JObject>(value); // TODO: Unsafe? Check to see if this can handle malicious input
                    var x = tup.GetValue("x").Value<double>();
                    var y = tup.GetValue("y").Value<double>();

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

        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            ReturnClientGamepad();
            return base.OnDisconnected(stopCalled);
        }
    }
}
