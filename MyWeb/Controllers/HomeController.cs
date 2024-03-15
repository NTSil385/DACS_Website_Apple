using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Facebook;
using MyWeb.DAO;
using MyWeb.Models;
using MyWeb.Models.Others;
using PagedList;
using System.Web.Security;
using static MyWeb.Models.Paylib;
using WebMatrix.WebData;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Data.Entity;
using System.Web.UI.WebControls;

namespace MyWeb.Controllers
{
    public class HomeController : Controller
    {
        MYWEBEntities1 db = new MYWEBEntities1();
        public ActionResult Index()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                var EventPresent = db.Events.Where(x => x.Status == true).FirstOrDefault();
                if(EventPresent == null)
                {
                    ViewBag.Status = false;
                    ViewBag.dateEnd = "Thoi gian da ket thuc";
                    return View();
                }
                else
                {
                    ViewBag.Event = EventPresent;
                    ViewBag.dateEnd = EventPresent.DateEnd;
                    return View();
                }
            }
          
        }
        public ActionResult Shopping(int? page, string currentFilter, string searchString)
        {
            var ls = new List<Product>();
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            if (!string.IsNullOrEmpty(searchString))
            {
                ls = db.Products.Where(s => s.Name.Contains(searchString)).ToList();
            }
            else
            {
                ls = db.Products.ToList();
            }
            ViewBag.CurrentFilter = searchString;

            if (page == null)
            {
                page = 1;
            }
            int pageSize = 9;

            int pageNumber = (page ?? 1);
            ls = ls.OrderByDescending(s => s.ID).ToList();
            return View(ls.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult sortPiceDscending(int? page, string currentFilter, string searchString)
        {
            var ls = new List<Product>();
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            if (!string.IsNullOrEmpty(searchString))
            {
                ls = db.Products.Where(s => s.Name.Contains(searchString)).ToList();
            }
            else
            {
                ls = db.Products.ToList();
            }
            ViewBag.CurrentFilter = searchString;

            if (page == null)
            {
                page = 1;
            }
            int pageSize = 9;

            int pageNumber = (page ?? 1);
            ls = ls.OrderByDescending(s => s.Price).ToList();
            return View(ls.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult sortPice(int? page, string currentFilter, string searchString)
        {
            var ls = new List<Product>();
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            if (!string.IsNullOrEmpty(searchString))
            {
                ls = db.Products.Where(s => s.Name.Contains(searchString)).ToList();
            }
            else
            {
                ls = db.Products.ToList();
            }
            ViewBag.CurrentFilter = searchString;

            if (page == null)
            {
                page = 1;
            }
            int pageSize = 9;

            int pageNumber = (page ?? 1);
            ls = ls.OrderBy(s => s.Price).ToList();
            return View(ls.ToPagedList(pageNumber, pageSize));
        }


        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var items = db.Products.SingleOrDefault(s => s.ID == id);
            if (items == null)
            {
                return HttpNotFound();
            }
            return View(items);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var check = db.Users.SingleOrDefault(s => s.username.ToLower() == username.ToLower()
            && s.password == password);

            if (check != null)
            {

                Session["username"] = check;

                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["error"] = "Sai tên đăng nhập hoặc mật khẩu!";
                return View();
            }

        }

        public ActionResult Logout()
        {
            Session.Remove("username");
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        public ActionResult Register()
        {
            return View(new User());
        }

        [HttpPost]
        public ActionResult Register(User user, string username, string Email)
        {
            var check = db.Users.SingleOrDefault(s => s.username.ToLower() == username.ToLower() || s.Email == Email);
            if(check != null)
            {
                TempData["error"] = "Tên đăng nhập hoặc email đã được sử dụng";
                return View();
            }
            else
            {
                if (user != null)
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "Nhập liệu Lỗi!");
                    return View(user);
                }
            }
        }

        public ActionResult Iphone(int? page)
        {
            if (page == null) page = 1;
            var items = db.Products.Where(s => s.Id_Category == 1).ToList();
            int pageSize = 12;

            int pageNumber = (page ?? 1);

            return View(items.ToPagedList(pageNumber, pageSize));

        }
        public ActionResult Imac(int? page)
        {
            if (page == null) page = 1;
            var items = db.Products.Where(s => s.Id_Category == 2).ToList();
            int pageSize = 12;

            int pageNumber = (page ?? 1);

            return View(items.ToPagedList(pageNumber, pageSize));

        }

        public ActionResult Watch(int? page)
        {
            if (page == null) page = 1;
            var items = db.Products.Where(s => s.Id_Category == 5).ToList();
            int pageSize = 12;

            int pageNumber = (page ?? 1);

            return View(items.ToPagedList(pageNumber, pageSize));

        }


        public ActionResult Ipad(int? page)
        {
            if (page == null) page = 1;
            var items = db.Products.Where(s => s.Id_Category == 6).ToList();
            int pageSize = 12;

            int pageNumber = (page ?? 1);

            return View(items.ToPagedList(pageNumber, pageSize));

        }

        public ActionResult Macbook(int? page)
        {
            if (page == null) page = 1;
            var items = db.Products.Where(s => s.Id_Category == 7).ToList();
            int pageSize = 12;

            int pageNumber = (page ?? 1);

            return View(items.ToPagedList(pageNumber, pageSize));

        }

        public ActionResult Another(int? page)
        {
            if (page == null) page = 1;
            var items = db.Products.Where(s => s.Id_Category == 8).ToList();
            int pageSize = 12;

            int pageNumber = (page ?? 1);

            return View(items.ToPagedList(pageNumber, pageSize));

        }

        public ActionResult Contact()
        {
            return View(new Contact());
        }

        [HttpPost]
        public ActionResult Contact(Contact model)
        {
            db.Contacts.Add(model);
            db.SaveChanges();
            return View("Index");
        }

        public ActionResult Blog(int? page, string currentFilter, string searchString)
        {
            var ls = new List<Post>();
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            if (!string.IsNullOrEmpty(searchString))
            {
                ls = db.Posts.Where(s => s.Title.Contains(searchString)).ToList();


            }
            else
            {
                ls = db.Posts.ToList();
            }
            ViewBag.CurrentFilter = searchString;

            if (page == null)
            {
                page = 1;
            }
            int pageSize = 5;

            int pageNumber = (page ?? 1);
            ls = ls.OrderByDescending(s => s.PostID).ToList();
            return View(ls.ToPagedList(pageNumber, pageSize));
        }

       
        
        //đăng nhập fb
        private Uri RedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url);
                uriBuilder.Query = null;
                uriBuilder.Fragment = null;
                uriBuilder.Path = Url.Action("FacebookCallBack");
                return uriBuilder.Uri;
            }
        }

        public ActionResult LoginFacebook()
        {
            var fb = new FacebookClient();
            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = ConfigurationManager.AppSettings["AppFbId"],
                client_secret = ConfigurationManager.AppSettings["FbAppSecret"],
                redirect_uri = RedirectUri.AbsoluteUri,
                response_type = "code",
                scope = "email",
            });
            return Redirect(loginUrl.AbsoluteUri);
        }
        public ActionResult FacebookCallBack(string code)
        {
            var fb = new FacebookClient();
            dynamic result = fb.Post("oauth/access_token", new
            {
                client_id = ConfigurationManager.AppSettings["AppFbId"],
                client_secret = ConfigurationManager.AppSettings["FbAppSecret"],
                redirect_uri = RedirectUri.AbsoluteUri,
                code = code
            });
            var accessToken = result.access_token;
            if (!string.IsNullOrEmpty(accessToken))
            {
                fb.AccessToken = accessToken;
                dynamic me = fb.Get("me?fields=first_name, middle_name, last_name, id, email");
                string email = me.email;
                string userName = me.email;
                string firstname = me.first_name;
                string middlename = me.middle_name;
                string lastname = me.last_name;

                var user = new User();
                user.Email = email;
                user.username = email;
                user.FullName = firstname + "" + middlename + "" + lastname;
                var resultfb = new UserDao().InsertUserFb(user);
                if(resultfb >0)
                {
                    var check = db.Users.SingleOrDefault(s => s.username.ToLower() == userName.ToLower());

                    if (check != null)
                    {

                        Session["username"] = check;

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["error"] = "Login fail!";
                        return View();
                    }
                }
                
            }
            return Redirect("/");
        }


        //ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string EmailID)
        {
            string resetCode = Guid.NewGuid().ToString();
            var verifyUrl = "/Home/ResetPassword/" + resetCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            using (var context = new MYWEBEntities1())
            {
                var getUser = (from s in context.Users where s.Email == EmailID select s).FirstOrDefault();
                if (getUser != null)
                {
                    getUser.ResetPasswordCode = resetCode;

                    //This line I have added here to avoid confirm password not match issue , as we had added a confirm password property 

                    context.Configuration.ValidateOnSaveEnabled = false;
                    context.SaveChanges();

                    var subject = "Yêu cầu thay đổi mật khẩu";
                    var body = "Chào người dùng " + getUser.FullName + ", <br/>Chúng tôi vừa nhận được yêu cầu đổi mật khẩu từ bạn, Vui lòng nhấn vào link bên dưới để tiến hành đổi mật khẩu, Nếu bạn không thực hiện thao tác này vui lòng bỏ qua email này!" +

                         " <br/><br/><a href='" + link + "'>" + link + "</a> <br/><br/>" +
                         "Nếu như đường dẫn không hoạt động, Vui lòng phản hồi lại mail này khi gặp phải sự cố, để chúng tôi có thể hỗ trợ bạn!.<br/><br/> Cảm ơn!";

                    SendEmail(getUser.Email, body, subject);

                    ViewBag.Message = "Yêu cầu đã được gửi vui lòng kiểm tra hộp thư";
                }
                else
                {
                    ViewBag.Faild = "Email không được liên kết với bất kì tài khoản!";
                    return View();
                }
            }

            return View();
        }

        private void SendEmail(string emailAddress, string body, string subject)
        {
            using (MailMessage mm = new MailMessage("tramv130597@gmail.com", emailAddress))
            {
                mm.Subject = subject;
                mm.Body = body;

                mm.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                NetworkCredential NetworkCred = new NetworkCredential("tramv130597@gmail.com", "apdvgysetqfjmtub");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = 587;
                smtp.Send(mm);

            }
        }

        public ActionResult ResetPassword(string id)
        {
            //Verify the reset password link
            //Find account associated with this link
            //redirect to reset password page
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }

            using (var context = new MYWEBEntities1())
            {
                var user = context.Users.Where(a => a.ResetPasswordCode == id).FirstOrDefault();
                if (user != null)
                {
                    ResetPasswordModel model = new ResetPasswordModel();
                    model.ResetCode = id;
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                using (var context = new MYWEBEntities1())
                {
                    var user = context.Users.Where(a => a.ResetPasswordCode == model.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        //you can encrypt password here, we are not doing it
                        user.password = model.NewPassword;
                        //make resetpasswordcode empty string now
                        user.ResetPasswordCode = "";
                        //to avoid validation issues, disable it
                        context.Configuration.ValidateOnSaveEnabled = false;
                        context.SaveChanges();
                        message = "Cập nhật thành công!";
                        ViewBag.Message = message;
                    }
                }
            }
            else
            {
                message = "Lỗi!";
            }
            ViewBag.Faild = message;
            return View(model);
        }

        public ActionResult Info(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var items = db.Users.SingleOrDefault(s => s.ID == id);
            if (items == null)
            {
                return HttpNotFound();
            }
            return View(items);
        }

        public ActionResult editInfo(int? id)
        {
            if(id == null)
            {
                return HttpNotFound();
            }
            User info = db.Users.SingleOrDefault(s => s.ID == id);
            if (info == null)
            {
                return HttpNotFound();
            }
            return View(info);
        }

        [HttpPost]
        public ActionResult editInfo(User user)
        {
            if (ModelState.IsValid == true)
            {
                try
                {
                    var edit = db.Users.SingleOrDefault(s => s.ID == user.ID);
                    edit.FullName = user.FullName;
                    edit.PhoneNumber = user.PhoneNumber;
                    edit.Address = user.Address;
                    edit.Email = user.Email;
                    edit.Birthday = user.Birthday;
                    db.SaveChanges();

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception)
                {
                    return HttpNotFound();
                }
            }
            else
            {
                ModelState.AddModelError("", "Lỗi!");
                return View(user);
            }
        }
       
        public ActionResult showAllBill()
        {
            User user = (User)Session["username"];

            var check = db.Bill_Sales.Where(s => s.Id_User == user.ID);
            check = check.OrderByDescending(s => s.ID);
            if (check == null)
            {
                return HttpNotFound();
            }
          
            return View(check);
        }

        public ActionResult DetailsBill(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            var check = db.Bill_Sales.FirstOrDefault(s => s.ID == id);
            if (check == null)
            {
                return HttpNotFound();
            }
            ViewBag.Detail = db.Info_BillSales.Where(s => s.ID_BillSale == id).ToList();

            return View(check);
        }


    }
}