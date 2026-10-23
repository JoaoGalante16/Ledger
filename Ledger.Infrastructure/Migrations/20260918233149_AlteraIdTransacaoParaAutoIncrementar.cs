using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ledger.Migrations
{
    /// <inheritdoc />
    public partial class AlteraIdTransacaoParaAutoIncrementar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "TransacaoIdSeq");

            migrationBuilder.AlterColumn<int>(
                name: "IdTransacao",
                table: "Lancamentos",
                type: "integer",
                nullable: false,
                defaultValueSql: "nextval('\"TransacaoIdSeq\"')",
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "TransacaoIdSeq");

            migrationBuilder.AlterColumn<int>(
                name: "IdTransacao",
                table: "Lancamentos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValueSql: "nextval('\"TransacaoIdSeq\"')");
        }
    }
}
