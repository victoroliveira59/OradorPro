using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaPalestras.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioCriadorIdEmOradorEMapeamentosMultiTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Congregacoes_Nome",
                table: "Congregacoes");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriadorId",
                table: "Oradores",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Oradores_UsuarioCriadorId",
                table: "Oradores",
                column: "UsuarioCriadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Congregacoes_Nome",
                table: "Congregacoes",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_Congregacoes_UsuarioCriadorId",
                table: "Congregacoes",
                column: "UsuarioCriadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Oradores_UsuarioCriadorId",
                table: "Oradores");

            migrationBuilder.DropIndex(
                name: "IX_Congregacoes_Nome",
                table: "Congregacoes");

            migrationBuilder.DropIndex(
                name: "IX_Congregacoes_UsuarioCriadorId",
                table: "Congregacoes");

            migrationBuilder.DropColumn(
                name: "UsuarioCriadorId",
                table: "Oradores");

            migrationBuilder.CreateIndex(
                name: "IX_Congregacoes_Nome",
                table: "Congregacoes",
                column: "Nome",
                unique: true);
        }
    }
}
