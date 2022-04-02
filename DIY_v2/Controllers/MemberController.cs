using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DIY_v2.Models;

namespace DIY_v2.Controllers
{
    [Authorize]  //指定Member控制器所有的動作方法必須通過授權才能執行。
    public class MemberController : Controller
    {
        //建立DIY_DBEntities 類別物件db
        DIY_DBEntities db = new DIY_DBEntities();

        // GET: BasicData
        public ActionResult BasicData()
        {
            string MemberAccount = User.Identity.Name;
            //找出未成為訂單明細的資料，即購物車內容
            var MemberID = db.Member.Where(x => x.MemberAccount == MemberAccount).ToList();

            return View(MemberID);
        }
        // GET: EditData
        public ActionResult EditData()
        {
            string MemberAccount = User.Identity.Name;
            //找出未成為訂單明細的資料，即購物車內容
            var Member = db.Member.Where(x => x.MemberAccount == MemberAccount).FirstOrDefault();

            return View(Member);
        }

        [HttpPost]
        public ActionResult EditData(Member member)
        {
            string MemberAccount = User.Identity.Name;
            var MemberItem = db.Member.Where(x => x.MemberAccount == MemberAccount).FirstOrDefault();
            MemberItem.MemberName = member.MemberName;
            MemberItem.MemberGender = member.MemberGender;
            MemberItem.MemberBirthday = member.MemberBirthday;
            MemberItem.MemberNickname = member.MemberNickname;
            MemberItem.MemberEmail = member.MemberEmail;
            MemberItem.MemberAddress = member.MemberAddress;
            MemberItem.MemberPhone = member.MemberPhone;
            db.SaveChanges();

            Session["memberEdit"] = "資料修改完成";
            return RedirectToAction("BasicData");
        }

        public ActionResult PasswordChange()
        {
            ViewData["olderPwdError"] = "display:none";
            ViewData["newPwdError"] = "display:none";
            ViewData["pwdCheckError"] = "display:none";
            return View();
        }
        [HttpPost]
        public ActionResult PasswordChange(string olderPwd, string newPwd, string pwdCheck)
        {


            // 依帳密取得會員並指定給member
            ViewData["newPwdError"] = "display:none";
            ViewData["pwdCheckError"] = "display:none";

            Regex rgx = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{8,12}");
            //若模型沒有通過驗證則顯示目前的View
            if (rgx.IsMatch(olderPwd) == true)
            {
                ViewData["olderPwdError"] = "display:none";
                if (rgx.IsMatch(newPwd) == true && rgx.IsMatch(pwdCheck) == true)
                {

                    olderPwd = MyEncrypt.HMACSHA256(olderPwd, "PutMyScretIn");
                    string MemberAccount = User.Identity.Name;
                    var member = db.Member
                      .Where(m => m.MemberAccount == MemberAccount && m.MemberPwd == olderPwd)
                      .FirstOrDefault();
                    if (member == null)
                    {
                        ViewBag.Message = "密碼錯誤，修改失敗";
                        return View();
                    }
                    else if (member != null && newPwd == pwdCheck)
                    {
                        newPwd = MyEncrypt.HMACSHA256(newPwd, "PutMyScretIn");
                        member.MemberPwd = newPwd;
                        db.SaveChanges();
                        ViewData["newPwdError"] = "display:none";
                        ViewData["pwdCheckError"] = "display:none";
                        ViewBag.Message = "密碼已修改";
                        return View();
                    }
                    else
                    {
                        ViewData["newPwdError"] = "display:none";
                        ViewData["pwdCheckError"] = "display:none";
                        ViewBag.Message = "新密碼不同，修改失敗";
                        return View();
                    }
                }
                else if (rgx.IsMatch(newPwd) == false && rgx.IsMatch(pwdCheck) == false)
                {
                    ViewData["newPwdError"] = "display:block";
                    return View();
                }
                else if (rgx.IsMatch(pwdCheck) == false)
                {
                    ViewData["pwdCheckError"] = "display:block";
                    return View();
                }
            }

            ViewData["olderPwdError"] = "display:block";
            return View();
        }

        // GET: Member/Index
        public ActionResult Index()
        {
            //取得所有產品放入products
            var products = db.Product.OrderByDescending(m => m.ProductID).ToList();
            //Index.cshtml檢視套用_LayoutMember版面配置頁，同時使用products模型
            return View("../Member/Index", products);
        }

        //Get:Member/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();   // 登出
            Session["Permission"] = null;
            Session["shoppingcarcoumt"] = 0;
            return RedirectToAction("Index", "Home");
        }

        //Get:Member/ShoppingCar
        public ActionResult ShoppingCar()
        {
            //UsersSearchModel searchVM = new UsersSearchModel();
            ////使用者群組checkboxlist	
            //searchVM.AvailableGroups = GetCheckboxListItems();


            //取得登入會員的帳號並指定給fUserId
            string MemberAccount = User.Identity.Name;
            //找出未成為訂單明細的資料，即購物車內容
            var MemberID = db.Member.Where(x => x.MemberAccount == MemberAccount).Select(x => x.MemberID).FirstOrDefault();
            //var orderDetails = (from O in db.Order_Detail
            //                    where O.MemberID == MemberID && O.OrderStatus == "購物車"
            //              select O).ToList();
            var orderDetails = db.Order_Detail.Where(x => x.MemberID == MemberID && x.OrderStatus == "購物車");
            //View使用orderDetails模型

            return View(orderDetails);
        }

        //*******/
        //Get:Member/DeleteCar
        public ActionResult DeleteCar(int SerialNumber)
        {

            // 依fId找出要刪除購物車狀態的產品
            var orderDetail = db.Order_Detail.Where
                (m => m.SerialNumber == SerialNumber).FirstOrDefault();
            //刪除購物車狀態的產品
            db.Order_Detail.Remove(orderDetail);
            db.SaveChanges();
            #region 更新購物車產品數量

            string MemberAccount = User.Identity.Name;//抓出使用者的帳號
            var Id = db.Member.Where(x => x.MemberAccount == MemberAccount).Select(x => x.MemberID).FirstOrDefault();

            int odcount = db.Order_Detail.Where(x => x.MemberID == Id).Where(x => x.OrderStatus == "購物車").Count();
            Session["shoppingcarcoumt"] = odcount.ToString();

            #endregion
            return RedirectToAction("ShoppingCar");
        }

        public ActionResult DeleteOrder(string OrderID)
        {
            // 依fId找出要刪除購物車狀態的產品

            var order = db.Orders.Where
                (m => m.OrderID == OrderID).FirstOrDefault();
            var orderDetail = db.Order_Detail.Where
                (m => m.OrderID == OrderID);
            //刪除購物車狀態的產品
            order.OrderStatus = "取消訂單";
            foreach (var item in orderDetail)
            {
                item.OrderStatus = "取消訂單";
            }
            db.SaveChanges();

            #region 更新購物車產品數量

            string MemberAccount = User.Identity.Name;//抓出使用者的帳號
            var Id = db.Member.Where(x => x.MemberAccount == MemberAccount).Select(x => x.MemberID).FirstOrDefault();

            int odcount = db.Order_Detail.Where(x => x.MemberID == Id).Where(x => x.OrderStatus == "購物車").Count();
            Session["shoppingcarcoumt"] = odcount.ToString();

            #endregion
            return RedirectToAction("Myorder");

        }




        //Post:Member/ShoppingCar
        [HttpPost]
        public ActionResult ShoppingCar(string RecipientName, string RecipientEmail, string RecipientAddress, string RecipientPhone, string paymethod)
        {
            //找出會員帳號並指定給fUserId
            string MemberAccount = User.Identity.Name;
            var MemberID = db.Member.Where(x => x.MemberAccount == MemberAccount).Select(x => x.MemberID).FirstOrDefault();

            //形成一對多的關係，即一筆訂單資料會對應到多筆訂單明細

            //var Orders = (from O in db.Orders
            //               join M in db.Member on O.MemberID equals M.MemberID
            //               where M.MemberAccount == MemberAccount && O.OrderStatus == "購物車"
            //               select O).ToList();
            //找出目前會員在訂單明細中是購物車狀態的產品
            var carList = db.Order_Detail.Where(x => x.MemberID == MemberID && x.OrderStatus == "購物車");
            //找出目前會員在訂單中是購物車狀態的訂單
            var orders = db.Orders.Where(x => x.MemberID == MemberID && x.OrderStatus == "購物車");

            //將購物車狀態產品的fIsApproved設為"是"，表示確認訂購產品
            foreach (var item in orders)
            {
                item.RecipientName = RecipientName;
                item.RecipientEmail = RecipientEmail;
                item.RecipientAddress = RecipientAddress;
                item.RecipientPhone = RecipientPhone;
                item.Paymethod = paymethod;
                item.OrderStatus = "是";

            }
            foreach (var item in carList)
            {
                item.OrderStatus = "是";
            }
            //更新資料庫，異動tOrder和tOrderDetail
            //完成訂單主檔和訂單明細的更新
            db.SaveChanges();
            #region 更新購物車產品數量


            var Id = db.Member.Where(x => x.MemberAccount == MemberAccount).Select(x => x.MemberID).FirstOrDefault();

            int odcount = db.Order_Detail.Where(x => x.MemberID == Id).Where(x => x.OrderStatus == "購物車").Count();
            Session["shoppingcarcoumt"] = odcount.ToString();

            #endregion
            return RedirectToAction("Myorder");
        }


        //Get:Member/OrderList
        public ActionResult Myorder()
        {
            //找出會員帳號並指定給fUserId
            string MemberAccount = User.Identity.Name;
            var MemberID = db.Member.Where(x => x.MemberAccount == MemberAccount).Select(x => x.MemberID).FirstOrDefault();
            var orders = (from O in db.Orders
                          where O.MemberID == MemberID
                          select O).ToList();
            //找出目前會員的所有訂單主檔記錄並依照fDate進行遞增排序
            //將查詢結果指定給orders
            //目前會員的訂單主檔OrderList.cshtml檢視使用orders模型
            return View(orders);
        }


        //Get:Member/OrderDetail
        public ActionResult OrderDetail(string OrderID)
        {
            //根據fOrderGuid找出和訂單主檔關聯的訂單明細，並指定給orderDetails
            var orderDetails = db.Order_Detail
                .Where(m => m.OrderID == OrderID).ToList();
            //目前訂單明細的OrderDetail.cshtml檢視使用orderDetails模型
            return View(orderDetails);
        }

    }
}