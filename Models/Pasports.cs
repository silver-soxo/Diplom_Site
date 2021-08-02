using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DiplomSite.Models
{
    //Класс, представляющий модель данных из БД офиса, таблицы Pasports
    public class Pasports
    {
        //Указывает, на то, что поле является ключем
        [Key]
        public int pasport_id { get; set; }
        public string pasport_cod { get; set; }
        public string pasport_number { get; set; }

        //Создание коллекции из таблицы Visitors, для реализации запросов к связанным таблицам
        public ICollection<Visitors> visitor { get; set; }

        //Создание списка
        public Pasports()
        {
            visitor = new List<Visitors>();
        }
    }
}