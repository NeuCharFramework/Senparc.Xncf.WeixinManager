using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Extensions;
using Senparc.Ncf.Core.Config;
using Senparc.Ncf.XncfBase;
using Senparc.Xncf.WeixinManager.Services;
using Senparc.Xncf.WeixinManager.Tests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Senparc.Xncf.WeixinManager.Services.Tests
{
    [TestClass()]
    public class WeixinApiServiceTests : BaseTest
    {
        public WeixinApiServiceTests() : base()
        {
        }

        [TestMethod()]
        public void GetWeixinApiAssemblyTest()
        {
            //Console.WriteLine("Ready for Init()");
            //Init();
            //Console.WriteLine("Finish Init()");

            SiteConfig.WebRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
            var result = ServiceCollection.StartEngine(Configuration);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            //程序执行会自动触发
            var weixinApis = Senparc.NeuChar.ApiBind.ApiBindInfoCollection.Instance.GetGroupedCollection();
            var weixinApiService = new WeixinApiService();
            var apiGroup = weixinApis.FirstOrDefault(z => z.Key == NeuChar.PlatformType.WeChat_OfficialAccount);
            if (apiGroup == null)
            {
                throw new Exception("找不到 Platform");
            }

            var apiCount = weixinApiService.BuildWebApi(apiGroup).Result;
            var weixinApiAssembly = weixinApiService.GetWeixinApiAssembly(apiGroup.Key);
            Console.WriteLine("API Count:" + apiCount);
            Console.WriteLine("Test Platform Assembly:" + weixinApiAssembly.FullName);
        }


        [TestMethod()]
        public void GetDocMethodNameTest()
        {
            var weixinApiService = new WeixinApiService();

            {
                var input = "M:Senparc.Weixin.MP.AdvancedAPIs.AnalysisApi.GetArticleSummary(System.String,System.String,System.String,System.Int32)";
                var result = weixinApiService.GetDocMethodInfo(input);
                Assert.AreEqual("Senparc.Weixin.MP.AdvancedAPIs.AnalysisApi.GetArticleSummary", result.MethodName);
                Assert.AreEqual("(System.String,System.String,System.String,System.Int32)", result.ParamsPart);
            }
            {
                var input = "T:Senparc.Weixin.MP.AdvancedAPIs.AnalysisApi";
                var result = weixinApiService.GetDocMethodInfo(input);
                Assert.AreEqual(null, result.MethodName);
                Assert.AreEqual(null, result.ParamsPart);
            }
            {
                var input = "P:Senparc.Weixin.MP.AdvancedAPIs.ShakeAround.QueryLottery_Result.drawed_value";
                var result = weixinApiService.GetDocMethodInfo(input);
                Assert.AreEqual(null, result.MethodName);
                Assert.AreEqual(null, result.ParamsPart);
            }
            {
                var input = "F:Senparc.Weixin.MP.MemberCard_CustomField_NameType.FIELD_NAME_TYPE_UNKNOW";
                var result = weixinApiService.GetDocMethodInfo(input);
                Assert.AreEqual(null, result.MethodName);
                Assert.AreEqual(null, result.ParamsPart);
            }

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine();
                {
                    var dt1 = SystemTime.Now;
                    var input = "M:Senparc.Weixin.MP.AdvancedAPIs.TemplateApi.SendTemplateMessage(System.String,System.String,Senparc.Weixin.Entities.TemplateMessage.ITemplateMessageBase,Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage.TempleteModel_MiniProgram,System.Int32)";
                    var result = weixinApiService.GetDocMethodInfo(input);
                    Console.WriteLine("Method 1 Cost:" + SystemTime.DiffTotalMS(dt1) + "ms");

                    Assert.AreEqual("Senparc.Weixin.MP.AdvancedAPIs.TemplateApi.SendTemplateMessage", result.MethodName);
                    Assert.AreEqual("(System.String,System.String,Senparc.Weixin.Entities.TemplateMessage.ITemplateMessageBase,Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage.TempleteModel_MiniProgram,System.Int32)", result.ParamsPart);
                }
            }
            
        }
    }

}