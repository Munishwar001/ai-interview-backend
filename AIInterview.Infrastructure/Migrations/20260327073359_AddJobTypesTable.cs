using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobTypesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Create JobTypes table
                CREATE TABLE IF NOT EXISTS job_types (
                    id SERIAL PRIMARY KEY,
                    name VARCHAR(50) UNIQUE NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );

                -- Seed default job types
                INSERT INTO job_types (name) VALUES 
                    ('Full-time'),
                    ('Part-time'),
                    ('Contract'),
                    ('Internship')
                ON CONFLICT (name) DO NOTHING;

                -- Add JobTypeId column to jobs table
                ALTER TABLE jobs
                ADD COLUMN IF NOT EXISTS job_type_id INT;

                -- Add foreign key constraint
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints 
                        WHERE constraint_name = 'fk_jobs_job_types'
                    ) THEN
                        ALTER TABLE jobs
                        ADD CONSTRAINT fk_jobs_job_types
                        FOREIGN KEY (job_type_id) REFERENCES job_types(id)
                        ON DELETE SET NULL;
                    END IF;
                END $$;

                -- Index for performance
                CREATE INDEX IF NOT EXISTS ix_jobs_job_type_id ON jobs(job_type_id);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Remove foreign key
                ALTER TABLE jobs
                DROP CONSTRAINT IF EXISTS fk_jobs_job_types;

                -- Remove column
                ALTER TABLE jobs
                DROP COLUMN IF EXISTS job_type_id;

                -- Drop table
                DROP TABLE IF EXISTS job_types;
            ");
        }
    }
}