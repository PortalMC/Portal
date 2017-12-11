using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Portal.Data.Migrations
{
    public partial class AddMinecraftAndForgeVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MinecraftVersions",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DockerImageVersion = table.Column<string>(nullable: false),
                    Version = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinecraftVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForgeVersions",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    FileName = table.Column<string>(nullable: false),
                    IsRecommend = table.Column<bool>(nullable: false),
                    MinecraftVersionId = table.Column<string>(nullable: true),
                    Version = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForgeVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForgeVersions_MinecraftVersions_MinecraftVersionId",
                        column: x => x.MinecraftVersionId,
                        principalTable: "MinecraftVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ForgeVersions_MinecraftVersionId",
                table: "ForgeVersions",
                column: "MinecraftVersionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForgeVersions");

            migrationBuilder.DropTable(
                name: "MinecraftVersions");
        }
    }
}
