using AIInterview.Application.Interface;
using AIInterview.Core.DTOs.Auth;
using AIInterview.Core.DTOs.User;
using Dapper;
using System.Data;

namespace AIInterview.Infrastructure.DataAccess
{
    public class UserRepository(IDbConnection connection) : IUserRepository
    {
        public async Task<IEnumerable<UserRole>> GetUserRoles(string userId)
        {
            try
            {
                const string sql = @"SELECT u.""Id"" AS ""UserId"", r.""Name"" AS ""RoleName"",
            NULL AS ""ContextID"", 'IDENTITY' AS ""UserType"" FROM ""AspNetUsers"" u
            INNER JOIN ""AspNetUserRoles"" ur ON u.""Id"" = ur.""UserId""
            INNER JOIN ""AspNetRoles"" r ON ur.""RoleId"" = r.""Id""
            WHERE u.""Id"" = @UserId;";

                return await connection.QueryAsync<UserRole>(sql, new { UserId = userId });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> AddNewDeleteOldUserRefreshToken(string userId, string newRefreshToken, string oldRefreshToken, DateTime issuedAt, DateTime expiresAt)
        {
            const string sql = @"BEGIN;
                DELETE FROM user_refresh_tokens WHERE user_id = @UserID
                AND refresh_token = @OldRefreshToken;
                INSERT INTO user_refresh_tokens (user_id, refresh_token, issued_at, expires_at)
                VALUES (@UserID, @NewRefreshToken, @IssuedAt, @ExpiresAt);
                COMMIT;";
            try
            {
                var param = new DynamicParameters();
                param.Add("@UserID", userId);
                param.Add("@OldRefreshToken", oldRefreshToken);
                param.Add("@NewRefreshToken", newRefreshToken);
                param.Add("@IssuedAt", issuedAt);
                param.Add("@ExpiresAt", expiresAt);

                var result = await connection.ExecuteAsync(sql, param);

                return result > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<UserRefreshToken> GetRefreshToken(string userId, string refreshToken)
        {
            string sql = @" SELECT  user_id  AS ""UserID"", refresh_token AS ""RefreshToken"", expires_at  AS ""ExpiresAt""
                FROM user_refresh_tokens WHERE user_id = @UserID AND refresh_token = @RefreshToken;";
            try
            {
                var param = new DynamicParameters();
                param.Add("@UserID", userId);
                param.Add("@RefreshToken", refreshToken);

                var result = await connection.QuerySingleOrDefaultAsync<UserRefreshToken>(sql, param);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> DeleteUserRefreshToken(string userId, string refreshToken)
        {

            try
            {
                const string sql = @"DELETE FROM user_refresh_tokens  WHERE user_id = @UserID AND refresh_token = @OldRefreshToken;";

                var param = new DynamicParameters();
                param.Add("@UserID", userId);
                param.Add("@OldRefreshToken", refreshToken);

                var result = await connection.ExecuteAsync(sql, param);

                if (result > 0)
                {

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
