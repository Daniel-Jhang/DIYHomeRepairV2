using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.IO;

using System.Web.Security;
using DIY_v2.Models;
namespace DIY_v2.Controllers
{
    public class TaskerMemberController : Controller
    {
        DIY_DBEntities db = new DIY_DBEntities();
        // GET: TaskerMember
        [Authorize]
        public ActionResult Index()
        {
            //取得登入會員的帳號並指定給fUserId
            string MemberAccount = User.Identity.Name;

            //找出未成為訂單明細的資料，即購物車內容
            var MemberID = db.Member.Where(x => x.MemberAccount == MemberAccount).Select(x => x.MemberID).FirstOrDefault();
            var result = db.Tasker.Where(x => x.MemberID == MemberID).Select(x => x).ToList();
            if (result.Count == 0)
            {
                return RedirectToAction("TaskerRegister", "TaskerMember");
            }
            return View(result);
        }
        #region 師傅註冊
        public ActionResult TaskerAccoumt()
        {
            ViewData["MemberPwdError"] = "display:none";
            return View();
        }
        [HttpPost]
        public ActionResult TaskerAccoumt(Member newMember)
        {
            // 依帳密取得會員並指定給member
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
                    newMember.Permission = "2";
                    db.Member.Add(newMember);
                    db.SaveChanges();
                    //執行Home控制器的Login動作方法
                    return RedirectToAction("Login", "MemberLogin");
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


        #endregion

        public ActionResult TaskerRegister()
        {
            ServiceAreaList cityList = new ServiceAreaList();
            Session["cityList"] = cityList.serviceAreaList;
            string MemberAccoumt = User.Identity.Name;

            int mid = db.Member.Where(x => x.MemberAccount == MemberAccoumt).Select(x => x.MemberID).FirstOrDefault();
            var Alreadydata = db.Tasker.Where(x => x.MemberID == mid).FirstOrDefault();

            if (Alreadydata != null)
            {
                Session["AlreadyData"] = "Y";
                return RedirectToAction("Index", "TaskerMember");/////////////////////這邊要放編輯網頁
            }
            var result = db.Tasker.Select(x => x).FirstOrDefault();
            Session["MemberID"] = mid;
            return View();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TaskerRegister(Tasker tasker, HttpPostedFileBase taskerPhoto, HttpPostedFileBase[] casePhotos, bool? serviceCategoryChk1, bool? serviceCategoryChk2, bool? serviceCategoryChk3, int MemberID = 19)
        {
            string MemberAccoumt = User.Identity.Name;
            int mid = db.Member.Where(x => x.MemberAccount == MemberAccoumt).Select(x => x.MemberID).FirstOrDefault();
            
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

            tasker.TaskerDescription = tasker.TaskerDescription.Replace(Environment.NewLine, "<br/>");
            db.Tasker.Add(tasker);
            db.SaveChanges();

            var nowmemberID = db.Tasker.Where(x => x.MemberID == mid).Select(x => x.TaskerID).FirstOrDefault();
            //把三個服務先弄出來  如果有 要加服務時能用
            TaskerService ts1 = new TaskerService() { TaskerID = nowmemberID, ServiceCategoryID = "1", ServiceCategory = "衛浴裝修" };
            TaskerService ts2 = new TaskerService() { TaskerID = nowmemberID, ServiceCategoryID = "2", ServiceCategory = "抓漏/堵塞" };
            TaskerService ts3 = new TaskerService() { TaskerID = nowmemberID, ServiceCategoryID = "3", ServiceCategory = "水電安裝/修繕" };

            #region 服務一判斷

            if (serviceCategoryChk1 == true)
            {
                db.TaskerService.Add(ts1);
                db.SaveChanges();
            }

            #endregion
            #region 服務二判斷

            if (serviceCategoryChk2 == true)
            {
                db.TaskerService.Add(ts2);
                db.SaveChanges();
            }
            #endregion

            #region 服務三判斷
            if (serviceCategoryChk3 == true)
            {
                db.TaskerService.Add(ts3);
                db.SaveChanges();
            }
            #endregion


            db.SaveChanges();


            // 傳資料給師父專區 > 查看主頁的師傅專區
            var TaskerID = db.Tasker.Where(x => x.MemberID == tasker.MemberID).Select(x => x.TaskerID).FirstOrDefault();
            Session["TaskerID"] = TaskerID;

            return RedirectToAction("Index", "Home");
        }

        #region 師傅註冊
        public ActionResult TaskerMemberEdit(int id)
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
        public ActionResult TaskerMemberEdit(Tasker tasker, HttpPostedFileBase taskerPhoto, HttpPostedFileBase[] casePhotos, bool? serviceCategoryChk1, bool? serviceCategoryChk2, bool? serviceCategoryChk3)
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
            return RedirectToAction("Index");
        }
        #endregion

        #region 師傅身分的會員，合併會員基本資料和水電行資料  的修改
        public ActionResult TaskerMemberEditV2(int id = 24)
        {
            ServiceAreaList cityList = new ServiceAreaList();
            Session["cityList"] = cityList.serviceAreaList;
            //下面三個Session 是為了進入編輯頁面時  因為action吃不到 ServerID 參數  所以用他存資料帶進View裡面  為了讓checkbox 能在編輯時依照他原本的服務 打勾起來
            Session["server1"] = db.TaskerService.Where(x => x.TaskerID == id).Where(x => x.ServiceCategoryID == "1").Count() > 0 ? "T" : "F";
            Session["server2"] = db.TaskerService.Where(x => x.TaskerID == id).Where(x => x.ServiceCategoryID == "2").Count() > 0 ? "T" : "F";
            Session["server3"] = db.TaskerService.Where(x => x.TaskerID == id).Where(x => x.ServiceCategoryID == "3").Count() > 0 ? "T" : "F";
            var taskerToEdit = db.Tasker.Where(x => x.MemberID == id).FirstOrDefault();
            var memberToEdit = db.Member.Where(x => x.MemberID == id).FirstOrDefault();

            CVMTaskerMemberEditV2 viewModel = new CVMTaskerMemberEditV2();
            viewModel.taskerTable = taskerToEdit;
            viewModel.memberTable = memberToEdit;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TaskerMemberEditV2(CVMTaskerMemberEditV2 viewModel, HttpPostedFileBase taskerPhoto, HttpPostedFileBase[] casePhotos, bool? serviceCategoryChk1, bool? serviceCategoryChk2, bool? serviceCategoryChk3)
        {
            if (ModelState.IsValid)
            {
                string MemberAccount = User.Identity.Name;
                var MemberItem = db.Member.Where(x => x.MemberAccount == MemberAccount).FirstOrDefault();
                MemberItem.MemberName = viewModel.memberTable.MemberName;
                MemberItem.MemberGender = viewModel.memberTable.MemberGender;
                MemberItem.MemberBirthday = viewModel.memberTable.MemberBirthday;
                MemberItem.MemberNickname = viewModel.memberTable.MemberNickname;
                MemberItem.MemberEmail = viewModel.memberTable.MemberEmail;
                MemberItem.MemberAddress = viewModel.memberTable.MemberAddress;
                MemberItem.MemberPhone = viewModel.memberTable.MemberPhone;


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
                        viewModel.taskerTable.TaskerImage = taskerFileName;
                    }
                }
                else
                {
                    viewModel.taskerTable.TaskerImage = db.Tasker.Where(x => x.TaskerID == viewModel.taskerTable.TaskerID).Select(x => x.TaskerImage).FirstOrDefault();
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
                                viewModel.taskerTable.CaseImage = db.Tasker.Where(x => x.TaskerID == viewModel.taskerTable.TaskerID).Select(x => x.CaseImage).FirstOrDefault();
                            }
                        }

                    }
                    //如果三個casephoto都沒傳資料  把之前的值傳到caseImage裡面  不然會是空白
                    if (casePhotos[0] == null && casePhotos[1] == null && casePhotos[2] == null)
                    {
                        viewModel.taskerTable.CaseImage = db.Tasker.Where(x => x.TaskerID == viewModel.taskerTable.TaskerID).Select(x => x.CaseImage).FirstOrDefault();
                    }
                    else
                    {
                        //把結尾逗號去掉  因為之前都是存   檔名,檔名,  最後會多一個,
                        strCaseImage = strCaseImage.TrimEnd(',');
                        viewModel.taskerTable.CaseImage = strCaseImage;
                    }
                }

                #endregion
                var temp = db.Tasker.Where(x => x.TaskerID == viewModel.taskerTable.TaskerID).FirstOrDefault();
                temp.TaskerName = viewModel.taskerTable.TaskerName;
                temp.ServiceQuote = viewModel.taskerTable.ServiceQuote;
                temp.TaskerTel = viewModel.taskerTable.TaskerTel;
                temp.TaskerPhone = viewModel.taskerTable.TaskerPhone;
                temp.ServiceTime_Start = viewModel.taskerTable.ServiceTime_Start;
                temp.ServiceTime_End = viewModel.taskerTable.ServiceTime_End;
                temp.ServiceArea = viewModel.taskerTable.ServiceArea;
                temp.TaskerImage = viewModel.taskerTable.TaskerImage;
                temp.License = viewModel.taskerTable.License;
                temp.Feature = viewModel.taskerTable.Feature;
                temp.TaskerDescription = viewModel.taskerTable.TaskerDescription;
                temp.CaseImage = viewModel.taskerTable.CaseImage;
                temp.Rate = viewModel.taskerTable.Rate;

                var temp1 = db.TaskerService.Where(x => x.TaskerID == viewModel.taskerTable.TaskerID).Where(x => x.ServiceCategoryID == "1").FirstOrDefault();
                var temp2 = db.TaskerService.Where(x => x.TaskerID == viewModel.taskerTable.TaskerID).Where(x => x.ServiceCategoryID == "2").FirstOrDefault();
                var temp3 = db.TaskerService.Where(x => x.TaskerID == viewModel.taskerTable.TaskerID).Where(x => x.ServiceCategoryID == "3").FirstOrDefault();
                //把三個服務先弄出來  如果有 要加服務時能用
                TaskerService ts1 = new TaskerService() { TaskerID = viewModel.taskerTable.TaskerID, ServiceCategoryID = "1", ServiceCategory = "衛浴裝修" };
                TaskerService ts2 = new TaskerService() { TaskerID = viewModel.taskerTable.TaskerID, ServiceCategoryID = "2", ServiceCategory = "抓漏/堵塞" };
                TaskerService ts3 = new TaskerService() { TaskerID = viewModel.taskerTable.TaskerID, ServiceCategoryID = "3", ServiceCategory = "水電安裝/修繕" };

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
            Session["memberEdit"] = "資料修改完成";
            return RedirectToAction("Index");
        }
        #endregion
    }
}