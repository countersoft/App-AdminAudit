using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Countersoft.Gemini.Commons.Meta;
using Countersoft.Foundation.Commons.Extensions;
using Countersoft.Gemini.Commons;
using Countersoft.Gemini.Commons.Entity;
using Countersoft.Gemini.Extensibility.Events;

namespace AdminAudit
{
    public class Helper
    {
        public static string CreateRollbackQuery(AdminAuditDto audit)
        {
            string query = string.Empty;

            switch(audit.AdminArea)
            {
                case AdminAreaVisibility.Process:
                    if (audit.Action == UserAction.Edited)
                    {
                        query = string.Format("Update gemini_issuetypes set {0} = '{1}' where typeid = {2}", audit.FieldChanged.ToLower(), audit.ValueBefore, audit.RowId);
                    }

                    if (audit.Action == UserAction.Created)
                    {
                        query = string.Format("DELETE from gemini_issuetypes where typeid = {0}", audit.RowId);
                    }

                    if (audit.Action == UserAction.Deleted)
                    {
                        var data = audit.Data.FromJson<IssueType>();

                        query = "SET IDENTITY_INSERT gemini_issuetypes ON";
                        
                        query += string.Format(" INSERT INTO gemini_issuetypes (typeid,seq,typedesc,imagepath,color,screen,workflow,tag,templateid) values ({0},{1},'{2}','{3}','{4}','{5}','{6}','{7}',{8}) ",
                                            data.Id, data.Sequence, data.Label, data.Image, data.Color, data.Screen.ToJson(), data.Workflow.ToJson(), data.Tag, data.TemplateId);
                        query += "SET IDENTITY_INSERT gemini_issuetypes OFF ";
                    }

                    return query;
                case AdminAreaVisibility.ProjectTemplate:
                    //Cache.ProjectTemplates.RemoveAll();
                    break;
                case AdminAreaVisibility.CustomField:
                    //Cache.CustomFields.RemoveAll();
                    break;
                case AdminAreaVisibility.Status:
                    //Cache.Meta.StatusRemoveAll();
                    break;
                case AdminAreaVisibility.Priority:
                    //Cache.Meta.PriorityRemoveAll();
                    break;
                case AdminAreaVisibility.Severity:
                    //Cache.Meta.SeverityRemoveAll();
                    break;
                case AdminAreaVisibility.Resolution:
                    //Cache.Meta.ResolutionRemoveAll();
                    break;
                case AdminAreaVisibility.LinkType:
                    //Cache.Meta.LinkTypeRemoveAll();
                    break;
                case AdminAreaVisibility.TimeType:
                    //Cache.Meta.TimeTypeRemoveAll();
                    break;
                case AdminAreaVisibility.Project:
                    //Cache.Projects.RemoveAll();
                    break;
                case AdminAreaVisibility.ProjectLabel:
                    //Cache.ProjectLabels.RemoveAll();
                    break;
                case AdminAreaVisibility.GeminiConfiguration:
                    //Update user property from session/cache?
                    break;
                case AdminAreaVisibility.PeopleUser:
                    //Cache.Users.RemoveAll();
                    break;
                case AdminAreaVisibility.PeopleGroup:
                    //Cache.ProjectGroups.RemoveAll();
                    break;
                case AdminAreaVisibility.PeoplePermission:
                    //Cache.Permissions.RemoveAll();
                    break;
                case AdminAreaVisibility.BreezeQueue:
                    //Update TicketingQueue property from session/cache?
                    break;
                case AdminAreaVisibility.BreezeSmtp:
                    //Update TicketingSmtp property from session/cache?
                    break;
                case AdminAreaVisibility.BreezeMailbox:
                    //Update TicketingMailbox property from session/cache?
                    break;
                case AdminAreaVisibility.BreezeExpression:
                    //Update TicketingExpression property from session/cache?
                    break;
                case AdminAreaVisibility.BreezeReplyFrom:
                    //Update TicketingReplyFrom property from session/cache?
                    break;
                case AdminAreaVisibility.Rules:
                   // Cache.RulesActions.RemoveAll();
                    break;
                case AdminAreaVisibility.Sla:
                    //Cache.SLA.RemoveAll();
                    break;
                case AdminAreaVisibility.SystemOption:
                    //Update SystemOption property from session/cache?
                    break;
                case AdminAreaVisibility.SystemEmail:
                    //Update SystemEmail property from session/cache?
                    break;
                case AdminAreaVisibility.SystemAlertTemplates:
                    //Update SystemAlertTemplates property from session/cache?
                    break;
                case AdminAreaVisibility.SystemActiveDirectory:
                    //Update SystemActiveDirectory property from session/cache?
                    break;
                case AdminAreaVisibility.SystemLicensing:
                    //Update SystemLicensing property from session/cache?
                    break;
 
            }

            return string.Empty;
        }

        public static string GetCustomFieldFullName(string type)
        {
            switch(type)
            {
                case Constants.CUSTOM_FIELD_TYPE_TEXT:
                    return "Textbox";
                case Constants.CUSTOM_FIELD_TYPE_RICHTEXT:
                    return "Rich Textbox";
                case Constants.CUSTOM_FIELD_TYPE_NUMERIC:
                    return "Numeric";
                case Constants.CUSTOM_FIELD_TYPE_COMBO:
                    return "Dropdown List";
                case Constants.CUSTOM_FIELD_TYPE_LIST:
                    return "Multi Select List";
                case Constants.CUSTOM_FIELD_TYPE_CHECK:
                    return "Checkbox";
                case Constants.CUSTOM_FIELD_TYPE_DATE:
                    return "Date Picker";
                case Constants.CUSTOM_FIELD_TYPE_ATTACHMENT:
                    return "Attachment";
                case Constants.CUSTOM_FIELD_TYPE_VERSION:
                    return "Version Picker";
                case Constants.CUSTOM_FIELD_TYPE_COMPONENT:
                    return "Component Picker";
                case Constants.CUSTOM_FIELD_TYPE_USER:
                    return "User Picker";
                default:
                    return type;
            }
        }

        public static string GetListLimiterFullName(CustomField customField, CustomFieldEventArgs args)
        {
            if (customField.ListLimiter.IsEmpty()) return string.Empty;

            if (customField.Type == Constants.CUSTOM_FIELD_TYPE_USER)
            {
                var allGroups = args.Context.ProjectGroups.GetAll();

                var previousSplitted = customField.ListLimiter.Split('|');
                return allGroups.FindAll(s => previousSplitted.Contains(s.Id.ToString())).Select(s => s.Name).ToDelimited(", ").TrimEnd(' ').TrimEnd(',');
            }
            else
            {
                switch (customField.ListLimiter)
                {
                    case Constants.CUSTOM_FIELD_LIMITER_ALL:
                        return "All";
                    case Constants.CUSTOM_FIELD_LIMITER_LOCKED:
                        return "Locked";
                    case Constants.CUSTOM_FIELD_LIMITER_UNLOCKED:
                        return "Unlocked";
                    case Constants.CUSTOM_FIELD_TYPE_COMBO:
                        return "Dropdown List";
                    case Constants.CUSTOM_FIELD_LIMITER_UNARCHIVEDUNRELEASED:
                        return "Unarchived, Unreleased";
                    case Constants.CUSTOM_FIELD_LIMITER_UNARCHIVEDRELEASED:
                        return "Unarchived, Released";
                    case Constants.CUSTOM_FIELD_LIMITER_ARCHIVEDUNRELEASED:
                        return "Archived, Unreleased";
                    case Constants.CUSTOM_FIELD_LIMITER_ARCHIVEDRELEASED:
                        return "Archived, Released";
                    default:
                        return customField.ListLimiter;
                }
            }
        }
    }
}
