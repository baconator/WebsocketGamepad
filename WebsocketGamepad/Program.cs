using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;
using VJoyWrapper;

namespace WebsocketGamepad
{
    class Program
    {
        static async Task Work()
        {
            var gamepad = await Gamepad.Construct();
            //await gamepad.ScrewAround();
        }

        static void Main(string[] args)
        {
            Work().Wait();
            //Demo(args);
        }
    }
}
