using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Portal.Data.Migrations
{
    public partial class UpdateModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessRights_SafeUsers_UserId",
                table: "AccessRights");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AccessRights",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessRights_SafeUsers_UserId",
                table: "AccessRights",
                column: "UserId",
                principalTable: "SafeUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessRights_SafeUsers_UserId",
                table: "AccessRights");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AccessRights",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddForeignKey(
                name: "FK_AccessRights_SafeUsers_UserId",
                table: "AccessRights",
                column: "UserId",
                principalTable: "SafeUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
