using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DiplomSite.Models;

namespace DiplomSite
{
    //Коласс управляющий жизненным циклом приложения
    public class MvcApplication : System.Web.HttpApplication
    {
        //Метод вызываемый при старте приложения
        protected void Application_Start()
        {
            //Создает базу данных пользователей
            //Database.SetInitializer<ApplicationDbContext>(new  AppDbInitializer());
            //Вызывает метод регистрации всех частей проекта
            AreaRegistration.RegisterAllAreas();
            //Определение фильтров ко всем методам, во всех контроллерах
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            //Регистрация маршрутов приложения
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //Регистрация набора скриптов и стилей т.е. бандла
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
