using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using YoYo.Models;

namespace YoYo.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            YoYo.Models.YoyoEntities db = new Models.YoyoEntities();

            return View(db.Videos.ToList());
        }

        [HttpPost]
        public ActionResult SignUp(string username, string password, string mail, HttpPostedFileBase picture)
        {
            MembershipUser user = Membership.CreateUser(username, password, mail);

            if (picture != null)
            {
                Models.Picture p = new Models.Picture();

                p.Id = new Guid(user.ProviderUserKey.ToString());
                p.Link = Guid.NewGuid().ToString().Replace("-", "");


                string ext = Path.GetExtension(picture.FileName);

                p.Link += ext;

                picture.SaveAs(Server.MapPath("~/ProfilePictures/" + p.Link));

                Models.YoyoEntities db = new Models.YoyoEntities();
                db.Pictures.Add(p);
                db.SaveChanges();
            }

            FormsAuthentication.SetAuthCookie(username, true);
            return RedirectToAction("Index");
        }

        
        public JsonResult Login(string un, string pw)
        {
            Models.User user = null;
            if (Membership.ValidateUser(un, pw))
            {
                FormsAuthentication.SetAuthCookie(un, true);

                Models.YoyoEntities db = new Models.YoyoEntities();

                user = db.Users.Where(x => x.UserName.Equals(un,StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                HttpCookie c = new HttpCookie("userId");
                c.Expires = DateTime.Now.AddDays(60);
                c.Value = user.UserId.ToString();
                Response.Cookies.Add(c);
            }

            object data = null;

            if (user == null)
                data = new { result = false };
            else
            {
                if (user.Picture == null)
                    user.Picture = new Models.Picture { Link = "profile.jpg" };

                data = new { result = true, username = user.UserName, videoCount = user.Videos.Count, pictureLink = user.Picture.Link };
            }

            return Json(data);

            //return RedirectToAction("Index");
        }

        public JsonResult LogOut()
        {

            FormsAuthentication.SignOut();
            Session.Abandon();

            return Json(1);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Upload(string name, string description, HttpPostedFileBase video)
        {

            Models.Video v = new Models.Video();
            v.Date = DateTime.Now;
            v.Name = name;
            v.Description = description;
            v.Link = Guid.NewGuid().ToString().Replace("-", "");

            v.UserId = new Guid(Membership.GetUser().ProviderUserKey.ToString());

            string ext = Path.GetExtension(video.FileName).ToLower(); 

            video.SaveAs(Server.MapPath("~/Videos/"+v.Link+ext));

            Models.YoyoEntities db = new Models.YoyoEntities();
            db.Videos.Add(v);
            db.SaveChanges();



            Microsoft.Expression.Encoder.AudioVideoFile audiovideo = new Microsoft.Expression.Encoder.AudioVideoFile(Server.MapPath("~/Videos/" + v.Link + ext));


            TimeSpan dur = audiovideo.Duration;

            int second = 1;

            int step = dur.Seconds / 5;

            for (int i = 1; i <= 5; i++)
            {

                int sc = second;

                int h = second / 3600;
                sc = sc - h * 3600;

                int m = sc / 60;
                sc = sc - m * 60;

                int s = sc;

                Bitmap bmp = audiovideo.GetThumbnail(new TimeSpan(h,m,s), new System.Drawing.Size(800, 600));
                bmp.Save(Server.MapPath("~/VideosPictures/" + v.Link+"_"+i + ".png"), ImageFormat.Png);

                second += step;
            }


            

            return RedirectToAction("Index");
        }

        public JsonResult GetVideo(int id)
        {

            Models.YoyoEntities db = new Models.YoyoEntities();

            var video = db.Videos.Where(x => x.Id == id).FirstOrDefault();

            return Json(new { url = video.Link, header = video.Name, description = video.Description,rating=video.Ratings.Count });
        

        }

        public JsonResult VideoPlayed(int id)
        {

            string userId = "";

            if (Request.Cookies["userId"] == null)
            {

                if (!User.Identity.IsAuthenticated)
                {
                    userId = Guid.NewGuid().ToString();
                }
                else
                {
                    userId = Membership.GetUser().ProviderUserKey.ToString();
                }

                HttpCookie c = new HttpCookie("userId");
                c.Expires=DateTime.Now.AddDays(60);
                
                c.Value = userId;
                Response.Cookies.Add(c);
            }
            else{

                userId = Request.Cookies["userId"].Value;
            }


            YoyoEntities db = new YoyoEntities();

            Guid uid = new Guid(userId);

            if (db.Ratings.Where(x => x.UserId == uid & x.VideoId == id).FirstOrDefault() == null)
            {
                Rating r = new Rating();

                r.VideoId = id;
                r.UserId = uid;
                r.Date = DateTime.Now;
                db.Ratings.Add(r);
                db.SaveChanges();
            }



            return Json(1);
        
        }

        public JsonResult SendComment(string com, int videoId)
        {

            Comment c = new Comment();
            c.Text = com;
            c.VideoId = videoId;
            c.UserId = Helper.GetUser().UserId;
            c.Date = DateTime.Now;

            YoyoEntities db = new YoyoEntities();
            db.Comments.Add(c);
            db.SaveChanges();

            return Json(new { commentId = c.Id });
        }

        public JsonResult GetComments(int videoId)
        { 
            
            YoyoEntities db = new YoyoEntities();

            var comm = from c in db.Comments
                       where c.VideoId == videoId
                       select new CommentDTO
                       {
                           Id = c.Id,
                           Text = c.Text,
                           Username = c.aspnet_Users.UserName,
                           Date = c.Date,
                           Photo=c.aspnet_Users.Picture.Link
                       };

            return Json(comm.ToList());
        }

        public JsonResult GetComment(int id)
        {
            YoyoEntities db = new YoyoEntities();

            var comm = from c in db.Comments
                       where c.Id == id
                       select new CommentDTO
                       {
                           Id = c.Id,
                           Text = c.Text,
                           Username = c.aspnet_Users.UserName,
                           Date = c.Date,
                           Photo = c.aspnet_Users.Picture.Link
                       };

            return Json(new {comment=comm.FirstOrDefault() });

        }

    }
}
