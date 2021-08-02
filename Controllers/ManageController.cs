using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using DiplomSite.Models;
using System.Data.SqlClient;
using System.Configuration;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data;

namespace DiplomSite.Controllers
{
    //Класс выполняющий функции менеджмента внутри аккаунта (удаление, редактирование и т.д.)
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        //Объявление контроллера для функционирования методов
        public ManageController()
        {
        }

        //Переопределение переменных из классав диспетчера входа и диспетчера пользователей приложения (файл IdentityConfig.cs)
        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        //Создание идентификатора класса диспетчера входа в приложение
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        //Создание идентификатора класса диспетчера пользователей приложения
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        //Представление управления аккаунтом (удалить, редактировать, изменить пароль)
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            //Создание статусных сообщений
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Ваш пароль изменен."
                : message == ManageMessageId.SetPasswordSuccess ? "Пароль задан."
                : message == ManageMessageId.Error ? "Произошла ошибка."
                : "";

            var userId = User.Identity.GetUserId();
            //Определение данных в моделе представления Index
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                /*PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),*/
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin

        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }*/

        //
        // GET: /Manage/ChangePassword
        //Метод отправляющий пользователя в представление смены пароля
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        //Метод принимающий данные из формы и реализующий смену пароля
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            //Усли модель данных не успешно передана, возвращает данные обратно в форму
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //Создание переменной результата, для проверки срабатывания функции изменения пароля
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            //Усли пароль изменен
            if (result.Succeeded)
            {
                //Поиск пользователя в БД по ID
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                //Усли пользователь не найден, остаться в системе
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                //Очистка памяти о том, что пользователь в системе и переход в представление аутентификации
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return RedirectToAction("Login", "Account");
            }
            AddErrors(result);
            return View(model);
        }

        /*//
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // Это сообщение означает наличие ошибки; повторное отображение формы
            return View(model);
        }*/

        //Метод для освобождения неуправляемых ресурсов
        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Вспомогательные приложения
        // Используется для защиты от XSRF-атак при добавлении внешних имен входа
        private const string XsrfKey = "XsrfId";

        //Возвращает контекст авторизации http-запроса
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        //Отправка в представление сообщений при ошибке
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        //Проверка на существование пароля
        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        [HttpGet]
        //Перенаправление пользователя в представление удаления аккаунта
        public ActionResult Delete()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Delete")]
        //Метод связанный с формой удаления пользователя
        public async Task<ActionResult> DeleteConfirmed()
        {
            //Поис пользователя по имени в БД
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity.Name);
            //Если пользователь найден
            if (user != null)
            {
                //Создание переменной которая говорит о срабатывании удаления пользователя
                IdentityResult result = await UserManager.DeleteAsync(user);
                //Если удаление прошло успешно
                if (result.Succeeded)
                {
                    //Очистка памети о пользователе и переход в представления авторизации
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    return RedirectToAction("Login", "Account");
                }
            }
            return RedirectToAction("About", "Home");
        }

        //Метод редактирования данных аккаунта
        public async Task<ActionResult> Edit()
        {
            //Поис пользователя по имени в БД
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity.Name);
            //Если пользователь найден
            if (user != null)
            {
                //Передача в представление данных об скомомпользователе
                DeleteCreateModel model = new DeleteCreateModel { UserName = user.UserName, Email = user.Email };
                return View(model);
            }
            return RedirectToAction("About", "Home");
        }

        [HttpPost]
        //Метод, получающий данные из формы редактирования пользователя
        public async Task<ActionResult> Edit(DeleteCreateModel model)
        {
            //Поис пользователя по имени в ЮД
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity.Name);
            //Если пользователь найден
            if (user != null)
            {
                //Переопределение данных в БД на основе данных из формы
                user.UserName = model.UserName;
                user.Email = model.Email;
                IdentityResult result = await UserManager.UpdateAsync(user);
                //Если данные удалось переписать
                if (result.Succeeded)
                {
                    //Реализация выхода из аккаунта
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    ModelState.AddModelError("", "Что-то пошло не так");
                }
            }
            else
            {
                ModelState.AddModelError("", "Пользователь не найден");
            }

            return View(model);
        }

        [HttpPost]
        //Удаление пользователя в аккаунте администратора
        public async Task<ActionResult> Delete_user(string id)
        {
            //Поиск пользователя в БД по ID
            ApplicationUser user = await UserManager.FindByIdAsync(id);
            //Если пользователь найден
            if (user != null)
            {
                //Удаление пользователя
                IdentityResult result = await UserManager.DeleteAsync(user);
                //Если удаление прошло успешно
                if (result.Succeeded)
                {
                    //Переход обратно в представление с менеджером пользователей
                    TempData["alertMessage"] = "Whatever you want to alert the user with";
                    Response.Redirect("~/Home/Index");
                    return View("Index");
                }
                else
                {
                    return View("Error", result.Errors);
                }
            }
            else
            {
                return View("Error", new string[] { "Пользователь не найден" });
            }
        }
        //Метод редактирования пользователя через администратора
        public async Task<ActionResult> Edit_user(string id)
        {
            //Поиск пользователя в БД по ID
            ApplicationUser user = await UserManager.FindByIdAsync(id);

            //Подключение к БД
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            connection.Open();

            //Создание запроса на поиск названия роли а БД
            SqlCommand cmd = new SqlCommand($"SELECT AspNetRoles.Name FROM AspNetUsers  INNER JOIN AspNetUserRoles ON AspNetUsers.Id = AspNetUserRoles.UserId INNER JOIN AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId WHERE AspNetUsers.Id = '{id}'", connection);
            string role = Convert.ToString(cmd.ExecuteScalar());

            //Запись полученных данных в модель
            EditModel model = new EditModel { UserName = user.UserName, Email = user.Email, Role = role };
            //Если запись прошла успешно, передать модель в представление
            if (user != null)
            {
                return View(model);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        //Метод который получает данные из формы редактирования пользователя и изменяет их 
        public async Task<ActionResult> Edit_user(EditModel model)
        {
            //Поиск пользователей по имени и ID в БД 
            ApplicationUser user1 = await UserManager.FindByNameAsync(User.Identity.Name);
            ApplicationUser user = await UserManager.FindByIdAsync(model.Id);
            //Если пользователь найден
            if (user != null)
            {
                //Проверка email на правильность
                user.Email = model.Email;
                IdentityResult validEmail
                    = await UserManager.UserValidator.ValidateAsync(user);

                //Если почта не верная, отправляет сообщение об ошибке
                if (!validEmail.Succeeded)
                {
                    AddErrorsFromResult(validEmail);
                }

                user.UserName = model.UserName;
                IdentityResult validName
                    = await UserManager.UserValidator.ValidateAsync(user);

                //Если логин не верный, отправляет сообщение об ошибке
                if (!validName.Succeeded)
                {
                    AddErrorsFromResult(validName);
                }

                IdentityResult validPass = null;
                //Если поле пароля было заполнено
                if (model.Password != null)
                {
                    //Проверка пароля на валидатность
                    validPass
                        = await UserManager.PasswordValidator.ValidateAsync(model.Password);

                    //Если пароль прошел проверку
                    if (validPass.Succeeded)
                    {
                        //Перепись пароля из БД на введенный
                        user.PasswordHash =
                            UserManager.PasswordHasher.HashPassword(model.Password);
                    }
                    else
                    {
                        AddErrorsFromResult(validPass);
                    }
                }

                //Если роль передана в модель
                if (model.Role != null)
                {
                    //Подключение к БД
                    SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
                    connection.Open();

                    //Передача данных в процедуру на смену роли
                    SqlCommand cmd = new SqlCommand("Role_update", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.Add(new SqlParameter("@rol_name", model.Role));
                    cmd.Parameters.Add(new SqlParameter("@user_id", model.Id));
                    cmd.ExecuteNonQuery();
                }

                //Если все данные верны
                if ((validEmail.Succeeded && validPass == null) ||
                        (validEmail.Succeeded && model.Password != string.Empty && validPass.Succeeded))
                {
                    //Обновление пользователя в БД на основе данных записанных в user
                    IdentityResult result = await UserManager.UpdateAsync(user);
                    //Если запись прошла успешно
                    if (result.Succeeded)
                    {
                        //Проверка на изменение своего аккаунта 
                        if (model.Id == user1.Id)
                        {
                            //Выход из аккаунта при изменении данных
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            return RedirectToAction("Login", "Account");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        AddErrorsFromResult(result);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Пользователь не найден");
            }
            return View(user);
        }
        [HttpGet]
        //Метод, отправляющий администратора в представление добавления пользователя
        public ActionResult Add_user()
        {
            return View();
        }

        [HttpPost]
        //Метод получающий данные из формы добавления нового пользователя администратором
        public async Task<ActionResult> Add_user(EditModel model)
        {
            SqlConnection SqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            SqlConnection.Open();
            //Создание переменной с данными о имени и почте пользователя из формы
            var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
            //Если данные переданы в переменную
            if (user != null)
            {
                //Получение результата создания нового пользователя
                var result = await UserManager.CreateAsync(user, model.Password);
                //Если пользователь создан
                if (result.Succeeded)
                {
                    //Добавление пользователю роли выбранной в фломе
                    await UserManager.AddToRoleAsync(user.Id, model.Role);
                    SqlCommand cmd = new SqlCommand($"UPDATE AspNetUsers SET VisitorAddConf = 'False' WHERE Id = '{user.Id}'", SqlConnection);
                    cmd.ExecuteNonQuery();

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            return View(model);
        }

        //Вывод ошибок 
        private void AddErrorsFromResult(IdentityResult validEmail)
        {
            throw new NotImplementedException();
        }

        //Перечисление используемых методов
        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        #endregion
    }
}