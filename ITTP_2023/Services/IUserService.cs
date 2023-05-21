﻿using ITTP_2023.Models;
using static ITTP_2023.Services.UserService;
using System.Security.Cryptography;

namespace ITTP_2023.Services
{
    public interface IUserService
    {
        public Task<IEnumerable<User>> GetActiveUsersAsync(User editor);
        public Task<User?> LogInByLoginAndPasswordAsync(string login, string password);
        public Task<User> GetUserByLoginAsync(User editor, string login);
        public Task<IEnumerable<User>> GetUsersOlderAsync(User editor, int age);

        public Task<User> DeleteSoftAsync(User editor, string login);
        public Task<User> DeleteHardAsync(User editor, string login);

        public Task<User> UnBlockAsync(User editor, string login);

        public Task<User> CreateAsync(User editor, string login, string password, string name, int gender, DateTime? birthday, bool isAdmin);

        public Task<User> UpdateAsync(User editor, string login, string name, int? gender, DateTime? birthday);
        public Task<User> UpdatePasswordAsync(User editor, string login, string password);
        public Task<User> UpdateLoginAsync(User editor, string oldlLogin, string newLogin);

        public Task<UserToken> LoginAndGetTokenAsync(string login, string password);

        public Task<User> LogInByTokenAsync(UserToken token);
        public Task<User> LogInByTokenAsync(string token);

        public Task C();
    }
}
