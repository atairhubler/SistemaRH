using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaRH.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSegundoEmailContratanteEValoresPJ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Contratante",
                table: "Funcionarios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email2",
                table: "Funcionarios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TelefoneAgil",
                table: "Funcionarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorContratado",
                table: "Funcionarios",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contratante",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Email2",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "TelefoneAgil",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "ValorContratado",
                table: "Funcionarios");
        }
    }
}
