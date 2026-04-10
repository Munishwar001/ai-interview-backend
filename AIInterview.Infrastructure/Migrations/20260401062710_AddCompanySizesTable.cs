using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanySizesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS company_sizes (
                    id SERIAL PRIMARY KEY,
                    label VARCHAR(50) NOT NULL,
                    min_employees INT NOT NULL,
                    max_employees INT NULL
                );

                CREATE INDEX IF NOT EXISTS ix_company_sizes_min_employees 
                ON company_sizes(min_employees);

                INSERT INTO company_sizes (id, label, min_employees, max_employees) VALUES
                    (1, '1-10 employees', 1, 10),
                    (2, '11-50 employees', 11, 50),
                    (3, '51-200 employees', 51, 200),
                    (4, '201-500 employees', 201, 500),
                    (5, '501-1000 employees', 501, 1000),
                    (6, '1001-5000 employees', 1001, 5000),
                    (7, '5000+ employees', 5000, NULL)
                ON CONFLICT (id) DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS company_sizes;
            ");
        }
    }
}
