using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ledger.Migrations
{
    /// <inheritdoc />
    public partial class RenomeiaLancamentoReferenciaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lancamentos_Lancamentos_LancamentoReferenciaId",
                table: "Lancamentos");

            migrationBuilder.RenameColumn(
                name: "LancamentoReferenciaId",
                table: "Lancamentos",
                newName: "IdLancamentoReferencia");

            migrationBuilder.RenameIndex(
                name: "IX_Lancamentos_LancamentoReferenciaId",
                table: "Lancamentos",
                newName: "IX_Lancamentos_IdLancamentoReferencia");

            migrationBuilder.AddForeignKey(
                name: "FK_Lancamentos_Lancamentos_IdLancamentoReferencia",
                table: "Lancamentos",
                column: "IdLancamentoReferencia",
                principalTable: "Lancamentos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lancamentos_Lancamentos_IdLancamentoReferencia",
                table: "Lancamentos");

            migrationBuilder.RenameColumn(
                name: "IdLancamentoReferencia",
                table: "Lancamentos",
                newName: "LancamentoReferenciaId");

            migrationBuilder.RenameIndex(
                name: "IX_Lancamentos_IdLancamentoReferencia",
                table: "Lancamentos",
                newName: "IX_Lancamentos_LancamentoReferenciaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lancamentos_Lancamentos_LancamentoReferenciaId",
                table: "Lancamentos",
                column: "LancamentoReferenciaId",
                principalTable: "Lancamentos",
                principalColumn: "Id");
        }
    }
}
