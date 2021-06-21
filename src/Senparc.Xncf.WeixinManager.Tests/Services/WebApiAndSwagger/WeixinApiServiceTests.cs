using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Extensions;
using Senparc.Xncf.WeixinManager.Services;
using Senparc.Xncf.WeixinManager.Tests;
using System;
using System.Collections.Generic;
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
            Init();

            //程序执行会自动触发

            var weixinApis = Senparc.NeuChar.ApiBind.ApiBindInfoCollection.Instance.GetGroupedCollection();
            var weixinApiService = new WeixinApiService();
            var apiGroup = weixinApis.FirstOrDefault(z => z.Key == NeuChar.PlatformType.WeChat_OfficialAccount);
            if (apiGroup == null)
            {
                throw new Exception("找不到 Platform");
            }
            var result = weixinApiService.GetWeixinApiAssembly(apiGroup);
            Console.WriteLine("API Count:" + result.apiCount);
        }
    }
}