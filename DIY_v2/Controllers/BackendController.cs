using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DIY_v2.Models;
using System.IO;
using PagedList;
namespace DIY_v2.Controllers
{
    //[Authorize(Users ="applelevel0")]
    public class BackendController : Controller
    {

        DIY_DBEntities db = new DIY_DBEntities();
        int pageSize = 10;
        // GET: Backend
        public ActionResult BackendUI()
        {
            return View();
        }

        #region 商品管理系統
        //商品清單
        public ActionResult ManageProduct(string keyword = "", int page = 1)//商品清單
        {
            var result = db.Product.Select(x => x).ToList();
            if (keyword != string.Empty)
            {
                Session["backkeyword"] = keyword;
                result = (from x in db.Product
                          where x.ProductName.Contains(keyword)
                          select x).ToList();
            }
            else
            {
                result = db.Product.ToList();
            }

            int currentPage = page < 1 ? 1 : page;// 避免頁數跑到負數
            return View(result.ToPagedList(currentPage, pageSize));
        }

        public ActionResult AddProduct()
        {
            var result = from x in db.Product_Category
                         select new SelectListItem { Text = x.ProductCategory, Value = x.ProductCategoryID };
            Session["ct"] = result;//把產品類別存到Session 裡面  等等要用
            return View();
        }
        [HttpPost]
        //增加商品
        public ActionResult AddProduct(Product pt, HttpPostedFileBase photo)
        {
            if (ModelState.IsValid)
            {
                if (photo != null)
                {
                    if (photo.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(photo.FileName);//取得檔名
                        var path = Path.Combine(Server.MapPath("~/Images/ToolImages"), fileName);//把檔名跟網站存圖片的路徑結合
                        photo.SaveAs(path);//存圖片到網站資料夾
                        string pimg = "../Images/ToolImages/" + fileName;//取得圖片路徑
                        pt.ProductImage = pimg;//把產品圖片路徑更新
                        db.SaveChanges();
                    }
                }
                db.Product.Add(pt);
                db.SaveChanges();
                return RedirectToAction("ManageProduct");
            }
            return View();

        }
        //編輯商品資訊
        public ActionResult EditProduct(string ProductID = "Pt001")
        {
            var result = db.Product.Where(x => x.ProductID == ProductID).FirstOrDefault();
            var result2 = from x in db.Product_Category
                          select new SelectListItem { Text = x.ProductCategory, Value = x.ProductCategoryID };//這邊是為了下拉式選單  他需要IEnumerable(SelectListItem)類型的資料  Text 是選單上顯示的值
            Session["choosect"] = result2;
            return View(result);
        }
        [HttpPost]
        public ActionResult EditProduct(Product pt, HttpPostedFileBase photo)
        {
            var result = db.Product.Where(x => x.ProductID == pt.ProductID).FirstOrDefault();
            //更新產品資訊


            if (photo != null)
            {
                if (photo.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(photo.FileName);//取得檔名
                    var path = Path.Combine(Server.MapPath("~/Images/ToolImages"), fileName);//把檔名跟網站存圖片的路徑結合
                    photo.SaveAs(path);//存圖片到網站資料夾
                    string pimg = "../Images/ToolImages/" + fileName;//取得圖片路徑
                    result.ProductImage = pimg;//把產品圖片路徑更新
                    db.SaveChanges();
                }
            }
            else
            {
                // 20220329 修改  需要修改圖片路徑
                var result3 = db.Product.Where(x => x.ProductID == pt.ProductID).Select(x => x.ProductImage).FirstOrDefault();
                result.ProductImage = result3;
                db.SaveChanges();
            }
            result.ProductID = pt.ProductID;
            result.ProductName = pt.ProductName;
            result.ProductPrice = pt.ProductPrice;
            result.ProductCategoryID = pt.ProductCategoryID;
            result.InStock = pt.InStock;
            result.ProductCost = pt.ProductCost;
            result.ProductDescription = pt.ProductDescription;
            db.SaveChanges();

            return RedirectToAction("ManageProduct");
        }
        //刪除商品
        public ActionResult DeleteProduct(string ProductID)
        {
            var result = db.Product.Where(x => x.ProductID == ProductID).FirstOrDefault();//抓到符合ID的產品
            db.Product.Remove(result);
            db.SaveChanges();
            return RedirectToAction("ManageProduct");
        }
        #endregion

        #region 師傅管理系統
        // GET: TaskerIndex
        public ActionResult TaskerIndex(string pickTasker, int page = 1)
        {
            int currentPage = page < 1 ? 1 : page; // 避免頁數跑到負數
            var taskers = (from t in db.Tasker
                           join ts in db.TaskerService on t.TaskerID equals ts.TaskerID
                           orderby t.TaskerID
                           select t).Distinct().ToList();

            if (!string.IsNullOrWhiteSpace(pickTasker))
            {
                taskers = taskers.Where(x => x.TaskerName.Contains(pickTasker)).ToList();
            }

            var result = taskers.ToPagedList(currentPage, 10);
            return View(result);
        }

        // GET: EditTasker
        public ActionResult EditTasker(int id)
        {
            ServiceAreaList cityList = new ServiceAreaList();
            Session["cityList"] = cityList.serviceAreaList;
            //下面三個Session 是為了進入編輯頁面時  因為action吃不到 ServerID 參數  所以用他存資料帶進View裡面  為了讓checkbox 能在編輯時依照他原本的服務 打勾起來
            Session["server1"] = db.TaskerService.Where(x => x.TaskerID == id).Where(x => x.ServiceCategoryID == "1").Count() > 0 ? "T" : "F";
            Session["server2"] = db.TaskerService.Where(x => x.TaskerID == id).Where(x => x.ServiceCategoryID == "2").Count() > 0 ? "T" : "F";
            Session["server3"] = db.TaskerService.Where(x => x.TaskerID == id).Where(x => x.ServiceCategoryID == "3").Count() > 0 ? "T" : "F";
            var taskerToEdit = db.Tasker.Where(x => x.TaskerID == id).FirstOrDefault();
            return View(taskerToEdit);
        }

        // POST: EditTasker
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTasker(Tasker tasker, HttpPostedFileBase taskerPhoto, HttpPostedFileBase[] casePhotos, bool? serviceCategoryChk1, bool? serviceCategoryChk2, bool? serviceCategoryChk3, bool? test000)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    #region 上傳圖片
                    string taskerFileName = "";// 圖檔名稱
                    if (taskerPhoto != null)
                    {
                        if (taskerPhoto.ContentLength > 0)
                        {
                            // 取得圖檔名稱
                            taskerFileName = Path.GetFileName(taskerPhoto.FileName);
                            var path = Path.Combine(Server.MapPath("~/Images/TaskerImages"), taskerFileName);
                            taskerPhoto.SaveAs(path);
                            tasker.TaskerImage = taskerFileName;
                        }
                    }
                    else
                    {
                        tasker.TaskerImage = db.Tasker.Where(x => x.TaskerID == tasker.TaskerID).Select(x => x.TaskerImage).FirstOrDefault();
                    }

                    string caseFileName = ""; // 圖檔名稱
                    string strCaseImage = "";
                    if (casePhotos != null)
                    {
                        // 使用 for 迴圈取得所有上傳的檔案
                        for (int i = 0; i < casePhotos.Length; i++)
                        {
                            // 取得目前檔案上傳的 HttpPostedFileBase 物件
                            // 即須引數的 photos[i] 可以取得第 i 個所上傳的檔案
                            HttpPostedFileBase f = (HttpPostedFileBase)casePhotos[i];
                            // 若目前檔案上傳的 HttpPostedFileBase 物件的檔案名稱不為空白
                            // 即表示第 i 個 f 物件有指定上傳檔案
                            if (f != null)// 判斷檔案是否不為null
                            {
                                // 取得圖檔名稱
                                caseFileName = Path.GetFileName(f.FileName);
                                strCaseImage += caseFileName + ",";
                                // 將檔案儲存道專案的 Images 資料夾下
                                var path = Path.Combine(Server.MapPath("~/Images/TaskerImages"), caseFileName);
                                f.SaveAs(path);
                            }
                            else
                            {
                                if (i == casePhotos.Length - 1)
                                {
                                    tasker.CaseImage = db.Tasker.Where(x => x.TaskerID == tasker.TaskerID).Select(x => x.CaseImage).FirstOrDefault();
                                }
                            }

                        }
                        //如果三個casephoto都沒傳資料  把之前的值傳到caseImage裡面  不然會是空白
                        if (casePhotos[0] == null && casePhotos[1] == null && casePhotos[2] == null)
                        {
                            tasker.CaseImage = db.Tasker.Where(x => x.TaskerID == tasker.TaskerID).Select(x => x.CaseImage).FirstOrDefault();
                        }
                        else
                        {
                            //把結尾逗號去掉  因為之前都是存   檔名,檔名,  最後會多一個,
                            strCaseImage = strCaseImage.TrimEnd(',');
                            tasker.CaseImage = strCaseImage;
                        }
                    }

                    #endregion
                    var temp = db.Tasker.Where(x => x.TaskerID == tasker.TaskerID).FirstOrDefault();
                    temp.TaskerName = tasker.TaskerName;
                    temp.ServiceQuote = tasker.ServiceQuote;
                    temp.TaskerTel = tasker.TaskerTel;
                    temp.TaskerPhone = tasker.TaskerPhone;
                    temp.ServiceTime_Start = tasker.ServiceTime_Start;
                    temp.ServiceTime_End = tasker.ServiceTime_End;
                    temp.ServiceArea = tasker.ServiceArea;
                    temp.TaskerImage = tasker.TaskerImage;
                    temp.License = tasker.License;
                    temp.Feature = tasker.Feature;
                    temp.TaskerDescription = tasker.TaskerDescription;
                    temp.CaseImage = tasker.CaseImage;
                    temp.Rate = tasker.Rate;

                    var temp1 = db.TaskerService.Where(x => x.TaskerID == tasker.TaskerID).Where(x => x.ServiceCategoryID == "1").FirstOrDefault();
                    var temp2 = db.TaskerService.Where(x => x.TaskerID == tasker.TaskerID).Where(x => x.ServiceCategoryID == "2").FirstOrDefault();
                    var temp3 = db.TaskerService.Where(x => x.TaskerID == tasker.TaskerID).Where(x => x.ServiceCategoryID == "3").FirstOrDefault();
                    //把三個服務先弄出來  如果有 要加服務時能用
                    TaskerService ts1 = new TaskerService() { TaskerID = tasker.TaskerID, ServiceCategoryID = "1", ServiceCategory = "衛浴裝修" };
                    TaskerService ts2 = new TaskerService() { TaskerID = tasker.TaskerID, ServiceCategoryID = "2", ServiceCategory = "抓漏/堵塞" };
                    TaskerService ts3 = new TaskerService() { TaskerID = tasker.TaskerID, ServiceCategoryID = "3", ServiceCategory = "水電安裝/修繕" };

                    #region 服務一判斷
                    if (temp1 != null)
                    {
                        if (serviceCategoryChk1 != true)
                        {
                            db.TaskerService.Remove(temp1);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        if (serviceCategoryChk1 == true)
                        {
                            db.TaskerService.Add(ts1);
                            db.SaveChanges();
                        }
                    }
                    #endregion
                    #region 服務二判斷
                    if (temp2 != null)
                    {
                        if (serviceCategoryChk2 != true)
                        {
                            db.TaskerService.Remove(temp2);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        if (serviceCategoryChk2 == true)
                        {
                            db.TaskerService.Add(ts2);
                            db.SaveChanges();
                        }
                    }
                    #endregion
                    #region 服務三判斷
                    if (temp3 != null)
                    {
                        if (serviceCategoryChk3 != true)
                        {
                            db.TaskerService.Remove(temp3);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        if (serviceCategoryChk3 == true)
                        {
                            db.TaskerService.Add(ts3);
                            db.SaveChanges();
                        }
                    }
                    #endregion

                    db.SaveChanges();
                }
            }
            catch (DbEntityValidationException ex)
            {
                var entityError = ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage);
                var getFullMessage = string.Join("; ", entityError);
                var exceptionMessage = string.Concat(ex.Message, "errors are: ", getFullMessage);
                throw;
            }

            return RedirectToAction("TaskerIndex");
        }


        public ActionResult DeleteTasker(int id)
        {
            // 先刪除TaskerService資料表裡的資料
            // 師傅可能會有複數個服務類型
            int count = db.TaskerService.Where(x => x.TaskerID == id).Count(); // 所以先算出要刪除的師傅，其服務類型的個數
            for (int i = 0; i < count; i++) // 用for loop 把TaskerService資料表裡，該師傅的資料先刪掉
            {
                var taskerService = db.TaskerService.Where(x => x.TaskerID == id).FirstOrDefault();
                db.TaskerService.Remove(taskerService);
                db.SaveChanges();
            }

            // 最後再刪除Tasker資料表裡，該師傅的資料，就能避免 Foreign Key reference 的問題 
            var tasker = db.Tasker.Where(x => x.TaskerID == id).FirstOrDefault();
            db.Tasker.Remove(tasker);
            db.SaveChanges();

            return RedirectToAction("TaskerIndex");
        }
        #endregion

        #region 會員管理系統
        public ActionResult ManageMember(string pickMember, int page = 1)   /*撈資料庫*/
        {
            int currentPage = page < 1 ? 1 : page; // 避免頁數跑到負數
            var members = db.Member.ToList();
            if (!string.IsNullOrWhiteSpace(pickMember))
            {
                members = members.Where(x => x.MemberAccount.Contains(pickMember)).ToList();
            }

            var result = members.ToPagedList(currentPage, 10);

            return View(result);
        }
        public ActionResult DeleteMember(int MemberID = 1)
        {
            //取得帳號ID跟參數一樣的訂單明細  把她刪除
            int Odcount = db.Order_Detail.Where(x => x.MemberID == MemberID).Count();
            int oscount = db.Orders.Where(x => x.MemberID == MemberID).Count();

            var deleteOrder_Detail = db.Order_Detail.Where(x => x.MemberID == MemberID).FirstOrDefault();
            var deleteMemberOrders = db.Orders.Where(x => x.MemberID == MemberID).FirstOrDefault();
            var deleteMember = db.Member.Where(x => x.MemberID == MemberID).FirstOrDefault();
            for (int i = 0; i < Odcount; i++)
            {
                deleteOrder_Detail = db.Order_Detail.Where(x => x.MemberID == MemberID).FirstOrDefault();
                db.Order_Detail.Remove(deleteOrder_Detail);
                db.SaveChanges();
            };
            for (int i = 0; i < oscount; i++)
            {
                deleteMemberOrders = db.Orders.Where(x => x.MemberID == MemberID).FirstOrDefault();
                db.Orders.Remove(deleteMemberOrders);
                db.SaveChanges();
            };
            db.Member.Remove(deleteMember);
            db.SaveChanges();
            //while (result != null)
            //{
            //    db.Order_Detail.Remove(result);
            //    result = db.Order_Detail.Where(x => x.MemberID == MemberID).FirstOrDefault();
            //}

            return RedirectToAction("ManageMember");
        }
        #endregion
        #region 商品留言管理
        public ActionResult ManageReply(string ProductID)
        {
            var result = db.ProductReply.Where(x => x.ProductID == ProductID).ToList();
            return View(result);
        }
        public ActionResult deleteReply(int ReplyID, string ProductID)

        {

           var result= db.ProductReply.Where(x=>x.ReplyID==ReplyID).FirstOrDefault();
            db.ProductReply.Remove(result);
            db.SaveChanges();
            return RedirectToAction("ManageReply");
        }
        #endregion
        #region 會員權限管理
        public ActionResult MemberDetail(int MemberID)
        {
            var result = db.Member.Where(x => x.MemberID == MemberID).FirstOrDefault();
            return View(result);
        }
        public ActionResult StopMember(int MemberID)
        {
            var result = db.Member.Where(x => x.MemberID == MemberID).FirstOrDefault();
            result.Permission = "N";
            db.SaveChanges();

            return RedirectToAction("ManageMember");

        }
        public ActionResult StarMember(int MemberID)
        {
            var result = db.Member.Where(x => x.MemberID == MemberID).FirstOrDefault();
            result.Permission = "1";
            db.SaveChanges();

            return RedirectToAction("ManageMember");
        }
        #endregion
        #region 訂單狀態管理
        public ActionResult ManageOrder()
        {
            var result = db.Orders.Where(x=>x.OrderStatus!="購物車").ToList();
            return View(result);
        }
        public ActionResult ManageOrderDetail(string OrderID)
        {
            Session["ordercount"] = 0;
            var result = db.Order_Detail.Where(x => x.OrderID == OrderID).ToList();
            CVMManageOrder Mo = new CVMManageOrder()
            {
                od = result
            };
            return View(Mo);
        }
        [HttpPost]
        public ActionResult ChangeOrder(string OrderID,string OrderStatus)
        {

            var result = db.Order_Detail.Where(x => x.OrderID == OrderID).ToList();
         foreach(var item in result)
            {
                item.OrderStatus = OrderStatus;
            }
            db.SaveChanges();
            var result2=db.Orders.Where(x=>x.OrderID == OrderID).FirstOrDefault();
            result2.OrderStatus = OrderStatus;
            db.SaveChanges();
            
            return RedirectToAction("ManageOrder");
        }
        #endregion

    }
}