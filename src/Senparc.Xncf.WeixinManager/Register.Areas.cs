using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Senparc.Ncf.Core.Areas;
using Senparc.Xncf.WeixinManager.Services;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        public IMvcBuilder AuthorizeConfig(IMvcBuilder builder, IHostEnvironment env)
        {

            var services = builder.Services;

            //services.AddMvcCore();//不启用
            services.AddControllers();

            #region Swagger

            services.AddScoped<WeixinApiService>();


            //.NET Core 3.0 for Swagger https://www.thecodebuzz.com/swagger-api-documentation-in-net-core-3-0/

            //初始化 ApiDoc
            WeixinApiService apiDocService = new WeixinApiService();
            foreach (var neucharApiDocAssembly in WeixinApiService.WeixinApiAssemblyNames)
            {
                var weixinApiAssembly = apiDocService.GetWeixinApiAssembly(neucharApiDocAssembly.Key);
                builder.AddApplicationPart(weixinApiAssembly);//程序部件：https://docs.microsoft.com/zh-cn/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-2.2
            }


            //添加Swagger
            services.AddSwaggerGen(c =>
            {
                //为每个程序集创建文档
                foreach (var neucharApiDocAssembly in WeixinApiService.WeixinApiAssemblyCollection)
                {
                    var version = WeixinApiService.WeixinApiAssemblyVersions[neucharApiDocAssembly.Key]; //neucharApiDocAssembly.Value.ImageRuntimeVersion;
                    var docName = WeixinApiService.GetDocName(neucharApiDocAssembly.Key);
                    c.SwaggerDoc(docName, new OpenApiInfo
                    {
                        Title = $"NeuChar Dynamic WebApi Engine : {neucharApiDocAssembly.Key}",
                        Version = $"v{version}",//"v16.5.4"
                        Description = $"Senparc NeuChar WebApi 自生成引擎（{neucharApiDocAssembly.Key} - v{version}）",
                        //License = new OpenApiLicense()
                        //{
                        //    Name = "Apache License Version 2.0",
                        //    Url = new Uri("https://github.com/JeffreySu/WeiXinMPSDK")
                        //},
                        Contact = new OpenApiContact()
                        {
                            Email = "zsu@senparc.com",
                            Name = "Senparc SDK Team",
                            Url = new Uri("https://www.senparc.com")
                        },
                        //TermsOfService = new Uri("https://github.com/JeffreySu/WeiXinMPSDK")
                    });
                    //暂时停用，检查效率

                    c.IncludeXmlComments($"App_Data/ApiDocXml/{WeixinApiService.WeixinApiAssemblyNames[neucharApiDocAssembly.Key]}.xml");
                }

                //分组显示  https://www.cnblogs.com/toiv/archive/2018/07/28/9379249.html
                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo))
                    {
                        return false;
                    }

                    var versions = methodInfo.DeclaringType
                          .GetCustomAttributes(true)
                          .OfType<SwaggerOperationAttribute>()
                          .Select(z => z.Tags[0].Split(':')[0]);

                    if (versions.FirstOrDefault() == null)
                    {
                        return false;//不符合要求的都不显示
                    }

                    //docName: $"{neucharApiDocAssembly.Key}-v1"
                    return versions.Any(z => docName.StartsWith(z));
                });

                c.OrderActionsBy(z => z.RelativePath);
                //c.DescribeAllEnumsAsStrings();//枚举显示字符串
                c.EnableAnnotations();
                c.DocumentFilter<RemoveVerbsFilter>();
                c.CustomSchemaIds(x => x.FullName);//规避错误：InvalidOperationException: Can't use schemaId "$JsApiTicketResult" for type "$Senparc.Weixin.Open.Entities.JsApiTicketResult". The same schemaId was already used for type "$Senparc.Weixin.MP.Entities.JsApiTicketResult"

                /* 需要登陆，暂不考虑    —— Jeffrey Su 2021.06.17
                var oAuthDocName = "oauth2";// WeixinApiService.GetDocName(PlatformType.WeChat_OfficialAccount);

                //添加授权
                var authorizationUrl = NeuChar.App.AppStore.Config.IsDebug
                                               //以下是 appPurachase 的 Id，实际应该是 appId
                                               //? "http://localhost:12222/App/LoginOAuth/Authorize/1002/"
                                               //: "https://www.neuchar.com/App/LoginOAuth/Authorize/4664/";
                                               //以下是正确的 appId
                                               ? "http://localhost:12222/App/LoginOAuth/Authorize?appId=xxx"
                                               : "https://www.neuchar.com/App/LoginOAuth/Authorize?appId=3035";

                c.AddSecurityDefinition(oAuthDocName,//"Bearer" 
                    new OpenApiSecurityScheme
                    {
                        Description = "请输入带有Bearer开头的Token",
                        Name = oAuthDocName,// "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.OAuth2,
                        //OpenIdConnectUrl = new Uri("https://www.neuchar.com/"),
                        Flows = new OpenApiOAuthFlows()
                        {
                            Implicit = new OpenApiOAuthFlow()
                            {
                                AuthorizationUrl = new Uri(authorizationUrl),
                                Scopes = new Dictionary<string, string> { { "swagger_api", "Demo API - full access" } }
                            }
                        }
                    });

                //认证方式，此方式为全局添加
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    { new OpenApiSecurityScheme(){ Name =oAuthDocName//"Bearer"
                    }, new List<string>() }
                    //{ "Bearer", Enumerable.Empty<string>() }
                });

                //c.OperationFilter<AuthResponsesOperationFilter>();//AuthorizeAttribute过滤

                */

            });

            #endregion

            return builder;
		}

        #endregion
    }
}
