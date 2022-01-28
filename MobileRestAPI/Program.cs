using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleUdp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MobileRestAPI
{
    public class Program
    {
        static CommonUtil myUtil = new CommonUtil();
        static UdpEndpoint udpServer;
        static int udpServerPort = 38157;
        static public string _rootPath;
        static public string _authFilePath;
        static public string result;
        public static void Main(string[] args)
        {
            try
            {
                //Configuration = configuration;
                //_rootPath = env.ContentRootPath;
                _rootPath = System.IO.Directory.GetCurrentDirectory();
                string localIP = myUtil.Get_Ini("system config", "LOCAL_IP", "192.168.100.200");
                myUtil.FileLogger("Local IP:" + localIP);
                udpServer = new UdpEndpoint(localIP, udpServerPort); //me
                //udpServer = new UdpEndpoint("192.168.100.238", 38157); //ceo
                udpServer.EndpointDetected += EndpointDetected;
                udpServer.DatagramReceived += DatagramReceived;
                udpServer.StartServer();

                myUtil.FileLogger("udpServer start : " + localIP + ":38157");
            }
            catch (Exception ex)
            {
                myUtil.FileLogger("Startup exception : " + ex.ToString());
            }


            CreateHostBuilder(args).Build().Run();
        }
        static public void EndpointDetected(object sender, EndpointMetadata md)
        {
            //Console.WriteLine("Endpoint detected: " + md.Ip + ":" + md.Port);
        }
        static public void DatagramReceived(object sender, Datagram dg)
        {
            //Console.WriteLine("[" + dg.Ip + ":" + dg.Port + "]: " + Encoding.UTF8.GetString(dg.Data));

            try
            {
                string str = Encoding.UTF8.GetString(dg.Data);
                string msg = JwtEncoder.JwtDecode(str, "www.jawootek.com");
                //Console.WriteLine(msg);

                string[] data = msg.Split('_');
                //string target = data[0]; // 전체 or 기기고유코드
                //string title = data[1];
                //string message = data[2];

                //string pdata = "[Recv from Host] ";
                //pdata += ", 타켓:" + target + ", 제목:" + title + ", 내용:" + message;
                //myUtil.FileLogger("==========================================================");
                //myUtil.FileLogger(pdata);

                SendToFirebaseMessagingServerAsync(data);
                //await OnGetAsync();
            }
            catch (Exception ex)
            {
                myUtil.FileLogger("[Recv from Host] Err : " + ex.Message);
            }
        }
        static public async Task SendToFirebaseMessagingServerAsync(String[] _data)
        {
            await OnGetAsync();
        }
        static public async Task OnGetAsync()
        {
            _authFilePath = _rootPath + ".\\Auth_parking.json";
            FirebaseApp app = null;
            try
            {
                app = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(_authFilePath)
                }, "myApp");
            }
            catch (Exception ex)
            {
                app = FirebaseApp.GetInstance("myApp");
            }

            var fcm = FirebaseAdmin.Messaging.FirebaseMessaging.GetMessaging(app);
            Message message = new Message()
            {
                Notification = new Notification
                {
                    Title = "My push notification title",
                    Body = "Content for this push notification"
                },
                Data = new Dictionary<string, string>()
                 {
                     { "AdditionalData1", "data 1" },
                     { "AdditionalData2", "data 2" },
                     { "AdditionalData3", "data 3" },
                 },

                Topic = "fcm_test"
            };

            try
            {
                result = await fcm.SendAsync(message);
                Console.WriteLine("sent fcm message");
            }
            catch (Exception ex)
            {
                Console.WriteLine("sent fcm message error : " + ex.Message );
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {


                    //webBuilder.ConfigureKestrel(options =>
                    //{
                    //    options.ListenAnyIP(5000, (httpsOpt) => //https
                    //    {
                    //        httpsOpt.UseHttps();
                    //    });
                    //    options.ListenAnyIP(5001);              //http
                    //});



                    webBuilder.UseStartup<Startup>();
                });
    }
}
