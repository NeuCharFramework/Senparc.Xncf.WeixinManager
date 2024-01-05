using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using Senparc.NeuChar.ApiHandlers;
using Senparc.Weixin.MP.MessageContexts;

namespace Senparc.Xncf.WeixinManager
{
    public class WechatAiContext : DefaultMpMessageContext
    {
        internal static ConcurrentDictionary<string, IWantToRun> IWantoRunDic = new ConcurrentDictionary<string, IWantToRun>();

        internal static IWantToRun GetIWantToRun(IServiceProvider services, PromptConfigParameter promptConfigParameter, string modelName, string openId)
        {
            if (!IWantoRunDic.ContainsKey(openId))
            {
                SemanticAiHandler _semanticAiHandler = (SemanticAiHandler)services.GetRequiredService<IAiHandler>();

                //配置和初始化模型
                var chatConfig = _semanticAiHandler.ChatConfig(promptConfigParameter, userId: openId,
                                               modelName: modelName /*, modelName: "gpt-4-32k"*/);

                var iWantToRun = chatConfig.iWantToRun;

                //IWantoRunDic.TryAdd(openId, iWantToRun);
                IWantoRunDic[openId] = iWantToRun;
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
