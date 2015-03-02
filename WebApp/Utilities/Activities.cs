using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Pocos;

namespace WebApp.Utilities
{
    public static class Activities
    {
        /// <summary>
        /// Add activity item
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="projectId"></param>
        /// <param name="announcementId"></param>
        public static void AddActivity(int? userId, int? projectId, int? announcementId, int? commentId, int languageId, string content = "")
        {
            if (userId == null && announcementId == null && projectId == null)
            {
                throw new Exception("projectId or announcementId should has value");
            }

            var activityText = "";

            if (announcementId != null)
            {
                var text = Strings.GetTextConstant("Activity_New_Announcement", languageId, "Ο {user} ανάρτησε μία νέα ανακόινωση");
                activityText = "Ο {user} ανάρτησε μία νέα ανακόινωση";
            }
            else if (commentId != null)
            {
                var text = Strings.GetTextConstant("Activity_New_Comment", languageId, "Ο {user} ανάρτησε ένα νέο σχόλιο στο project {project}");
                activityText = "Ο {user} ανάρτησε ένα νέο σχόλιο στο project {project}";
            }

            if (!string.IsNullOrEmpty(content))
            {
                activityText = content;
            }

            using (var db = new DataContainer())
            {
                var activity = new Activity
                {
                    activityAnnouncementId = announcementId,
                    activityDatetime = DateTime.Now,
                    activityProjectId = projectId,
                    activityUserId = userId,
                    activityCommentId = commentId,
                    activityContent = activityText
                };

                db.Activities.Add(activity);
                db.SaveChanges();
                db.Database.Connection.Close();
            }

        }

        public static void AddAnnouncement(int userId, int announcementId, int languageId)
        {
            InsertActivity(userId, null, announcementId, null, null, ActivityAction.AddAnnouncement, languageId);
        }

        public static void RemoveAnnouncement(int userId, int announcementId, int languageId)
        {
            InsertActivity(userId, null, announcementId, null, null, ActivityAction.RemoveAnnouncement, languageId);
        }

        public static void AddProject(int userId, int projectId, int languageId)
        {
            InsertActivity(userId, projectId, null, null, null, ActivityAction.AddProject, languageId);
        }

        public static void RemoveProject(int userId, int projectId, int languageId)
        {
            InsertActivity(userId, projectId, null, null, null, ActivityAction.RemoveProject, languageId);
        }

        public static void AddComment(int userId, int projectId, int commentId, int languageId)
        {
            InsertActivity(userId, projectId, null, commentId, null, ActivityAction.AddComment, languageId);
        }

        public static void RemoveComment(int userId, int projectId, int commentId, int languageId)
        {
            InsertActivity(userId, projectId, null, commentId, null, ActivityAction.RemoveComment, languageId);
        }

        public static void AddProjectUser(int userId, int projectId, int relatedUserId, int languageId)
        {
            InsertActivity(userId, projectId, null, null, relatedUserId, ActivityAction.AddProjectUser, languageId);
        }

        public static void RemoveProjectUsers(int userId, int projectId, int relatedUserId, int languageId)
        {
            InsertActivity(userId, projectId, null, null, relatedUserId, ActivityAction.RemoveProjectUser, languageId);
        }

        private static void InsertActivity(int userId, int? projectId, int? announcementId, int? commentId, int? relatedUserId, ActivityAction action, int languageId)
        {
            using (var db = new DataContainer())
            {
                //var activity = new Activity
                //{
                //    activityAction = action.ToString(),
                //    activityDatetime = DateTime.Now,
                //    activityProjectId = projectId,
                //    activityUserId = userId,
                //    activityRelatedUser = relatedUserId,
                //    activityContent = GenerateActivityContent(action),
                //    activityAnnouncementId = announcementId,
                //    activityCommentId = commentId
                //};

                var activity = new Activity();
                activity.activityAction = action.ToString();
                activity.activityDatetime = DateTime.Now;
                activity.activityProjectId = projectId;
                activity.activityUserId = userId;
                activity.activityRelatedUser = relatedUserId;
                activity.activityContent = GenerateActivityContent(action, languageId);
                activity.activityAnnouncementId = announcementId;
                activity.activityCommentId = commentId;

                db.Activities.Add(activity);
                db.SaveChanges();
                db.Database.Connection.Close();
            }
        }

        private static string GenerateActivityContent(ActivityAction action, int languageId)
        {
            var text = "";
            switch ((int)action)
            {
                case 1:
                    text = Strings.GetTextConstant("Activity_New_User", languageId, "Ο {activityUserId} προσέθεσε τον χρήστη {activityRelatedUser} στον project {activityProjectId}");
                    //text = "Ο {activityUserId} προσέθεσε τον χρήστη {activityRelatedUser} στον project {activityProjectId}";
                    break;
                case 2:
                    text = Strings.GetTextConstant("Activity_Delete_User", languageId, "Ο {activityUserId} διέγραψε τον χρήστη {activityRelatedUser} από το project {activityProjectId}");
                    //text = "Ο {activityUserId} διέγραψε τον χρήστη {activityRelatedUser} από το project {activityProjectId}";
                    break;
                case 3:
                    text = Strings.GetTextConstant("Activity_New_ProjectComment", languageId, "Ο {activityUserId} σχολίασε το project {activityProjectId}");
                    text = "Ο {activityUserId} σχολίασε το project {activityProjectId}";
                    break;
                case 4:
                    text = Strings.GetTextConstant("Activity_Delete_ProjectComment", languageId, "Ο {activityUserId} διέγραψε το σχόλιο του από το project {activityProjectId}");
                    //text = "Ο {activityUserId} διέγραψε το σχόλιο του από το project {activityProjectId}";
                    break;
                case 5:
                    text = Strings.GetTextConstant("Activity_New_Project", languageId, "Ο {activityUserId} δημιούργησε ένα καινούριο project με τίτλο {activityProjectId}");
                    //text = "Ο {activityUserId} δημιούργησε ένα καινούριο project με τίτλο {activityProjectId}";
                    break;
                case 6:
                    text = Strings.GetTextConstant("Activity_Delete_Project", languageId, "Ο {activityUserId} διέγραψε το project {activityProjectId}");
                    //text = "Ο {activityUserId} διέγραψε το project {activityProjectId}";
                    break;
                case 7:
                    text = Strings.GetTextConstant("Activity_New_Announcement", languageId, "Ο {activityUserId} ανάρτησε μία νέα ανακόινωση");
                    //text = "Ο {activityUserId} ανάρτησε μία νέα ανακόινωση";
                    break;
                case 8:
                    text = Strings.GetTextConstant("Activity_Delete_Announcement", languageId, "Ο {activityUserId} διέγραψε μία ανακόινωση");
                    //text = "Ο {activityUserId} διέγραψε μία ανακόινωση";
                    break;


            }

            return text;
        }
    }
}