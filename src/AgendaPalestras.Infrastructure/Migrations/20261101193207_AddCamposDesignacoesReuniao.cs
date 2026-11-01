using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaPalestras.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposDesignacoesReuniao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PresidenteReuniao",
                table: "Agendas",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeitorSentinela",
                table: "Agendas",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OracaoFinal",
                table: "Agendas",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PresidenteReuniao",
                table: "Agendas");

            migrationBuilder.DropColumn(
                name: "LeitorSentinela",
                table: "Agendas");

            migrationBuilder.DropColumn(
                name: "OracaoFinal",
                table: "Agendas");
        }
    }
}
