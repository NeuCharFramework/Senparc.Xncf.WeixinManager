using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Senparc.Xncf.WeixinManager.Services.WebApiAndSwagger
{
    public class DocMembersCollectionValue
    {
        public DocMembersCollectionValue(XElement element, string paramsPart)
        {
            Element = element;
            ParamsPart = paramsPart;
        }

        /// <summary>
        /// Xml 节点
        /// </summary>
        public XElement Element { get; }
        /// <summary>
        /// 参数部分，如：(System.String,System.String,System.String,System.Int32)
        /// </summary>
        public string ParamsPart { get; }
    }
}
