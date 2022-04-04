using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DIY_v2.Models;
using PagedList;
using System.Web.Security;
namespace DIY_v2.Controllers
{
    public class ToolController : Controller
    {
        DIY_DBEntities db = new DIY_DBEntities();
        int pageSize = 6;
      
        #region 商品購買系統  
        public ActionResult Product(int? Maxprice, int? Minprice, string ProductCategoryID = "", string keyword = "", int page = 1)
        {
            int currentPage = page < 1 ? 1 : page;// 避免頁數跑到負數
            var Categoriesresult = db.Product_Category.Select(x => x);
            Session["isMember"] = User.Identity.Name;

            Session["nowCategoryID"] = ProductCategoryID;//這個值是為了讓我們設定價格查詢時因為沒有類別ID參數  設定一個隱藏input vale值為她
            Session["OP"] = null;//判斷是否有設定價格條件  這邊是為了PageList 頁數設定的  因為他的頁數會根據url送的參數傳回的結果來決定頁數

            var result = db.Product.Select(x => x);
            //這邊先判斷是否有給ID參數  
            if (ProductCategoryID != String.Empty)
            {
                result = db.Product.Where(x => x.ProductCategoryID == ProductCategoryID);
            }



            if (keyword != string.Empty)
            {
                result = from x in db.Product
                         where x.ProductName.Contains(keyword)
                         select x;
                Session["keyword"] = keyword;
            }
            else
            {
                Session["keyword"] = null;
            }
            //這邊先判斷是否有給價格參數  
            if (Maxprice != null && Minprice != null)
            {
                Session["OP"] = 1;
                Session["Maxprice"] = Maxprice;
                Session["Minprice"] = Minprice;
                result = result.Where(x => x.ProductPrice <= Maxprice && x.ProductPrice >= Minprice).OrderBy(x => x.ProductPrice);
            }
            //把兩個需要的資料表的資料 傳進ViewModel裡面
            Check ch = new Check()
            {
                Products = result.ToList().ToPagedList(currentPage, pageSize),
                Categories = Categoriesresult.ToList(),
            };

            //if (Session["Permission"]?.ToString()=="1")
            //{
            //    return View("Product", "Error", ch);
            //}


            return View(ch);


        }

        public ActionResult ProductDetail(string ProductID="Pt001")
        {

            var result = db.Product.Where(x => x.ProductID == ProductID).ToList();//把符合ID的產品抓出來
            var result2 = db.ProductReply.Where(x => x.ProductID == ProductID).ToList();
            Check ch = new Check()
                 {
                   Products_normal=result
                    ,replies=result2
                 };
            
            return View(ch);
        }
       // [Authorize]
        [HttpPost]
        public ActionResult ProductDetail(string ProductID = "Pt001", string replymain = "")
        {
            string MemberAccount = User.Identity.Name;//抓出使用者的帳號
            var Id = db.Member.Where(x => x.MemberAccount == MemberAccount).Select(x => x.MemberID).FirstOrDefault();
            var result = db.Product.Where(x => x.ProductID == ProductID).ToList();//把符合ID的產品抓出來
    
          
            ProductReply pr = new ProductReply()
            {
                ProductID = ProductID,
                MemberAccount= MemberAccount,
                MemberID = Id,
               Replymain=replymain,
                Initdate= System.DateTime.Now,
                Star=5
            };
            db.ProductReply.Add(pr);
            db.SaveChanges();
            var result2 = db.ProductReply.Where(x => x.ProductID == ProductID).ToList();
            Check ch = new Check()
            {
                Products_normal = result ,
                replies = result2
            };

            return View(ch);
        }
        #endregion
        #region 加入購物車
        [Authorize]
        public ActionResult AddShoppingcar(int? ProductPrice, int? Quantity = 0, string ProductID = "", string ProductName = "")
        {
            string MemberAccount = User.Identity.Name;//抓出使用者的帳號
            string NowTime = System.DateTime.Now.ToString("yyyyMMdd");//把現在的日期轉成  20220324 的形式

            var count = db.Orders.Count() + 1;//計算訂單的比數
            String fnum;
            string OrderID;
            var Id = db.Member.Where(x => x.MemberAccount == MemberAccount).Select(x => x.MemberID).FirstOrDefault();
            var Oscount = db.Orders.Where(x => x.MemberID == Id).Where(x => x.OrderStatus=="購物車").Count();
           
            //如果訂單購物車的產品都結帳了
            if (Oscount == 0)
            {
                fnum = String.Format("{0:0000}", Convert.ToInt32(count));//把後面訂單編號的格式設定  4位數  不足補0
                OrderID = NowTime + fnum;//把日期跟編號結合  變成訂單編號
                Orders od = new Orders() { OrderID = OrderID, MemberID = Id, OrderStatus = "購物車" };
                db.Orders.Add(od);//如果購物車都結帳了  就產生新訂單
                db.SaveChanges();
                if (Quantity == 0)
                {
                    //如果沒有數量參數  就是直接在外面案購物車  增加商品訂單  數量為1
                    Order_Detail ods = new Order_Detail() { OrderID = OrderID, OrderDate = System.DateTime.Now, MemberID = Id, ProductID = ProductID, ProductName = ProductName, ProductPrice = Convert.ToInt16(ProductPrice), OrderQuantity = 1, OrderStatus = "購物車" };
                    db.Order_Detail.Add(ods);

                    db.SaveChanges();
                }
                else
                {
                    //如果有數量參數  就是在產品詳細裡面案購物車 增加商品訂單  數量為設定的數量
                    Order_Detail ods = new Order_Detail() { OrderID = OrderID, OrderDate = System.DateTime.Now, MemberID = Id, ProductID = ProductID, ProductName = ProductName, ProductPrice = Convert.ToInt16(ProductPrice), OrderQuantity = Convert.ToInt16(Quantity), OrderStatus = "購物車" };
                    db.Order_Detail.Add(ods);
                    db.SaveChanges();
                }

            }
            //如果購物車的產品還沒結帳 就不產生新訂單  只增加訂單詳細(就是一個訂單裡有多個訂購商品)
            else
            {
                fnum = String.Format("{0:0000}", Convert.ToInt32(count == 1 ? 1 : count - 1));//把後面訂單編號的格式設定  4位數  不足補0
                OrderID = NowTime + fnum;//把日期跟編號結合  變成訂單編號

                var checkday = db.Orders.Where(x => x.MemberID == Id).Select(x => x.OrderID).FirstOrDefault();
                if(checkday != OrderID)//這邊把前一天如果沒結帳的購物車清單ID改成今天的
                {
                    var result=db.Orders.Where(x=>x.MemberID==Id).FirstOrDefault();
                    result.OrderID=checkday;
                    db.SaveChanges();
                }

                if (Quantity == 0)
                {//如果沒有數量參數  就是直接在外面案購物車  增加商品訂單  數量為1
                    Order_Detail ods = new Order_Detail() { OrderID = OrderID, OrderDate = System.DateTime.Now, MemberID = Id, ProductID = ProductID, ProductName = ProductName, ProductPrice = Convert.ToInt16(ProductPrice), OrderQuantity = 1, OrderStatus = "購物車" };
                    db.Order_Detail.Add(ods);
                    db.SaveChanges();
                }
                else
                { //如果有數量參數  就是在產品詳細裡面案購物車 增加商品訂單  數量為設定的數量
                    Order_Detail ods = new Order_Detail() { OrderID = OrderID, OrderDate = System.DateTime.Now, MemberID = Id, ProductID = ProductID, ProductName = ProductName, ProductPrice = Convert.ToInt16(ProductPrice), OrderQuantity = Convert.ToInt16(Quantity), OrderStatus = "購物車" };
                    db.Order_Detail.Add(ods);
                    db.SaveChanges();
                }
            }
            //購物車商品數量更新
            int odcount = db.Order_Detail.Where(x => x.MemberID == Id).Where(x=>x.OrderStatus=="購物車").Count();
            Session["shoppingcarcoumt"] = odcount.ToString();
            
            return RedirectToAction("Product", new { ProductCategoryID = ViewBag.nowCategoryID });
        }
        #endregion
        //為了把每個點購物車進入到登入頁面的人  登入後都能回到案購物車的那一頁
        public ActionResult SetClickshopcar()
        {
            Session["Clickshoppingcar"] = Request.UrlReferrer?.ToString();
           return  RedirectToAction("Login", "MemberLogin");
        }
    }
}
