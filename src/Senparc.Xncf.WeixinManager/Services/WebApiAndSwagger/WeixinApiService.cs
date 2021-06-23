using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Senparc.Ncf.Core.Config;
using Senparc.Ncf.Core.Extensions;
using Senparc.NeuChar;
using Senparc.Xncf.WeixinManager.Services.WebApiAndSwagger;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Senparc.Xncf.WeixinManager.Services
{
    public class WeixinApiService
    {
        //private static ConcurrentDictionary<PlatformType, Dictionary<string, ApiBindInfo>> _apiCollection = new ConcurrentDictionary<PlatformType, Dictionary<string, ApiBindInfo>>();

        public static ConcurrentDictionary<PlatformType, Assembly> WeixinApiAssemblyCollection { get; set; } = new ConcurrentDictionary<PlatformType, Assembly>();

        public static ConcurrentDictionary<PlatformType, string> WeixinApiAssemblyNames { get; private set; } = new ConcurrentDictionary<PlatformType, string>(); //= "WeixinApiAssembly";
        public static ConcurrentDictionary<PlatformType, string> WeixinApiAssemblyVersions { get; private set; } = new ConcurrentDictionary<PlatformType, string>(); //= "WeixinApiAssembly";

        private bool _showDetailApiLog = false;

        private int _taskCount;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskCount">同时执行线程数</param>
        /// <param name="showDetailApiLog"></param>
        public WeixinApiService(int taskCount = 30, bool showDetailApiLog = false)
        {
            _taskCount = taskCount;
            _showDetailApiLog = showDetailApiLog;

            //测试时关闭部分模块
            WeixinApiAssemblyNames[PlatformType.WeChat_OfficialAccount] = $"NeuCharDocApi.{PlatformType.WeChat_OfficialAccount}";
            WeixinApiAssemblyNames[PlatformType.WeChat_MiniProgram] = $"NeuCharDocApi.{PlatformType.WeChat_MiniProgram}";
            WeixinApiAssemblyNames[PlatformType.WeChat_Open] = $"NeuCharDocApi.{PlatformType.WeChat_Open}";
            WeixinApiAssemblyNames[PlatformType.WeChat_Work] = $"NeuCharDocApi.{PlatformType.WeChat_Work}";
            //WeixinApiAssemblyNames[PlatformType.WeChat_Tenpay] = $"NeuCharDocApi.{PlatformType.WeChat_Tenpay}";
        }

        /// <summary>
        /// 控制台打印日志
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="addLogCondition"></param>
        private void WriteLog(string msg, bool addLogCondition = false)
        {
            if (!addLogCondition || _showDetailApiLog)
            {
                Console.WriteLine($"{SystemTime.Now:yyyy-MM-dd HH:ss:mm.ffff}[{Thread.CurrentThread.ManagedThreadId:00}]\t\t{msg}");
            }
        }

        /// <summary>
        /// 获取全局统一 docName
        /// </summary>
        /// <param name="platformType"></param>
        /// <returns></returns>
        public static string GetDocName(PlatformType platformType)
        {
            return $"{platformType}-v{WeixinApiAssemblyVersions[platformType]}";
        }


        /// <summary>
        /// 检查并创建物理目录
        /// </summary>
        private void TryCreateDir()
        {
            var dir = Path.Combine(SiteConfig.WebRootPath, "..", "App_Data", "ApiDocXml");// ServerUtility.ContentRootMapPath("~/App_Data/ApiDocXml");
            WriteLog($"检查目录：{dir}");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                WriteLog($"创建目录：{dir}");
            }
        }

        /// <summary>
        /// 获取 DocName
        /// </summary>
        /// <param name="xmlAttr"></param>
        /// <returns></returns>
        public DocMethodInfo GetDocMethodInfo(string xmlAttr)
        {
            var pattern = @"(M\:)(?<docName>[^(]+)(?<paramsPart>\({1}.+\){1})";
            var result = Regex.Match(xmlAttr, pattern);
            if (result.Success && result.Groups["docName"] != null && result.Groups["paramsPart"] != null)
            {
                return new DocMethodInfo(result.Groups["docName"].Value, result.Groups["paramsPart"].Value);
            }
            return new DocMethodInfo(null, null);

            //以下方法速度略慢：
            /*
            if (xmlAttr.StartsWith("M:"))
            {
                var methodLastIndex = xmlAttr.IndexOf("(");
                return (xmlAttr[2..methodLastIndex], xmlAttr[methodLastIndex..]);
            }

            return (null, null);
            */
        }

        /// <summary>
        /// 生成单个线程（或 Task任务）的 API 操作（最小粒度）
        /// </summary>
        private void BuildApiForOneThread(IGrouping<PlatformType, KeyValuePair<string, ApiBindInfo>> apiGroup,
            KeyValuePair<string, ApiBindInfo>[] apiBindInfoBlock, ConcurrentDictionary<string, string> apiMethodName, string keyName,
            TypeBuilder tb, ConcurrentDictionary<string, DocMembersCollectionValue> docMembersCollection, int apiIndex)
        {
            foreach (var apiBindInfo in apiBindInfoBlock)
            {
                #region ApiBuild

                try
                {
                    if (apiIndex > 9999)//200-250
                    {
                        return;//用于小范围分析
                    }

                    var platformType = apiGroup.Key;

                    //定义版本号
                    if (!WeixinApiAssemblyVersions.ContainsKey(platformType))
                    {
                        WeixinApiAssemblyVersions[platformType] = apiBindInfo.Value.MethodInfo.DeclaringType.Assembly.GetName().Version.ToString(3);
                    }

                    //当前方法名称
                    var methodName = apiBindInfo.Value.ApiBindAttribute.Name.Replace(".", "_").Replace("-", "_").Replace("/", "_");
                    var apiGroupName = apiBindInfo.Value.ApiBindAttribute.Name.Split('.')[0];
                    var indexOfApiGroupDot = apiBindInfo.Value.ApiBindAttribute.Name.IndexOf(".");
                    var apiName = apiBindInfo.Value.ApiBindAttribute.Name.Substring(indexOfApiGroupDot + 1, apiBindInfo.Value.ApiBindAttribute.Name.Length - indexOfApiGroupDot - 1);

                    //确保名称不会有重复
                    while (apiMethodName.ContainsKey(methodName))
                    {
                        methodName += "0";
                        apiName += "0";
                    }
                    apiMethodName[methodName] = apiName;

                    //当前 API 的 MethodInfo
                    MethodInfo apiMethodInfo = apiBindInfo.Value.MethodInfo;
                    //当前 API 的所有参数信息
                    var parameters = apiMethodInfo.GetParameters();

                    if (_showDetailApiLog)
                    {
                        WriteLog($"\t search API[{apiIndex}]: {keyName} > {apiBindInfo.Key} -> {methodName} \t\t Parameters Count: {parameters.Count()}\t\t");
                    }


                    MethodBuilder setPropMthdBldr =
                        tb.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Virtual,
                        apiMethodInfo.ReturnType, //返回类型
                        parameters.Select(z => z.ParameterType).ToArray()//输入参数
                        );

                    //Controller已经使用过一次SwaggerOperationAttribute
                    var t2_3 = typeof(SwaggerOperationAttribute);
                    var tagName = new[] { $"{keyName}:{apiGroupName}" };
                    var tagAttrBuilder = new CustomAttributeBuilder(t2_3.GetConstructor(new Type[] { typeof(string), typeof(string) }),
                        new object[] { (string)null, (string)null },
                        new[] { t2_3.GetProperty("Tags") }, new[] { tagName });
                    setPropMthdBldr.SetCustomAttribute(tagAttrBuilder);
                    //其他Method排序方法参考：https://stackoverflow.com/questions/34175018/grouping-of-api-methods-in-documentation-is-there-some-custom-attribute


                    //[Route("/wxapi/...", Name="xxx")]
                    var t2_4 = typeof(RouteAttribute);
                    //var routeName = apiBindInfo.Value.ApiBindAttribute.Name.Split('.')[0];
                    var apiPath = $"/wxapi/{keyName}/{apiGroupName}/{apiName}";
                    var routeAttrBuilder = new CustomAttributeBuilder(t2_4.GetConstructor(new Type[] { typeof(string) }),
                        new object[] { apiPath }/*, new[] { t2_2.GetProperty("Name") }, new[] { routeName }*/);
                    setPropMthdBldr.SetCustomAttribute(routeAttrBuilder);

                    WriteLog($"added Api path: {apiPath}", true);

                    //[HttpGet]
                    var t3 = typeof(HttpGetAttribute);
                    setPropMthdBldr.SetCustomAttribute(new CustomAttributeBuilder(t3.GetConstructor(new Type[0]), new object[0]));

                    //用户限制  ——  Jeffrey Su 2021.06.18
                    //var t4 = typeof(UserAuthorizeAttribute);//[UserAuthorize("UserOnly")]
                    //setPropMthdBldr.SetCustomAttribute(new CustomAttributeBuilder(t4.GetConstructor(new Type[] { typeof(string) }), new[] { "UserOnly" }));


                    //设置返回类型
                    //setPropMthdBldr.SetReturnType(apiMethodInfo.ReturnType);

                    //设置参数
                    var boundType = false;
                    //定义其他参数
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var p = parameters[i];
                        ParameterBuilder pb = setPropMthdBldr.DefineParameter(i + 1/*从1开始，0为返回值*/, p.Attributes, p.Name);
                        //处理参数，反之出现复杂类型的参数，抛出异常：InvalidOperationException: Action 'WeChat_OfficialAccountController.CardApi_GetOrderList (WeixinApiAssembly)' has more than one parameter that was specified or inferred as bound from request body. Only one parameter per action may be bound from body. Inspect the following parameters, and use 'FromQueryAttribute' to specify bound from query, 'FromRouteAttribute' to specify bound from route, and 'FromBodyAttribute' for parameters to be bound from body:
                        if (p.ParameterType.IsClass)
                        {
                            if (boundType == false)
                            {
                                //第一个绑定，可以不处理
                                boundType = true;
                            }
                            else
                            {
                                //第二个开始使用标签
                                var tFromQuery = typeof(FromQueryAttribute);
                                pb.SetCustomAttribute(new CustomAttributeBuilder(tFromQuery.GetConstructor(new Type[0]), new object[0]));
                            }
                        }
                        try
                        {
                            //设置默认值
                            if (p.HasDefaultValue)
                            {
                                pb.SetConstant(p.DefaultValue);
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                    }

                    //执行具体方法
                    var il = setPropMthdBldr.GetILGenerator();

                    //FieldBuilder fb = tb.DefineField("id", typeof(System.String), FieldAttributes.Private);

                    var methodInfo = apiMethodInfo;

                    LocalBuilder local = null;
                    if (methodInfo.ReturnType != typeof(void))
                    {
                        //il.Emit(OpCodes.Ldstr, "The I.M implementation of C");
                        local = il.DeclareLocal(apiMethodInfo.ReturnType); // create a local variable
                                                                           //il.Emit(OpCodes.Ldarg_0);
                                                                           //动态创建字段   
                    }


                    //il.Emit(OpCodes.Ldarg_0); // this  //静态方法不需要使用this
                    //il.Emit(OpCodes.Ldarg_1); // the first one in arguments list
                    il.Emit(OpCodes.Nop); // the first one in arguments list
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var p = parameters[i];
                        //WriteLog($"\t\t Ldarg: {p.Name}\t isOptional:{p.IsOptional}\t defaultValue:{p.DefaultValue}");
                        il.Emit(OpCodes.Ldarg, i + 1); // the first one in arguments list
                    }

                    //WriteLog($"\t get static method: {methodInfo.Name}\t returnType:{methodInfo.ReturnType}");

                    il.Emit(OpCodes.Call, methodInfo);

                    ////if (apiMethodInfo.GetType() == apiMethodInfo.DeclaringType)//注意：此处使用不同的方法，会出现不同的异常
                    //if (typeof(Senparc.Weixin.MP.CommonAPIs.CommonApi) == methodInfo.DeclaringType)
                    //    il.Emit(OpCodes.Call, methodInfo);
                    //else
                    //    il.Emit(OpCodes.Callvirt, methodInfo);

                    if (methodInfo.ReturnType != typeof(void))
                    {
                        il.Emit(OpCodes.Stloc, local); // set local variable
                        il.Emit(OpCodes.Ldloc, local); // load local variable to stack 
                                                       //il.Emit(OpCodes.Pop);
                    }

                    il.Emit(OpCodes.Ret);

                    //var dt1 = SystemTime.Now;
                    //修改XML文档
                    BuildXmlDoc(methodName, methodInfo, tb, docMembersCollection);
                    //WriteLog($"methodName 文档修改耗时：{SystemTime.DiffTotalMS(dt1)}ms");
                }
                catch (Exception ex)
                {
                    //遇到错误
                    WriteLog($"==== Error ====\r\n \t{ex.ToString()}");
                }
                #endregion

            }
        }

        /// <summary>
        /// 创建动态程序集
        /// </summary>
        /// <param name="assembleName"></param>
        /// <param name="apiGroup"></param>
        /// <returns></returns>
        private BuildDynamicAssemblyResult BuildDynamicAssembly(string assembleName, IGrouping<PlatformType, KeyValuePair<string, ApiBindInfo>> apiGroup)
        {
            var platformType = apiGroup.Key;

            //动态创建程序集
            AssemblyName dynamicApiAssemblyName = new AssemblyName(assembleName); //Assembly.GetExecutingAssembly().GetName();// new AssemblyName("DynamicAssembly");
            AppDomain currentDomain = Thread.GetDomain();
            AssemblyBuilder dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicApiAssemblyName, AssemblyBuilderAccess.RunAndCollect);

            //动态创建模块
            ModuleBuilder mb = dynamicAssembly.DefineDynamicModule(dynamicApiAssemblyName.Name);

            //储存 API
            //_apiCollection[platformType] = new Dictionary<string, ApiBindInfo>(apiGroup);
            var controllerKeyName = platformType.ToString();//不要随意改规则，全局需要保持一致

            WriteLog($"search key: {platformType} -> {controllerKeyName}", true);

            //动态创建类 XXController
            var controllerClassName = $"{controllerKeyName}Controller";
            TypeBuilder tb = mb.DefineType(controllerClassName, TypeAttributes.Public, typeof(Controller));
            //ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            //多个Get参数放在同一个Controller中可能发生问题：NotSupportedException: HTTP method "GET" & path "wxapi/WeChat_OfficialAccount/CommonApi_CreateMenu" overloaded by actions - WeChat_OfficialAccountController.CommonApi_CreateMenu (WeixinApiAssembly),WeChat_OfficialAccountController.CommonApi_CreateMenu (WeixinApiAssembly). Actions require unique method/path combination for OpenAPI 3.0. Use ConflictingActionsResolver as a workaround

            /*以下方法如果遇到参数含有 IEnumerable<T> 参数，会抛出异常：
             * InvalidOperationException: Action 'WeChat_MiniProgramController.TcbApi_UpdateIndex (NeuCharDocApi.WeChat_MiniProgram)' has more than one parameter that was specified or inferred as bound from request body. Only one parameter per action may be bound from body. Inspect the following parameters, and use 'FromQueryAttribute' to specify bound from query, 'FromRouteAttribute' to specify bound from route, and 'FromBodyAttribute' for parameters to be bound from body:
            IEnumerable<CreateIndex> create_indexes
            IEnumerable<DropIndex> drop_indexes
             */
            //var t = typeof(ApiControllerAttribute);
            //tb.SetCustomAttribute(new CustomAttributeBuilder(t.GetConstructor(new Type[0]), new object[0]));


            //暂时取消登录验证  —— Jeffrey Su 2021.06.18
            //var t_0 = typeof(AuthorizeAttribute);
            //tb.SetCustomAttribute(new CustomAttributeBuilder(t_0.GetConstructor(new Type[0]), new object[0]));


            var t2 = typeof(RouteAttribute);
            tb.SetCustomAttribute(new CustomAttributeBuilder(t2.GetConstructor(new Type[] { typeof(string) }), new object[] { $"/wxapi/{controllerKeyName}" }));

            //添加Controller级别的分类（暂时无效果）

            var t2_0 = typeof(SwaggerOperationAttribute);
            var t2_0_tagName = new[] { controllerKeyName };
            var t2_0_tagAttrBuilder = new CustomAttributeBuilder(t2_0.GetConstructor(new Type[] { typeof(string), typeof(string) }),
                new object[] { (string)null, (string)null },
                new[] { t2_0.GetProperty("Tags") }, new[] { t2_0_tagName });
            tb.SetCustomAttribute(t2_0_tagAttrBuilder);

            ////添加返回类型标签 https://docs.microsoft.com/zh-cn/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-2.2&tabs=visual-studio
            //var t2_1 = typeof(ProducesAttribute);
            //tb.SetCustomAttribute(new CustomAttributeBuilder(t2_1.GetConstructor(new Type[] { typeof(string), typeof(string[]) }), new object[] { "application/json", (string[])null }));

            ////添加针对Controller的GroupName
            //var t2_2 = typeof(ApiExplorerSettingsAttribute);
            //var t2_2_groupName = platformType.ToString();
            //var t2_2_tagAttrBuilder = new CustomAttributeBuilder(t2_2.GetConstructor(new Type[0]), new object[0],
            //    new[] { t2_0.GetProperty("GroupName") }, new[] { t2_2_groupName });
            //tb.SetCustomAttribute(t2_2_tagAttrBuilder);

            return new BuildDynamicAssemblyResult(assemblyBuilder: dynamicAssembly, mb: mb, tb: tb, controllerKeyName: controllerClassName);
        }

        /// <summary>
        /// 获取 XML 中所有 Member
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private async IAsyncEnumerable<XElement> GetXmlMembers(XElement root)
        {
            foreach (var item in root.Element("members").Elements("member"))
            {
                yield return item;
            }
        }

        /// <summary>
        /// 创建动态 WebApi
        /// </summary>
        public async Task<int> BuildWebApi(IGrouping<PlatformType, KeyValuePair<string, ApiBindInfo>> apiGroup)
        {
            var platformType = apiGroup.Key;
            var dt1 = SystemTime.Now;
            WriteLog("", true);
            WriteLog($"==== Begin BuildWebApi for {platformType} ====", true);

            if (apiGroup.Count() == 0 || !WeixinApiAssemblyNames.ContainsKey(platformType))
            {
                WriteLog($"apiGroup 不存在可用对象: {platformType}");
                return 0;
            }

            var groupStartTime = SystemTime.Now;

            var xmlFileName = $"{apiGroup.First().Value.MethodInfo.DeclaringType.Assembly.GetName().Name}.xml";//XML 文件名
            var sourceName = $"Senparc.Xncf.WeixinManager.App_Data.ApiDocXml.{xmlFileName}";//嵌入资源地址
            var sourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(sourceName);

            var assembleName = WeixinApiAssemblyNames[platformType];

            XDocument document = await XDocument.LoadAsync(sourceStream, LoadOptions.None, Task.Factory.CancellationToken);
            var root = document.Root;
            root.Element("assembly").Element("name").Value = assembleName;
            var docMembers = GetXmlMembers(root);// root.Element("members").Elements("member");

            ConcurrentDictionary<string, DocMembersCollectionValue> docMembersCollection = new ConcurrentDictionary<string, DocMembersCollectionValue>();

            //var dtDoc = SystemTime.Now;
            await foreach (var x in docMembers)
            {
                if (x.HasAttributes)
                {
                    var docMethodInfo = GetDocMethodInfo(x.FirstAttribute.Value);
                    if (docMethodInfo.MethodName != null && docMethodInfo.ParamsPart != null)
                    {
                        docMembersCollection[docMethodInfo.MethodName] = new DocMembersCollectionValue(x, docMethodInfo.ParamsPart);
                    }
                }
            }

            //WriteLog($"docMembersCollection init cost:{SystemTime.DiffTotalMS(dtDoc)}ms");

            //WriteLog("Document Root Name:" + root.Name);
            //WriteLog($"find docMembers:{docMembers.Count()}");

            #region 动态创建程序集

            var dynamicAssembly = BuildDynamicAssembly(assembleName, apiGroup);

            #endregion

            var apiFilterList = apiGroup.Where(z => !z.Value.ApiBindAttribute.Name.EndsWith("Async")
                                     && z.Value.MethodInfo.ReturnType != typeof(Task<>)
                                     && z.Value.MethodInfo.ReturnType != typeof(void)
                                     && !z.Value.MethodInfo.IsGenericMethod //SemanticApi.SemanticSend 是泛型方法

                                     //临时过滤 IEnumerable 对象   —— Jeffrey Su 2021.06.17
                                     && !z.Value.MethodInfo.GetParameters().Any(z =>
                                                        z.IsOut ||
                                                        z.ParameterType.Name.Contains("IEnumerable") ||
                                                        z.ParameterType.Name.Contains("IList`1")))
                                .OrderBy(z => z.Value.ApiBindAttribute.Name)
                                .ToArray();

            //把 CommonApi 提前到头部
            //Func<KeyValuePair<string, ApiBindInfo>, bool> funcCommonApi = z => z.Value.ApiBindAttribute.Name.StartsWith("CommonApi.");
            //var commonApiList = filterList.Where(z => funcCommonApi(z)).ToList();
            //filterList.RemoveAll(z => funcCommonApi(z));
            //filterList.InsertRange(0, commonApiList);

            int apiIndex = 0;
            ConcurrentDictionary<string, string> apiMethodName = new ConcurrentDictionary<string, string>();

            List<Task> taskList = new List<Task>();

            //预分配每个线程需要领取的任务（索引范围）
            var apiFilterMaxIndex = apiFilterList.Length - 1;//最大索引
            var avgBlockCount = (int)((apiFilterList.Length - 1) / _taskCount);//每个线程（块）领取的平均数量
            var lastEndIndex = -1;//上一个块的结束索引

            for (int taskIndex = 0; taskIndex < _taskCount; taskIndex++)
            {
                if (lastEndIndex >= apiFilterMaxIndex)
                {
                    break;//已经排满，终止
                }

                var blockStart = Math.Min(lastEndIndex + 1, apiFilterMaxIndex);//当前块起始索引
                var blockEnd = 0;//当前快结束索引
                if (taskIndex == _taskCount - 1 || /*最后一个快，一直分配到最后（解决余数问题）*/
                    avgBlockCount == 0                  /*如果API总数比线程数还要少，则只够一个模块*/)
                {
                    blockEnd = apiFilterMaxIndex;//
                }
                else
                {
                    blockEnd = Math.Min(blockStart + avgBlockCount, apiFilterMaxIndex);//非最后一个快，取平均数量
                }
                lastEndIndex = blockEnd;//记录当前快位置

                var apiTask = Task.Factory.StartNew(() =>
                {
                    Range blockRange = blockStart..(blockEnd + 1);
                    var apiBindInfoBlock = apiFilterList[blockRange];//截取一段，分配给当前任务

                    apiIndex++;

                    #region ApiBuild

                    BuildApiForOneThread(apiGroup, apiBindInfoBlock, apiMethodName, dynamicAssembly.ControllerKeyName, dynamicAssembly.Tb, docMembersCollection, apiIndex);

                    #endregion
                });
                taskList.Add(apiTask);
            }

            await Task.WhenAll(taskList);

            WriteLog("Api Task Count:" + taskList.Count, true);

            TypeInfo objectTypeInfo = dynamicAssembly.Tb.CreateTypeInfo();
            var myType = objectTypeInfo.AsType();

            WriteLog($"\t create type:  {myType.Namespace} - {myType.FullName}");

            //WeixinApiAssembly = myType.Assembly;//注意：此处会重复赋值相同的对象，不布偶股影响效率

            #region 保存新的 Xml 文件

            var newDocFile = $"App_Data/ApiDocXml/{assembleName}.xml";
            try
            {
                //using (XmlWriter xw = XmlWriter.Create(newDocFile, new XmlWriterSettings() { Async = true }))
                //{
                //    await document.SaveAsync(xw, new CancellationToken()).ConfigureAwait(false);//保存
                //}
                document.Save(newDocFile);

                WriteLog($"new document file saved: {newDocFile}, assembly cost:{SystemTime.NowDiff(groupStartTime)}");
            }
            catch (Exception ex)
            {
                WriteLog($"save document xml faild: {ex.Message}\r\n{ex.ToString()}");
            }

            #endregion

            WeixinApiAssemblyCollection[platformType] = dynamicAssembly.Mb.Assembly;//储存程序集

            var timeCost = SystemTime.NowDiff(dt1);

            WriteLog($"==== Finish BuildWebApi for {platformType} / Total Time: {timeCost.TotalMilliseconds:###,###} ms ====");
            WriteLog("");

            return apiMethodName.Count;
        }

        private void BuildXmlDoc(string methodName, MethodInfo methodInfo, TypeBuilder tb, ConcurrentDictionary<string, DocMembersCollectionValue> docMembersCollection)
        {
            //生成文档
            var docMethodName = $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";//以(结尾确定匹配到完整的方法名

            if (_showDetailApiLog)
            {
                WriteLog($"\t search for docName:  {docMethodName}");//\t\tSDK Method：{apiMethodInfo.ToString()}
            }

            if (docMembersCollection.ContainsKey(docMethodName))
            {
                var docMethodInfo = docMembersCollection[docMethodName];
                // like: "M:Senparc.Weixin.MP.AdvancedAPIs.AnalysisApi.GetArticleSummary(System.String,System.String,System.String,System.Int32)"
                var newAttrName = $"M:{tb.FullName}.{methodName}{docMethodInfo.ParamsPart}";
                docMethodInfo.Element.SetValue(newAttrName);
                //WriteLog($"\t change document name:  {attr.Value} -> {newAttrName}");
            }
        }


        /// <summary>
        /// 获取 WeixinApiAssembly 程序集对象
        /// </summary>
        /// <returns></returns>
        public Assembly GetWeixinApiAssembly(PlatformType platformType)
        {
            return WeixinApiAssemblyCollection[platformType];
        }

        /// <summary>
        /// 初始化动态API
        /// </summary>
        public void InitDynamicApi(IMvcBuilder builder)
        {
            //预载入程序集，确保在下一步 RegisterApiBind() 可以顺利读取所有接口
            bool preLoad = typeof(Senparc.Weixin.MP.AdvancedAPIs.AddGroupResult).ToString() != null
                && typeof(Senparc.Weixin.WxOpen.AdvancedAPIs.CustomApi).ToString() != null
                && typeof(Senparc.Weixin.Open.AccountAPIs.AccountApi).ToString() != null
                && typeof(Senparc.Weixin.TenPay.V3.TenPayV3).ToString() != null
                && typeof(Senparc.Weixin.Work.AdvancedAPIs.AppApi).ToString() != null;

            //确保 ApiBind 已经执行
            Senparc.NeuChar.Register.RegisterApiBind(preLoad);//参数为 true，确保重试绑定成功

            //确保目录存在
            TryCreateDir();

            var dt1 = SystemTime.Now;

            var weixinApis = Senparc.NeuChar.ApiBind.ApiBindInfoCollection.Instance.GetGroupedCollection();

            ConcurrentDictionary<PlatformType, (int apiCount, double costMs)> assemblyBuildStat = new ConcurrentDictionary<PlatformType, (int, double)>();

            List<Task> taskList = new List<Task>();

            //因为模块数量比较少，这里使用异步反而会开销略大
            //WeixinApiAssemblyNames.Keys.AsParallel().ForAll(async platformType =>
            //WeixinApiAssemblyNames.Keys.ToList().ForEach(platformType =>
            foreach (var platformType in WeixinApiAssemblyNames.Keys)
            {
                var wrapperTask = Task.Factory.StartNew(async () =>
                   {
                       //此处使用 Task 效率并不比 Keys.ToList() 方法快
                       WriteLog($"get weixinApis groups: {weixinApis.Count()}, now dealing with: {platformType}");
                       var dtStart = SystemTime.Now;
                       var apiGroup = weixinApis.FirstOrDefault(z => z.Key == platformType);

                       var apiCount = await BuildWebApi(apiGroup).ConfigureAwait(false);
                       var weixinApiAssembly = GetWeixinApiAssembly(platformType);

                       builder.AddApplicationPart(weixinApiAssembly);//程序部件：https://docs.microsoft.com/zh-cn/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-2.2

                       assemblyBuildStat[platformType] = (apiCount: apiCount, costMs: SystemTime.DiffTotalMS(dtStart));
                   });
                taskList.Add(wrapperTask.Unwrap());
            }

            Task.WaitAll(taskList.ToArray());

            #region 统计数据
            var totalCost = SystemTime.DiffTotalMS(dt1);

            //Func<object, int, string> outputResult = (text, length) => string.Format($"{{0,{length}}}", text);

            WriteLog("");
            WriteLog(string.Format("{0,25} | {1,15}| {2,15} |{3,15}", "Platform Name", "API Count", "Cost Time", "Average"));
            WriteLog(new string('-', 80));
            foreach (var item in assemblyBuildStat)
            {
                var apiCount = item.Value.apiCount;
                var cost = item.Value.costMs;
                var avg = Math.Round(cost / apiCount, 3);
                WriteLog(string.Format("{0,25} | {1,15}| {2,15} |{3,15}", item.Key, apiCount, $"{cost}ms", $"{avg}ms"));
            }
            WriteLog(new string('=', 80));
            var totalApi = assemblyBuildStat.Values.Sum(z => z.apiCount);
            WriteLog(string.Format("{0,25} | {1,15}| {2,15} |{3,15}", $"Total", $"API Count:{totalApi}", $"Cost:{totalCost}ms", $""));
            WriteLog($"Total Average Cost: {Math.Round(totalCost / totalApi, 4)} ms \t\tTask Count: {_taskCount}");
            WriteLog("");

            #endregion
        }
    }
}
