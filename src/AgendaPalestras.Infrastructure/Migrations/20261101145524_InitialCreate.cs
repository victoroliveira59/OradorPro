using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaPalestras.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Congregacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioCriadorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Circuito = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DiasReuniao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HorarioReuniao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Congregacoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Discursos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Tema = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discursos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    HashSenha = table.Column<string>(type: "text", nullable: false),
                    Papel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Oradores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CongregacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TelefoneWhatsapp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oradores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Oradores_Congregacoes_CongregacaoId",
                        column: x => x.CongregacaoId,
                        principalTable: "Congregacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Agendas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CongregacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    OradorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MotivoCancelamentoOuRecusa = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataConviteEnviado = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataResposta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataLembreteEnviado = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agendas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agendas_Congregacoes_CongregacaoId",
                        column: x => x.CongregacaoId,
                        principalTable: "Congregacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Agendas_Discursos_DiscursoId",
                        column: x => x.DiscursoId,
                        principalTable: "Discursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Agendas_Oradores_OradorId",
                        column: x => x.OradorId,
                        principalTable: "Oradores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OradoresDiscursos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OradorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OradoresDiscursos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OradoresDiscursos_Discursos_DiscursoId",
                        column: x => x.DiscursoId,
                        principalTable: "Discursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OradoresDiscursos_Oradores_OradorId",
                        column: x => x.OradorId,
                        principalTable: "Oradores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MensagensHistorico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgendaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Direcao = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Conteudo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IdentificadorProvedor = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ErroDetalhe = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensagensHistorico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MensagensHistorico_Agendas_AgendaId",
                        column: x => x.AgendaId,
                        principalTable: "Agendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agendas_Congregacao_DataHora",
                table: "Agendas",
                columns: new[] { "CongregacaoId", "DataHora" });

            migrationBuilder.CreateIndex(
                name: "IX_Agendas_DiscursoId",
                table: "Agendas",
                column: "DiscursoId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendas_Orador_DataHora",
                table: "Agendas",
                columns: new[] { "OradorId", "DataHora" });

            migrationBuilder.CreateIndex(
                name: "IX_Congregacoes_Nome",
                table: "Congregacoes",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Discursos_Numero",
                table: "Discursos",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MensagensHistorico_AgendaId",
                table: "MensagensHistorico",
                column: "AgendaId");

            migrationBuilder.CreateIndex(
                name: "IX_MensagensHistorico_IdentificadorProvedor",
                table: "MensagensHistorico",
                column: "IdentificadorProvedor");

            migrationBuilder.CreateIndex(
                name: "IX_Oradores_CongregacaoId",
                table: "Oradores",
                column: "CongregacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_OradoresDiscursos_DiscursoId",
                table: "OradoresDiscursos",
                column: "DiscursoId");

            migrationBuilder.CreateIndex(
                name: "IX_OradoresDiscursos_OradorId_DiscursoId",
                table: "OradoresDiscursos",
                columns: new[] { "OradorId", "DiscursoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MensagensHistorico");

            migrationBuilder.DropTable(
                name: "OradoresDiscursos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Agendas");

            migrationBuilder.DropTable(
                name: "Discursos");

            migrationBuilder.DropTable(
                name: "Oradores");

            migrationBuilder.DropTable(
                name: "Congregacoes");
        }
    }
}
