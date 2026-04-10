using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyProfilesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS company_profiles (
                    id SERIAL PRIMARY KEY,

                    user_id TEXT NOT NULL,  -- AspNetUsers uses string (GUID)

                    company_name VARCHAR(150) NOT NULL,
                    tagline VARCHAR(255),
                    description TEXT,

                    website VARCHAR(255),
                    industry VARCHAR(100),

                    company_size_id INT,
                    founded_year INT,

                    logo_url VARCHAR(500),
                    cover_image_url VARCHAR(500),

                    email VARCHAR(150),
                    phone VARCHAR(50),

                    address_line1 VARCHAR(255),
                    address_line2 VARCHAR(255),
                    city VARCHAR(100),
                    state VARCHAR(100),
                    country VARCHAR(100),
                    postal_code VARCHAR(20),

                    linkedin_url VARCHAR(255),
                    twitter_url VARCHAR(255),

                    is_verified BOOLEAN DEFAULT FALSE,
                    profile_completion_percentage INT DEFAULT 0,

                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP NULL
                );

                ALTER TABLE company_profiles
                ADD CONSTRAINT fk_company_profiles_user
                FOREIGN KEY (user_id)
                REFERENCES ""AspNetUsers""(""Id"")
                ON DELETE CASCADE;

                ALTER TABLE company_profiles
                ADD CONSTRAINT fk_company_profiles_company_size
                FOREIGN KEY (company_size_id)
                REFERENCES company_sizes(id)
                ON DELETE SET NULL;

                CREATE INDEX IF NOT EXISTS ix_company_profiles_user_id 
                ON company_profiles(user_id);

                CREATE INDEX IF NOT EXISTS ix_company_profiles_company_size_id 
                ON company_profiles(company_size_id);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS company_profiles;
            ");
        }
    }
}
