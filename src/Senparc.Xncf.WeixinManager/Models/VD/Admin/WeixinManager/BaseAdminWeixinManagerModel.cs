using Senparc.Ncf.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Senparc.Xncf.WeixinManager.Models.VD.Admin
{
    public class BaseAdminWeixinManagerModel : Senparc.Ncf.AreaBase.Admin.AdminXncfModulePageModelBase
    {
        public BaseAdminWeixinManagerModel(Lazy<XncfModuleService> xncfModuleService) : base(xncfModuleService)
        {
        }
    }
}
