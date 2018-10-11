using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;

namespace LRC_NET_Framework.Models
{
    public class AssessActivityModels
    {
        public tb_Assessment _Assessment { get; set; }
        public tb_Activity _Activity { get; set; }
        public IEnumerable<tb_ActivityStatus> _ActivityStatus { get; set; }
        public IEnumerable<tb_Assessment> _MemberAssessments { get; set; }
        public IEnumerable<tb_MemberActivity> _MemberActivity { get; set; }
        public int _CollegeID { get; set; }
    }

    public class ActivityByMemberModels
    {
        public int MemberActivityID { get; set; }
        public int ActivityID { get; set; }
        [Required(ErrorMessage = "You must select an Activity Status")]
        public int ActivityStatusID { get; set; }
        public bool Membership { get; set; }
        public int MembershipCommitment { get; set; }
        public int MemberID { get; set; }
        public string TwitterHandle { get; set; }
        public string FacebookID { get; set; }
        public bool? Participated { get; set; }

        public virtual tb_ActivityStatus tb_ActivityStatus { get; set; }
        public virtual tb_MemberMaster tb_MemberMaster { get; set; }
        public virtual tb_Activity tb_Activity { get; set; }
        public List<SelectListItem> MemberCollection { get; set; } //for Chosen Plugin DDL

        public List<string> AddEditActivity(int memberId, int activityID, int activityStatusID, bool? isParticipated, DateTime activityDate, string activityNote, string userId)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();
            List<string> warnings = new List<string>();
            string warning = String.Empty;
            using (LRCEntities context = new LRCEntities())
            {
                try
                {
                    var existingActivity = context.tb_MemberActivity.Where(t => t.MemberID == memberId && t.ActivityID == activityID).FirstOrDefault();
                    if (existingActivity == null) // Add new activity for selected Member
                    {
                        tb_MemberActivity memberActivity = new tb_MemberActivity
                        {
                            ActivityID = activityID,
                            MemberID = memberId,
                            ActivityStatusID = activityStatusID,
                            Participated = isParticipated
                        };
                        context.tb_MemberActivity.Add(memberActivity);
                    }
                    else //Edit existing activity for selected Member
                    {
                        existingActivity.ActivityStatusID = activityStatusID;
                        existingActivity.Participated = isParticipated;
                    }

                    tb_Activity activity = context.tb_Activity.Find(activityID);
                    // Change if Activity was edited (activityDate or ActivityNote fields)
                    if (activity.ActivityDate != activityDate || activity.ActivityNote.ToUpper() != activityNote.ToUpper())
                    {
                        activity.ModifiedBy = userId;
                        activity.ModifiedDateTime = DateTime.UtcNow;
                        activity.ActivityDate = activityDate;
                        activity.ActivityNote = activityNote;
                    }
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode);
                    foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                    {
                        error.errMsg += ". Object: " + validationError.Entry.Entity.ToString();
                        foreach (DbValidationError err in validationError.ValidationErrors)
                        {
                            error.errMsg += ". " + err.ErrorMessage;
                        }
                    }
                    errs.Add("Error #" + error.errCode.ToString() + "!" + error.errMsg);
                    return errs;
                }
            }
                return errs;
        }

        public List<SelectListItem> GetFullListOfMembers()
        {
            ActivityByMemberModels memberActivities = new ActivityByMemberModels();
            using (LRCEntities context = new LRCEntities())
            {
                var tb_MemberMaster = context.tb_MemberMaster.ToList();
                List<SelectListItem> members = new List<SelectListItem>();
                foreach (var member in tb_MemberMaster)
                {
                    members.Add(new SelectListItem()
                    {
                        Text = member.LastName + ", " + member.FirstName,
                        Value = member.MemberID.ToString()
                        //Selected = member.MemberID == mId ? true : false
                    });
                    memberActivities.MemberCollection = members.OrderBy(s => s.Text).ToList();
                }
            }
            return memberActivities.MemberCollection;
        }
    }

    public class ActivityJSON
    {
        public string ActivityDate { get; set; }
        public string ActivityName { get; set; }
        public string ActivityNote { get; set; }
        public string AddedBy { get; set; }
        public string AddedDateTime { get; set; }
        public string ActivityStatusBeforeTheFact { get; set; }
        public string ActivityStatusAfterTheFact { get; set; }
    }
}