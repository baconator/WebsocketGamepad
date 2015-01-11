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
using System.Text.RegularExpressions;
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
            // TODO: listen on multiple addresses?
            options.Urls.Add(url);
            using (WebApp.Start(options, app => {
                app.Use((context, next) =>{
                    return next();
                });
                app.UseStaticFiles(new StaticFileOptions() {
                    FileSystem = new PhysicalFileSystem(@"WebClient")
                });
                app.UseCors(CorsOptions.AllowAll);
                GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(2);
                app.MapSignalR();
            }))
            {
                Console.WriteLine("Server running on {0}", url);
                while (true) {
                    Console.Write(">> ");
                    var input = Console.ReadLine();
                    var commands = input.Split(' ');
                    switch (commands[0]) {
                        case "list":
                            foreach(var connection in PhoneHub.Connections) {
                                Console.WriteLine(connection.ToString());
                            }
                            break;
                        case "ban":
                            PhoneHub.Ban(commands[1]);
                            break;
                        case "unban":
                            PhoneHub.Unban(commands[1]);
                            break;
                    }
                }
            }
        }
    }
}
