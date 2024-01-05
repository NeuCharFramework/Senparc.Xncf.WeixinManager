﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using Senparc.CO2NET.Extensions;
using Senparc.Weixin.MP.MessageContexts;
using Senparc.Xncf.PromptRange.Domain.Services;
using Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.Dto;

namespace Senparc.Xncf.WeixinManager
{
    public class WechatAiContext : DefaultMpMessageContext
    {
        internal static ConcurrentDictionary<string, IWantToRun> IWantoRunDic = new ConcurrentDictionary<string, IWantToRun>();

        /// <summary>
        /// 构建并获取 IWantToRun 对象
        /// </summary>
        /// <param name="services"></param>
        /// <param name="mpAccountDto"></param>
        /// <param name="promptConfigParameter"></param>
        /// <param name="modelName"></param>
        /// <param name="openId"></param>
        /// <returns></returns>
        internal static async Task<IWantToRun> GetIWantToRunAsync(IServiceProvider services, MpAccountDto mpAccountDto, PromptConfigParameter promptConfigParameter, string modelName, string openId)
        {
            if (!IWantoRunDic.ContainsKey(openId))
            {
                SemanticAiHandler _semanticAiHandler = (SemanticAiHandler)services.GetRequiredService<IAiHandler>();

                using (var scope = services.CreateScope())
                {
                    var promptItemService = scope.ServiceProvider.GetService<PromptItemService>();

                    string chatPrompt = null;

                    if (!mpAccountDto.PromptRangeCode.IsNullOrEmpty())
                    {
                        var promptResult = await promptItemService.GetWithVersionAsync(mpAccountDto.PromptRangeCode);

                        chatPrompt = Senparc.AI.DefaultSetting.DEFAULT_PROMPT_FOR_CHAT;
                    }
                    else
                    {
                        chatPrompt = Senparc.AI.DefaultSetting.DEFAULT_PROMPT_FOR_CHAT;
                    }

                    //配置和初始化模型
                    var chatConfig = _semanticAiHandler.ChatConfig(promptConfigParameter,
                                                    userId: openId,
                                                    modelName: modelName,
                                                    chatPrompt: chatPrompt
                                                    /*, modelName: "gpt-4-32k"*/);

                    var iWantToRun = chatConfig.iWantToRun;

                    //IWantoRunDic.TryAdd(openId, iWantToRun);
                    IWantoRunDic[openId] = iWantToRun;
                }
            }

            return IWantoRunDic[openId];
        }


        public override void OnRemoved()
        {
            var openId = base.RequestMessages.LastOrDefault()?.FromUserName ?? "I Don't Know";

            if (IWantoRunDic.ContainsKey(openId))
            {
                IWantoRunDic.TryRemove(openId, out var iWantToRun);
            }

            base.OnRemoved();
        }
    }
}