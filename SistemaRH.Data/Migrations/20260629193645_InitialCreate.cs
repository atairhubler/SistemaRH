using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaRH.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RazaoSocial = table.Column<string>(type: "TEXT", nullable: false),
                    Cnpj = table.Column<string>(type: "TEXT", nullable: false),
                    InscricaoEstadual = table.Column<string>(type: "TEXT", nullable: false),
                    Endereco = table.Column<string>(type: "TEXT", nullable: false),
                    Cidade = table.Column<string>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    Cep = table.Column<string>(type: "TEXT", nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Funcionarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", nullable: false),
                    Endereco = table.Column<string>(type: "TEXT", nullable: false),
                    Cidade = table.Column<string>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    Cep = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TipoFuncionario = table.Column<int>(type: "INTEGER", nullable: false),
                    Cpf = table.Column<string>(type: "TEXT", nullable: true),
                    DataNascimento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Cargo = table.Column<string>(type: "TEXT", nullable: true),
                    Departamento = table.Column<string>(type: "TEXT", nullable: true),
                    DataAdmissao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataDemissao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ctps = table.Column<string>(type: "TEXT", nullable: true),
                    PisPassep = table.Column<string>(type: "TEXT", nullable: true),
                    SalarioBruto = table.Column<decimal>(type: "TEXT", nullable: true),
                    Cnpj = table.Column<string>(type: "TEXT", nullable: true),
                    RazaoSocial = table.Column<string>(type: "TEXT", nullable: true),
                    TemDireitoFerias = table.Column<bool>(type: "INTEGER", nullable: true),
                    FeriasRemuneradas = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funcionarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Funcionarios_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Atestados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FuncionarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataAtestado = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataFim = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DiasFaltados = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Cid = table.Column<string>(type: "TEXT", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: false),
                    Arquivo = table.Column<byte[]>(type: "BLOB", nullable: false),
                    NomeArquivo = table.Column<string>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atestados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Atestados_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContratosPJ",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FuncionarioPJId = table.Column<int>(type: "INTEGER", nullable: false),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoContrato = table.Column<int>(type: "INTEGER", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataFim = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ValorProjeto = table.Column<decimal>(type: "TEXT", nullable: true),
                    ValorHora = table.Column<decimal>(type: "TEXT", nullable: true),
                    ValorFixo = table.Column<decimal>(type: "TEXT", nullable: true),
                    HorasMensais = table.Column<int>(type: "INTEGER", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    RpaBrancoConhecimento = table.Column<string>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContratosPJ", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContratosPJ_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContratosPJ_Funcionarios_FuncionarioPJId",
                        column: x => x.FuncionarioPJId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ferias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FuncionarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataAdmissao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DiasDisponiveis = table.Column<int>(type: "INTEGER", nullable: false),
                    DiasUtilizados = table.Column<int>(type: "INTEGER", nullable: false),
                    DiasRemunerados = table.Column<int>(type: "INTEGER", nullable: false),
                    DiasNaoRemunerados = table.Column<int>(type: "INTEGER", nullable: false),
                    DataUltimaRenovacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ferias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ferias_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalarioHistorico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FuncionarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataVigencia = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SalarioBruto = table.Column<decimal>(type: "TEXT", nullable: false),
                    DescontoINSS = table.Column<decimal>(type: "TEXT", nullable: false),
                    DescontoIRPF = table.Column<decimal>(type: "TEXT", nullable: false),
                    DescontoOutros = table.Column<decimal>(type: "TEXT", nullable: false),
                    SalarioLiquido = table.Column<decimal>(type: "TEXT", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalarioHistorico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalarioHistorico_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RpasNfsPJ",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContratoPJId = table.Column<int>(type: "INTEGER", nullable: false),
                    FuncionarioPJId = table.Column<int>(type: "INTEGER", nullable: false),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoDocumento = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroDocumento = table.Column<string>(type: "TEXT", nullable: false),
                    DataDocumento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Mes = table.Column<int>(type: "INTEGER", nullable: false),
                    Ano = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorBruto = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorDescontos = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorLiquido = table.Column<decimal>(type: "TEXT", nullable: false),
                    DescricaoServico = table.Column<string>(type: "TEXT", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: false),
                    StatusPagamento = table.Column<int>(type: "INTEGER", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Arquivo = table.Column<byte[]>(type: "BLOB", nullable: false),
                    NomeArquivo = table.Column<string>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RpasNfsPJ", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RpasNfsPJ_ContratosPJ_ContratoPJId",
                        column: x => x.ContratoPJId,
                        principalTable: "ContratosPJ",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RpasNfsPJ_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RpasNfsPJ_Funcionarios_FuncionarioPJId",
                        column: x => x.FuncionarioPJId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodosFerias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FeriasId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataFim = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Dias = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Remunerada = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodosFerias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodosFerias_Ferias_FeriasId",
                        column: x => x.FeriasId,
                        principalTable: "Ferias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Atestados_FuncionarioId_DataInicio_DataFim",
                table: "Atestados",
                columns: new[] { "FuncionarioId", "DataInicio", "DataFim" });

            migrationBuilder.CreateIndex(
                name: "IX_ContratosPJ_EmpresaId",
                table: "ContratosPJ",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratosPJ_FuncionarioPJId",
                table: "ContratosPJ",
                column: "FuncionarioPJId");

            migrationBuilder.CreateIndex(
                name: "IX_Ferias_FuncionarioId",
                table: "Ferias",
                column: "FuncionarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_EmpresaId_Tipo",
                table: "Funcionarios",
                columns: new[] { "EmpresaId", "Tipo" });

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_Tipo",
                table: "Funcionarios",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosFerias_FeriasId",
                table: "PeriodosFerias",
                column: "FeriasId");

            migrationBuilder.CreateIndex(
                name: "IX_RpasNfsPJ_ContratoPJId",
                table: "RpasNfsPJ",
                column: "ContratoPJId");

            migrationBuilder.CreateIndex(
                name: "IX_RpasNfsPJ_EmpresaId",
                table: "RpasNfsPJ",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_RpasNfsPJ_FuncionarioPJId",
                table: "RpasNfsPJ",
                column: "FuncionarioPJId");

            migrationBuilder.CreateIndex(
                name: "IX_RpasNfsPJ_Mes_Ano",
                table: "RpasNfsPJ",
                columns: new[] { "Mes", "Ano" });

            migrationBuilder.CreateIndex(
                name: "IX_RpasNfsPJ_StatusPagamento",
                table: "RpasNfsPJ",
                column: "StatusPagamento");

            migrationBuilder.CreateIndex(
                name: "IX_SalarioHistorico_FuncionarioId",
                table: "SalarioHistorico",
                column: "FuncionarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Atestados");

            migrationBuilder.DropTable(
                name: "PeriodosFerias");

            migrationBuilder.DropTable(
                name: "RpasNfsPJ");

            migrationBuilder.DropTable(
                name: "SalarioHistorico");

            migrationBuilder.DropTable(
                name: "Ferias");

            migrationBuilder.DropTable(
                name: "ContratosPJ");

            migrationBuilder.DropTable(
                name: "Funcionarios");

            migrationBuilder.DropTable(
                name: "Empresas");
        }
    }
}
