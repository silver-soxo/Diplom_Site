using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomSite.Models
{
    //Модель данных, которая работает с отправкой заявки на посещение пользователем
    public class VisitorAdd
    {
        [Required]
        [Display(Name = "Почта")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Фамилия")]
        public string Surname { get; set; }

        [Required]
        [Display(Name = "Серия паспорта")]
        public string Pasport_cod { get; set; }

        [Required]
        [Display(Name = "Номер паспорта")]
        public string Pasport_number { get; set; }

        [Required]
        [Display(Name = "Телефон")]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "Департамент")]
        public string Department_name { get; set; }
    }
}
