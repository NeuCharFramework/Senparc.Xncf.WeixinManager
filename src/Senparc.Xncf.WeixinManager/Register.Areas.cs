using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Senparc.Ncf.Core.Areas;
using System.Collections.Generic;

namespace Senparc.Xncf.WeixinManager
{
	public partial class Register : IAreaRegister //注册 XNCF 页面接口（按需选用）
	{
		#region IAreaRegister 接口

		public string HomeUrl => "/Admin/WeixinManager/Index";

		public List<AreaPageMenuItem> AareaPageMenuItems => new List<AreaPageMenuItem>() {
			 new AreaPageMenuItem(GetAreaUrl("/Admin/WeixinManager/Index"),"首页","fa fa-laptop"),
			 new AreaPageMenuItem(GetAreaUrl("/Admin/WeixinManager/MpAccount"),"公众号管理","fa fa-comments"),
			 new AreaPageMenuItem(GetAreaUrl("/Admin/WeixinManager/WeixinUser"),"用户管理","fa fa-users"),
		};

		public IMvcBuilder AuthorizeConfig(IMvcBuilder builder, IWebHostEnvironment env)
		{
			return builder;
		}

		#endregion
	}
}
