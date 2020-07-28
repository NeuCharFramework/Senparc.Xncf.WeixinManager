using Senparc.Ncf.Repository;
using Senparc.Ncf.Service;
using Senparc.Xncf.WeixinManager.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.Xncf.WeixinManager.Services
{
    public class MpAccountService : ServiceBase<MpAccount>,IServiceBase<MpAccount>
    {
        public MpAccountService(IRepositoryBase<MpAccount> repo, IServiceProvider serviceProvider) : base(repo, serviceProvider)
        {
        }
    }
}
