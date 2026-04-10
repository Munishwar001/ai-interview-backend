using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationInterviewsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS application_interviews (
                    id             SERIAL PRIMARY KEY,
                    application_id INT NOT NULL,
                    job_id         INT NOT NULL,
                    employer_id    TEXT NOT NULL,
                    user_id        TEXT NOT NULL,
                    room_id        TEXT NOT NULL,
                    notes          TEXT,
                    scheduled_at   TIMESTAMP WITH TIME ZONE NOT NULL,
                    status         TEXT NOT NULL DEFAULT 'Scheduled',
                    created_at     TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,

                    CONSTRAINT fk_application_interviews_application
                        FOREIGN KEY (application_id)
                        REFERENCES job_applications(id)
                        ON DELETE CASCADE,

                    CONSTRAINT fk_application_interviews_job
                        FOREIGN KEY (job_id)
                        REFERENCES jobs(id)
                        ON DELETE CASCADE,

                    CONSTRAINT fk_application_interviews_employer
                        FOREIGN KEY (employer_id)
                        REFERENCES ""AspNetUsers""(""Id"")
                        ON DELETE CASCADE,

                    CONSTRAINT fk_application_interviews_user
                        FOREIGN KEY (user_id)
                        REFERENCES ""AspNetUsers""(""Id"")
                        ON DELETE CASCADE,

                    CONSTRAINT chk_application_interviews_status
                        CHECK (status IN ('Scheduled', 'Completed', 'Cancelled'))
                );

                CREATE INDEX IF NOT EXISTS idx_application_interviews_application_id
                    ON application_interviews(application_id);

                CREATE INDEX IF NOT EXISTS idx_application_interviews_job_id
                    ON application_interviews(job_id);

                CREATE INDEX IF NOT EXISTS idx_application_interviews_employer_id
                    ON application_interviews(employer_id);

                CREATE INDEX IF NOT EXISTS idx_application_interviews_user_id
                    ON application_interviews(user_id);

                CREATE UNIQUE INDEX IF NOT EXISTS uq_application_interviews_room_id
                    ON application_interviews(room_id);

                CREATE INDEX IF NOT EXISTS idx_application_interviews_scheduled_at
                    ON application_interviews(scheduled_at);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS application_interviews;
            ");
        }
    }
}
