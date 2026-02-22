using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Infra.Persistence.Migrations_Core
{
    /// <inheritdoc />
    public partial class AddedByPassIP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BypassIp",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BypassIp",
                table: "AspNetUsers");
        }
    }
}
