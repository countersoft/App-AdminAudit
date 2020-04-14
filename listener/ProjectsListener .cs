using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Countersoft.Foundation.Commons;
using Countersoft.Foundation.Commons.Extensions;
using Countersoft.Foundation.Contracts;
using Countersoft.Gemini;
using Countersoft.Gemini.Commons;
using Countersoft.Gemini.Contracts;
using Countersoft.Gemini.Extensibility;
using Countersoft.Gemini.Extensibility.Events;
using System.Linq;
using Countersoft.Gemini.Extensibility.Apps;
using Countersoft.Gemini.Infrastructure.Apps;
using Countersoft.Gemini.Commons.Dto;
using System.Configuration;
//using NHibernate.Cfg.ConfigurationSchema;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using Countersoft.Gemini.Commons.Meta;

namespace AdminAudit
{
    [AppType(AppTypeEnum.Event),
    AppControlGuid("89F07463-E9C3-46F1-8AC6-34FD4CAAFE7B"),
    AppGuid(CustomConstants.APPGUID),
    AppAuthor(CustomConstants.APPAUTHOR),
    AppName(CustomConstants.APPNAME),
    AppDescription(CustomConstants.APPDESCRIPTION),
    AppRequiresConfigScreen(true)]
    [OutputCache(Duration = 0, NoStore = true, Location = System.Web.UI.OutputCacheLocation.None)]
    public class ProjectsListener : IAfterProjectListener, IAfterProjectLabelListener
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string AppGuid { get; set; }

        #region PROJECTS

        public void AfterProjectCreated(ProjectEventArgs args)
        {
            if (!args.Previous.PermissionId.GetValueOrDefault().Equals(args.Entity.PermissionId.GetValueOrDefault()))
            {
                AfterProjectUpdated(args);
            }
            else
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Entity.Id;
                audit.Action = UserAction.Created;
                audit.AdminArea = AdminAreaVisibility.Project;
                audit.RowName = audit.ValueAfter = args.Entity.Name;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        public void AfterProjectDeleted(ProjectEventArgs args)
        {
            if (!args.Previous.PermissionId.GetValueOrDefault().Equals(args.Entity.PermissionId.GetValueOrDefault()))
            {
                AfterProjectUpdated(args);
            }
            else
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Data = args.Previous.ToJson();
                audit.Action = UserAction.Deleted;
                audit.AdminArea = AdminAreaVisibility.Project;
                audit.RowName = audit.ValueBefore = args.Previous.Name;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        public void AfterProjectUpdated(ProjectEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Code.Equals(args.Entity.Code))
            {
                changedValues.Add(new Triplet() { First = "projectcode", Second = args.Previous.Code, Third = args.Entity.Code });
            }

            if (!args.Previous.Name.Equals(args.Entity.Name))
            {
                changedValues.Add(new Triplet() { First = "projectname", Second = args.Previous.Name, Third = args.Entity.Name });
            }

            if (!args.Previous.Description.Equals(args.Entity.Description))
            {
                changedValues.Add(new Triplet() { First = "projectdesc", Second = args.Previous.Description, Third = args.Entity.Description });
            }

            if (!args.Previous.ReadOnly.Equals(args.Entity.ReadOnly))
            {
                changedValues.Add(new Triplet() { First = "projectreadonly", Second = args.Previous.ReadOnly, Third = args.Entity.ReadOnly });
            }

            if (!args.Previous.Archived.Equals(args.Entity.Archived))
            {
                changedValues.Add(new Triplet() { First = "projectarchived", Second = args.Previous.Archived, Third = args.Entity.Archived });
            }

            if (!args.Previous.ResourceAssignmentMode.Equals(args.Entity.ResourceAssignmentMode))
            {
                changedValues.Add(new Triplet() { First = "resourcemode", Second = args.Previous.ResourceAssignmentMode, Third = args.Entity.ResourceAssignmentMode });
            }

            if (!args.Previous.ComponentAssignmentMode.Equals(args.Entity.ComponentAssignmentMode))
            {
                changedValues.Add(new Triplet() { First = "componentmode", Second = args.Previous.ComponentAssignmentMode, Third = args.Entity.ComponentAssignmentMode });
            }

            if (!args.Previous.PermissionId.GetValueOrDefault().Equals(args.Entity.PermissionId.GetValueOrDefault()))
            {
                var data = new Triplet() { First = "globalschemeid" };
                data.Second = args.Previous.PermissionId.HasValue ? args.Context.Permissions.Get(args.Previous.PermissionId.Value).Name : string.Empty;
                data.Third = args.Entity.PermissionId.HasValue ? args.Context.Permissions.Get(args.Entity.PermissionId.Value).Name : string.Empty;

                changedValues.Add(data);                                
            }

            if (!args.Previous.LeadId.Equals(args.Entity.LeadId))
            {
                var data = new Triplet() { First = "userid" };
                data.Second = args.Previous.LeadId != 0 ? args.Context.Users.Get(args.Previous.LeadId).Fullname : string.Empty;
                data.Third = args.Entity.LeadId != 0 ? args.Context.Users.Get(args.Entity.LeadId).Fullname : string.Empty;

                changedValues.Add(data);
            }

            if (!args.Previous.LabelId.GetValueOrDefault().Equals(args.Entity.LabelId.GetValueOrDefault()))
            {
                var data = new Triplet() { First = "projectlabelid"};
                data.Second = args.Previous.LabelId.HasValue ? args.Context.ProjectLabels.Get(args.Previous.LabelId.Value).Name : string.Empty;
                data.Third = args.Entity.LabelId.HasValue ? args.Context.ProjectLabels.Get(args.Entity.LabelId.Value).Name : string.Empty;

                changedValues.Add(data);
            }

            if (!args.Previous.TemplateId.Equals(args.Entity.TemplateId))
            {
                var data = new Triplet() { First = "templateid" };
                data.Second = args.Previous.TemplateId != 0 ? args.Context.ProjectTemplates.Get(args.Previous.TemplateId).Name : string.Empty;
                data.Third = args.Entity.TemplateId != 0 ? args.Context.ProjectTemplates.Get(args.Entity.TemplateId).Name : string.Empty;

                changedValues.Add(data);
            }

            if (!args.Previous.Color.Equals(args.Entity.Color))
            {
                changedValues.Add(new Triplet() { First = "color", Second = args.Previous.Color, Third = args.Entity.Color });
            }

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.Project;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Name;
                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion

        #region PROJECT LABEL

        public void AfterProjectLabelCreated(ProjectLabelEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.ProjectLabel;
            audit.RowName = audit.ValueAfter = args.Entity.Name;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterProjectLabelDeleted(ProjectLabelEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.ProjectLabel;
            audit.RowName = audit.ValueBefore = args.Previous.Name;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterProjectLabelUpdated(ProjectLabelEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Name.Equals(args.Entity.Name))
            {
                changedValues.Add(new Triplet() { First = "labelname", Second = args.Previous.Name, Third = args.Entity.Name });
            }

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.ProjectLabel;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Name;
                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion
    }

}
