using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaPalestras.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancyCongregacaoEGoogleMaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CongregacaoId",
                table: "Usuarios",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Endereco",
                table: "Congregacoes",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkGoogleMaps",
                table: "Congregacoes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_CongregacaoId",
                table: "Usuarios",
                column: "CongregacaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Congregacoes_CongregacaoId",
                table: "Usuarios",
                column: "CongregacaoId",
                principalTable: "Congregacoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Congregacoes_CongregacaoId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_CongregacaoId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CongregacaoId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Endereco",
                table: "Congregacoes");

            migrationBuilder.DropColumn(
                name: "LinkGoogleMaps",
                table: "Congregacoes");
        }
    }
}
