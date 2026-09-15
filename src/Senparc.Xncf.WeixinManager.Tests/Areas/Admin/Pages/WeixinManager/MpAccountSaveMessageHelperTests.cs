using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.Xncf.WeixinManager.Areas.Admin.Pages.WeixinManager;
using System;

namespace Senparc.Xncf.WeixinManager.Tests.Areas.Admin.Pages.WeixinManager
{
    [TestClass]
    public class MpAccountSaveMessageHelperTests
    {
        [TestMethod]
        public void BuildAccessTokenErrorMessage_ReturnsWhitelistMessage_WhenInvalidIpIsPresent()
        {
            var ex = new Exception("errcode:40164,errmsg:invalid ip 127.0.0.1 ipv6 ::1, not in whitelist rid: 123456");

            var result = MpAccountSaveMessageHelper.BuildAccessTokenErrorMessage(ex);

            StringAssert.Contains(result, "IP 白名单");
            StringAssert.Contains(result, "127.0.0.1");
            StringAssert.Contains(result, "::1");
        }

        [TestMethod]
        public void BuildAccessTokenErrorMessage_ReturnsDefaultMessage_WhenErrorIsNotWhitelistRelated()
        {
            var ex = new Exception("appid invalid");

            var result = MpAccountSaveMessageHelper.BuildAccessTokenErrorMessage(ex);

            Assert.AreEqual("账号已经保存，但 AppId 或 AppSecret 可能无效，或当前服务器暂时无法正常访问微信接口，请检查后重试。", result);
        }

        [TestMethod]
        public void BuildAccessTokenErrorMessage_ReturnsWhitelistMessage_WhenOnlyIpv6IsPresent()
        {
            var ex = new Exception("errcode:40164,errmsg:invalid ipv6 ::1, not in whitelist rid: 654321");

            var result = MpAccountSaveMessageHelper.BuildAccessTokenErrorMessage(ex);

            StringAssert.Contains(result, "IP 白名单");
            StringAssert.Contains(result, "::1");
        }

        [TestMethod]
        public void BuildAccessTokenErrorMessage_ReturnsWhitelistMessage_WhenIpv6AppearsBeforeIpv4()
        {
            var ex = new Exception("errcode:40164,errmsg:ipv6 ::1 invalid ip 127.0.0.1, not in whitelist rid: 888888");

            var result = MpAccountSaveMessageHelper.BuildAccessTokenErrorMessage(ex);

            StringAssert.Contains(result, "127.0.0.1");
            StringAssert.Contains(result, "::1");
        }
    }
}
