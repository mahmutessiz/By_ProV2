using System;
using System.Data;
using Microsoft.Data.SqlClient;
using By_ProV2.Models;
using By_ProV2.Helpers;

namespace By_ProV2.DataAccess
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository()
        {
            _connectionString = ConfigurationHelper.GetConnectionString("db");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public User GetUserByUsername(string username)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Users WHERE Username = @Username AND IsActive = 1";
            command.Parameters.Add(new SqlParameter("@Username", username));
            
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Username = reader["Username"].ToString(),
                    PasswordHash = reader["PasswordHash"].ToString(),
                    Email = reader["Email"]?.ToString(),
                    FullName = reader["FullName"]?.ToString(),
                    Role = reader["Role"]?.ToString() ?? "User",
                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                    LastLoginAt = reader["LastLoginAt"] != DBNull.Value ? Convert.ToDateTime(reader["LastLoginAt"]) : (DateTime?)null
                };
            }
            return null;
        }

        public User GetUserById(int id)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Users WHERE Id = @Id AND IsActive = 1";
            command.Parameters.Add(new SqlParameter("@Id", id));
            
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Username = reader["Username"].ToString(),
                    PasswordHash = reader["PasswordHash"].ToString(),
                    Email = reader["Email"]?.ToString(),
                    FullName = reader["FullName"]?.ToString(),
                    Role = reader["Role"]?.ToString() ?? "User",
                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                    LastLoginAt = reader["LastLoginAt"] != DBNull.Value ? Convert.ToDateTime(reader["LastLoginAt"]) : (DateTime?)null
                };
            }
            return null;
        }

        public bool CreateUser(User user)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Users 
                (Username, PasswordHash, Email, FullName, Role, IsActive, CreatedAt) 
                VALUES (@Username, @PasswordHash, @Email, @FullName, @Role, @IsActive, @CreatedAt)";
            
            command.Parameters.Add(new SqlParameter("@Username", user.Username));
            command.Parameters.Add(new SqlParameter("@PasswordHash", user.PasswordHash));
            command.Parameters.Add(new SqlParameter("@Email", user.Email ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@FullName", user.FullName ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Role", user.Role ?? "User"));
            command.Parameters.Add(new SqlParameter("@IsActive", user.IsActive));
            command.Parameters.Add(new SqlParameter("@CreatedAt", user.CreatedAt));
            
            return command.ExecuteNonQuery() > 0;
        }

        public bool UpdatePassword(int userId, string newPasswordHash)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Users SET PasswordHash = @PasswordHash WHERE Id = @Id";
            command.Parameters.Add(new SqlParameter("@PasswordHash", newPasswordHash));
            command.Parameters.Add(new SqlParameter("@Id", userId));
            
            return command.ExecuteNonQuery() > 0;
        }

        public bool UpdateLastLogin(int userId)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Users SET LastLoginAt = @LastLoginAt WHERE Id = @Id";
            command.Parameters.Add(new SqlParameter("@LastLoginAt", DateTime.Now));
            command.Parameters.Add(new SqlParameter("@Id", userId));
            
            return command.ExecuteNonQuery() > 0;
        }

        public bool HasUsers()
        {
            using var connection = CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users";
            int count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }

        public int GetUserCount()
        {
            using var connection = CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users";
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using var connection = CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Users ORDER BY FullName";
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var user = new User
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Username = reader["Username"].ToString(),
                    PasswordHash = reader["PasswordHash"].ToString(),
                    Email = reader["Email"]?.ToString(),
                    FullName = reader["FullName"]?.ToString(),
                    Role = reader["Role"]?.ToString() ?? "User",
                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                    LastLoginAt = reader["LastLoginAt"] != DBNull.Value ? Convert.ToDateTime(reader["LastLoginAt"]) : (DateTime?)null
                };
                users.Add(user);
            }
            return users;
        }
    }
}