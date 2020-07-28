using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Senparc.Ncf.Core.Models.DataBaseModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.Xncf.WeixinManager.Models
{
    public class UserTagConfigurationMapping : ConfigurationMappingWithIdBase<UserTag, int>
    {
        public override void Configure(EntityTypeBuilder<UserTag> builder)
        {
            base.Configure(builder);

            builder.HasOne(z => z.MpAccount)
               .WithMany(z => z.UserTags)
               .HasForeignKey(z => z.MpAccountId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName($"FK__{nameof(UserTag)}__{nameof(UserTag.MpAccountId)}");
        }
    }
}
