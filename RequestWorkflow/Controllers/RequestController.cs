using RequestWorkflow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RequestWorkflow.Controllers
{
    public class RequestController : Controller
    {
        DBEntities db = null;
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
    }
}