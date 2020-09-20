using Senparc.Ncf.XncfBase.Database;
using System;
using System.IO;

namespace Senparc.Xncf.WeixinManager
{
    /// <summary>
    /// 设计时 DbContext 创建（仅在开发时创建 Code-First 的数据库 Migration 使用，在生产环境不会执行）
    /// </summary>
    public class SenparcDbContextFactory : SenparcDesignTimeDbContextFactoryBase<WeixinSenparcEntities, Register>
    {
        /// <summary>
        /// 用于寻找 App_Data 文件夹，一般为项目根目录，从而找到数据库连接字符串配置信息
        /// </summary>
        public static string RootDictionaryPath => Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\");

        public SenparcDbContextFactory() : base(RootDictionaryPath)
        {

        }
    }
}
