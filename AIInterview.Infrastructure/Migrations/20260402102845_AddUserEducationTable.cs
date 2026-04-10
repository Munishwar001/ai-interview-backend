using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserEducationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS user_education (
                    id              SERIAL PRIMARY KEY,
                    user_id         TEXT NOT NULL,
                    degree          VARCHAR(150) NOT NULL,
                    institution     VARCHAR(150) NOT NULL,
                    field_of_study  VARCHAR(150),
                    start_year      INT NOT NULL,
                    end_year        INT NULL,
                    is_current      BOOLEAN DEFAULT FALSE,
                    description     TEXT,
                    created_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at      TIMESTAMP NULL
                );

                ALTER TABLE user_education
                    ADD CONSTRAINT fk_user_education_user
                    FOREIGN KEY (user_id)
                    REFERENCES ""AspNetUsers""(""Id"")
                    ON DELETE CASCADE;

                CREATE INDEX IF NOT EXISTS ix_user_education_user_id ON user_education(user_id);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.Sql(@"DROP TABLE IF EXISTS user_education;");
        }
    }
}
