using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VJoyWrapper
{
    public partial class Axis
    {
        protected Gamepad Gamepad;
        public Axis(Gamepad gamepad) {
            Gamepad = gamepad;
            X = new Dimension(this, HID_USAGES.HID_USAGE_X);
            Y = new Dimension(this, HID_USAGES.HID_USAGE_Y);
            Z = new Dimension(this, HID_USAGES.HID_USAGE_Z);
        }

        public Dimension X;
        public Dimension Y;
        public Dimension Z;
    }
}
