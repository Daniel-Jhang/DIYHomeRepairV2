using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DIY_v2.Models;
using PagedList;
using PagedList.Mvc;
namespace DIY_v2.Models
{
    public class Check
    {
        public List<Product_Category> Categories { get; set; }
        public IPagedList<Product> Products { get; set; }

        public List<ProductReply> replies { get; set; }
        public List<Product> Products_normal { get; set; }

        public string Accoumt { get; set; }
    }
}