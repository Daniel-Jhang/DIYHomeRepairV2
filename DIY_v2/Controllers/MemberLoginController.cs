using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

using System.Web.Security;
using DIY_v2.Models;

namespace DIY_v2.Controllers
{
    public class MemberLoginController : Controller
    {
        //建立可存取 dbShoppingCarTest.mdf 資料庫的 dbShoppingCarTestEntities 類別物件db
        //課本方式可能與實際專題 資料庫連接方式有差異 但是同樣都是 建立 資料庫名Entities 類別物件db
        DIY_DBEntities db = new DIY_DBEntities();

        // GET: MyLoginTest
        public ActionResult Index()
        {
            return View();
        }
        //Get: Home/Login
        public ActionResult Login()
        {
            return View();
        }
        //Post: Home/Login
        [HttpPost]
        public ActionResult Login(string MemberAccount, string MemberPwd)
        {

            MemberPwd = MyEncrypt.HMACSHA256(MemberPwd, "PutMyScretIn");
            // 依帳密取得會員並指定給member
            var member = db.Member
                        .Where(m => m.MemberAccount == MemberAccount && m.MemberPwd == MemberPwd)
                        .FirstOrDefault();
            //若member為null，表示會員未註冊
            if (member == null)
            {
                ViewBag.Message = "帳密錯誤，登入失敗";
                return View();
            }
            else
            {
                #region 更新購物車產品數量


                var Id = db.Member.Where(x => x.MemberAccount == MemberAccount).Select(x => x.MemberID).FirstOrDefault();

                int odcount = db.Order_Detail.Where(x => x.MemberID == Id).Where(x => x.OrderStatus == "購物車").Count();
                Session["shoppingcarcoumt"] = odcount.ToString();

                #endregion
                //更新權限
                Session["Permission"] = member.Permission;
                Session["MemberName"] = member.MemberName;
            }
            //使用Session變數記錄歡迎詞
            Session["WelCome"] = member.MemberName + "歡迎光臨";
            FormsAuthentication.RedirectFromLoginPage(MemberAccount, true);

            if (MemberAccount == "applelevel0")
            {
                return RedirectToAction("BackendUI", "Backend");
            }


            return RedirectToAction("Index", "Home");
        }
        //Get:Home/Register
        public ActionResult Register()
        {
            ViewData["MemberPwdError"] = "display:none";
            return View();
        }
        //Post:Home/Register
        [HttpPost]
        public ActionResult Register(Member newMember)
        {
            Regex rgx = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{8,12}");
            // 依帳號取得會員並指定給member
            var member = db.Member
              .Where(m => m.MemberAccount == newMember.MemberAccount)
              .FirstOrDefault();

            //若模型沒有通過驗證則顯示目前的View
            if (ModelState.IsValid == false)
            {
                if (newMember.MemberPwd == null)
                {
                    ViewData["MemberPwdError"] = "display:none";
                    return View();
                }
                else if (rgx.IsMatch(newMember.MemberPwd) == true)
                {
                    ViewData["MemberPwdError"] = "display:none";
                    return View();
                }
                else
                {
                    ViewData["MemberPwdError"] = "display:block";
                    return View();
                }
            }
            else if (member == null)
            {
                //若member為null，表示會員未註冊
                if (rgx.IsMatch(newMember.MemberPwd) == true)
                {
                    ViewData["MemberPwdError"] = "display:none";
                    //將會員記錄新增到tMember資料表
                    newMember.MemberPwd = MyEncrypt.HMACSHA256(newMember.MemberPwd, "PutMyScretIn");
                    newMember.Permission = "1";
                    db.Member.Add(newMember);
                    db.SaveChanges();
                    //執行Home控制器的Login動作方法
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewData["MemberPwdError"] = "display:block";
                    return View();
                }
            }
            else
            {
                ViewBag.Message = "此帳號己有人使用，註冊失敗";
                return View();
            }
        }
    }
}