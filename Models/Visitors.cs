using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DiplomSite.Models
{
    //Класс, представляющий модель данных из БД офиса, таблицы Visitors
    public class Visitors
    {
        //Указывает, на то, что поле является ключем
        [Key]
        public int visitor_id { get; set; }
        public string visitor_name { get; set; }
        public string visitor_surname { get; set; }
        public string visitor_phone { get; set; }
        public string visitor_email { get; set; }

        //Связь с моделью паспорта
        public int? pasport_id { get; set; }
        public Pasports Pasports { get; set; }
        
        //Связь с моделью департамента
        public int? department_id { get; set; }
        public Departments Departments { get; set; }
    }
}