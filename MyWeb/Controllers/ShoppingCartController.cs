using MyWeb.Models;
using MyWeb.Models.Others;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Text;
using System.Web.UI.WebControls;
using static MyWeb.Models.Paylib;

namespace MyWeb.Controllers
{
    public class ShoppingCartController : Controller
    {
        MYWEBEntities1 db = new MYWEBEntities1();
        // GET: ShoppingCart
        public Cart GetCart()
        { 
            Cart cart = Session["Cart"] as Cart;
            if (cart == null || Session["Cart"] == null)
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }
            return cart;
        }
  
        public ActionResult ShowtoCart()
        {
            if (Session["Cart"] == null)
            {
                return RedirectToAction("ShowtoCart", "ShoppingCart");
            }

            Cart cart = Session["Cart"] as Cart;
            return View(cart);

        }
        public ActionResult AddtoCart(int id)
        {
            var pro = db.Products.SingleOrDefault(s => s.ID == id);
            if (pro != null)
            {
                GetCart().Add(pro);
            }
            return RedirectToAction("ShowtoCart", "ShoppingCart");
        }

        public ActionResult Update_quantity_Cart(FormCollection form)
        {
            Cart cart = Session["Cart"] as Cart;
            int id_pro = int.Parse(form["ID_Product"]);
            int quantity = int.Parse(form["quantity"]);
            cart.Update_quantity_Cart(id_pro, quantity);
            return RedirectToAction("ShowtoCart", "ShoppingCart");
        }
        public ActionResult Remove_Item(int id)
        {
            Cart cart = Session["Cart"] as Cart;
            cart.Remove_CartItems(id);
            return RedirectToAction("ShowtoCart", "ShoppingCart");

        }
        public PartialViewResult BagCart()
        {
            int count = 0;
            Cart cart = Session["Cart"] as Cart;
            if(cart != null)
            {
                count = cart.Count_Quantity();
            }
            ViewBag.infoCart = count;
            return PartialView("BagCart");
        }
        public ActionResult Shopping_Success()
        {
            return View();
        }

        public ActionResult Cash()
        {
            return View();
        }

        public ActionResult CheckOut(FormCollection form, string Discount)
        {
            if (ModelState.IsValid == true)
            {
                User user = (User)Session["username"];
                Cart cart = Session["Cart"] as Cart;
               
                Bill_Sales order = new Bill_Sales();
                double? sum = cart.Items.Sum(s => s._shopping_product.Price * s._shopping_quantity);
                Event ev = db.Events.Where(s => s.Event_Name == Discount.ToLower() && s.Status == true).FirstOrDefault();
                if (ev != null)
                {

                    order.Total = sum - (sum * (ev.DisscountPercent / 100));
                    order.DateFounded = DateTime.Now;
                    order.Des = form["Address"];
                    order.Id_User = user.ID;
                    user.PhoneNumber = form["PhoneNumber"];
                    order.Status = 3;

                    foreach (var items in cart.Items)
                    {

                        Info_BillSales info_order = new Info_BillSales();
                        info_order.ID_BillSale = order.ID;
                        info_order.ID_Product = items._shopping_product.ID;
                        info_order.Price = double.Parse((items._shopping_product.Price).ToString());
                        info_order.Quantity = items._shopping_quantity;
                        info_order.Total = info_order.Price * info_order.Quantity;

                        db.Info_BillSales.Add(info_order);
                    }
                    db.Bill_Sales.Add(order);
                    cart.ClearCart();


                    Promotion_Apply promotion = new Promotion_Apply();
                    promotion.ID_Event = ev.ID;
                    promotion.ID_BillSale = order.ID;
                    ev.Quantity -= 1;
                    if (ev.Quantity <= 0)
                    {
                        ev.Status = false;
                        ev.DisscountPercent = 0;
                    }
                    db.Promotion_Apply.Add(promotion);
                }
                else
                {
                    order.Total = sum;
                    order.DateFounded = DateTime.Now;
                    order.Des = form["Address"];
                    order.Id_User = user.ID;
                    user.PhoneNumber = form["PhoneNumber"];
                    order.Status = 3;

                    foreach (var items in cart.Items)
                    {

                        Info_BillSales info_order = new Info_BillSales();
                        info_order.ID_BillSale = order.ID;
                        info_order.ID_Product = items._shopping_product.ID;
                        info_order.Price = double.Parse((items._shopping_product.Price).ToString());
                        info_order.Quantity = items._shopping_quantity;
                        info_order.Total = info_order.Price * info_order.Quantity;

                        db.Info_BillSales.Add(info_order);
                    }
                    db.Bill_Sales.Add(order);
                    cart.ClearCart();
                }
                db.SaveChanges();
                return RedirectToAction("Shopping_Success", "ShoppingCart");
            }
            else
            {
                return Content("Error!");
            }
        }

        public ActionResult VNpay()
        {
            return View();
        }

        //thanh toan
        public ActionResult Payment(string Discount)
        {
            string url = ConfigurationManager.AppSettings["Url"];
            string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
            string tmnCode = ConfigurationManager.AppSettings["TmnCode"];
            string hashSecret = ConfigurationManager.AppSettings["HashSecret"];

            PayLib pay = new PayLib();
            Cart cart = Session["Cart"] as Cart;
            User user = (User)Session["username"];

            double? sum = cart.Items.Sum(s => s._shopping_product.Price * s._shopping_quantity);
            Event ev = db.Events.Where(s => s.Event_Name == Discount.ToLower() && s.Status == true).FirstOrDefault();
            double? total;
            if(ev != null)
            {
                total = sum - (sum * (ev.DisscountPercent / 100));
                Session["total"] = total;
                Session["discount"] = ev.DisscountPercent;
            }
            else
            {
                total = sum;
                Session["total"] = total;
            }
           
            pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.1.0
            pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
            pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website của merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
            pay.AddRequestData("vnp_Amount", ( total* 100).ToString()); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
            pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
            pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
            pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress()); //Địa chỉ IP của khách hàng thực hiện giao dịch
            pay.AddRequestData("vnp_Locale", "vn"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
            pay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang"); //Thông tin mô tả nội dung thanh toán
            pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
            pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString()); //mã hóa đơn

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);
            return Redirect(paymentUrl);

        }

        public ActionResult saveBillPays(FormCollection input)
        {
            Bill_Sales order = new Bill_Sales();
            User user = (User)Session["username"];
            Cart cart = Session["Cart"] as Cart;
            double? sum = cart.Items.Sum(s => s._shopping_product.Price);
            
            order.Des = input["addressConfirm"];
            user.PhoneNumber = input["PhoneNumber"];

            Event evFalse = db.Events.Where(s=> s.Status == false).FirstOrDefault();  
            if(evFalse != null) {

                order.Total = (double?)Session["total"];
                order.DateFounded = DateTime.Now;

                order.Id_User = user.ID;
                order.Status = 7;


                db.Bill_Sales.Add(order);
                foreach (var items in cart.Items)
                {
                    Info_BillSales info_order = new Info_BillSales();
                    info_order.ID_BillSale = order.ID;
                    info_order.ID_Product = items._shopping_product.ID;
                    info_order.Price = double.Parse((items._shopping_product.Price).ToString());
                    info_order.Quantity = items._shopping_quantity;
                    info_order.Total = info_order.Price * info_order.Quantity;

                    db.Info_BillSales.Add(info_order);
                }
                cart.ClearCart();

            }
            Event ev = db.Events.Where(s => s.Status == true).FirstOrDefault();
            if(ev != null)
            {
                double? check = sum - (sum * (ev.DisscountPercent / 100));

                order.Total = (double?)Session["total"];
                order.DateFounded = DateTime.Now;

                order.Id_User = user.ID;
                order.Status = 7;


                db.Bill_Sales.Add(order);
                foreach (var items in cart.Items)
                {
                    Info_BillSales info_order = new Info_BillSales();
                    info_order.ID_BillSale = order.ID;
                    info_order.ID_Product = items._shopping_product.ID;
                    info_order.Price = double.Parse((items._shopping_product.Price).ToString());
                    info_order.Quantity = items._shopping_quantity;
                    info_order.Total = info_order.Price * info_order.Quantity;

                    db.Info_BillSales.Add(info_order);
                }
                cart.ClearCart();


                if (order.Total == check)
                {
                    Promotion_Apply promotion = new Promotion_Apply();
                    promotion.ID_Event = ev.ID;
                    promotion.ID_BillSale = order.ID;
                    ev.Quantity -= 1;
                    if (ev.Quantity <= 0)
                    {
                        ev.Status = false;
                        ev.DisscountPercent = 0;
                    }
                    db.Promotion_Apply.Add(promotion);
                }


            }

            db.SaveChanges();
            return RedirectToAction("Shopping_Success");
        }

        public ActionResult PaymentConfirm()
        {
            if (Request.QueryString.Count > 0)
            {
                string hashSecret = ConfigurationManager.AppSettings["HashSecret"]; //Chuỗi bí mật
                var vnpayData = Request.QueryString;
                PayLib pay = new PayLib();

                //lấy toàn bộ dữ liệu được trả về
                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(s, vnpayData[s]);
                    }
                }

                long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef")); //mã hóa đơn
                long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo")); //mã giao dịch tại hệ thống VNPAY
                string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode"); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"]; //hash của dữ liệu trả về

                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?

                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00")
                    {
                        //Thanh toán thành công
                        
                        ViewBag.Message = "Thanh toán thành công hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId;
                        return View("CashOnline");
                    }
                    else
                    {
                        //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                        ViewBag.Message = "Quá trình thanh toán bị gián đoạn" + orderId + " | Mã giao dịch: " + vnpayTranId + " | Mã lỗi: " + vnp_ResponseCode;
                    }
                }
                else
                {
                    ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý";
                }
            }

            return View();
        }


    }
}