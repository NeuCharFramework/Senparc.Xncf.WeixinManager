using Senparc.Ncf.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Senparc.Xncf.WeixinManager.Models
{
    public class UserTag_CreateOrUpdateDto : DtoBase
    {
        public int MpAccountId { get; set; }
        public int TagId { get; set; }
        [Required]
        public string Name { get; set; }
        public int Count { get; set; }

        public UserTag_CreateOrUpdateDto() { }

        public MpAccount MpAccount { get; set; }

        public IList<UserTag_WeixinUser> UserTags_WeixinUsers { get; set; }
    }
}
