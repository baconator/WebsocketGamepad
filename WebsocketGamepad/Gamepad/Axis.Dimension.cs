using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VJoyWrapper
{
    public partial class Axis
    {
        public class Dimension
        {
            Axis Axis;
            HID_USAGES Usage;

            public readonly long Min;
            public readonly long Max;
            public readonly bool Available;
            private long ValuePrivate;
            public long Value
            {
                get
                {
                    return ValuePrivate;
                }

                set
                {
                    if (!Available)
                    {
                        return;
                    }
                    var safe = Math.Max(Math.Min(Max, value), Min);
                    var result = Axis.Gamepad.Joystick.SetAxis((int)safe, Axis.Gamepad.Id, Usage);
                    if (result)
                    {
                        ValuePrivate = safe;
                    }
                }
            }

            public Dimension(Axis axis, HID_USAGES dim)
            {
                var joystick = axis.Gamepad.Joystick;
                Available = joystick.GetVJDAxisExist(axis.Gamepad.Id, dim);
                joystick.GetVJDAxisMin(axis.Gamepad.Id, dim, ref Min);
                joystick.GetVJDAxisMax(axis.Gamepad.Id, dim, ref Max);
                Usage = dim;
            }
        }
    }
}
