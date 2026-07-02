using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaRH.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFuncionarioEstagiario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Bolsa",
                table: "Funcionarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rg",
                table: "Funcionarios",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bolsa",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Rg",
                table: "Funcionarios");
        }
    }
}
