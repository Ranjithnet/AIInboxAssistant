using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInboxAssistant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreEmailFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Emails",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Emails");
        }
    }
}
