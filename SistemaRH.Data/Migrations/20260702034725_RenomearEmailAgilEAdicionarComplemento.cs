using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaRH.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenomearEmailAgilEAdicionarComplemento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email2",
                table: "Funcionarios",
                newName: "EmailAgil");

            migrationBuilder.AddColumn<string>(
                name: "Complemento",
                table: "Funcionarios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Complemento",
                table: "Funcionarios");

            migrationBuilder.RenameColumn(
                name: "EmailAgil",
                table: "Funcionarios",
                newName: "Email2");
        }
    }
}
