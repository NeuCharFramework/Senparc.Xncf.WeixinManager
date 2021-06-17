﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Senparc.Xncf.WeixinManager.Models;

namespace Senparc.Xncf.WeixinManager.Migrations.Migrations.Sqlite
{
    [DbContext(typeof(WeixinSenparcEntities_Sqlite))]
    partial class WeixinSenparcEntities_SqliteModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Models.MpAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AddTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("AdminRemark")
                        .HasMaxLength(300)
                        .HasColumnType("TEXT");

                    b.Property<string>("AppId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("AppSecret")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("EncodingAESKey")
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<bool>("Flag")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastUpdateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Logo")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Remark")
                        .HasMaxLength(300)
                        .HasColumnType("TEXT");

                    b.Property<int>("TenantId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("WeixinManager_MpAccount");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Models.UserTag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AddTime")
                        .HasColumnType("datetime");

                    b.Property<string>("AdminRemark")
                        .HasMaxLength(300)
                        .HasColumnType("TEXT");

                    b.Property<int>("Count")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Flag")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastUpdateTime")
                        .HasColumnType("datetime");

                    b.Property<int>("MpAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Remark")
                        .HasMaxLength(300)
                        .HasColumnType("TEXT");

                    b.Property<int>("TagId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TenantId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("MpAccountId");

                    b.ToTable("WeixinManager_UserTag");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Models.UserTag_WeixinUser", b =>
                {
                    b.Property<int>("UserTagId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WeixinUserId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AddTime")
                        .HasColumnType("datetime");

                    b.Property<bool>("Flag")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastUpdateTime")
                        .HasColumnType("datetime");

                    b.Property<int>("TenantId")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserTagId", "WeixinUserId");

                    b.HasIndex("WeixinUserId");

                    b.ToTable("WeixinManager_UserTag_WeixinUser");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Models.WeixinUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AddTime")
                        .HasColumnType("datetime");

                    b.Property<string>("AdminRemark")
                        .HasMaxLength(300)
                        .HasColumnType("TEXT");

                    b.Property<string>("City")
                        .HasColumnType("TEXT");

                    b.Property<string>("Country")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Flag")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Groupid")
                        .HasColumnType("INTEGER");

                    b.Property<string>("HeadImgUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("Language")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastUpdateTime")
                        .HasColumnType("datetime");

                    b.Property<int>("MpAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("NickName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OpenId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Province")
                        .HasColumnType("TEXT");

                    b.Property<uint>("Qr_Scene")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Qr_Scene_Str")
                        .HasColumnType("TEXT");

                    b.Property<string>("Remark")
                        .HasColumnType("TEXT");

                    b.Property<int>("Sex")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Subscribe")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Subscribe_Scene")
                        .HasColumnType("TEXT");

                    b.Property<long>("Subscribe_Time")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TenantId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UnionId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("MpAccountId");

                    b.ToTable("WeixinManager_WeixinUser");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Models.UserTag", b =>
                {
                    b.HasOne("Senparc.Xncf.WeixinManager.Models.MpAccount", "MpAccount")
                        .WithMany("UserTags")
                        .HasForeignKey("MpAccountId")
                        .HasConstraintName("FK__UserTag__MpAccountId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("MpAccount");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Models.UserTag_WeixinUser", b =>
                {
                    b.HasOne("Senparc.Xncf.WeixinManager.Models.UserTag", "UserTag")
                        .WithMany("UserTags_WeixinUsers")
                        .HasForeignKey("UserTagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Senparc.Xncf.WeixinManager.Models.WeixinUser", "WeixinUser")
                        .WithMany("UserTags_WeixinUsers")
                        .HasForeignKey("WeixinUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserTag");

                    b.Navigation("WeixinUser");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Models.WeixinUser", b =>
                {
                    b.HasOne("Senparc.Xncf.WeixinManager.Models.MpAccount", "MpAccount")
                        .WithMany("WeixinUsers")
                        .HasForeignKey("MpAccountId")
                        .HasConstraintName("FK__WeixinUser__MpAccountId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("MpAccount");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Models.MpAccount", b =>
                {
                    b.Navigation("UserTags");

                    b.Navigation("WeixinUsers");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Models.UserTag", b =>
                {
                    b.Navigation("UserTags_WeixinUsers");
                });

            modelBuilder.Entity("Senparc.Xncf.WeixinManager.Models.WeixinUser", b =>
                {
                    b.Navigation("UserTags_WeixinUsers");
                });
#pragma warning restore 612, 618
        }
    }
}