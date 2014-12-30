using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VJoyWrapper
{
    public partial class Gamepad
    {
        static class Pool
        {
            private static SemaphoreSlim PoolLock = new SemaphoreSlim(1, 1);
            private static bool[] Leases = Enumerable.Repeat(false, 16).ToArray(); // False: not leased

            public static async Task<int> Lease()
            {
                await PoolLock.WaitAsync();
                var output = Array.FindIndex(Leases, b => !b);
                Leases[output] = true;
                PoolLock.Release();
                return output;
            }

            public static void Release(int index)
            {
                Leases[index] = false;
            }
        }

        public static async Task<Gamepad> Construct()
        {
            var id = await Pool.Lease();
            int count = 0;
            while ((count++) < 16)
            {
                var gamepad = new Gamepad(id + 1);
                var status = gamepad.Joystick.GetVJDStatus(gamepad.Id);
                switch (status)
                {
                    case VjdStat.VJD_STAT_OWN:
                    case VjdStat.VJD_STAT_FREE:
                        break;
                    case VjdStat.VJD_STAT_BUSY:
                        id += 1;
                        id %= 16;
                        continue;
                    case VjdStat.VJD_STAT_MISS:
                        //throw new Exception("Driver missing.");
                        continue;
                    default:
                        throw new Exception("General exception.");
                }
                gamepad.Joystick.AcquireVJD(gamepad.Id);
                var result = gamepad.Joystick.ResetVJD(gamepad.Id);
                return gamepad;
            }
            throw new Exception("All devices claimed by other programs.");
        }
    }
}
