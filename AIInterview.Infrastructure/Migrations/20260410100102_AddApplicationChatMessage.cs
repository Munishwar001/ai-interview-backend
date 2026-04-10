using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS application_chat_messages (
                    id BIGSERIAL PRIMARY KEY,
                    application_id INT NOT NULL,
                    sender_id TEXT NOT NULL,
                    message TEXT NOT NULL,
                    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,

                    CONSTRAINT fk_application_chat_messages_application
                        FOREIGN KEY (application_id)
                        REFERENCES job_applications(id)
                        ON DELETE CASCADE,

                    CONSTRAINT fk_application_chat_messages_sender
                        FOREIGN KEY (sender_id)
                        REFERENCES ""AspNetUsers""(""Id"")
                        ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS idx_application_chat_messages_application_id
                    ON application_chat_messages(application_id);

                CREATE INDEX IF NOT EXISTS idx_application_chat_messages_created_at
                    ON application_chat_messages(created_at);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS application_chat_messages;
            ");
        }
    }
}
