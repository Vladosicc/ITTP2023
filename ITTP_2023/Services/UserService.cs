﻿using ITTP_2023.DbContexts;
using ITTP_2023.Helpers;
using ITTP_2023.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ITTP_2023.Services
{
    public class UserService : IUserService
    {
        private readonly UserContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserService(UserContext dataContext, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<User> CreateAsync(User editor, string login, string password, string name, int gender, DateTime? birthday, bool isAdmin)
        {
            if (editor == null)
                throw new Exception("Неверный логин и/или пароль");
            //Пользователь с таким логином существует
            if (await GetUserByLoginAsync(login) != null)
                throw new Exception("Пользователь с таким логином уже существует");

            if (!editor.Admin)
                throw new Exception("Пользователя пытается создать не администратор");

            if (!IsValidLogin(login))
            {
                throw new Exception("Логин имеет недопустимые символы");
            }

            if (!IsValidPassword(password))
            {
                throw new Exception("Пароль имеет недопустимые символы");
            }

            if (!IsValidName(name))
            {
                throw new Exception("Имя имеет недопустимые символы");
            }

            User user = new User()
            {
                Guid = Guid.NewGuid(),
                
                Login = login,
                Password = password,
                Name = name,
                Gender = gender,
                Birthday = birthday,
                Admin = isAdmin,

                CreatedOn = DateTime.UtcNow,
                CreatedBy = editor.Login,

                ModifiedBy = editor.Login,
                ModifiedOn = DateTime.UtcNow
            };

            var response = await _dataContext.Users.AddAsync(user);
            await _dataContext.SaveChangesAsync();

            return response.Entity;
        }

        //UPDATE-1
        public async Task<User> UpdateAsync(User editor, string login, string name, int? gender, DateTime? birthday)
        {
            if (editor == null)
                throw new Exception("Неверный логин и/или пароль");

            var user = await GetUserByLoginAsync(login);
            //Пользователь с таким логином не существует
            if (user == null)
                throw new Exception($"{login} не найден");

            if(editor.Admin == false)
                if (editor.Login != user.Login)
                    throw new Exception("Нет доступа к запросу");

            if (editor.Login == user.Login && user.RevokedOn.HasValue)
                throw new Exception("Аккаунт заблокирован");

            if(!string.IsNullOrEmpty(name))
                if (!IsValidName(name))
                    throw new Exception("Имя имеет недопустимые символы");

            if(gender.HasValue)
                if (!IsValidGender(gender))
                    throw new Exception("Гендер имеет недопустимые значения. (0 <= gender <= 2)");

            if(!string.IsNullOrEmpty(name))
                user.Name = name;

            if(gender.HasValue)
                user.Gender = gender.Value;

            if (birthday.HasValue)
                user.Birthday = birthday.Value;

            user.ModifiedBy = editor.Login;
            user.ModifiedOn = DateTime.UtcNow;

            var response = _dataContext.Users.Update(user);
            await _dataContext.SaveChangesAsync();

            return response.Entity;
        }

        public async Task<User> UpdatePasswordAsync(User editor, string login, string password)
        {
            if (editor == null)
                throw new Exception("Неверный логин и/или пароль");

            var user = await GetUserByLoginAsync(login);
            //Пользователь с таким логином не существует
            if (user == null)
                throw new Exception($"{login} не найден");

            if (editor.Admin == false)
                if (editor.Login != user.Login)
                    throw new Exception("Нет доступа к запросу");

            if (editor.Login == user.Login && user.RevokedOn.HasValue)
                throw new Exception("Аккаунт заблокирован");

            if (!IsValidPassword(password))
            {
                throw new Exception("Пароль имеет недопустимые символы");
            }

            if (!string.IsNullOrEmpty(password))
            {
                user.Password = password;

                user.ModifiedBy = editor.Login;
                user.ModifiedOn = DateTime.UtcNow;

                var response = _dataContext.Users.Update(user);
                await _dataContext.SaveChangesAsync();

                return response.Entity;
            }
            else
            {
                return null;
            }
        }

        public async Task<User> UpdateLoginAsync(User editor, string oldLogin, string newLogin)
        {
            if (editor == null)
                throw new Exception("Неверный логин и/или пароль");

            if (await GetUserByLoginAsync(newLogin) != null)
                throw new Exception($"{newLogin} занят");

            var user = await GetUserByLoginAsync(oldLogin);
            //Пользователь с таким логином существует
            if (user == null)
                throw new Exception($"{oldLogin} не найден");

            if (editor.Admin == false)
                if (editor.Login != user.Login)
                    throw new Exception("Нет доступа к запросу");

            if (editor.Login == user.Login && user.RevokedOn.HasValue)
                throw new Exception("Аккаунт заблокирован");

            if (!IsValidLogin(newLogin))
            {
                throw new Exception("Логин имеет недопустимые символы");
            }

            if (!string.IsNullOrEmpty(newLogin))
            {
                user.Login = newLogin;

                user.ModifiedBy = editor.Login;
                user.ModifiedOn = DateTime.UtcNow;

                var response = _dataContext.Users.Update(user);
                await _dataContext.SaveChangesAsync();

                return response.Entity;
            }
            else
            {
                return null;
            }
        }

        //READ
        public async Task<IEnumerable<User>> GetActiveUsersAsync(User editor)
        {
            if (editor == null)
                throw new Exception("Неверный логин и/или пароль");
            if (!editor.Admin)
                throw new Exception("Нет доступа к запросу");

            return await _dataContext.Users.AsNoTracking().Where(i => i.RevokedOn == null).OrderBy(i => i.CreatedOn).ToListAsync();
        }

        public async Task<User> GetUserByLoginAsync(User editor, string login)
        {
            if (editor == null)
                throw new Exception("Неверный логин и/или пароль");
            if (!editor.Admin)
                throw new Exception("Нет доступа к запросу");

            var user = await GetUserByLoginAsync(login);

            if (user == null)
                throw new Exception("Неверный логин в запросе");

            return new User() {Name = user.Name, Gender = user.Gender, Birthday = user.Birthday, Admin = user.Admin, RevokedOn = user.RevokedOn };
        }


        public async Task<User?> LogInByLoginAndPasswordAsync(string login, string password)
        {
            return (await _dataContext.Users.ToListAsync()).FirstOrDefault(i => i.Login.ToLower() == login.ToLower() && i.Password == password);
        }

        public async Task<IEnumerable<User>> GetUsersOlderAsync(User editor, int age)
        {
            if (editor == null)
                throw new Exception("Неверный логин и/или пароль");
            if (!editor.Admin)
                throw new Exception("Нет доступа к запросу");


            return _dataContext.Users.AsEnumerable().Where(i => i.Birthday.HasValue && new DateDifference(DateTime.UtcNow, i.Birthday.Value).Year > age);
        }

        //DELETE
        public async Task<User> DeleteHardAsync(User editor, string login)
        {
            if (editor == null)
                throw new Exception("Неверный логин и/или пароль");
            var user = await GetUserByLoginAsync(login);
            //Пользователь с таким логином не существует
            if (user == null)
                throw new Exception("Пользователь с таким логином не существует");

            if (!editor.Admin)
                throw new Exception("Пользователя пытается удалить не администратор");

            var response = _dataContext.Users.Remove(user);
            await _dataContext.SaveChangesAsync();

            return response.Entity;
        }

        public async Task<User> DeleteSoftAsync(User editor, string login)
        {
            if (editor == null)
                throw new Exception("Неверный логин и/или пароль");
            
            var user = await GetUserByLoginAsync(login);
           

            //Пользователь с таким логином не существует
            if (user == null)
                throw new Exception("Пользователь с таким логином не существует");

            if (!editor.Admin)
                throw new Exception("Пользователя пытается удалить не администратор");

            user.RevokedOn = DateTime.UtcNow;
            user.RevokedBy = editor.Login;
            user.ModifiedBy = editor.Login;
            user.ModifiedOn = DateTime.UtcNow;
            var response = _dataContext.Users.Update(user);
            await _dataContext.SaveChangesAsync();

            return response.Entity;
        }

        //UPDATE-2
        public async Task<User> UnBlockAsync(User editor, string login)
        {
            if (editor == null)
                throw new Exception("Неверный логин и/или пароль");
            if (!editor.Admin)
                throw new Exception("Нет доступа к запросу");

            var user = await GetUserByLoginAsync(login);
            //Пользователь с таким логином не существует
            if (user == null)
                return user;

            user.RevokedOn = null;
            user.RevokedBy = string.Empty;
            user.ModifiedBy = editor.Login;
            user.ModifiedOn = DateTime.UtcNow;

            var response = _dataContext.Users.Update(user);
            await _dataContext.SaveChangesAsync();

            return response.Entity;
        }

        //TOKENS
        public async Task<UserToken> LoginAndGetTokenAsync(string login, string password)
        {
            var user = await LogInByLoginAndPasswordAsync(login, password);

            if (user == null)
                throw new Exception("Неверные пароль и/или логин");

            if(string.IsNullOrEmpty(user.Token))
            {
                var sha512 = SHA512.Create();
                user.Token = Convert.ToHexString(sha512.ComputeHash(Guid.NewGuid().ToByteArray()));
                _dataContext.Users.Update(user);
                await _dataContext.SaveChangesAsync();
            }

            return new UserToken(user.Token);
        }

        public async Task<User> LogInByTokenAsync(UserToken token)
        {
            return await LogInByTokenAsync(token.Token);
        }
        public async Task<User> LogInByTokenAsync(string token)
        {
            return await _dataContext.Users.FirstOrDefaultAsync(x => x.Token == token);
        }


        private async Task<User> GetUserByLoginAsync(string login)
        {
            return (await _dataContext.Users.ToListAsync()).FirstOrDefault(i => i.Login.ToLower() == login.ToLower());
        }

        private bool IsValidPassword(string password)
        {
            return Regex.IsMatch(password, "^[a-zA-Z0-9]+$");
        }

        private bool IsValidLogin(string login)
        {
            return Regex.IsMatch(login, "^[a-zA-Z0-9]+$");
        }

        private bool IsValidName(string name)
        {
            return Regex.IsMatch(name, "^[a-zA-Zа-яА-Я]+$");
        }

        private bool IsValidGender(int? gender)
        {
            if(gender < 0 || gender > 2) 
                return false;
            return true;
        }

        public async Task C()
        {
            _dataContext.Users.Add(new User()
            {
                Admin = true,
                Name = "Admin",
                Login = "Admin",
                Password = "Admin",
                ModifiedBy = "",
                ModifiedOn = DateTime.UtcNow,
                CreatedBy = "Admin",
                CreatedOn = DateTime.UtcNow,
                RevokedBy = "",
                RevokedOn = null,
                Birthday = null,
                Gender = 2,
                Guid = Guid.NewGuid(),
            });
            _dataContext.SaveChanges();
            await LoginAndGetTokenAsync("Admin", "Admin");
        }

        public class UserToken
        {
            public string Token { get; }
            public UserToken(string token)
            {
                Token = token;
            }
        }
    }
}
