using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMIT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatriculeNumero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "Teachers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Teachers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Matricule",
                table: "Students",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                WITH numbered AS (
                    SELECT ""Id"", ROW_NUMBER() OVER (ORDER BY ""Id"") AS rn
                    FROM ""Teachers""
                )
                UPDATE ""Teachers"" t
                SET ""Numero"" = 'PROF-' || EXTRACT(YEAR FROM NOW())::int || '-' || LPAD(n.rn::text, 3, '0')
                FROM numbered n
                WHERE t.""Id"" = n.""Id"" AND t.""Numero"" = '';
            ");

            migrationBuilder.Sql(@"
                WITH numbered AS (
                    SELECT ""Id"", ROW_NUMBER() OVER (ORDER BY ""Id"") AS rn
                    FROM ""Students""
                )
                UPDATE ""Students"" s
                SET ""Matricule"" = 'STU-' || EXTRACT(YEAR FROM NOW())::int || '-' || LPAD(n.rn::text, 3, '0')
                FROM numbered n
                WHERE s.""Id"" = n.""Id"" AND s.""Matricule"" = '';
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_Numero",
                table: "Teachers",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_Matricule",
                table: "Students",
                column: "Matricule",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teachers_Numero",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_Matricule",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "Matricule",
                table: "Students");
        }
    }
}
