using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RequestWorkflow.BO
{
    public class SchdularInfo
    {
        public int schedule_id { get; set; }
        public string name { get; set; }
        public int enabled { get; set; }
        public int freq_type { get; set; }
        public int freq_interval { get; set; }
        public int freq_subday_type { get; set; }
        public int freq_subday_interval { get; set; }
        public int freq_relative_interval { get; set; }
        public int freq_recurrence_factor { get; set; }
        public int active_start_date { get; set; }
        public int active_end_date { get; set; }
        public int active_start_time { get; set; }
        public int active_end_time { get; set; }
    }
}