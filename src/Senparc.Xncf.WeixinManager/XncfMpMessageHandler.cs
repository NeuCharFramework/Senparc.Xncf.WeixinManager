using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Senparc.AI.Entities;
using Senparc.AI.Kernel;
using Senparc.CO2NET.MessageQueue;
using Senparc.NeuChar.App.AppStore;
using Senparc.NeuChar.Context;
using Senparc.NeuChar.Entities;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageHandlers;
using Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.Dto;

namespace Senparc.Xncf.WeixinManager
{
    public class XncfMpMessageHandler<TMC> : MessageHandler<TMC>
        where TMC : WechatAiContext, new()
    {
        protected readonly MpAccountDto _mpAccountDto;
        protected readonly ServiceProvider _services;

        public XncfMpMessageHandler(MpAccountDto mpAccountDto, Stream stream, PostModel postModel, int maxRecordCount, ServiceProvider services) : this(stream, postModel, maxRecordCount)
        {
            this._mpAccountDto = mpAccountDto;
            this._services = services;
        }

        public XncfMpMessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0, bool onlyAllowEncryptMessage = false, DeveloperInfo developerInfo = null, IServiceProvider serviceProvider = null) : base(inputStream, postModel, maxRecordCount, onlyAllowEncryptMessage, developerInfo, serviceProvider)
        {
        }

        protected virtual Action RunBot(IServiceProvider services, RequestMessageText requestMessage) => async () =>
        {
            var dt = SystemTime.Now;

            //定义 AI 请求参数
            var parameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5
            };

            //定义 AI 模型
            var modelName = "gpt-35-turbo";//text-davinci-003

            //获取 AI 处理器
            var iWantToRun = await WechatAiContext.GetIWantToRunAsync(services, _mpAccountDto, parameter, modelName, requestMessage.FromUserName);
            SemanticAiHandler semanticAiHandler = iWantToRun.SemanticAiHandler;

            //发送到 AI 模型，获取结果
            var result = await semanticAiHandler.ChatAsync(iWantToRun, requestMessage.Content);

            if (result == null)
            {
                await Console.Out.WriteLineAsync("result is null");
            }


            //异步发送 AI 结果到用户
            var resultMsg = $"{result.Output}\r\n -- AI 计算耗时：{SystemTime.DiffTotalMS(dt)}毫秒";
            _ = Senparc.Weixin.MP.AdvancedAPIs.CustomApi.SendTextAsync(_mpAccountDto.AppId, requestMessage.FromUserName, resultMsg);

            //_ = Senparc.Weixin.MP.AdvancedAPIs.CustomApi.SendTextAsync(_mpAccountDto.AppId, requestMessage.FromUserName, $"总共耗时：{SystemTime.DiffTotalMS(dt)}ms");
        };

        public override async Task<IResponseMessageBase> OnTextRequestAsync(RequestMessageText requestMessage)
        {
            //var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            //responseMessage.Content = "您发送了文字：" + requestMessage.Content;
            //return responseMessage;

            var services = base.ServiceProvider;

            var smq = new SenparcMessageQueue();
            var smqKey = "WechatBot:" + requestMessage.MsgId;
            smq.Add(smqKey, RunBot(services, requestMessage));

            //不返回任何信息
            return this.CreateResponseMessage<ResponseMessageNoResponse>();

            //返回明文提示
            var reponseMessage = this.CreateResponseMessage<ResponseMessageText>();
            reponseMessage.Content = "消息已收到，正在思考中……";
            return reponseMessage;
        }


        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "欢迎访问公众号：" + _mpAccountDto.Name;
            return responseMessage;
        }
    }
}
