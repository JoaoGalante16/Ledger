using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ledger.Migrations
{
    /// <inheritdoc />
    public partial class RestringeExclusaoDeUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contas_AspNetUsers_IdUsuario",
                table: "Contas");

            migrationBuilder.AddForeignKey(
                name: "FK_Contas_AspNetUsers_IdUsuario",
                table: "Contas",
                column: "IdUsuario",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contas_AspNetUsers_IdUsuario",
                table: "Contas");

            migrationBuilder.AddForeignKey(
                name: "FK_Contas_AspNetUsers_IdUsuario",
                table: "Contas",
                column: "IdUsuario",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
