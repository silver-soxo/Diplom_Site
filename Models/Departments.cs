using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomSite.Models
{
    //Класс, представляющий модель данных из БД офиса, таблицы Departments
    public class Departments
    {
        //Указывает, на то, что поле является ключем
        [Key]
        public int department_id { get; set; }
        public string department_name { get; set; }
        public int department_lvl { get; set; }

        //Создание коллекции из таблицы Visitors, для реализации запросов к связанным таблицам
        public ICollection<Visitors> visitor { get; set; }

        //Создание списка
        public Departments()
        {
            visitor = new List<Visitors>();
        }

    }
}
