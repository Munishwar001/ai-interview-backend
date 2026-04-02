using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSkillsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS user_skills (
                    id          SERIAL PRIMARY KEY,
                    user_id     TEXT NOT NULL,
                    skill_id    INT NOT NULL,
                    created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT fk_user_skills_user  FOREIGN KEY (user_id)  REFERENCES ""AspNetUsers""(""Id"") ON DELETE CASCADE,
                    CONSTRAINT fk_user_skills_skill FOREIGN KEY (skill_id) REFERENCES skills(id) ON DELETE CASCADE,
                    CONSTRAINT uq_user_skill        UNIQUE (user_id, skill_id)
                );

                CREATE INDEX IF NOT EXISTS ix_user_skills_user_id ON user_skills(user_id);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS user_skills;");
        }
    }
}
