using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMockInterviewTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE EXTENSION IF NOT EXISTS pgcrypto;

                -- Mock Interview Sessions
                CREATE TABLE IF NOT EXISTS mock_interview_sessions (
                    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    user_id     TEXT NOT NULL,
                    skills      JSONB NOT NULL DEFAULT '[]',
                    status      TEXT NOT NULL DEFAULT 'active',
                    created_at  TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
                    updated_at  TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,

                    CONSTRAINT fk_mock_sessions_user
                        FOREIGN KEY (user_id) REFERENCES ""AspNetUsers""(""Id"") ON DELETE CASCADE,

                    CONSTRAINT chk_mock_sessions_status
                        CHECK (status IN ('active', 'completed'))
                );

                CREATE INDEX IF NOT EXISTS idx_mock_interview_sessions_user_id 
                    ON mock_interview_sessions(user_id);

                CREATE INDEX IF NOT EXISTS idx_mock_interview_sessions_status 
                    ON mock_interview_sessions(status);


                -- Mock Interview Messages (chat history)
                CREATE TABLE IF NOT EXISTS mock_interview_messages (
                    id          SERIAL PRIMARY KEY,
                    session_id  UUID NOT NULL,
                    role        TEXT NOT NULL,
                    content     TEXT NOT NULL,
                    created_at  TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,

                    CONSTRAINT fk_mock_messages_session
                        FOREIGN KEY (session_id) 
                        REFERENCES mock_interview_sessions(id) 
                        ON DELETE CASCADE,

                    CONSTRAINT chk_mock_messages_role
                        CHECK (role IN ('ai', 'user'))
                );

                CREATE INDEX IF NOT EXISTS idx_mock_interview_messages_session_id 
                    ON mock_interview_messages(session_id);

                CREATE INDEX IF NOT EXISTS idx_mock_interview_messages_created_at 
                    ON mock_interview_messages(created_at);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS mock_interview_messages;
                DROP TABLE IF EXISTS mock_interview_sessions;
            ");
        }
    }
}