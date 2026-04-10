using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    public partial class AddProfileTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer")
                              .Annotation("Npgsql:ValueGenerationStrategy",
                                          Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),

                    user_id = table.Column<string>(type: "text", nullable: false),

                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),

                    avatar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    initial = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),

                    profile_completion = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),

                    resume_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    resume_file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),

                    linkedin = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    github = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    website = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),

                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_profiles", x => x.id);

                    table.ForeignKey(
                        name: "FK_user_profiles_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // UNIQUE constraint on user_id (1 user = 1 profile)
            migrationBuilder.CreateIndex(
                name: "IX_user_profiles_user_id",
                table: "user_profiles",
                column: "user_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_profiles");
        }
    }
}