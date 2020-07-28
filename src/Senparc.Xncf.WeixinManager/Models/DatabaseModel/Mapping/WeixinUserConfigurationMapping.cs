using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Senparc.Ncf.Core.Models.DataBaseModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.Xncf.WeixinManager.Models
{
    public class WeixinUserConfigurationMapping : ConfigurationMappingWithIdBase<WeixinUser, int>
    {
        public override void Configure(EntityTypeBuilder<WeixinUser> builder)
        {
            base.Configure(builder);

            builder.HasOne(z => z.MpAccount)
                .WithMany(z => z.WeixinUsers)
                .HasForeignKey(z => z.MpAccountId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName($"FK__{nameof(WeixinUser)}__{nameof(WeixinUser.MpAccountId)}");
        }
    }
}
