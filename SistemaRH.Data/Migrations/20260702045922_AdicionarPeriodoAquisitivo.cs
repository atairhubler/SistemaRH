using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaRH.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPeriodoAquisitivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PeriodoAquisitivoId",
                table: "PeriodosFerias",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoUso",
                table: "PeriodosFerias",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PeriodosAquisitivos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FeriasId = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroPeriodo = table.Column<int>(type: "INTEGER", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataFim = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataLimiteUso = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DiasDireito = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodosAquisitivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodosAquisitivos_Ferias_FeriasId",
                        column: x => x.FeriasId,
                        principalTable: "Ferias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosFerias_PeriodoAquisitivoId",
                table: "PeriodosFerias",
                column: "PeriodoAquisitivoId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosAquisitivos_FeriasId",
                table: "PeriodosAquisitivos",
                column: "FeriasId");

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodosFerias_PeriodosAquisitivos_PeriodoAquisitivoId",
                table: "PeriodosFerias",
                column: "PeriodoAquisitivoId",
                principalTable: "PeriodosAquisitivos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeriodosFerias_PeriodosAquisitivos_PeriodoAquisitivoId",
                table: "PeriodosFerias");

            migrationBuilder.DropTable(
                name: "PeriodosAquisitivos");

            migrationBuilder.DropIndex(
                name: "IX_PeriodosFerias_PeriodoAquisitivoId",
                table: "PeriodosFerias");

            migrationBuilder.DropColumn(
                name: "PeriodoAquisitivoId",
                table: "PeriodosFerias");

            migrationBuilder.DropColumn(
                name: "TipoUso",
                table: "PeriodosFerias");
        }
    }
}
