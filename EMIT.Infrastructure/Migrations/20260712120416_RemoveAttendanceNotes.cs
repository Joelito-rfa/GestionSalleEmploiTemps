using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMIT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAttendanceNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Attendances");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Attendances",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
