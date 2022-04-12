using MVC.DAL;
using MVC.Models;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC.Areas.ModulStudentskaSluzba.Data;

namespace MVC.Areas.ModulStudentskaSluzba.Controllers
{
    public class StudentController : Controller
    {
        MyContext mc = new MyContext();

        // defaultna akcija (kada unesemo samo http://localhost:50913/Student/)
        public ActionResult Index()
        {
            return View("Index");
        }


        // pristupamo sa: http://localhost:50913/Student/Prikazi)
        public ActionResult Prikazi(int? smjerId)   // mozemo prikazati sve studente bez zadavanja smjera (null)
        {
            StudentPrikaziViewModel Model = new StudentPrikaziViewModel();

            var s = mc.Studenti
                 .Where(x => !smjerId.HasValue || x.SmjerId == smjerId)     // ako smjerId nema vrijednost (ako je null), drugi uslov se nece ni gledati, pa ce vratiti sve studente
                 .Include(x => x.Korisnik)
                 .Include(x => x.Smjer)
                 .Include(x => x.Smjer.Fakultet)
                 .AsEnumerable();

            Model.studenti = s.Select(x => new StudentPrikaziViewModel.StudenInfo()
            {
                Student = x,
                ECTSukupno = mc.SlusaPredmete.Where(y => y.UpisGodine.StudentId == x.Id && y.FinalnaOcjena > 5).Sum(z => (float?) z.Predaje.Predmet.Ects) ?? 0, // float? jer moze da ne vrati nista (npr. ECTS nije unesen)
                BrojPolozenihPredmeta = mc.SlusaPredmete.Where(y => y.UpisGodine.StudentId == x.Id && y.FinalnaOcjena > 5).Count()
            })
                 .ToList();

            Model.smjerovi = mc.Smjerovi
                .Include(x => x.Fakultet)
                .ToList();

            //ViewData["studenti"] = studenti;  // nepotrebno jer sad svi podaci idu kao Model na View (koristenjem View konstruktora)

            return View("Prikazi", Model);
        }

        public ActionResult Obrisi(int studentId)
        {
            Student s = mc.Studenti.Where(x => x.Id == studentId).Include(x => x.Korisnik).FirstOrDefault();
            mc.Studenti.Remove(s);
            mc.SaveChanges();

            return RedirectToAction("Prikazi");
        }

        //  http://localhost:50913/Student/Dodaj
        public ActionResult Dodaj()
        {
            StudentUrediViewModel Model = new StudentUrediViewModel();
            Model.student = new Student();
            Model.student.Korisnik = new Korisnik();
            Model.smjeroviStavke = mc.Smjerovi.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Fakultet.Naziv + " " + x.Naziv }).ToList();

            return View("Uredi", Model);
        }

        public ActionResult Uredi(int studentId)
        {
            StudentUrediViewModel Model = new StudentUrediViewModel();
            Model.student = mc.Studenti.Where(x => x.Id == studentId).Include(x => x.Korisnik).FirstOrDefault();      // Include() ne radi sa Find() pa moramo koristiti Where()
            Model.smjeroviStavke = mc.Smjerovi.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Fakultet.Naziv + " " + x.Naziv }).ToList();

            return View("Uredi", Model);    // Objekat Model koristimo na formi za punjenje drop down liste smjerova
        }

        public ActionResult Snimi(Student student)  // ovaj objekat student je sa forme (podaci koje mi unosimo)
        {
            Student studentDB;  // objekat preuzet iz baze, pa njegovo mijenjanje utjece na podatke u bazi

            if (student.Id == 0)
            {
                // za akciju Dodaj (novi student)
                studentDB = new Student();
                studentDB.Korisnik = new Korisnik();
                mc.Studenti.Add(studentDB);     // Mozemo prvo dodati objekat pa ga onda setovat i obrnuto - redoslijed nije bitan (objekat se nalazi u memoriji)
            }
            else
            {
                // za akciju Uredi (postojeci student)
                studentDB = mc.Studenti.Where(x => x.Id == student.Id).Include(x => x.Korisnik).FirstOrDefault();
            }

            // setovanje objekta
            studentDB.Korisnik.Ime = student.Korisnik.Ime;
            studentDB.Korisnik.Prezime = student.Korisnik.Prezime;
            studentDB.Korisnik.Username = student.Korisnik.Username;
            studentDB.Korisnik.Password = student.Korisnik.Password;
            studentDB.Korisnik.DatumRodjenja = student.Korisnik.DatumRodjenja;
            studentDB.BrojIndexa = student.BrojIndexa;
            studentDB.SmjerId = student.SmjerId;

            mc.SaveChanges();   // snima objekat u bazu

            return RedirectToAction("Prikazi");
        }





    }
}