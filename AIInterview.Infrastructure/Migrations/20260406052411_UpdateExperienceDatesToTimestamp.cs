using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExperienceDatesToTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            ALTER TABLE user_experiences
            ALTER COLUMN start_date TYPE TIMESTAMP WITH TIME ZONE 
            USING start_date::timestamptz;

            ALTER TABLE user_experiences
            ALTER COLUMN end_date TYPE TIMESTAMP WITH TIME ZONE 
            USING end_date::timestamptz;
        ");
        }
            
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            ALTER TABLE user_experiences
            ALTER COLUMN start_date TYPE DATE 
            USING start_date::date;

            ALTER TABLE user_experiences
            ALTER COLUMN end_date TYPE DATE 
            USING end_date::date;
        ");
        }
    }
}
