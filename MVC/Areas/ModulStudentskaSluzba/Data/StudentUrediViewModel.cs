using MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC.Areas.ModulStudentskaSluzba.Data
{
    public class StudentUrediViewModel
    {
        public Student student { get; set; }
        public List<SelectListItem> smjeroviStavke { get; set; }
    }
}