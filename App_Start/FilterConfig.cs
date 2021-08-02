using System.Web;
using System.Web.Mvc;

namespace DiplomSite
{
    //Реализация фильтра, который применяется ко всем действиям во всех контроллерах
    public class FilterConfig
    {
        //Добавляет в фильтры атрибут, использующийся для обработки исключений
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
