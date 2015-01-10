using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace VJoyWrapper
{
    public class LeasePool {
        private bool[] Leases;
        private SemaphoreSlim PoolLock;
        public LeasePool(int capacity, bool[] existingLeases = null) {
            Leases = existingLeases ?? Enumerable.Repeat(false, capacity).ToArray();
            PoolLock = new SemaphoreSlim(capacity, capacity);
        }

        public async Task<int> Lease() {
            await PoolLock.WaitAsync();
            var output = Array.FindIndex(Leases, b => !b);
            Leases[output] = true;
            return output;
        }

        public void Release(int index) {
            Leases[index] = false;
            PoolLock.Release();
        }
    }

    public partial class Gamepad
    {
        static LeasePool Pool;
        static Gamepad() {
            // Build a list of possible gamepads and populate the pool.
            var availability = Enumerable
                .Range(1, 16)
                .Select(id => {
                    try
                    {
                        var gamepad = new Gamepad(id);
                        var status = gamepad.Joystick.GetVJDStatus(gamepad.Id);
                        if (status == VjdStat.VJD_STAT_FREE)
                        {
                            return false; // You can loan it!
                        }
                        else
                        {
                            return true; // You can't :|
                        }
                    }
                    catch (Exception e) {
                        return true;
                    }

                });
            Pool = new LeasePool(availability.Count(b => !b), availability.ToArray());
        }

        public static async Task<Gamepad> Construct()
        {
            var id = await Pool.Lease();
            var gamepad = new Gamepad(id + 1);
            var status = gamepad.Joystick.GetVJDStatus(gamepad.Id);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                case VjdStat.VJD_STAT_FREE:
                    break;
                case VjdStat.VJD_STAT_BUSY:
                case VjdStat.VJD_STAT_MISS:
                default:
                    throw new Exception("General exception.");
            }
            gamepad.Joystick.AcquireVJD(gamepad.Id);
            var result = gamepad.Joystick.ResetVJD(gamepad.Id);
            return gamepad;
            throw new Exception("All devices claimed by other programs.");
        }
    }
}
