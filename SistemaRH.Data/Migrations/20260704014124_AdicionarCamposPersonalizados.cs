using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaRH.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCamposPersonalizados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CamposPersonalizadosJson",
                table: "Funcionarios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CamposPersonalizados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Rotulo = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Opcoes = table.Column<string>(type: "TEXT", nullable: false),
                    Obrigatorio = table.Column<bool>(type: "INTEGER", nullable: false),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false),
                    AplicavelA = table.Column<int>(type: "INTEGER", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CamposPersonalizados", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CamposPersonalizados");

            migrationBuilder.DropColumn(
                name: "CamposPersonalizadosJson",
                table: "Funcionarios");
        }
    }
}
