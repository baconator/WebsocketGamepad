using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VJoyWrapper
{
    public class Buttons
    {
        Gamepad Gamepad;
        private bool[] ButtonStatuses;
        public Buttons(Gamepad gamepad) {
            Gamepad = gamepad;
            ButtonStatuses = Enumerable.Repeat(false, Gamepad.Joystick.GetVJDButtonNumber(gamepad.Id)).ToArray();
        }

        public bool this[int index] { // 0 indexed, as opposed to vjoy. Sorry.
            get {
                return ButtonStatuses[index];
            }

            set {
                if(index >= 0 && index < ButtonStatuses.Length)
                {
                    var result = Gamepad.Joystick.SetBtn(value, Gamepad.Id, (uint)index+1);
                    if (result) {
                        ButtonStatuses[index] = value;
                    }
                }
            }
        }
    }
}
