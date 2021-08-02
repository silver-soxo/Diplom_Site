using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomSite.Models
{
    //Модель данных, которая работает с таблицой пользователей майта из БД
    public class Site_users
    {
        [Key]
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string VisitorAddConf { get; set; }
    }
}
