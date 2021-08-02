using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DiplomSite
{
    //Класс, реализующий механизм определения маршрутов
    public class RouteConfig
    {
        //Статический метод установки маршрутов
        public static void RegisterRoutes(RouteCollection routes)
        {
            //Отключает обработку запросов для некоторых файлов, пример: файлы с расширением .axd
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //Определение маршрута по умолчанию (при запуске приложения открывает представление "Index" (О сайте))
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "About", id = UrlParameter.Optional }
            );
        }
    }
}
