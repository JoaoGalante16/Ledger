using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ledger.Migrations
{
    /// <inheritdoc />
    public partial class LigaUsuarioEConta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdUsuario",
                table: "Contas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Contas_IdUsuario",
                table: "Contas",
                column: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Contas_AspNetUsers_IdUsuario",
                table: "Contas",
                column: "IdUsuario",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contas_AspNetUsers_IdUsuario",
                table: "Contas");

            migrationBuilder.DropIndex(
                name: "IX_Contas_IdUsuario",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "IdUsuario",
                table: "Contas");
        }
    }
}
