using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Portal.Data.Migrations
{
    public partial class ManageMinecraftAndForgeVersionInDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MinecraftVersion",
                table: "Projects",
                newName: "MinecraftVersionId");

            migrationBuilder.RenameColumn(
                name: "ForgeVersion",
                table: "Projects",
                newName: "ForgeVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ForgeVersionId",
                table: "Projects",
                column: "ForgeVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_MinecraftVersionId",
                table: "Projects",
                column: "MinecraftVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_ForgeVersions_ForgeVersionId",
                table: "Projects",
                column: "ForgeVersionId",
                principalTable: "ForgeVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_MinecraftVersions_MinecraftVersionId",
                table: "Projects",
                column: "MinecraftVersionId",
                principalTable: "MinecraftVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ForgeVersions_ForgeVersionId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_MinecraftVersions_MinecraftVersionId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ForgeVersionId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_MinecraftVersionId",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "MinecraftVersionId",
                table: "Projects",
                newName: "MinecraftVersion");

            migrationBuilder.RenameColumn(
                name: "ForgeVersionId",
                table: "Projects",
                newName: "ForgeVersion");
        }
    }
}
