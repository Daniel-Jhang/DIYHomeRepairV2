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
        public IEnumerable<SelectListItem> selectListItems { get; set; }
        public List<Order_Detail> od { get; set; }
        public IEnumerable<SelectListItem> toyList = new List<SelectListItem>()// 服務地區清單
        {
        
          
            new SelectListItem() { Text = "是", Value="是"},
            new SelectListItem() { Text = "已出貨", Value="已出貨"},
           
        };
        public CVMManageOrder()
        {
            selectListItems = toyList;
        }
    }
}