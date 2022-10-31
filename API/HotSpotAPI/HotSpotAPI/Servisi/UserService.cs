﻿using HotSpotAPI.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using HotSpotAPI.Modeli;
using HotSpotAPI.ModeliZaZahteve;

namespace HotSpotAPI.Servisi
{
    public interface IUserService
    {
        public int GetUserId();
        public string ChangePassword(string username, out bool ind);
        string chengePassInDataBase(String username, password password, out bool ind);
        public string ConfirmCode(string username, string code, out bool ind);
    }
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor httpContext;
        private MySqlDbContext context;
        private readonly ImailService mailService;
        private readonly IMySQLServis mysqlServis;

        public UserService(IHttpContextAccessor httpContext, MySqlDbContext context, ImailService mailService, IMySQLServis mysqlServis)
        {
            this.httpContext = httpContext;
            this.context = context;
            this.mailService = mailService;
            this.mysqlServis = mysqlServis;
        }

        public int GetUserId()
        {
            int rez = -1; // ako nije ulogovan vraca -1
            var pom = httpContext.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (httpContext.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) != null)
            {
                rez = int.Parse(httpContext.HttpContext.User.FindFirstValue("id"));
                var dbid = context.Korisnici.Find(rez);
                if (dbid == null)
                    rez = -1;
            }
            return rez;
        }
        public string ChangePassword(string username, out bool ind)
        {
            var user = context.Korisnici.FirstOrDefault(x => x.Username == username);
            if (user == null)
            {
                ind = false;
                return "wrong username";
            }

            string email = user.Email;
            if (email == null)
            {
                ind = false;
                return "korisnikov mail je nevalidan";
            }

            Random rnd = new Random();
            int code = rnd.Next(1000,9999);

            user.Code = code;
            context.SaveChanges();

            MailData maildata = new MailData(new List<string> { user.Email }, "Izmena lozinke");
            Task<bool> sendResult = mailService.SendAsync(maildata, new CancellationToken(), code);
            if (sendResult != null)
            {
                ind = true;
                return "Proverite vas email i pratite dalja uputstva za izmenu lozinke";
            }
            else
            {
                ind = false;
                return "greska pri slanju e-mail-a";
            }
        }

        public string ConfirmCode(string username, string code, out bool ind)
        {
            var user = context.Korisnici.FirstOrDefault(x => x.Username == username);
            if (user == null)
            {
                ind = false;
                return "Username ne postoji";
            }
            int kod=int.Parse(code);
            if(user.Code == kod)
            {
                ind = true;
                return "Kod je validan";
            }

            ind = false;
            return "Kod nije validan";
        }
        public string chengePassInDataBase(string username, password password, out bool ind)
        {
            var user = context.Korisnici.FirstOrDefault(x => x.Username == username);
            if(user == null)
            {
                ind = false;
                return "korisnik sa ovim usernameom ne postoji";
            }

            mysqlServis.CreatePasswordHash(password.newpassword, out byte[] passhash, out byte[] passsalt);
            if(passhash != null && passsalt!=null)
            {
                user.PasswordSalt = passsalt;
                user.PasswordHash = passhash;
                context.SaveChanges();
                ind = true;
                return "Uspesno izmenjena lozinka";
            }
            ind = false;
            return "Greska pri kreiranju lozinke";
        }
    }
}
