using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using By_ProV2.DataAccess;
using By_ProV2.Models;

namespace By_ProV2.Services
{
    public class AuthenticationService
    {
        private readonly UserRepository _userRepository;
        private User _currentUser;

        public AuthenticationService()
        {
            _userRepository = new UserRepository();
        }

        public User CurrentUser => _currentUser;

        public bool IsLoggedIn => _currentUser != null;

        public bool Login(string username, string password)
        {
            var user = _userRepository.GetUserByUsername(username);
            if (user != null && VerifyPassword(password, user.PasswordHash) && user.IsActive)
            {
                _currentUser = user;
                _userRepository.UpdateLastLogin(user.Id);
                return true;
            }
            return false;
        }

        public void Logout()
        {
            _currentUser = null;
        }

        public bool RegisterUser(string username, string password, string fullName, string email, string role = "User")
        {
            // Check if username already exists
            if (_userRepository.GetUserByUsername(username) != null)
            {
                return false; // Username already taken
            }

            string passwordHash = HashPassword(password);
            var user = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                FullName = fullName,
                Email = email,
                Role = role
            };
            
            return _userRepository.CreateUser(user);
        }

        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var user = _userRepository.GetUserById(userId);
            if (user != null && VerifyPassword(oldPassword, user.PasswordHash))
            {
                string newPasswordHash = HashPassword(newPassword);
                return _userRepository.UpdatePassword(userId, newPasswordHash);
            }
            return false;
        }

        public bool HasUsers()
        {
            return _userRepository.HasUsers();
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            string passwordHash = HashPassword(password);
            return passwordHash.Equals(hash);
        }
    }
}