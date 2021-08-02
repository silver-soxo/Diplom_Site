using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DiplomSite.Models
{
    //Класс, представляющий контекст данных, связывающий модели (таблицы) БД офиса в один поток
    public class CheckingContext : DbContext
    {
        //Создание набора БД по модели Visitors из таблицы visitor (на конце названия таблици Entity Framework сам дописывает s)
        //Все таблицы с которыми вы хотите работать через Entity Framework должны оканьчиваться на s
        public DbSet<Visitors> visitor { get; set; }
        public DbSet<Users> user { get; set; }
        public DbSet<Pasports> pasport { get; set; }
        public DbSet<Departments> department { get; set; }
    }
}