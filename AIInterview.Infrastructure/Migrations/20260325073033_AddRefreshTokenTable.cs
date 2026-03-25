using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    public partial class AddRefreshTokenTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS user_refresh_tokens (
                    id SERIAL PRIMARY KEY,
                    user_id VARCHAR(450) NOT NULL,
                    refresh_token TEXT NOT NULL,
                    issued_at TIMESTAMPTZ NOT NULL,
                    expires_at TIMESTAMPTZ NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_user_refresh_tokens_user_id
                ON user_refresh_tokens (user_id);

                CREATE INDEX IF NOT EXISTS idx_user_refresh_tokens_token
                ON user_refresh_tokens (refresh_token);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS user_refresh_tokens;
            ");
        }
    }
}