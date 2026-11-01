using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaPalestras.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConvitesCadastro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConvitesCadastro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmailConvidado = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CriadoPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Usado = table.Column<bool>(type: "boolean", nullable: false),
                    DataUso = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UsuarioCriadoId = table.Column<Guid>(type: "uuid", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConvitesCadastro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConvitesCadastro_Usuarios_CriadoPorUsuarioId",
                        column: x => x.CriadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConvitesCadastro_Usuarios_UsuarioCriadoId",
                        column: x => x.UsuarioCriadoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConvitesCadastro_Codigo",
                table: "ConvitesCadastro",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConvitesCadastro_CriadoPorUsuarioId",
                table: "ConvitesCadastro",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ConvitesCadastro_UsuarioCriadoId",
                table: "ConvitesCadastro",
                column: "UsuarioCriadoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConvitesCadastro");
        }
    }
}
