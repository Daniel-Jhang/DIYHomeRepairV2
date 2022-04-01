using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.ComponentModel;
using DIY_v2.Models;
namespace DIY_v2.Models
{
    public class CVMManageOrder
    {
        public List<Order_Detail> od { get; set; }
        public IEnumerable<SelectListItem> cityList = new List<SelectListItem>()// 服務地區清單
        {
            new SelectListItem() { Text = "購物車", Value="購物車"},
            new SelectListItem() { Text = "取消訂單", Value="取消訂單"},
            new SelectListItem() { Text = "是", Value="是"},
            new SelectListItem() { Text = "已出貨", Value="已出貨"},
           
        };
    }
}