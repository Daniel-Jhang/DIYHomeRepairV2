using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DIY_v2.Models
{
    public class CVMTaskerDetail
    {
        public int? TaskerID { get; set; }
        public List<Tasker> taskers { get; set; }
        public List<TaskerService> taskersService { get; set; }

        // 留言板
        public List<TaskerComment> taskerComment { get; set; }
        public string Accoumt { get; set; }
    }
}