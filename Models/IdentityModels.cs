using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DiplomSite.Models
{
    //Класс отвечающий за полльзователей приложения, наследуется от класса идентифицированных пользователей
    public class ApplicationUser : IdentityUser
    {
        //Метод создания асинхонной идентификации пользователя
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Обратите внимание, что authenticationType должен совпадать с типом, определенным в CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Здесь добавьте утверждения пользователя
            return userIdentity;
        }
    }

    //Определение контекста БД пользователей
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

    //Класс в котором реализовано создание двух ролей, используется один раз
    //при создание БД и после отключается в файле Global.asax
    public class AppDbInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>());

            //Создание двух ролей
            var role1 = new IdentityRole {Name = "admin"};
            var role2 = new IdentityRole {Name = "user"};

            //Добавление ролей в бд
            roleManager.Create(role1);
            roleManager.Create(role2);

            //Создаем пользователей
            var admin = new ApplicationUser {Email = "gelios90@bk.ru", UserName = "Andrey58"};
            string password = "123456";
            var result = userManager.Create(admin, password);

            //Если создание пользователя прошло успешно
            if (result.Succeeded)
            {
                //Добавляем для пользователя роль
                userManager.AddToRole(admin.Id, role1.Name);
                userManager.AddToRole(admin.Id, role2.Name);
            }
            //Общая идея Seed Method состоит в том, чтобы инициализировать данные в базе данных, которая создается Code First или развивается посредством Migrations.
            base.Seed(context);
        }
    }

}