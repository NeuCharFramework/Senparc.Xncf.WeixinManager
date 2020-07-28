using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.RegisterServices;
using Senparc.Ncf.Core.Config;
using Senparc.Ncf.Core.Models;
using Senparc.Ncf.XncfBase.Database;
using System;
using System.Configuration;
using System.IO;

namespace Senparc.Xncf.WeixinManager
{
    /// <summary>
    /// 设计时 DbContext 创建（仅在开发时创建 Code-First 的数据库 Migration 使用，在生产环境不会执行）
    /// </summary>
    public class SenparcDbContextFactory : SenparcDesignTimeDbContextFactoryBase<WeixinSenparcEntities, Register>
    {
        /// <summary>
        /// 用于寻找 App_Data 文件夹，从而找到数据库连接字符串配置信息
        /// </summary>
        public override string RootDictionaryPath => Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"/*项目根目录*/);
    }
}
