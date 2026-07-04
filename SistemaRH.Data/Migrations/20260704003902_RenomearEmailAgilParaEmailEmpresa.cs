using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaRH.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenomearEmailAgilParaEmailEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailAgil",
                table: "Funcionarios",
                newName: "EmailEmpresa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailEmpresa",
                table: "Funcionarios",
                newName: "EmailAgil");
        }
    }
}
