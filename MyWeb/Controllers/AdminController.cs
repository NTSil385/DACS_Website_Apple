using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using MyWeb.Models;
using PagedList;
using PagedList.Mvc;
using System.Web.UI.WebControls;

namespace MyWeb.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        MYWEBEntities1 db = new MYWEBEntities1();
        // GET: Admin/Admin
        public ActionResult Index(int? page, string currentFilter, string searchString)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login");
            }
            else
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



        }

        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var check = db.Admins.SingleOrDefault(s => s.userName.ToLower() == username.ToLower()
            && s.password == password);

            if (check != null)
            {

                Session["Admin"] = check;
                return RedirectToAction("Index", "Admin");



            }
            else
            {
                TempData["error"] = "Login fail!";
                return View();
            }



        }

        public ActionResult Logout()
        {
            Session.Remove("Admin");
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
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
            var verifyUrl = "/Admin/ResetPassword/" + resetCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            using (var context = new MYWEBEntities1())
            {
                var getUser = (from s in context.Admins where s.Email == EmailID select s).FirstOrDefault();
                if (getUser != null)
                {
                    getUser.ResetPasswordCode = resetCode;

                    //This line I have added here to avoid confirm password not match issue , as we had added a confirm password property 

                    context.Configuration.ValidateOnSaveEnabled = false;
                    context.SaveChanges();

                    var subject = "Yêu cầu thay đổi mật khẩu";
                    var body = "Chào người dùng " + getUser.FullName + ", <br/>Chúng tôi vừa nhận được yêu cầu đổi mật khẩu từ bạn, Vui lòng nhấn vào link bên dưới để tiến hành đổi mật khẩu, Nếu bạn không thực hiện thao tác này vui lòng bỏ qua email này!" +

                         " <br/><br/><a href='" + link + "'>" + link + "</a> <br/><br/>" +
                         "Nếu như đường dẫn không hoạt động, Vui lòng phản hồi lại mail này khi gặp phải sự cố, để chúng tôi có thể hỗ trợ bạn!.<br/><br/> Thank you";

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
                var user = context.Admins.Where(a => a.ResetPasswordCode == id).FirstOrDefault();
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
                    var user = context.Admins.Where(a => a.ResetPasswordCode == model.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        //you can encrypt password here, we are not doing it
                        user.password = model.NewPassword;
                        //make resetpasswordcode empty string now
                        user.ResetPasswordCode = "";
                        //to avoid validation issues, disable it
                        context.Configuration.ValidateOnSaveEnabled = false;
                        context.SaveChanges();
                        message = "New password updated successfully";
                    }
                }
            }
            else
            {
                message = "Something invalid";
            }
            ViewBag.Message = message;
            return View(model);
        }



        public ActionResult Register()
        {
            return View(new Admin());
        }

        [HttpPost]
        public ActionResult Register(Admin user, string username, string Email)
        {
            var check = db.Admins.SingleOrDefault(s => s.userName.ToLower() == username.ToLower() || s.Email == Email);
            if (check != null)
            {
                TempData["error"] = "Tên đăng nhập hoặc email đã được sử dụng";
                return View();
            }
            else
            {
                if (user != null)
                {
                    db.Admins.Add(user);
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

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            Product items = db.Products.SingleOrDefault(s => s.ID == id);
            if (items == null)
            {
                return HttpNotFound();
            }
            return View(items);

        }

        [HttpPost]
        public ActionResult Edit(Product items, HttpPostedFileBase uploadFile)
        {
            if (ModelState.IsValid == true)
            {
                try
                {
                    var edit = db.Products.SingleOrDefault(s => s.ID == items.ID);
                    edit.Name = items.Name;
                    edit.Discription = items.Discription;
                    edit.Price = items.Price;

                    edit.Producer = items.Producer;

                    edit.Id_Category = items.Id_Category;


                    if (uploadFile != null && uploadFile.ContentLength > 0)
                    {
                        int id = items.ID;
                        string _FileName = "";
                        int index = uploadFile.FileName.IndexOf(".");
                        _FileName = "img" + id.ToString() + "." + uploadFile.FileName.Substring(index + 1);
                        string _path = Path.Combine(Server.MapPath("~/Upload"), _FileName);
                        uploadFile.SaveAs(_path);

                        Product uls = db.Products.FirstOrDefault(s => s.ID == id);
                        uls.Images = _FileName;

                    }

                    db.SaveChanges();

                    return RedirectToAction("Index", "Admin");
                }
                catch (Exception)
                {
                    return HttpNotFound();
                }
            }
            else
            {
                ModelState.AddModelError("", "Lỗi!");
                return View(items);
            }
        }

        public ActionResult Create()
        {
            return View(new Product());
        }

        [HttpPost]
        public ActionResult Create(Product model, HttpPostedFileBase uploadFile)
        {
            if (ModelState.IsValid == true)
            {
                db.Products.Add(model);
                db.SaveChanges();

            }
            else
            {
                ModelState.AddModelError("", "Nhập liệu Lỗi!");
                return View(model);
            }

            if (uploadFile != null && uploadFile.ContentLength > 0)
            {
                // Lấy id

                int id = int.Parse(db.Products.ToList().Last().ID.ToString());
                string _FileName = "";
                int index = uploadFile.FileName.IndexOf(".");
                _FileName = "img" + id.ToString() + "." + uploadFile.FileName.Substring(index + 1);
                string _path = Path.Combine(Server.MapPath("~/Upload"), _FileName);
                uploadFile.SaveAs(_path);

                Product uls = db.Products.FirstOrDefault(s => s.ID == id);
                uls.Images = _FileName;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            Product items = db.Products.SingleOrDefault(s => s.ID == id);
            db.Products.Remove(items);
            db.SaveChanges();
            return RedirectToAction("Index");
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

        public ActionResult Contact(int? page, string currentFilter, string searchString)
        {

            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                var ls = new List<Contact>();
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
                    ls = db.Contacts.Where(s => s.Email.Contains(searchString)).ToList();


                }
                else
                {
                    ls = db.Contacts.ToList();
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

        }
        public ActionResult DetailsContact(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var items = db.Contacts.SingleOrDefault(s => s.ID == id);
            if (items == null)
            {
                return HttpNotFound();
            }
            return View(items);
        }


        public ActionResult Post(int? page, string currentFilter, string searchString)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login");
            }
            else
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
                int pageSize = 9;

                int pageNumber = (page ?? 1);
                ls = ls.OrderByDescending(s => s.PostID).ToList();
                return View(ls.ToPagedList(pageNumber, pageSize));
            }
        }

        public ActionResult CreatePost()
        {
            return View(new Post());
        }

        [HttpPost]
        public ActionResult CreatePost(Post model, HttpPostedFileBase uploadFile)
        {
            if (ModelState.IsValid == true)
            {
                db.Posts.Add(model);
                db.SaveChanges();

            }
            else
            {
                ModelState.AddModelError("", "Nhập liệu Lỗi!");
                return View(model);
            }

            if (uploadFile != null && uploadFile.ContentLength > 0)
            {
                // Lấy id

                int id = int.Parse(db.Posts.ToList().Last().PostID.ToString());
                string _FileName = "";
                int index = uploadFile.FileName.IndexOf(".");
                _FileName = "img" + id.ToString() + "." + uploadFile.FileName.Substring(index + 1);
                string _path = Path.Combine(Server.MapPath("~/Upload"), _FileName);
                uploadFile.SaveAs(_path);

                Post uls = db.Posts.FirstOrDefault(s => s.PostID == id);
                uls.images = _FileName;
                db.SaveChanges();
            }
            return RedirectToAction("Post");
        }

        public ActionResult EditPost(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            Post items = db.Posts.SingleOrDefault(s => s.PostID == id);
            if (items == null)
            {
                return HttpNotFound();
            }
            return View(items);

        }

        [HttpPost]
        public ActionResult EditPost(Post items, HttpPostedFileBase uploadFile)
        {
            if (ModelState.IsValid == true)
            {
                var edit = db.Posts.Find(items.PostID);
                if (edit == null)
                {
                    return Content("Lỗi ở đây nè má!");
                }
                else
                {
                    edit.ID_postcategory = items.ID_postcategory;
                    edit.Title = items.Title;
                    edit.Description = items.Description;
                    edit.Author = items.Author;
                    edit.DateCreated = items.DateCreated;
                    edit.images = items.images;

                    if (uploadFile != null && uploadFile.ContentLength > 0)
                    {
                        int id = items.PostID;
                        string _FileName = "";
                        int index = uploadFile.FileName.IndexOf(".");
                        _FileName = "img" + id.ToString() + "." + uploadFile.FileName.Substring(index + 1);
                        string _path = Path.Combine(Server.MapPath("~/Upload"), _FileName);
                        uploadFile.SaveAs(_path);

                        Post uls = db.Posts.FirstOrDefault(s => s.PostID == id);
                        uls.images = _FileName;

                    }

                    db.SaveChanges();

                    return RedirectToAction("Post", "Admin");
                }

            }
            else
            {
                ModelState.AddModelError("", "Lỗi!");
                return View(items);
            }
        }

        public ActionResult DeletePost(int id)
        {
            Post items = db.Posts.SingleOrDefault(s => s.PostID == id);
            db.Posts.Remove(items);
            db.SaveChanges();
            return RedirectToAction("post");
        }


        public ActionResult List(int? page, string currentFilter, string searchString)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                var ls = new List<User>();
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
                    ls = db.Users.Where(s => s.Email.Contains(searchString)).ToList();
                }
                else
                {
                    ls = db.Users.ToList();
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



        }

        public ActionResult Update(int id)
        {
            var user = db.Users.Find(id);
            return View("Update", user);
        }

        [HttpPost]
        public ActionResult Update(User user)
        {
            var find = db.Users.Find(user.ID);

            find.username = user.username;
            find.password = user.password;
            find.PhoneNumber = user.PhoneNumber;
            find.Email = user.Email;
            find.Address = user.Address;
            find.Birthday = user.Birthday;

            db.SaveChanges();
            return RedirectToAction("List", "Admin");
        }

        public ActionResult DeleteUser(int? id)
        {
            User user = db.Users.SingleOrDefault(s => s.ID == id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("List");
        }

        public ActionResult Bill(int? page, string currentFilter, string searchString)
        {
            var ls = new List<Bill_Sales>();
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
                ls = db.Bill_Sales.Where(s => s.User.Email.Contains(searchString)).ToList();
            }
            else
            {
                ls = db.Bill_Sales.ToList();
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


        public ActionResult DeleteBill(int? id)
        {
            Bill_Sales bill = db.Bill_Sales.SingleOrDefault(s => s.ID == id);
            db.Bill_Sales.Remove(bill);
            db.SaveChanges();
            return RedirectToAction("Bill");
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

        public ActionResult EditBill(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            Bill_Sales items = db.Bill_Sales.SingleOrDefault(s => s.ID == id);
            if (items == null)
            {
                return HttpNotFound();
            }
            return View(items);
        }
        [HttpPost]
        public ActionResult EditBill(Bill_Sales model)
        {
            if (ModelState.IsValid == true)
            {
                try
                {
                    var edit = db.Bill_Sales.SingleOrDefault(s => s.ID == model.ID);
                    edit.User.PhoneNumber = model.User.PhoneNumber;
                    edit.User.Email = model.User.Email;
                    edit.Des = model.Des;
                    edit.Status = model.Status;
                    db.SaveChanges();
                    return RedirectToAction("Bill", "Admin");
                }
                catch (Exception)
                {
                    return HttpNotFound();
                }
            }
            else
            {
                ModelState.AddModelError("", "Lỗi!");
                return View(model);
            }
        }


        public ActionResult Event(int? page, string currentFilter, string searchString)
        {
            var ls = new List<Event>();
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
                ls = db.Events.Where(s => s.Event_Name.Contains(searchString)).ToList();
            }
            else
            {
                ls = db.Events.ToList();
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

        public ActionResult createEvent()
        {
            return View(new Event());
        }

        [HttpPost]
        public ActionResult createEvent(Event model)
        {
            if (ModelState.IsValid)
            {
                if (model.DateEnd <= model.DateStart)
                {
                    ViewData["ErrorDate"] = "Ngày kết thúc không hợp lệ!";
                }
                else
                {
                    model.Status = false;
                    db.Events.Add(model);
                    db.SaveChanges();
                    return RedirectToAction("Event");
                }
            }
            else
            {
                ModelState.AddModelError("", "Nhập liệu Lỗi!");
                return View(model);
            }
            return View(model);

        }

        public ActionResult editEvent(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            Event items = db.Events.SingleOrDefault(s => s.ID == id);
            if (items == null)
            {
                return HttpNotFound();
            }
            return View(items);

        }

        [HttpPost]
        public ActionResult editEvent(Event items)
        {
            if (ModelState.IsValid == true)
            {
                try
                {
                    var edit = db.Events.SingleOrDefault(s => s.ID == items.ID);
                    edit.Event_Name = items.Event_Name;
                    edit.DisscountPercent = items.DisscountPercent;
                    edit.Quantity = items.Quantity;

                    edit.DateStart = items.DateStart;

                    edit.DateEnd = items.DateEnd;
                    edit.Fomular = items.Fomular;


                    if (items.DateEnd <= items.DateStart)
                    {
                        ViewData["ErrorDate"] = "Ngày kết thúc không hợp lệ!";
                    }

                     db.SaveChanges();

                    return RedirectToAction("Event", "Admin");
                }
                catch (Exception)
                {
                    return HttpNotFound();
                }
            }
            else
            {
                ModelState.AddModelError("", "Lỗi!");
                return View(items);
            }
        }


        public ActionResult DetailsEvent(int? id) {
            if(id == null) { return HttpNotFound(); }

            var check = db.Events.SingleOrDefault(s => s.ID == id);

            if(check == null) { return HttpNotFound(); }
            return View(check);   
        }

        public ActionResult StartEvent(int? id, string strURL)
        {
            if(id == null) { return HttpNotFound(); }
            var check = db.Events.SingleOrDefault(e => e.ID == id);
            if(check != null) {
               
                    check.Status = true;
                    db.SaveChanges();
                    return Redirect(strURL);
                
            }
            return Redirect(strURL);
        }

        public ActionResult PauseEvent(int? id, string strURL)
        {
            if (id == null) { return HttpNotFound(); }
            var check = db.Events.SingleOrDefault(e => e.ID == id);
            if (check != null)
            {
                if (check.DateEnd > DateTime.Now)
                {
                    check.Status = false;
                    db.SaveChanges();
                    return Redirect(strURL);
                }
            }
            return Redirect(strURL);
        }

        public ActionResult deleteEvent(int? id)
        {
            Event ev = db.Events.SingleOrDefault(s => s.ID == id);
            db.Events.Remove(ev);
            db.SaveChanges();
            return RedirectToAction("Event", "Admin");
        }


        public ActionResult Producer(int? page, string currentFilter, string searchString)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                var ls = new List<Producer>();
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
                    ls = db.Producers.Where(s => s.Name.Contains(searchString)).ToList();
                }
                else
                {
                    ls = db.Producers.ToList();
                }

                ViewBag.CurrentFilter = searchString;

                if (page == null)
                {
                    page = 1;
                }
                int pageSize = 9;

                int pageNumber = (page ?? 1);
                ls = ls.OrderByDescending(s => s.Id).ToList();
                return View(ls.ToPagedList(pageNumber, pageSize));
            }



        }

        public ActionResult createProducer()
        {
            return View(new Producer());
        }

        [HttpPost]
        public ActionResult createProducer(Producer model)
        {
            if (ModelState.IsValid)
            {
                    db.Producers.Add(model);
                    db.SaveChanges();
                    return RedirectToAction("Producer");
                
            }
            else
            {
                ModelState.AddModelError("", "Nhập liệu Lỗi!");
                return View(model);
            }

        }


        public ActionResult Import(int? page)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                if (page == null) page = 1;
                var items = db.Imports.OrderByDescending(s => s.ID).ToList(); ;
                int pageSize = 12;

                int pageNumber = (page ?? 1);

                return View(items.ToPagedList(pageNumber, pageSize));

            }
        }

        public ActionResult DraftImport(int? page)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                if (page == null) page = 1;
                var check = db.Imports.Where(s => s.Status == false).ToList();
                var items = check.OrderByDescending(s => s.ID).ToList(); ;
                int pageSize = 12;

                int pageNumber = (page ?? 1);

                return View(items.ToPagedList(pageNumber, pageSize));

            }
        }
        public ActionResult createImport()
        {
            ViewBag.IM = db.Producers.ToList();
            return View();
        }

        [HttpPost]
        public ActionResult createImport(Import model)
        {
            if (ModelState.IsValid)
            {
                Admin admin = (Admin)Session["Admin"];
                model.DateImprt = DateTime.Now;
                model.Status = false;
                model.IdAdmin = admin.ID;
                model.Total = 0;
                db.Imports.Add(model);
                db.SaveChanges();
                return RedirectToAction("Import");
            }
            else
            {
                ModelState.AddModelError("", "Nhập liệu Lỗi!");
                return View(model);
            }
        }
        public ActionResult ImportDetail(int? page, int id)
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                if (page == null) page = 1;
                var items = db.Imprt_Details.Where(x => x.IdImport == id).OrderByDescending(s => s.IdProduct).ToList(); ;
                int pageSize = 12;

                int pageNumber = (page ?? 1);

                return View(items.ToPagedList(pageNumber, pageSize));

            }
        }
        public ActionResult createImportDetail(int id)
        {
            var detail = new Imprt_Details { IdImport = id };
            ViewBag.IM = db.Products.ToList();
            return View(detail);
        }

        public ActionResult createImportDetail()
        {
            ViewBag.IM = db.Products.ToList();
            return View();
        }
    }
}
