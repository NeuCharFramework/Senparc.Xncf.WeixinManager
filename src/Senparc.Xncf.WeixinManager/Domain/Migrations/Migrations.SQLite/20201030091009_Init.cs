using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Senparc.Xncf.WeixinManager.Migrations.Migrations.Sqlite
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeixinManager_MpAccount",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Flag = table.Column<bool>(nullable: false),
                    AddTime = table.Column<DateTime>(nullable: false),
                    LastUpdateTime = table.Column<DateTime>(nullable: false),
                    AdminRemark = table.Column<string>(maxLength: 300, nullable: true),
                    Remark = table.Column<string>(maxLength: 300, nullable: true),
                    Logo = table.Column<string>(maxLength: 200, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    AppId = table.Column<string>(maxLength: 100, nullable: false),
                    AppSecret = table.Column<string>(maxLength: 100, nullable: false),
                    Token = table.Column<string>(maxLength: 500, nullable: false),
                    EncodingAESKey = table.Column<string>(maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeixinManager_MpAccount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeixinManager_UserTag",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Flag = table.Column<bool>(nullable: false),
                    AddTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastUpdateTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    AdminRemark = table.Column<string>(maxLength: 300, nullable: true),
                    Remark = table.Column<string>(maxLength: 300, nullable: true),
                    MpAccountId = table.Column<int>(nullable: false),
                    TagId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Count = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeixinManager_UserTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK__UserTag__MpAccountId",
                        column: x => x.MpAccountId,
                        principalTable: "WeixinManager_MpAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WeixinManager_WeixinUser",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Flag = table.Column<bool>(nullable: false),
                    AddTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastUpdateTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    AdminRemark = table.Column<string>(maxLength: 300, nullable: true),
                    MpAccountId = table.Column<int>(nullable: false),
                    Subscribe = table.Column<int>(nullable: false),
                    OpenId = table.Column<string>(nullable: false),
                    NickName = table.Column<string>(nullable: false),
                    Sex = table.Column<int>(nullable: false),
                    Language = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    Province = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    HeadImgUrl = table.Column<string>(nullable: true),
                    Subscribe_Time = table.Column<long>(nullable: false),
                    UnionId = table.Column<string>(nullable: true),
                    Remark = table.Column<string>(nullable: true),
                    Groupid = table.Column<int>(nullable: false),
                    Subscribe_Scene = table.Column<string>(nullable: true),
                    Qr_Scene = table.Column<uint>(nullable: false),
                    Qr_Scene_Str = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeixinManager_WeixinUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK__WeixinUser__MpAccountId",
                        column: x => x.MpAccountId,
                        principalTable: "WeixinManager_MpAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WeixinManager_UserTag_WeixinUser",
                columns: table => new
                {
                    UserTagId = table.Column<int>(nullable: false),
                    WeixinUserId = table.Column<int>(nullable: false),
                    Flag = table.Column<bool>(nullable: false),
                    AddTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastUpdateTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeixinManager_UserTag_WeixinUser", x => new { x.UserTagId, x.WeixinUserId });
                    table.ForeignKey(
                        name: "FK_WeixinManager_UserTag_WeixinUser_WeixinManager_UserTag_UserTagId",
                        column: x => x.UserTagId,
                        principalTable: "WeixinManager_UserTag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeixinManager_UserTag_WeixinUser_WeixinManager_WeixinUser_WeixinUserId",
                        column: x => x.WeixinUserId,
                        principalTable: "WeixinManager_WeixinUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeixinManager_UserTag_MpAccountId",
                table: "WeixinManager_UserTag",
                column: "MpAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_WeixinManager_UserTag_WeixinUser_WeixinUserId",
                table: "WeixinManager_UserTag_WeixinUser",
                column: "WeixinUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WeixinManager_WeixinUser_MpAccountId",
                table: "WeixinManager_WeixinUser",
                column: "MpAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeixinManager_UserTag_WeixinUser");

            migrationBuilder.DropTable(
                name: "WeixinManager_UserTag");

            migrationBuilder.DropTable(
                name: "WeixinManager_WeixinUser");

            migrationBuilder.DropTable(
                name: "WeixinManager_MpAccount");
        }
    }
}
