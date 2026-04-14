using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileViewsToProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE user_profiles
                ADD COLUMN IF NOT EXISTS profile_views INT NOT NULL DEFAULT 0;

                ALTER TABLE company_profiles
                ADD COLUMN IF NOT EXISTS profile_views INT NOT NULL DEFAULT 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE user_profiles
                DROP COLUMN IF EXISTS profile_views;

                ALTER TABLE company_profiles
                DROP COLUMN IF EXISTS profile_views;
            ");
        }
    }
}
