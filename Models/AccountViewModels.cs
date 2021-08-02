using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiplomSite.Models
{
    //Модель, использующаяся для авторизации пользователя на сайте
    public class LoginViewModel
    {
        //[Required]
        //Указывет название, которое будет показано при импользование html.LabelFor
        [Display(Name = "Адрес электронной почты")]
        //Представляет адрес электронной почты
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Имя пользователя")]
        //Атрибут указывает на то, что поле обязательное
        [Required]
        public string UserName { get; set; }

        [Required]
        //Указывает на тип данных поля пароля
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }

    //Модель данных, использующаяся при регистрации нового пользователя на сайте
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }

        [Display(Name = "Имя пользователя"), Required]
        public string UserName { get; set; }

        [Required]
        //Указывает на сообщение при ошибке ввода пароля и его минимальную длину
        [StringLength(100, ErrorMessage = "Значение {0} должно содержать не менее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }
    }

    //Модель данных, использующаяся для смены пароля
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Значение {0} должно содержать не менее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    //Модель данных, использующаяся при вызове метода восстановления пароля
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Почта")]
        public string Email { get; set; }
    }
}
