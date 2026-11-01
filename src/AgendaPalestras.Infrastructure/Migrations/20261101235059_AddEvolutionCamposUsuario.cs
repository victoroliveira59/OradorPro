using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaPalestras.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEvolutionCamposUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EvolutionApiKey",
                table: "Usuarios",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvolutionApiUrl",
                table: "Usuarios",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvolutionInstanceName",
                table: "Usuarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvolutionApiKey",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EvolutionApiUrl",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EvolutionInstanceName",
                table: "Usuarios");
        }
    }
}
