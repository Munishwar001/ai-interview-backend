using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserExperienceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS user_experiences (
                    id              SERIAL PRIMARY KEY,
                    user_id         TEXT NOT NULL,
                    job_title       VARCHAR(150) NOT NULL,
                    company         VARCHAR(150) NOT NULL,
                    location        VARCHAR(150),
                    start_date      DATE NOT NULL,
                    end_date        DATE NULL,
                    is_current      BOOLEAN DEFAULT FALSE,
                    description     TEXT,
                    created_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at      TIMESTAMP NULL
                );

                ALTER TABLE user_experiences
                    ADD CONSTRAINT fk_user_experiences_user
                    FOREIGN KEY (user_id)
                    REFERENCES ""AspNetUsers""(""Id"")
                    ON DELETE CASCADE;

                CREATE INDEX IF NOT EXISTS ix_user_experiences_user_id ON user_experiences(user_id);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS user_experiences;");
        }
    }
}
