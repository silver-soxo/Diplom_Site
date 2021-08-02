using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DiplomSite.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;

namespace DiplomSite.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationUserManager _userManager;

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

        //Объявление переменной контекста всех моделей
        CheckingContext db2 = new CheckingContext();

        //Метод доступынй только пользователю с ролью "администратор"
        [Authorize(Roles = "admin")]
        //Метод который реализует представление с таблицами
        public ActionResult Index()
        {
            //Подключение к базе данных
            SqlConnection SqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            SqlConnection.Open();

            SqlConnection SqlConnection2 = new SqlConnection(ConfigurationManager.ConnectionStrings["CheckingContext"].ConnectionString);
            SqlConnection2.Open();

            //Запрос на получение данных о пользователях сайта из БД
            SqlDataAdapter adapter = new SqlDataAdapter($"SELECT AspNetUsers.Id, AspNetUsers.UserName, AspNetUsers.Email, AspNetRoles.Name, AspNetUsers.VisitorAddConf FROM AspNetUsers INNER JOIN AspNetUserRoles ON AspNetUsers.Id = AspNetUserRoles.UserId INNER JOIN AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId", SqlConnection);
            DataTable table = new DataTable();
            adapter.Fill(table);

            //Поиск пользователя по ID в БД
            var user2 = UserManager.FindById(User.Identity.GetUserId());
            ViewBag.user_id = user2.Id;

            //Создание списка пользователей сайта
            List<Site_users> users = new List<Site_users>();
            for(int i = 0; i < table.Rows.Count; i++)
            {
                Site_users site = new Site_users();
                site.Id = Convert.ToString(table.Rows[i]["Id"]);
                site.UserName = Convert.ToString(table.Rows[i]["UserName"]);
                site.Email = Convert.ToString(table.Rows[i]["Email"]);
                site.Role = Convert.ToString(table.Rows[i]["Name"]);
                site.VisitorAddConf = Convert.ToString(table.Rows[i]["VisitorAddConf"]);
                users.Add(site);
            }

            ViewBag.site_users = users;

            SqlDataAdapter adapter2 = new SqlDataAdapter($"SELECT visitor_name FROM visitors", SqlConnection2);
            DataSet logpas = new DataSet();
            adapter2.Fill(logpas, "user_lvl");
            int search = logpas.Tables["user_lvl"].Rows.Count;
            if (search == 0)
            {
                ViewBag.visitorcount = 0;
            }
            else
            {
                ViewBag.visitorcount = 1;
            }

            //Создание переменной представляющией связанные таблицы посетителя, его паспорта и отдела
            var visitor = db2.visitor.Include(p => p.Pasports).Include(d => d.Departments);
            var user = db2.user;
            ViewBag.visitor = visitor;
            ViewBag.user = user;
            return View();
        }
        //Метод который реализует представление с информацией о проекте
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        //Метод доступный только пользователю с ролью "user" и представляющий контактную информацию
        [Authorize(Roles = "user")]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        //Метод который реализует представление с анкетой на посещение офиса
        public ActionResult VisitorAdd()
        {
            //Подключение к БД сайта
            SqlConnection SqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            SqlConnection.Open();

            SqlConnection SqlConnection2 = new SqlConnection(ConfigurationManager.ConnectionStrings["CheckingContext"].ConnectionString);
            SqlConnection2.Open();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
                
            //Поиск email пользователя который сейчас вошел в аккаунт
            SqlCommand cmd = new SqlCommand($"SELECT Email FROM AspNetUsers WHERE Id = '{User.Identity.GetUserId()}'", SqlConnection);
            var str = cmd.ExecuteScalar();

            SqlCommand cmd2 = new SqlCommand($"SELECT VisitorAddConf FROM AspNetUsers WHERE Id = '{User.Identity.GetUserId()}'", SqlConnection);
            var str2 = cmd2.ExecuteScalar();

            var user = UserManager.FindById(User.Identity.GetUserId());

            int search;
            DataSet info = new DataSet();
            sqlDataAdapter = new SqlDataAdapter($"SELECT visitor_email FROM visitors WHERE visitor_email = '{user.Email}'", SqlConnection2);
            sqlDataAdapter.Fill(info, "email");
            search = info.Tables["email"].Rows.Count;

            if (search == 0)
            {
                SqlCommand cmd3 = new SqlCommand($"UPDATE AspNetUsers SET VisitorAddConf = 'False' WHERE Id = '{user.Id}'", SqlConnection);
                cmd3.ExecuteNonQuery();
            }

            ViewBag.Email = str;
            ViewBag.VisitorAdd = Convert.ToString(str2);

            //Передача таблицы департаментов из БД в представление
            var departments = db2.department;
            ViewBag.departments = departments;
            return View();
        }

        [HttpPost]
        //Метод который получает данные из формы заявки на посещение и записаывает д БД
        public ActionResult VisitorAdd(VisitorAdd model, string department_name)
        {
            //Подключение к БД офиса
            SqlConnection SqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CheckingContext"].ConnectionString);
            SqlConnection.Open();
            var user2 = UserManager.FindById(User.Identity.GetUserId());

            //Передача данных из формы в процедуру добавления посетителя
            SqlCommand cmd = new SqlCommand("visitor_add", SqlConnection)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@pasport_cod", model.Pasport_cod));
            cmd.Parameters.Add(new SqlParameter("@pasport_number", model.Pasport_number));
            cmd.Parameters.Add(new SqlParameter("@department_name", department_name));
            cmd.Parameters.Add(new SqlParameter("@visitor_name", model.Name));
            cmd.Parameters.Add(new SqlParameter("@visitor_surname", model.Surname));
            cmd.Parameters.Add(new SqlParameter("@visitor_phone", model.Phone));
            cmd.Parameters.Add(new SqlParameter("@visitor_email", model.Email));
            cmd.ExecuteNonQuery();

            //Подключение к БД пользователей сайта
            SqlConnection SqlConnection2 = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            SqlConnection2.Open();
            
            //Получение почты пользователя выполняющего действия в аккаунте
            SqlCommand cmd2 = new SqlCommand($"SELECT Email FROM AspNetUsers WHERE Id = '{User.Identity.GetUserId()}'", SqlConnection2);
            //Отправка письма о том, что заявка на посещение офиса сдлана
            string email = Convert.ToString(cmd2.ExecuteScalar());
            string caption = "Уведрмление о заявке на посещение офиса";
            string message =
                $"Здравствуйте, {User.Identity.GetUserName()}! Ваша заявка принята и в скором времени будет рассмотрена и отправлена на создание карты посетителя, о готовности вас оповестят письмом!";
            string from2 = "gelios90@bk.ru";


            MailAddress from = new MailAddress("gelios90@bk.ru", "Andrey");
            MailAddress to = new MailAddress(email);
            MailMessage m = new MailMessage(from, to);
            m.Subject = "Уведрмление о заявке на посещение офиса";
            m.Body =
                $"<h2>Здравствуйте, {User.Identity.GetUserName()}! Ваша заявка принята и в скором времени будет рассмотрена и отправлена на создание карты посетителя, о готовности вас оповестят письмом!</h2>";
            m.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient("smtp.bk.ru", 2525);
            smtp.Credentials = new NetworkCredential("gelios90@bk.ru", "Arhangel_58");
            smtp.EnableSsl = true;

            smtp.Send(m);

            SqlCommand cmd3 = new SqlCommand($"UPDATE AspNetUsers SET VisitorAddConf = 'True' WHERE Id = '{user2.Id}'", SqlConnection2);
            cmd3.ExecuteNonQuery();

            
            Response.Redirect("~/Home");
            return null;
        }
    }
}