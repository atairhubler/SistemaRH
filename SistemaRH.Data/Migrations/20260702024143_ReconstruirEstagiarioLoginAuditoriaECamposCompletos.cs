using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaRH.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReconstruirEstagiarioLoginAuditoriaECamposCompletos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Motivo",
                table: "SalarioHistorico",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "AjudaDeCusto",
                table: "Funcionarios",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AuxilioEducacao",
                table: "Funcionarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Bolsa",
                table: "Funcionarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Funcionarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Comissionado",
                table: "Funcionarios",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ComplementoSalarial",
                table: "Funcionarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DadosBancarios",
                table: "Funcionarios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataFim",
                table: "Funcionarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataInicio",
                table: "Funcionarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiaSolicitacaoNF",
                table: "Funcionarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmiteNF",
                table: "Funcionarios",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Genero",
                table: "Funcionarios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Observacoes",
                table: "Funcionarios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ramal",
                table: "Funcionarios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Rg",
                table: "Funcionarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoServico",
                table: "Funcionarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidadeContrato",
                table: "Funcionarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorServico",
                table: "Funcionarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LogsAuditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DataHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Usuario = table.Column<string>(type: "TEXT", nullable: false),
                    Acao = table.Column<string>(type: "TEXT", nullable: false),
                    Entidade = table.Column<string>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    ValoresAntes = table.Column<string>(type: "TEXT", nullable: true),
                    ValoresDepois = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsAuditoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NomeUsuario = table.Column<string>(type: "TEXT", nullable: false),
                    SenhaHash = table.Column<string>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogsAuditoria_DataHora",
                table: "LogsAuditoria",
                column: "DataHora");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NomeUsuario",
                table: "Usuarios",
                column: "NomeUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogsAuditoria");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Motivo",
                table: "SalarioHistorico");

            migrationBuilder.DropColumn(
                name: "AjudaDeCusto",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "AuxilioEducacao",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Bolsa",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Comissionado",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "ComplementoSalarial",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "DadosBancarios",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "DataFim",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "DataInicio",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "DiaSolicitacaoNF",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "EmiteNF",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Genero",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Observacoes",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Ramal",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Rg",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "TipoServico",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "ValidadeContrato",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "ValorServico",
                table: "Funcionarios");
        }
    }
}
