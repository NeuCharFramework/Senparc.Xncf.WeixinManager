﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Senparc.Xncf.WeixinManager.Domain.Models.MultipleDatabase;

#nullable disable

namespace Senparc.Xncf.WeixinManager.Domain.Migrations.Migrations.MySql
{
    [DbContext(typeof(WeixinSenparcEntities_MySql))]
    partial class WeixinSenparcEntities_MySqlModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.MpAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("AddTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("AdminRemark")
                        .HasMaxLength(300)
                        .HasColumnType("varchar(300)");

                    b.Property<string>("AppId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("AppSecret")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("EncodingAESKey")
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.Property<bool>("Flag")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("LastUpdateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Logo")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("PromptRangeCode")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Remark")
                        .HasMaxLength(300)
                        .HasColumnType("varchar(300)");

                    b.Property<int>("TenantId")
                        .HasColumnType("int");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.HasKey("Id");

                    b.ToTable("WeixinManager_MpAccount");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.UserTag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("AddTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("AdminRemark")
                        .HasMaxLength(300)
                        .HasColumnType("varchar(300)");

                    b.Property<int>("Count")
                        .HasColumnType("int");

                    b.Property<bool>("Flag")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("LastUpdateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("MpAccountId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Remark")
                        .HasMaxLength(300)
                        .HasColumnType("varchar(300)");

                    b.Property<int>("TagId")
                        .HasColumnType("int");

                    b.Property<int>("TenantId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("MpAccountId");

                    b.ToTable("WeixinManager_UserTag");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.UserTag_WeixinUser", b =>
                {
                    b.Property<int>("UserTagId")
                        .HasColumnType("int");

                    b.Property<int>("WeixinUserId")
                        .HasColumnType("int");

                    b.Property<DateTime>("AddTime")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("Flag")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("LastUpdateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("TenantId")
                        .HasColumnType("int");

                    b.HasKey("UserTagId", "WeixinUserId");

                    b.HasIndex("WeixinUserId");

                    b.ToTable("WeixinManager_UserTag_WeixinUser");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.WeixinUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("AddTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("AdminRemark")
                        .HasMaxLength(300)
                        .HasColumnType("varchar(300)");

                    b.Property<string>("City")
                        .HasColumnType("longtext");

                    b.Property<string>("Country")
                        .HasColumnType("longtext");

                    b.Property<bool>("Flag")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("Groupid")
                        .HasColumnType("int");

                    b.Property<string>("HeadImgUrl")
                        .HasColumnType("longtext");

                    b.Property<string>("Language")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("LastUpdateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("MpAccountId")
                        .HasColumnType("int");

                    b.Property<string>("NickName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("OpenId")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Province")
                        .HasColumnType("longtext");

                    b.Property<uint>("Qr_Scene")
                        .HasColumnType("int unsigned");

                    b.Property<string>("Qr_Scene_Str")
                        .HasColumnType("longtext");

                    b.Property<string>("Remark")
                        .HasColumnType("longtext");

                    b.Property<int>("Sex")
                        .HasColumnType("int");

                    b.Property<int>("Subscribe")
                        .HasColumnType("int");

                    b.Property<string>("Subscribe_Scene")
                        .HasColumnType("longtext");

                    b.Property<long>("Subscribe_Time")
                        .HasColumnType("bigint");

                    b.Property<int>("TenantId")
                        .HasColumnType("int");

                    b.Property<string>("UnionId")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("MpAccountId");

                    b.ToTable("WeixinManager_WeixinUser");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.UserTag", b =>
                {
                    b.HasOne("Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.MpAccount", "MpAccount")
                        .WithMany("UserTags")
                        .HasForeignKey("MpAccountId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK__UserTag__MpAccountId");

                    b.Navigation("MpAccount");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.UserTag_WeixinUser", b =>
                {
                    b.HasOne("Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.UserTag", "UserTag")
                        .WithMany("UserTags_WeixinUsers")
                        .HasForeignKey("UserTagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.WeixinUser", "WeixinUser")
                        .WithMany("UserTags_WeixinUsers")
                        .HasForeignKey("WeixinUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserTag");

                    b.Navigation("WeixinUser");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.WeixinUser", b =>
                {
                    b.HasOne("Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.MpAccount", "MpAccount")
                        .WithMany("WeixinUsers")
                        .HasForeignKey("MpAccountId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK__WeixinUser__MpAccountId");

                    b.Navigation("MpAccount");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.MpAccount", b =>
                {
                    b.Navigation("UserTags");

                    b.Navigation("WeixinUsers");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.UserTag", b =>
                {
                    b.Navigation("UserTags_WeixinUsers");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.WeixinUser", b =>
                {
                    b.Navigation("UserTags_WeixinUsers");
                });
#pragma warning restore 612, 618
        }
    }
}
