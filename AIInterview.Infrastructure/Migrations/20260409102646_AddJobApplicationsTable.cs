using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobApplicationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Job Applications Table
                CREATE TABLE IF NOT EXISTS job_applications (
                    id           SERIAL PRIMARY KEY,
                    job_id       INT NOT NULL,
                    user_id      TEXT NOT NULL,
                    cover_letter TEXT,
                    status       TEXT NOT NULL DEFAULT 'Pending',
                    applied_at   TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

                    CONSTRAINT fk_job_applications_job
                        FOREIGN KEY (job_id) 
                        REFERENCES jobs(id) 
                        ON DELETE CASCADE,

                    CONSTRAINT chk_job_applications_status
                        CHECK (status IN ('Pending', 'Shortlisted', 'Rejected', 'Hired')),

                    CONSTRAINT uq_job_applications_job_user
                        UNIQUE (job_id, user_id)
                );

                CREATE INDEX IF NOT EXISTS idx_job_applications_job_id  
                    ON job_applications(job_id);

                CREATE INDEX IF NOT EXISTS idx_job_applications_user_id 
                    ON job_applications(user_id);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS job_applications;
            ");
        }
    }
}