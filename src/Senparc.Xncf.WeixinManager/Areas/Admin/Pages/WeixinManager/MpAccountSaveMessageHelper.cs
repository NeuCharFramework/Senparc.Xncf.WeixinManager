using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Senparc.Xncf.WeixinManager.Areas.Admin.Pages.WeixinManager
{
    public static class MpAccountSaveMessageHelper
    {
        private const string DefaultAccessTokenErrorMessage = "账号已经保存，但 AppId 或 AppSecret 可能无效，或当前服务器暂时无法正常访问微信接口，请检查后重试。";

        private static readonly Regex InvalidIpv4Regex = new Regex(@"invalid ip\s+(?<ip>(?:(?:25[0-5]|2[0-4]\d|1?\d?\d)\.){3}(?:25[0-5]|2[0-4]\d|1?\d?\d))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex InvalidIpv6Regex = new Regex(@"invalid (?:ip|ipv6)\s+(?<ipv6>[0-9a-fA-F:]*:[0-9a-fA-F:]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex Ipv6Regex = new Regex(@"ipv6\s+(?<ipv6>[0-9a-fA-F:]*:[0-9a-fA-F:]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string BuildAccessTokenErrorMessage(Exception ex)
        {
            var exceptionMessage = ex?.ToString() ?? string.Empty;
            if (exceptionMessage.IndexOf("invalid ip", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exceptionMessage.IndexOf("not in whitelist", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exceptionMessage.IndexOf("40164", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var ipTip = "当前服务器 IP";
                var ipList = GetWhitelistIps(exceptionMessage);
                if (ipList.Count > 0)
                {
                    ipTip = $"当前服务器 IP（{string.Join(" / ", ipList)}）";
                }

                return $"账号已经保存，但 {ipTip} 未加入微信公众平台“开发接口管理”中的 IP 白名单，请将该 IP 加入白名单后重试。";
            }

            return DefaultAccessTokenErrorMessage;
        }

        private static List<string> GetWhitelistIps(string exceptionMessage)
        {
            var ipList = new List<string>();

            var ipv4Match = InvalidIpv4Regex.Match(exceptionMessage);
            if (ipv4Match.Success)
            {
                ipList.Add(ipv4Match.Groups["ip"].Value);
            }

            foreach (var regex in new[] { InvalidIpv6Regex, Ipv6Regex })
            {
                foreach (Match match in regex.Matches(exceptionMessage))
                {
                    if (match.Groups["ipv6"].Success)
                    {
                        ipList.Add(match.Groups["ipv6"].Value);
                    }
                }
            }

            return ipList.Where(z => !string.IsNullOrWhiteSpace(z)).Distinct().ToList();
        }
    }
}
