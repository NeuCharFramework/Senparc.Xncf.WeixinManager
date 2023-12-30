using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Trace;
using Senparc.Ncf.XncfBase;
using Senparc.NeuChar.Entities;
using Senparc.NeuChar.MessageHandlers;
using Senparc.Weixin.Entities;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageContexts;
using Senparc.Weixin.MP.MessageHandlers.Middleware;
using Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.Dto;
using Senparc.Xncf.WeixinManager.Domain.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Senparc.Xncf.WeixinManager
{
    public partial class Register : IXncfMiddleware  //需要引入中间件的模块
    {
        public static ConcurrentDictionary<string, Type> MpMessageHandlerNames = new ConcurrentDictionary<string, Type>();

        #region IXncfMiddleware 接口

        public IApplicationBuilder UseMiddleware(IApplicationBuilder app)
        {
            try
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var messageHandlerTypes = AppDomain.CurrentDomain.GetAssemblies()
                                        .SelectMany(a =>
                                        {
                                            try
                                            {
                                                var aTypes = a.GetTypes();
                                                return aTypes.Where(t =>
                                                {
                                                    var attr = t.GetCustomAttributes(false).FirstOrDefault(z => z is MpMessageHandlerAttribute) as MpMessageHandlerAttribute;
                                                    if (attr != null)
                                                    {
                                                        MpMessageHandlerNames[attr.Name] = t;
                                                        return true;
                                                    }
                                                    return false;
                                                }
                                                     //!t.IsAbstract &&
                                                     //t is IMessageHandler &&
                                                     );
                                            }
                                            catch
                                            {
                                                return new List<Type>();
                                            }
                                        });

                    SenparcTrace.SendCustomLog("Senparc.Xncf.WeixinManager 搜索 MessageHandler", $"搜索到 {messageHandlerTypes.Count()} 个");

                    if (messageHandlerTypes == null || messageHandlerTypes.Count() == 0)
                    {
                        return app;//未注册
                    }

                    foreach (var mpMessageHandlerNamePair in MpMessageHandlerNames)
                    {
                        var mpMessageHandlerName = mpMessageHandlerNamePair.Key;
                        var mpMessageHandlerType = mpMessageHandlerNamePair.Value;

                        Func<IServiceProvider, MpAccountDto> mpAccountDtoFunc = serviceProvider =>
                        {
                            var httpCotextAssessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                            var httpContext = httpCotextAssessor.HttpContext;
                            var param = httpContext.Request.Query["parameter"].ToString();

                            if (!int.TryParse(param, out int mpAccountId))
                            {
                                throw new WeixinException("ID 错误！");
                            }

                            var mpAccountService = serviceProvider.GetRequiredService<MpAccountService>();
                            var mpAccountDto = mpAccountService.GetMpAccount(mpAccountId);
                            return mpAccountDto;
                        };

                        Func<Stream, PostModel, int, IServiceProvider, MessageHandler<DefaultMpMessageContext, IRequestMessageBase, IResponseMessageBase>> messageHandlerFunc =
                          (stream, postModel, maxRecordCount, services) =>
                          {
                              try
                              {
                                  using (var middlewareScope = services.CreateScope())
                                  {

                                      var mpAccountDto = mpAccountDtoFunc(middlewareScope.ServiceProvider);

                                      var senparcWeixinSetting = new SenparcWeixinSetting();
                                      senparcWeixinSetting.WeixinAppId = mpAccountDto.AppId;
                                      senparcWeixinSetting.WeixinAppSecret = mpAccountDto.AppSecret;
                                      senparcWeixinSetting.Token = mpAccountDto.Token;
                                      senparcWeixinSetting.EncodingAESKey = mpAccountDto.EncodingAESKey;

                                      Senparc.Weixin.Config.SenparcWeixinSetting[$"DynamicMP-{mpAccountDto.Id}"] = senparcWeixinSetting;

                                      var messageHandler = Activator.CreateInstance(mpMessageHandlerType, new object[] { mpAccountDto, stream, postModel, maxRecordCount, services });

                                      //TODO：使用依赖注入生成 MessageHandler

                                      //SenparcTrace.SendCustomLog("messageHandler 类型", messageHandler.GetType().FullName);
                                      return messageHandler as MessageHandler<DefaultMpMessageContext, IRequestMessageBase, IResponseMessageBase>;
                                  }
                              }
                              catch (Exception ex)
                              {
                                  throw new Exception($"{mpMessageHandlerType.FullName} 必须具有以下结构和参数顺序的构造函数：(MpAccountDto mpAccountDto, Stream inputStream, PostModel postModel, int maxRecordCount, IServiceProvider serviceProvider)", ex);
                              }
                          };


                        //注册中间件
                        app.UseMessageHandlerForMp($"/WeixinMp/{mpMessageHandlerName}", messageHandlerFunc, options =>
                        {
                            //说明：此代码块中演示了较为全面的功能点，简化的使用可以参考下面小程序和企业微信

                            #region 配置 SenparcWeixinSetting 参数，以自动提供 Token、EncodingAESKey 等参数

                            //此处为委托，可以根据条件动态判断输入条件（必须）
                            options.AccountSettingFunc = context =>
                            {
                                var senparcWeixinSetting = new SenparcWeixinSetting();

                                var mpAccountDto = mpAccountDtoFunc(context.RequestServices);

                                senparcWeixinSetting.WeixinAppId = mpAccountDto.AppId;
                                senparcWeixinSetting.WeixinAppSecret = mpAccountDto.AppSecret;
                                senparcWeixinSetting.Token = mpAccountDto.Token;
                                senparcWeixinSetting.EncodingAESKey = mpAccountDto.EncodingAESKey;
                                return senparcWeixinSetting;

                            };

                            //TODO：注册 Config.SenparcWeixinSetting

                            //方法二：使用指定配置：
                            //Config.SenparcWeixinSetting["<Your SenparcWeixinSetting's name filled with Token, AppId and EncodingAESKey>"]; 

                            //方法三：结合 context 参数动态判断返回Setting值

                            #endregion

                            //对 MessageHandler 内异步方法未提供重写时，调用同步方法（按需）
                            options.DefaultMessageHandlerAsyncEvent = DefaultMessageHandlerAsyncEvent.SelfSynicMethod;

                            //对发生异常进行处理（可选）
                            options.AggregateExceptionCatch = ex =>
                            {
                                //逻辑处理...
                                return false;//系统层面抛出异常
                            };
                        });

                    }


                    //var mpAccounts = GetAllMpAccounts(scope.ServiceProvider).Where(z => true);
                    //foreach (var mpAccountDto in mpAccounts)
                    //{
                    //    var messageHandlerType = messageHandlerTypes.First();//TODO:筛选

                    //    Func<Stream, PostModel, int, IServiceProvider, MessageHandler<DefaultMpMessageContext, IRequestMessageBase, IResponseMessageBase>> messageHandlerFunc =
                    //        (stream, postModel, maxRecordCount, services) =>
                    //    {
                    //        try
                    //        {
                    //            var messageHandler = Activator.CreateInstance(messageHandlerType, new object[] { mpAccountDto, stream, postModel, maxRecordCount, services });

                    //            //TODO：使用依赖注入生成 MessageHandler

                    //            //SenparcTrace.SendCustomLog("messageHandler 类型", messageHandler.GetType().FullName);
                    //            return messageHandler as MessageHandler<DefaultMpMessageContext, IRequestMessageBase, IResponseMessageBase>;
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            throw new Exception($"{messageHandlerType.FullName} 必须具有以下结构和参数顺序的构造函数：(MpAccountDto mpAccountDto, Stream inputStream, PostModel postModel, int maxRecordCount, IServiceProvider serviceProvider)", ex);
                    //        }
                    //    };

                    //    //注册中间件
                    //    app.UseMessageHandlerForMp($"/WeixinMp/{mpAccountDto.Id}", messageHandlerFunc, options =>
                    //    {
                    //        //说明：此代码块中演示了较为全面的功能点，简化的使用可以参考下面小程序和企业微信

                    //        #region 配置 SenparcWeixinSetting 参数，以自动提供 Token、EncodingAESKey 等参数

                    //        var senparcWeixinSetting = new SenparcWeixinSetting();

                    //        senparcWeixinSetting.WeixinAppId = mpAccountDto.AppId;
                    //        senparcWeixinSetting.WeixinAppSecret = mpAccountDto.AppSecret;
                    //        senparcWeixinSetting.Token = mpAccountDto.Token;
                    //        senparcWeixinSetting.EncodingAESKey = mpAccountDto.EncodingAESKey;

                    //        //此处为委托，可以根据条件动态判断输入条件（必须）
                    //        options.AccountSettingFunc = context => senparcWeixinSetting;


                    //        //TODO：注册 Config.SenparcWeixinSetting

                    //        //方法二：使用指定配置：
                    //        //Config.SenparcWeixinSetting["<Your SenparcWeixinSetting's name filled with Token, AppId and EncodingAESKey>"]; 

                    //        //方法三：结合 context 参数动态判断返回Setting值

                    //        #endregion

                    //        //对 MessageHandler 内异步方法未提供重写时，调用同步方法（按需）
                    //        options.DefaultMessageHandlerAsyncEvent = DefaultMessageHandlerAsyncEvent.SelfSynicMethod;

                    //        //对发生异常进行处理（可选）
                    //        options.AggregateExceptionCatch = ex =>
                    //            {
                    //                //逻辑处理...
                    //                return false;//系统层面抛出异常
                    //            };
                    //    });
                    //}

                }
                return app;

            }
            catch (Exception ex)
            {
                SenparcTrace.SendCustomLog("Senparc.Xncf.WeixinManager IXncfMiddleware 异常", ex.Message);
                SenparcTrace.BaseExceptionLog(ex);
                throw;
            }
        }

        #endregion


    }
}
