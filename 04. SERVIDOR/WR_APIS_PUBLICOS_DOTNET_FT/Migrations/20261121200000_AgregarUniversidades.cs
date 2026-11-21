using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WR_APIS_PUBLICOS_DOTNET_FT.Migrations
{
    /// <inheritdoc />
    public partial class AgregarUniversidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Universidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Dominio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SitioWeb = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Pais = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Universidades", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Universidades_Nombre_Pais",
                table: "Universidades",
                columns: new[] { "Nombre", "Pais" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Universidades");
        }
    }
}
