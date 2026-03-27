using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS skills (
                    id SERIAL PRIMARY KEY,
                    name VARCHAR(100) UNIQUE NOT NULL
                );

                CREATE TABLE IF NOT EXISTS job_skills (
                    id SERIAL PRIMARY KEY,
                    job_id INT NOT NULL,
                    skill_id INT NOT NULL,

                    CONSTRAINT fk_job_skills_jobs 
                        FOREIGN KEY (job_id) REFERENCES jobs(id) ON DELETE CASCADE,

                    CONSTRAINT fk_job_skills_skills 
                        FOREIGN KEY (skill_id) REFERENCES skills(id) ON DELETE CASCADE
                );

                --  Indexes
                CREATE INDEX IF NOT EXISTS ix_job_skills_job_id ON job_skills(job_id);
                CREATE INDEX IF NOT EXISTS ix_job_skills_skill_id ON job_skills(skill_id);

                -- Seed Skills Data
                INSERT INTO skills (name) VALUES

                -- Frontend
                ('html'), ('css'), ('javascript'), ('typescript'), ('react'), ('angular'), ('vue.js'), ('next.js'), ('redux'), ('tailwind css'), ('bootstrap'), ('sass'),

                -- Backend
                ('node.js'), ('express.js'), ('asp.net core'), ('django'), ('flask'), ('spring boot'), ('laravel'),

                -- Programming Languages
                ('c'), ('c++'), ('c#'), ('java'), ('python'), ('go'), ('rust'), ('kotlin'), ('swift'),

                -- Databases
                ('sql'), ('postgresql'), ('mysql'), ('mongodb'), ('redis'), ('firebase'), ('oracle'),

                -- Cloud & DevOps
                ('aws'), ('azure'), ('google cloud'), ('docker'), ('kubernetes'), ('ci/cd'), ('jenkins'), ('github actions'), ('terraform'),

                -- Security
                ('jwt'), ('oauth'), ('authentication'), ('authorization'), ('cybersecurity'),

                -- Data & AI
                ('machine learning'), ('deep learning'), ('data science'), ('pandas'), ('numpy'), ('tensorflow'), ('pytorch'),

                -- Testing
                ('unit testing'), ('integration testing'), ('jest'), ('mocha'), ('selenium'), ('cypress'),

                -- Tools
                ('git'), ('github'), ('gitlab'), ('bitbucket'), ('postman'), ('swagger'),

                -- Mobile
                ('react native'), ('flutter'), ('android'), ('ios'),

                -- Concepts
                ('data structures'), ('algorithms'), ('system design'), ('oop'), ('design patterns'),

                -- Others
                ('microservices'), ('rest apis'), ('graphql'), ('websockets'), ('socket.io')

                ON CONFLICT (name) DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS job_skills;
                DROP TABLE IF EXISTS skills;
            ");
        }
    }
}
