using System;
using System.Text.RegularExpressions;

namespace Senparc.Xncf.WeixinManager.Areas.Admin.Pages.WeixinManager
{
    public static class MpAccountSaveMessageHelper
    {
        private const string DefaultAccessTokenErrorMessage = "账号已经保存，但 AppId 或 AppSecret 可能无效，或当前服务器暂时无法正常访问微信接口，请检查后重试。";

        private static readonly Regex InvalidIpRegex = new Regex(@"invalid ip\s+(?<ip>[0-9a-fA-F\.:]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string BuildAccessTokenErrorMessage(Exception ex)
        {
            var exceptionMessage = ex?.ToString() ?? string.Empty;
            if (exceptionMessage.IndexOf("invalid ip", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exceptionMessage.IndexOf("not in whitelist", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exceptionMessage.IndexOf("40164", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var match = InvalidIpRegex.Match(exceptionMessage);
                var ipTip = match.Success
                    ? $"当前服务器 IP（{match.Groups["ip"].Value}）"
                    : "当前服务器 IP";

                return $"账号已经保存，但 {ipTip} 未加入微信公众平台“开发接口管理”中的 IP 白名单，请将该 IP 加入白名单后重试。";
            }

            return DefaultAccessTokenErrorMessage;
        }
    }
}
