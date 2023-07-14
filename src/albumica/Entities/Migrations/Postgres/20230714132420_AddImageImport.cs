using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace albumica.Entities.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddImageImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "import",
                table: "media",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(@"UPDATE media SET import = original;");

            migrationBuilder.AlterColumn<string>(
                name: "import",
                table: "media",
                nullable: false,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "import",
                table: "media");
        }
    }
}
