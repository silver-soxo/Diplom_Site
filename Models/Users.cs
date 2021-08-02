using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DiplomSite.Models
{
    //Класс, представляющий модель данных из БД офиса, таблицы Users
    public class Users
    {
        //Указывает, на то, что поле является ключем
        [Key]
        public int user_id { get; set; }
        public string user_name { get; set; }
        public string user_surname { get; set; }
        public string user_login { get; set; }
        public string user_password { get; set; }
        public string user_lvl { get; set; }
    }
}