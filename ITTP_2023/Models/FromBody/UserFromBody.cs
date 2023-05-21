using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace ITTP_2023.Models.FromBody
{
    public class UserFromBody
    {
        [Required]
        [DisplayName("Уникальный Логин (запрещены все символы кроме латинских букв и цифр)")]
        public string Login { get; set; } = string.Empty;

        [Required]
        [DisplayName("Пароль(запрещены все символы кроме латинских букв и цифр)")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DisplayName("Имя (запрещены все символы кроме латинских и русских букв)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [DisplayName("Пол 0 - женщина, 1 - мужчина, 2 - неизвестно")]
        public int Gender { get; set; }

        [MaybeNull]
        [DisplayName("Поле даты рождения может быть Null")]
        public DateTime? Birthday { get; set; } = null;

        [Required]
        [DisplayName("Указание - является ли пользователь админом")]
        public bool Admin { get; set; }
    }
}
