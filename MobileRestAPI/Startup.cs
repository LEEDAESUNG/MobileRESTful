using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleUdp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace MobileRestAPI
{
    public class Startup
    {
        //public string _rootPath;
        //public string _authFilePath;
        //public string result;
        //static UdpEndpoint udpServer;
        //static int udpServerPort = 38157;
        //CommonUtil myUtil = new CommonUtil();

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            try
            {
                //Configuration = configuration;
            //  _rootPath = env.ContentRootPath;
            //    string localIP = myUtil.Get_Ini("system config", "LOCAL_IP", "192.168.100.200");
            //    myUtil.FileLogger("Local IP:" + localIP);
            //    udpServer = new UdpEndpoint(localIP, udpServerPort); //me
            //    //udpServer = new UdpEndpoint("192.168.100.238", 38157); //ceo
            //    udpServer.EndpointDetected += EndpointDetected;
            //    udpServer.DatagramReceived += DatagramReceived;
            //    udpServer.StartServer();

            //    myUtil.FileLogger("udpServer start : " + localIP + ":38157");
            }
            catch(Exception ex)
            {
                // myUtil.FileLogger("Startup exception : " + ex.ToString());
            }
        }

        // public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddControllersWithViews().AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        //public void EndpointDetected(object sender, EndpointMetadata md)
        //{
        //    //Console.WriteLine("Endpoint detected: " + md.Ip + ":" + md.Port);
        //}
        //public void DatagramReceived(object sender, Datagram dg)
        //{
        //    //Console.WriteLine("[" + dg.Ip + ":" + dg.Port + "]: " + Encoding.UTF8.GetString(dg.Data));

        //    try
        //    {
        //        string str = Encoding.UTF8.GetString(dg.Data);
        //        //string msg = JwtEncoder.JwtDecode(str, "www.jawootek.com");
        //        //Console.WriteLine(msg);

        //        //string[] data = msg.Split('_');
        //        //string target = data[0]; // 전체 or 기기고유코드
        //        //string title = data[1];
        //        //string message = data[2];

        //        //string pdata = "[Recv from Host] ";
        //        //pdata += ", 타켓:" + target + ", 제목:" + title + ", 내용:" + message;
        //        //myUtil.FileLogger("==========================================================");
        //        //myUtil.FileLogger(pdata);

        //        //SendToFirebaseMessagingServer(data);
        //        OnGetAsync();

        //        //    if (inout == "IN")
        //        //        InCarProc(carNo, dong, ho, gateNo);
        //        //    else
        //        //        OutCarProc(carNo, dong, ho, gateNo);
        //    }
        //    catch (Exception ex)
        //    {
        //        myUtil.FileLogger("[Recv from Host] Err : " + ex.Message);
        //    }
        //}
        //void SendToFirebaseMessagingServer(String []_data)
        //{
        //    OnGetAsync();
        //}
        //public async Task OnGetAsync()
        //{
        //    _authFilePath = _rootPath + "\\Auth_parking.json";
        //    FirebaseApp app = null;
        //    try
        //    {
        //        app = FirebaseApp.Create(new AppOptions()
        //        {
        //            Credential = GoogleCredential.FromFile(_authFilePath)
        //        }, "myApp");
        //    }
        //    catch (Exception ex)
        //    {
        //        app = FirebaseApp.GetInstance("myApp");
        //    }

        //    var fcm = FirebaseAdmin.Messaging.FirebaseMessaging.GetMessaging(app);
        //    Message message = new Message()
        //    {
        //        Notification = new Notification
        //        {
        //            Title = "My push notification title",
        //            Body = "Content for this push notification"
        //        },
        //        Data = new Dictionary<string, string>()
        //         {
        //             { "AdditionalData1", "data 1" },
        //             { "AdditionalData2", "data 2" },
        //             { "AdditionalData3", "data 3" },
        //         },

        //        Topic = "fcm_test"
        //    };

        //    this.result = await fcm.SendAsync(message);
        //    Console.WriteLine("sent fcm message");
        //}
    }
}
