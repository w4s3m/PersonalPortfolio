using Microsoft.Data.SqlClient;
using PersonelProtfolio;
using PersonelProtfolio.EventLogs;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;


namespace PersonalProtfolioDataTier
{

    public class UserDataDTO
    {
        public UserDataDTO(int UserID, string UserName, string PasswordHash, string FullName, string Email, string Role, DateTime Lastlogin, bool IsActive)
        {
            this.UserID = UserID;
            this.UserName = UserName;
            this.PasswordHash = PasswordHash;
            this.FullName = FullName;
            this.Email = Email;
            this.Role = Role;
            this.LastLogin = Lastlogin;
            this.IsActive = IsActive;
        }   

        public int UserID { get; set; }

        // To tell swagger that this field is required and should not be empty, we can use the [Required] attribute along with
        // [DefaultValue("")] to indicate that the default value is an empty string.
        [DefaultValue("")]
        [Required]
        public string UserName { get; set; }

        [DefaultValue("")]
        [Required]
        public string PasswordHash { get; set; }

        [DefaultValue("")]
        [Required]
        public string FullName { get; set; }
       
        [DefaultValue("")]
        [Required]
        public string Email { get; set; }

        [DefaultValue("")]
        [Required]
        public string Role { get; set; }

        [Required]
        public DateTime LastLogin { get; set; } = DateTime.Now;

        [DefaultValue(false)]
        [Required]
        public bool IsActive { get; set; }
    
    }


    public class UsersData
    {
        private static string? _connectionString = clsConnectionString.connectionString;
        public static async Task<bool> UpdateLastLogin(int userId)
        {
            string query = "UPDATE Users SET LastLogin = @LastLogin WHERE UserID = @UserID";

            using (SqlConnection connection = new (_connectionString))
            using (SqlCommand cmd = new (query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@LastLogin", DateTime.Now);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    
                    await connection.OpenAsync();

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Error in UpdateLastLogin!");
                    return false;
                }
            }
        }

        public async static Task<bool> UpdateUserPassword(int userId, string newPasswordHash)
        {
            string query = "UPDATE Users SET PasswordHash = @PasswordHash WHERE UserID = @UserID";
            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    await connection.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Error in UpdateUserPassword!");
                    return false;
                }
            }
        }

        public async static Task<bool> SetUserActiveStatus(int userId, bool isActive)
        {
            string query = "UPDATE Users SET IsActive = @IsActive WHERE UserID = @UserID";

            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@IsActive", isActive);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    await connection.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Error in SetUserActiveStatus!");
                    return false;
                }
            }
        }

        public static async Task<UserDataDTO?> GetUserDataByID(int UserID)
        {
            string query = "SELECT UserID, UserName, PasswordHash, FullName, Email, Role, LastLogin, IsActive FROM Users WHERE UserID = @UserID";

            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@UserID", UserID);
                    await connection.OpenAsync();
                    
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new UserDataDTO(
                                reader.GetInt32(reader.GetOrdinal("UserID")), 
                                reader.GetString(reader.GetOrdinal("UserName")),
                                reader.GetString(reader.GetOrdinal("PasswordHash")),
                                reader.GetString(reader.GetOrdinal("FullName")),
                                reader.GetString(reader.GetOrdinal("Email")),
                                reader.GetString(reader.GetOrdinal("Role")),
                                reader.GetDateTime(reader.GetOrdinal("LastLogin")), 
                                reader.GetBoolean(reader.GetOrdinal("IsActive")) 
                            );
                        }
                        else
                            return null;
                    }
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Error in GetUserDataByID!");
                    return null;
                }
            }
        }

        public static async Task<string?> GetAdminEmail()
        {
            string Query = "SELECT Email FROM Users WHERE Role = 'Admin' ";

            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(Query, connection))
            {
                try
                {
                    await connection.OpenAsync();

                    var result = await cmd.ExecuteScalarAsync();

                    return result != null ? result.ToString() : string.Empty;
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Error in GetAdminEmail!");
                    return string.Empty;
                }
            }
        }



        public static async Task<UserDataDTO?> LoginUserByUserNameAndPassword(string userName, string password)
        {
            string query = @"SELECT UserID, UserName, PasswordHash, FullName, Email, Role, LastLogin, IsActive
                             FROM Users WHERE UserName = @UserName";
          
            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string storedPasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash"));
                            if (BCrypt.Net.BCrypt.Verify(password, storedPasswordHash))
                            {
                                return new UserDataDTO(
                                    reader.GetInt32(reader.GetOrdinal("UserID")),
                                    reader.GetString(reader.GetOrdinal("UserName")),
                                    storedPasswordHash,
                                    reader.GetString(reader.GetOrdinal("FullName")),
                                    reader.GetString(reader.GetOrdinal("Email")),
                                    reader.GetString(reader.GetOrdinal("Role")),
                                    reader.GetDateTime(reader.GetOrdinal("LastLogin")),
                                    reader.GetBoolean(reader.GetOrdinal("IsActive"))
                                );
                            }
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Error in LoginUserByUserNameAndPassword!");
                    return null;
                }
            }
        }   

        public async static Task<int> AddNewUser(UserDataDTO user)
        {
            int newUserID = -1;

            string query = @"INSERT INTO Users (UserName, PasswordHash, FullName, Email, Role, LastLogin, IsActive)
                             VALUES (@UserName, @PasswordHash, @FullName, @Email, @Role, @LastLogin, @IsActive);
                             SELECT SCOPE_IDENTITY();";
            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@UserName", user.UserName);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Role", user.Role);
                    cmd.Parameters.AddWithValue("@LastLogin", user.LastLogin);
                    cmd.Parameters.AddWithValue("@IsActive", user.IsActive);

                    await connection.OpenAsync();

                    var result = await cmd.ExecuteScalarAsync();

                    if (result != null && int.TryParse(result.ToString(), out int insertedID))
                        newUserID = insertedID;

                    return newUserID;
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Error in AddNewUser!");
                    return -1;
                }
            }
        }
        
        public static async Task<bool> UpdateUser(UserDataDTO user)
        {
            string query = @"UPDATE Users SET 
                             UserName = @UserName, 
                             PasswordHash = @PasswordHash, 
                             FullName = @FullName, 
                             Email = @Email, 
                             Role = @Role, 
                             LastLogin = @LastLogin, 
                             IsActive = @IsActive
                             WHERE UserID = @UserID";

            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@UserID", user.UserID);
                    cmd.Parameters.AddWithValue("@UserName", user.UserName);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Role", user.Role);
                    cmd.Parameters.AddWithValue("@LastLogin", user.LastLogin);
                    cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                    await connection.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Error in UpdateUser!");
                    return false;
                }
            }
        }
        public async static Task<bool> DeleteUser(int userId)
        {
            string query = "DELETE FROM Users WHERE UserID = @UserID";

            using (SqlConnection connection = new(_connectionString))
            using (SqlCommand cmd = new(query, connection))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    await connection.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    clsEventLog.EnterEventLog(ex, "Error in DeleteUser!");
                    return false;
                }
            }
        }
    }
}
