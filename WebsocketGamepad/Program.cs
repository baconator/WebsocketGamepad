using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Owin;
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
            //Work().Wait();
            //Demo(args);
            var url = "http://207.23.195.31:8000";
            var options = new StartOptions();
            options.Urls.Add(url);
            using (WebApp.Start(options, app => {
                app.Use((context, next) =>{
                    return next();
                });
                app.UseStaticFiles(new StaticFileOptions() {
                    FileSystem = new PhysicalFileSystem(@"C:\Users\Bacon\Documents\WebsocketGamepad\WebsocketGamepad\WebClient\")
                });
                app.UseCors(CorsOptions.AllowAll);
                app.MapSignalR();
            }))
            {
                Console.WriteLine("Server running on {0}", url);
                Console.ReadLine();
            }
        }
    }
}
