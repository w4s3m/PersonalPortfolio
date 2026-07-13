using System;
using System.Runtime.CompilerServices;
using Microsoft.IdentityModel.Abstractions;
using PersonalProtfolioDataTier;


namespace PersonalProtfolioBusniessTier
{
    public class Users
    {

        public enum _enRole
        {
            Admin = 0,
            User = 1
        }
        _enRole _RoleMode;

        public string LastErrorMessage { get; private set; } = string.Empty;

        public enum _enMode
        {
            enAdd = 0,
            enUpdate = 1
        }
        _enMode _Mode;
        public UserDataDTO UDTO
        {
            get
            {
                return new UserDataDTO(this.UserID, this.UserName, this.PasswordHash, this.FullName,
                            this.Email, this.Role, this.LastLogin, this.IsActive);
            }
        }

        public int UserID { get; set; }
        public string UserName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public DateTime LastLogin { get; private set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;


        public Users(UserDataDTO UDTO, _enMode mode = _enMode.enAdd)
        {
            this.UserID = UDTO.UserID;
            this.UserName = UDTO.UserName;
            this.PasswordHash = UDTO.PasswordHash;
            this.FullName = UDTO.FullName;
            this.Email = UDTO.Email;
            this.Role = UDTO.Role;
            this.LastLogin = UDTO.LastLogin;
            this.IsActive = UDTO.IsActive;
            this._Mode = mode;
            this.Role = UDTO.Role;

            if (this.Role == "admin")
                this._RoleMode = _enRole.Admin;
            else
                this._RoleMode = _enRole.User;
        }
        public async Task<bool> UpdateLastLogin()
        {
            this.LastLogin = DateTime.Now;
            return await UsersData.UpdateLastLogin(this.UserID);
        }

        public async Task<bool> AddNewUser()
        {
            if (!IsValid())
                return false;
            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(this.PasswordHash);
            this.PasswordHash = newPasswordHash;
            this.UDTO.PasswordHash = newPasswordHash;

            this.UserID = await UsersData.AddNewUser(this.UDTO);
            return this.UserID > 0;
        }
        public async Task<bool> UpdateUserData()
        {
            if (!IsValid())
                return false;

            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(this.PasswordHash);
            this.PasswordHash = newPasswordHash;
            this.UDTO.PasswordHash = newPasswordHash;

            return await UsersData.UpdateUser(this.UDTO);
        }

        public async Task<bool> UpdatePassword(string Password)
        {
            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);
            this.PasswordHash = newPasswordHash;
            return await UsersData.UpdateUserPassword(this.UserID, newPasswordHash);
        }

        public static async Task<bool> DeleteUserByID(int UserID)
        {
            return await UsersData.DeleteUser(UserID);
        }
        public static async Task<Users?> GetUserByID(int UserID)
        {
            UserDataDTO? userData = await UsersData.GetUserDataByID(UserID);

            if (userData != null)
                return new Users(userData, _enMode.enUpdate);

            return null;
        }
        public static async Task <string?> GetAdminEmail()
        {
            return await UsersData.GetAdminEmail();
        }

        public static async Task<UserDataDTO?> LoginUserByUserNameAndPassword(string userName, string PasswordHash)
        {
            
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(PasswordHash))
                return null;

            UserDataDTO? Us = await UsersData.LoginUserByUserNameAndPassword(userName, PasswordHash);

            if (Us.Role != "user" && Us.IsActive == false)
                return null;
            else
                return Us;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
            }
            catch
            {
                return false;
            }
        }
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                LastErrorMessage = "اسم المستخدم لا يمكن أن يكون فارغاً.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(PasswordHash) || PasswordHash.Length < 8)
            {
                LastErrorMessage = "كلمة المرور يجب أن لا تقل عن 8 رموز.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email) || !IsValidEmail(Email))
            {
                LastErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(FullName))
            {
                LastErrorMessage = "يجب إدخال الاسم الكامل.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Role))
            {
                LastErrorMessage = "يجب تحديد صلاحية للمستخدم.";
                return false;
            }

            LastErrorMessage = string.Empty;
            return true;
        }


    }
}
