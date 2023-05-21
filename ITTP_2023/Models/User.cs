using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ITTP_2023.Models
{
    public class User
    {
        [Key]
        [Required]
        [DisplayName("Уникальный идентификатор пользователя")]
        public Guid Guid { get; set; }

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
        public DateTime? Birthday { get; set; }

        [Required]
        [DisplayName("Указание - является ли пользователь админом")]
        public bool Admin { get; set; }

        [Required]
        [DisplayName("Дата создания пользователя")]
        public DateTime CreatedOn { get; set; }

        [Required]
        [DisplayName("Логин Пользователя, от имени которого этот пользователь создан")]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        [DisplayName("Дата изменения пользователя")]
        public DateTime ModifiedOn { get; set; }

        [Required]
        [DisplayName("Логин Пользователя, от имени которого этот пользователь изменён")]
        public string ModifiedBy { get; set; } = string.Empty;

        [MaybeNull]
        [DisplayName("Дата удаления пользователя")]
        public DateTime? RevokedOn { get; set; } = null;

        [Required]
        [DisplayName("Логин Пользователя, от имени которого этот пользователь удалён")]
        public string RevokedBy { get; set; } = string.Empty;

        [DisplayName("Токен")]
        public string Token { get; set; } = string.Empty;
    }
}
