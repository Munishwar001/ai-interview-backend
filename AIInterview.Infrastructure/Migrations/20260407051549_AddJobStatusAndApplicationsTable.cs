using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobStatusAndApplicationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.Sql(@"
                -- Add missing columns to jobs table
                ALTER TABLE jobs ADD COLUMN IF NOT EXISTS status VARCHAR(20) NOT NULL DEFAULT 'Active';
                ALTER TABLE jobs ADD COLUMN IF NOT EXISTS views INT NOT NULL DEFAULT 0;
                ALTER TABLE jobs ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP;

                -- Index on status for filtering Active/Closed
                CREATE INDEX IF NOT EXISTS ix_jobs_status ON jobs(status);

                -- job_applications table
                CREATE TABLE IF NOT EXISTS job_applications (
                    id          SERIAL PRIMARY KEY,
                    job_id      INT NOT NULL,
                    user_id     VARCHAR(450) NOT NULL,
                    status      VARCHAR(50) NOT NULL DEFAULT 'Applied',
                    applied_at  TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    updated_at  TIMESTAMP,

                    CONSTRAINT fk_job_applications_jobs
                        FOREIGN KEY (job_id) REFERENCES jobs(id) ON DELETE CASCADE,

                    CONSTRAINT fk_job_applications_users
                        FOREIGN KEY (user_id) REFERENCES ""AspNetUsers""(""Id"") ON DELETE CASCADE,

                    CONSTRAINT uq_job_applications_job_user
                        UNIQUE (job_id, user_id)
                );

                CREATE INDEX IF NOT EXISTS ix_job_applications_job_id  ON job_applications(job_id);
                CREATE INDEX IF NOT EXISTS ix_job_applications_user_id ON job_applications(user_id);
                CREATE INDEX IF NOT EXISTS ix_job_applications_status  ON job_applications(status);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS job_applications;

                ALTER TABLE Jobs DROP COLUMN IF EXISTS status;
                ALTER TABLE Jobs DROP COLUMN IF EXISTS views;
                ALTER TABLE Jobs DROP COLUMN IF EXISTS updated_at;
            ");
        }
    }
}
