using RequestWorkflow.BO;
using RequestWorkflow.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace RequestWorkflow.Controllers
{
    public class RequestController : Controller
    {
        DBEntities db = null;
        string strConString = @"Data Source=.;Initial Catalog=msdb;Integrated Security=True";
        public RequestController()
        {
            db = new DBEntities();
            db.Configuration.ProxyCreationEnabled = false;
        }
        public ActionResult Index()
        {
            ViewBag.Modules = db.ModuleMasters.ToList();
            var req = new UserRequest();
            req.UserId = Convert.ToInt32(Session["LoginId"]);
            return View(req);
        }
        public ActionResult List()
        {
            return View();
        }

        public ActionResult SaveRequest(UserRequest req)
        {
            bool isSaved = false;
            try
            {
                //fetch approvers list
                var lstApprover = db.ApproverFlows.Where(x => x.UserId == req.UserId)?.ToList();
                if (lstApprover != null && lstApprover.Count() > 0)
                {
                    List<UserMaster> lstUser = db.UserMasters.ToList();
                    List<ModuleMaster> lstModule = db.ModuleMasters.ToList();

                    req.RequestDate = DateTime.Now;
                    req.ApproverId = lstApprover.OrderBy(x => x.ApprovalLevel).FirstOrDefault().ApproverId;
                    req.Isapproved = null;
                    db.UserRequests.Add(req);

                    UserLog userLog = new UserLog();
                    userLog.LogDate = DateTime.Now;
                    userLog.LogMsg = $"User Name: {lstUser.FirstOrDefault(x => x.Id == req.UserId)?.UserName} " +
                        $"have add request for {lstModule.FirstOrDefault(x => x.Id == req.ModuleId)?.Name} " +
                        $"to its approver: {lstUser.FirstOrDefault(x => x.Id == req.ApproverId)?.UserName}";
                    userLog.ModuleId = req.ModuleId;
                    userLog.UserId = req.ApproverId;
                    userLog.IsSystemGenerated = false;
                    userLog.IsViewed = false;
                    db.UserLogs.Add(userLog);

                    db.SaveChanges();

                    isSaved = true;

                    return Json(new { isSaved, msg = "Request Saves Successfully" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isSaved, msg = "Approver not assigned to user." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { isSaved, msg = "Something went wrong." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult RequestList()
        {
            bool isSuccess = false;
            try
            {
                int userId = Convert.ToInt32(Session["LoginId"]);
                var reqList = db.USP_FETCH_REQ_LIST(userId)?.ToList();

                return Json(new { isSuccess = true, reqList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess, msg = "Something went wrong" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PendingReqList()
        {
            bool isSuccess = false;
            try
            {
                int userId = Convert.ToInt32(Session["LoginId"]);
                var pendingReqList = db.USP_FETCH_PENDING_REQ(userId)?.ToList();

                return Json(new { isSuccess = true, pendingReqList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess, msg = "Something went wrong" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ApproveRequest(int reqId)
        {
            bool isSuccess = false;
            try
            {
                var result = db.USP_APPROVE_USER_REQUEST(reqId, Convert.ToInt32(Session["LoginId"]), false).FirstOrDefault();
                return Json(new { isSuccess = result.isSuccess, msg = result.msg });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    isSuccess,
                    msg = "Something went wrong"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult RejectRequest(int reqId)
        {
            bool isSuccess = false;
            try
            {
                var result = db.USP_REJECT_REQUEST(reqId, Convert.ToInt32(Session["LoginId"]), false).FirstOrDefault();
                return Json(new { isSuccess = result.IsDeleted, msg = result.msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    isSuccess,
                    msg = "Something went wrong"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateSchedule()
        {
            FillDropDown();
            return View();
        }

        public ActionResult UpdateSchedular(SchdularInfo schdularInfo)
        {
            try
            {

                StringBuilder sb = new StringBuilder();

                //sb.Append(" USE msdb GO ");

                sb.Append(" EXEC dbo.sp_update_schedule ");
                sb.Append("@schedule_id = 11  ");
                sb.Append($", @enabled =  {schdularInfo.enabled} ");
                sb.Append($", @freq_type = {schdularInfo.freq_type} ");
                sb.Append($", @freq_interval = {schdularInfo.freq_interval}");
                sb.Append($", @freq_subday_type = {schdularInfo.freq_subday_type}");
                sb.Append($", @freq_subday_interval = {schdularInfo.freq_subday_interval}");
                sb.Append($", @freq_relative_interval =  {schdularInfo.freq_relative_interval} ");
                sb.Append($", @freq_recurrence_factor =  {schdularInfo.freq_recurrence_factor} ");
                sb.Append($", @active_start_date =  {schdularInfo.active_start_date}  ");
                sb.Append($", @active_end_date =   {schdularInfo.active_end_date} ");
                sb.Append($", @active_start_time =  {schdularInfo.active_start_time} ");
                sb.Append($", @active_end_time =  {schdularInfo.active_end_time} ");

                
                using (SqlConnection con = new SqlConnection(strConString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(sb.ToString(), con);
                    cmd.ExecuteNonQuery();
                }

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetSchedularInfo(int id)
        {
            SchdularInfo info = null;
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(" SELECT schedule_id, name, enabled,freq_type, freq_interval,freq_subday_type, ");
                sb.Append(" freq_subday_interval, freq_relative_interval, freq_recurrence_factor, active_start_date, ");
                sb.Append(" active_end_date, active_start_time, active_end_time ");
                sb.Append($" FROM sysschedules WHERE schedule_id = {id} ");

                using (SqlConnection con = new SqlConnection(strConString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(sb.ToString(), con);
                    SqlDataReader dr = cmd.ExecuteReader();

                    if(dr != null)
                    {
                        dr.Read();
                        info = new SchdularInfo();
                        info.schedule_id = Convert.ToInt32(dr["schedule_id"]);
                        info.name = Convert.ToString(dr["name"]);
                        info.enabled = Convert.ToInt32(dr["enabled"]);
                        info.freq_type = Convert.ToInt32(dr["freq_type"]);
                        info.freq_interval = Convert.ToInt32(dr["freq_interval"]);
                        info.freq_subday_type = Convert.ToInt32(dr["freq_subday_type"]);
                        info.freq_subday_interval = Convert.ToInt32(dr["freq_subday_interval"]);
                        info.freq_relative_interval = Convert.ToInt32(dr["freq_relative_interval"]);
                        info.freq_recurrence_factor = Convert.ToInt32(dr["freq_recurrence_factor"]);
                        info.active_start_date = Convert.ToInt32(dr["active_start_date"]);
                        info.active_end_date = Convert.ToInt32(dr["active_end_date"]);
                        info.active_start_time = Convert.ToInt32(dr["active_start_time"]);
                        info.active_end_time = Convert.ToInt32(dr["active_end_time"]);
                    }
                    dr.Close();
                }

                return Json(new { isSuccess = true, info }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { isSuccess = false, info }, JsonRequestBehavior.AllowGet);
            }
        }

        private void FillDropDown()
        {
            ViewBag.ScheduleType = new List<SelectListItem>()
            {
                new SelectListItem(){Text = "Once", Value = "1"},
                new SelectListItem(){Text = "Daily", Value = "4"},
                new SelectListItem(){Text = "Weekly", Value = "8"},
                new SelectListItem(){Text = "Monthly", Value = "16"}
            };

            List<SelectListItem> lstWeeklyInterval = new List<SelectListItem>()
            {
                new SelectListItem(){Text = "Sunday", Value = "1"},
                new SelectListItem(){Text = "Monday", Value = "2"},
                new SelectListItem(){Text = "Tuesday", Value = "4"},
                new SelectListItem(){Text = "Wednesday", Value = "8"},
                new SelectListItem(){Text = "Thursday", Value = "16"},
                new SelectListItem(){Text = "Friday", Value = "32"},
                new SelectListItem(){Text = "Saturday", Value = "64"},
            };

            List<SelectListItem> lstMonthlyRelative = new List<SelectListItem>()
            {
                new SelectListItem(){Text = "Sunday", Value = "1"},
                new SelectListItem(){Text = "Monday", Value = "2"},
                new SelectListItem(){Text = "Tuesday", Value = "3"},
                new SelectListItem(){Text = "Wednesday", Value = "4"},
                new SelectListItem(){Text = "Thursday", Value = "5"},
                new SelectListItem(){Text = "Friday", Value = "6"},
                new SelectListItem(){Text = "Saturday", Value = "7"},
                new SelectListItem(){Text = "Day", Value = "8"},
                new SelectListItem(){Text = "Weekday", Value = "9"},
                new SelectListItem(){Text = "Weekend day", Value = "10"},
            };

            ViewBag.DailyFrequencyInterval = new List<SelectListItem>()
            {
                new SelectListItem(){Text = "Hour(s)", Value="8"},
                new SelectListItem(){Text = "Minute(s)", Value="4"},
                new SelectListItem(){Text = "Second(s)", Value="2"}
            };


            List<SelectListItem> lstSchedular = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(strConString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT schedule_id, name FROM sysschedules WHERE name IN ('LeaveScheduler','HelpDeskScheduler','TestSchedule')", con);
                SqlDataReader dr = cmd.ExecuteReader();

                if(dr != null)
                {
                    while (dr.Read())
                    {
                        lstSchedular.Add(new SelectListItem() { Text = Convert.ToString(dr["name"]), Value = Convert.ToString(dr["schedule_id"]) });
                    }
                }

                dr.Close();
            }

            ViewBag.SchedularList = lstSchedular;
        }

    }
}