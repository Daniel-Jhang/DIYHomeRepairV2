using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO; // 處理檔案必須引用 System.IO 命名空間
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DIY_v2.Models;
using PagedList; // 引用第三方分頁套件

namespace DIY_v2.Controllers
{
    public class TaskerController : Controller
    {
        DIY_DBEntities db = new DIY_DBEntities();

        // 單頁可容納之資料筆數(可參數化此數值)
        private const int pageSize = 9;

        // GET: Tasker
        public ActionResult Tasker(CVMQueryTasker viewModel)
        {
            int currentPage = viewModel.page < 1 ? 1 : viewModel.page; // 避免頁數跑到負數
            var finalTasker = db.Tasker.Where(x=>x.Permission=="2").ToList(); // 預設抓全部(Permission為正常)的師傅
            var finalCategories = db.TaskerService.ToList(); // 預設抓全部的類別

            #region 處理 服務地區
            // 處理 服務地區
            Session["serviceArea"] = viewModel.serviceArea;
            if (!string.IsNullOrWhiteSpace(viewModel.serviceArea) && viewModel.serviceArea != "請選擇縣市") // 如果使用者有設定想要找的地區
            {
                finalTasker = finalTasker.Where(x => x.ServiceArea == viewModel.serviceArea).ToList();
            }
            #endregion

            #region 處理 服務類別 
            // 處理 服務類別 
            Session["serviceCategories"] = viewModel.serviceCategories;
            if (!string.IsNullOrWhiteSpace(viewModel.serviceCategories)) // 如果使用者有設定想要找的分類
            {
                #region 測試的SQL
                //參考解法 (from https://zh-tw.coderbridge.com/discussions/62c3a73524454f7aa8fa13812ff79fc3)
                //var q = from t1 in table1
                //        let t2s = from t2 in table2
                //                  where < Conditions for table2 >
                //                   select t2.KeyField
                //         where t2s.Contains(t1.KeyField)
                //         select t1;

                //SELECT* FROM Tasker where TaskerID in (
                //SELECT TaskerID FROM TaskerService Where ServiceCategory = '衛浴裝修') ==> 45筆

                //SELECT* FROM Tasker where ServiceArea = '台中市' and TaskerID in (
                //SELECT TaskerID FROM TaskerService Where ServiceCategory = '衛浴裝修') ==> 9筆

                //SELECT* FROM Tasker where ServiceArea = '台南市' and ServiceQuote Between 300 and 500 and TaskerID in (
                //SELECT TaskerID FROM TaskerService Where ServiceCategory = '水電安裝/修繕') ==> 6筆
                #endregion

                var temp = from x in finalTasker
                           let y = from z in db.TaskerService
                                   where z.ServiceCategory == viewModel.serviceCategories
                                   select z.TaskerID
                           where y.Contains(x.TaskerID)
                           select x;
                finalTasker = temp.ToList();
            }
            #endregion

            #region 處理 服務價格(最小金額~最大金額)
            // 處理 服務價格(最小金額~最大金額)
            Session["min"] = viewModel.minPrice;
            Session["max"] = viewModel.maxPrice;
            if (viewModel.minPrice != null && viewModel.maxPrice != null) // 如果使用者有設定最大、最小值
            {
                finalTasker = finalTasker.Where(x => x.ServiceQuote >= viewModel.minPrice && x.ServiceQuote <= viewModel.maxPrice).ToList();
            }
            #endregion

            viewModel.taskers = finalTasker.ToPagedList(currentPage, pageSize); // 把所有條件篩選完的師傅清單傳給viewModel給前端Model使用
            // viewModel.taskersService = finalCategories.ToPagedList(currentPage, pageSize);  // 把所有條件篩選完的服務清單傳給viewModel給前端Model使用
            viewModel.taskersService = finalCategories.ToList();
            return View(viewModel);
        }

        // GET : TaskerDetail
        public ActionResult TaskerDetail(int TaskerID)
        {
            var TaskerDetail = db.Tasker.Where(x => x.TaskerID == TaskerID).ToList();
            var TaskerDetailService = db.TaskerService.Where(x => x.TaskerID == TaskerID).ToList();
            var commentList = db.TaskerComment.Where(x => x.TaskerID == TaskerID).ToList();
            CVMTaskerDetail viewModel = new CVMTaskerDetail()
            {
                TaskerID = TaskerID,
                taskers = TaskerDetail,
                taskersService = TaskerDetailService,
                taskerComment = commentList
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult TaskerDetail(int TaskerID, string comment = "")
        {
            string MemberAccount = User.Identity.Name; //抓出使用者的帳號
            var theMemberID = db.Member.Where(x => x.MemberAccount == MemberAccount).Select(x => x.MemberID).FirstOrDefault();
            var TaskerDetail = db.Tasker.Where(x => x.TaskerID == TaskerID).ToList(); //把符合ID的師傅抓出來
            var TaskerDetailService = db.TaskerService.Where(x => x.TaskerID == TaskerID).ToList();

            TaskerComment taskerComment = new TaskerComment()
            {
                TaskerID = TaskerID,
                MemberID = theMemberID,
                MemberAccount = MemberAccount,
                Comment = comment,
                CreateDate = DateTime.Now,
                Star = 5
            };
            db.TaskerComment.Add(taskerComment);
            db.SaveChanges();

            var commentList = db.TaskerComment.Where(x=>x.TaskerID==TaskerID).ToList();
            CVMTaskerDetail viewModel = new CVMTaskerDetail()
            {
                //TaskerID = TaskerID,
                taskers = TaskerDetail,
                taskersService = TaskerDetailService,
                taskerComment = commentList
            };

            return View(viewModel);
        }
    }
}