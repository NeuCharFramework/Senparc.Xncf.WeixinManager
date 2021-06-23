using Microsoft.AspNetCore.Mvc;
using Senparc.NeuChar;
using Senparc.Xncf.WeixinManager.Services.WebApiAndSwagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Senparc.Xncf.WeixinManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FindWeixinApiController : ControllerBase
    {
        private readonly FindWeixinApiService _findWeixinApiService;

        public FindWeixinApiController(FindWeixinApiService findWeixinApiService)
        {
            this._findWeixinApiService = findWeixinApiService;
        }

        [HttpGet]
        public FindWeixinApiResult OnGetAsync(PlatformType? platformType, bool? isAsync, string keyword)
        {
            var result = _findWeixinApiService.FindWeixinApiResult(platformType, isAsync, keyword);
            return result;
        }
    }
}
