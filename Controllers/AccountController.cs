using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using DiplomSite.Models;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DiplomSite.Controllers
{
    //Класс осуществляющий контроль действий в системе аутентификации на сайте
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        //Метод необходимый для входа в аккаунт
        public AccountController()
        {
        }
        //Переопределение полученных переменных (файл IdentityConfig.cs)
        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
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
        // GET: /Account/Login
        //Осуществляет вход пользователя в аккаутн и передает в представление обратную ссылку
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        //Проаеряет данные с формы ваторизации
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            //Если по данным из модели нет зарегистрированных пользователей возвращает модель обратно в представление
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Сбои при входе не приводят к блокированию учетной записи
            // Чтобы ошибки при вводе пароля инициировали блокирование учетной записи, замените на shouldLockout: true
            //Вызов метода проверки данных пользователя из модели на авторизованность
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                //Если вход успешный перенаправляет на страницу информации
                case SignInStatus.Success:
                    Response.Redirect("~/Home/About");
                    return RedirectToLocal(returnUrl);
                /*case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:*/
                default:
                    ModelState.AddModelError("", "Неудачная попытка входа.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        //Перенаправляет пользователя в представление регистрации
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        //Получает данные из формы в представлении
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            //Если с данными в модели все впорядке
            if (ModelState.IsValid)
            {
                //Создание нового пользователя
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);

                SqlConnection SqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
                SqlConnection.Open();
                //Если создание нового пользователя прошло успешно
                if (result.Succeeded)
                {
                    //Назначаем новому пользователю роль "user"
                    await UserManager.AddToRoleAsync(user.Id, "user");
                    //Осуществление входа в аккаунт по данным созданного пользователя
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                    SqlCommand cmd = new SqlCommand($"UPDATE AspNetUsers SET VisitorAddConf = 'False' WHERE Id = '{user.Id}'", SqlConnection);
                    cmd.ExecuteNonQuery();
                    return RedirectToAction("About", "Home");
                }
                AddErrors(result);
            }

            // Появление этого сообщения означает наличие ошибки; повторное отображение формы
            return View(model);
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        //Отправляет пользователя в представление для email пользователя, забывшего пароль
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        //Метод принимающий почту забывшего пароль
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            //Проверка на успешное получение данных модели
            if (ModelState.IsValid)
            {
                //Поиск пользователя по email
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Не показывать, что пользователь не существует
                    return View("ForgotPasswordConfirmation");
                }
                //Создание токена по ID пользователя для дальнейшего использование в сбросе пароля
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                //Сщздание ссылки на представление сброса пароля
                var callbackUrl = Url.Action("ResetPassword", "Account",
                    new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                //Отправка письма на почту пользователя с сылкой на страницу сброса пароля
                await UserManager.SendEmailAsync(user.Id, "Сброс пароля",
                    "Для сброса пароля, перейдите по ссылке <a href=\"" + callbackUrl + "\">сбросить</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        //Отправляет пользователя на страницу с сообщением о проверки почты
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        //Отправка на страницу сообщения об ошибке
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        //Метод получающий данне с формы из придставления на смену пароля
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            //Если полученная подель данных завершена не успешно возвращает модель обратно в представление
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //Поиск пользователя в БД по email
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Не показывать, что пользователь не существует
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            //Отпрака данных пользователя включая уникальный код в метод сброса пароля
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            //Если сброс прошел успешно, отправка в представление о поддтверждении сброса
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        //Перенаправляет пользователя в представление о поддтверждении сброса
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        //Метод выхода из аккаунта
        public ActionResult LogOff()
        {
            //Убирает данные о вошедшем пользователе из cookie
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }
        //Метод для освобождения неуправляемых ресурсов
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
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
        //Это вспомогательный метод из шаблона интернет-приложения MVC 5, который действительно проверяет, что returnUrl действительно является локальным URL-адресом.
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        /*internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }*/
        #endregion
    }
}