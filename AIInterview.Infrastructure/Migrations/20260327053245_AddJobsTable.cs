using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS Jobs (
                    Id SERIAL PRIMARY KEY,
                    Title VARCHAR(200) NOT NULL,
                    Description TEXT NOT NULL,
                    Location VARCHAR(150),
                    SalaryMin NUMERIC(10,2),
                    SalaryMax NUMERIC(10,2),
                    CompanyName VARCHAR(200),
                    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    EmployerId VARCHAR(450) NOT NULL,

                   CONSTRAINT FK_Jobs_AspNetUsers 
                   FOREIGN KEY (EmployerId) REFERENCES ""AspNetUsers""(""Id"")
                   ON DELETE CASCADE
                );

                -- Index on EmployerId
                CREATE INDEX IF NOT EXISTS IX_Jobs_EmployerId ON Jobs(EmployerId);

                -- Index on CreatedAt
                CREATE INDEX IF NOT EXISTS IX_Jobs_CreatedAt ON Jobs(CreatedAt);

                -- Index on Location
                CREATE INDEX IF NOT EXISTS IX_Jobs_Location ON Jobs(Location);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS Jobs;
            ");
        }
    }
}
