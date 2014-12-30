using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace VJoyWrapper
{
    // TODO: remove all mentions of vjoy. Their interface is gross :|
    public partial class Gamepad : IDisposable
    {
        private int IdPrivate = -1;
        public uint Id {
            get {
                return (uint)IdPrivate;
            }
        }

        internal vJoy Joystick = new vJoy();
        internal vJoy.JoystickState Report = new vJoy.JoystickState();

        public void Dispose()
        {
            // This probably causes obscure problems. Consider how to not do it later.
            Pool.Release((int)Id);
            Joystick.RelinquishVJD(Id);
        }

        protected Gamepad(int id) {
            IdPrivate = id;
            Axes = new Axis(this);
            Buttons = new Buttons(this);
        }

        /*public Task ScrewAround() {
            int i = 0;
            while (true) {
                i += 10;
                bool result = Joystick.SetAxis(i, Id, HID_USAGES.HID_USAGE_SL0);
                if (i % 30 == 0) {
                    Buttons[1] = !Buttons[1];
                }
                Thread.Sleep(50);
            }
        }*/
    }
}
