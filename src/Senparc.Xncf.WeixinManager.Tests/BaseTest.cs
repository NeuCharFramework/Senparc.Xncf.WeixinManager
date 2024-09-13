using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.CO2NET;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.RegisterServices;
using Senparc.Ncf.Core;
using Senparc.Ncf.Core.Areas;
using Senparc.Ncf.Core.AssembleScan;
using Senparc.Ncf.Core.Config;
using Senparc.Ncf.Database;
using Senparc.Ncf.Database.Sqlite;
using Senparc.Ncf.XncfBase;
using Senparc.Xncf.AreasBase;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Senparc.Xncf.WeixinManager.Tests
{
    [TestClass]
    public class BaseTest
    {
        public IServiceCollection ServiceCollection { get; }
        public IServiceProvider ServiceProvider { get; set; }
        public IConfiguration Configuration { get; set; }

        protected IRegisterService registerService;
        protected SenparcSetting _senparcSetting;

        protected Mock<IWebHostEnvironment/*IHostingEnvironment*/> _env;

        public BaseTest()
        {
            _env = new Mock<IWebHostEnvironment/*IHostingEnvironment*/>();
            _env.Setup(z => z.ContentRootPath).Returns(() => Path.GetFullPath("..\\..\\..\\"));

            Init();

            //SiteConfig.WebRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
            //ServiceCollection = new ServiceCollection();
            //var result = ServiceCollection.StartEngine(Configuration);

        }

        public void Init()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            RegisterServiceCollection();
            RegisterServiceStart();
            Console.WriteLine("��� TaseTest ��ʼ��");
        }

        /// <summary>
        /// ע�� IServiceCollection �� MemoryCache
        /// </summary>
        public void RegisterServiceCollection()
        {
            var serviceCollection = new ServiceCollection();
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appsettings.json", false, false);
            var config = configBuilder.Build();
            Configuration = config;

            _senparcSetting = new SenparcSetting() { IsDebug = true };
            config.GetSection("SenparcSetting").Bind(_senparcSetting);


            SiteConfig.WebRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");

            Senparc.Ncf.Core.Register.TryRegisterMiniCore(services => { });
            serviceCollection.AddSenparcGlobalServices(config);

            serviceCollection.AddMemoryCache();//ʹ���ڴ滺��
            serviceCollection.AddRouting();

            serviceCollection.StartWebEngine(config, _env.Object, null);

            var builder = serviceCollection.AddRazorPages();

            //builder.AddNcfAreas(_env.Object);

            //�Զ�����ע��ɨ��
            serviceCollection.ScanAssamblesForAutoDI();
            //�Ѿ���������г����Զ�ɨ���ί�У�����ִ��ɨ�裨���룩
            AssembleScanHelper.RunScan(null);


            //var result = serviceCollection.StartNcfEngine(Configuration, _env.Object, null);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// ע�� RegisterService.Start()
        /// </summary>
        public void RegisterServiceStart(bool autoScanExtensionCacheStrategies = false)
        {
            //ע��
            registerService = Senparc.CO2NET.AspNet.RegisterServices.RegisterService.Start(_env.Object, _senparcSetting)
                .UseSenparcGlobal(autoScanExtensionCacheStrategies);

            var builder = WebApplication.CreateBuilder();

            //var app = builder.Build();// new ApplicationBuilder(ServiceProvider);
            var app = new ApplicationBuilder(ServiceProvider);


            app.UseNcfDatabase<SqliteMemoryDatabaseConfiguration>();//ʹ�� SQLServer���ݿ�


            app.UseRouting();

            //app
            //app.MapRazorPages();
            //app.MapControllers();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapRazorPages();
            //    endpoints.MapControllers();
            //});

            Console.WriteLine("Senparc.Ncf.XncfBase.Register.UseXncfModules");
            //XncfModules�����룩
            Senparc.Ncf.XncfBase.Register.UseXncfModules(app, registerService);
        }
    }
}
