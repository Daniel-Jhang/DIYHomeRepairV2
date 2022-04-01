using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DIY_v2.Models
{
    public class CVMQueryTasker
    {
        // 屬性 property
        public string serviceArea { get; set; } // 搜尋條件: 服務地區

        public string serviceCategories { get; set; } // 搜尋條件: 服務分類

        public int? minPrice { get; set; } // 搜尋條件: 最小金額

        public int? maxPrice { get; set; } // 搜尋條件: 最大金額

        public IPagedList<Tasker> taskers { get; set; }

        public List<TaskerService> taskersService { get; set; }

        public int page { get; set; }  // 頁碼

        // Constructors
        public CVMQueryTasker()
        {
            serviceArea = string.Empty;
            serviceCategories = string.Empty;
            minPrice = null;
            maxPrice = null;
            page = 1;
        }
    }
}