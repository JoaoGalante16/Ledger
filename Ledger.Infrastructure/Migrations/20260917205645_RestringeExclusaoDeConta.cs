using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ledger.Migrations
{
    /// <inheritdoc />
    public partial class RestringeExclusaoDeConta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lancamentos_Contas_NumeroConta",
                table: "Lancamentos");

            migrationBuilder.AddForeignKey(
                name: "FK_Lancamentos_Contas_NumeroConta",
                table: "Lancamentos",
                column: "NumeroConta",
                principalTable: "Contas",
                principalColumn: "Numero",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lancamentos_Contas_NumeroConta",
                table: "Lancamentos");

            migrationBuilder.AddForeignKey(
                name: "FK_Lancamentos_Contas_NumeroConta",
                table: "Lancamentos",
                column: "NumeroConta",
                principalTable: "Contas",
                principalColumn: "Numero",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
