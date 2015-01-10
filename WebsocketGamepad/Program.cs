using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;
using VJoyWrapper;

namespace WebsocketGamepad
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = Dns
                .GetHostAddresses(Dns.GetHostName())
                .Where(address => address.AddressFamily == AddressFamily.InterNetwork) // Only use ipv4 for the time being.
                .Select(address => new UriBuilder("http", address.ToString(), 8000))
                .First().ToString();
            var options = new StartOptions();
            options.Urls.Add(url);
            using (WebApp.Start(options, app => {
                app.Use((context, next) =>{
                    return next();
                });
                app.UseStaticFiles(new StaticFileOptions() {
                    FileSystem = new PhysicalFileSystem(@"WebClient")
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
