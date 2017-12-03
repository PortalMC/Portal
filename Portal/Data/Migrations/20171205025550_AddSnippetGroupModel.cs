using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Portal.Data.Migrations
{
    public partial class AddSnippetGroupModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Group",
                table: "Snippets",
                newName: "GroupId");

            migrationBuilder.CreateTable(
                name: "SnippetGroups",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DisplayName = table.Column<string>(nullable: true),
                    Order = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnippetGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Snippets_GroupId",
                table: "Snippets",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Snippets_SnippetGroups_GroupId",
                table: "Snippets",
                column: "GroupId",
                principalTable: "SnippetGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Snippets_SnippetGroups_GroupId",
                table: "Snippets");

            migrationBuilder.DropTable(
                name: "SnippetGroups");

            migrationBuilder.DropIndex(
                name: "IX_Snippets_GroupId",
                table: "Snippets");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "Snippets",
                newName: "Group");
        }
    }
}
