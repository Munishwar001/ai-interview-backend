using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeAnalysisTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE EXTENSION IF NOT EXISTS pgcrypto;

                CREATE TABLE IF NOT EXISTS resume_analysis (
                    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    user_id       TEXT NOT NULL,
                    resume_hash   TEXT NOT NULL,
                    resume_text   TEXT NOT NULL,
                    ai_response   JSONB NOT NULL,
                    created_at    TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
                    updated_at    TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,

                    CONSTRAINT fk_resume_analysis_user
                        FOREIGN KEY (user_id) REFERENCES ""AspNetUsers""(""Id"") ON DELETE CASCADE,

                    CONSTRAINT uq_resume_analysis_user
                        UNIQUE (user_id)
                );

                CREATE INDEX IF NOT EXISTS ix_resume_analysis_user_id    ON resume_analysis(user_id);
                CREATE INDEX IF NOT EXISTS ix_resume_analysis_resume_hash ON resume_analysis(resume_hash);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS resume_analysis;");
        }
    }
}
