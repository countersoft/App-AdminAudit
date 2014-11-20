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
using NHibernate.Cfg.ConfigurationSchema;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using Countersoft.Gemini.Commons.Meta;
using Countersoft.Gemini.Commons.Permissions;


namespace AdminAudit
{
    [AppType(AppTypeEnum.Event),
    AppGuid("DA83F146-8C92-49AB-97C1-83EEDCCD3910"),
    AppControlGuid("8004A65F-04FC-4656-B52A-908D5FE66B73"),
    AppAuthor("Countersoft"),
    AppName("Admin Audit"),
    AppDescription("View a history of all admin actions"),
    AppRequiresConfigScreen(true)]
    [OutputCache(Duration = 0, NoStore = true, Location = System.Web.UI.OutputCacheLocation.None)]
    public class Systemistener : IAfterAlertTemplateListener
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string AppGuid { get; set; }

        #region Alert Templates

        public void AfterAlertTemplateCreated(AlertTemplateEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.SystemAlertTemplates;
            audit.RowName = audit.ValueAfter = args.Entity.Label;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterAlertTemplateDeleted(AlertTemplateEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.SystemAlertTemplates;
            audit.RowName = audit.ValueBefore = args.Previous.Label;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterAlertTemplateUpdated(AlertTemplateEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.AlertType.Equals(args.Entity.AlertType))
            {
                changedValues.Add(new Triplet() { First = "alerttype", Second = args.Previous.AlertType, Third = args.Entity.AlertType });
            }

            if (!args.Previous.Label.Equals(args.Entity.Label))
            {
                changedValues.Add(new Triplet() { First = "label", Second = args.Previous.Label, Third = args.Entity.Label });
            }

            if (!args.Previous.Content.Equals(args.Entity.Content))
            {
                changedValues.Add(new Triplet() { First = "alertcontent", Second = args.Previous.Content, Third = args.Entity.Content });
            }

            if (!args.Previous.GetAssociatedProjects().ToJson().Equals(args.Entity.GetAssociatedProjects().ToJson()))
            {
                var allProjects = args.Context.Projects.GetAll();

                var previousData = string.Empty;
                var currentData = string.Empty;
                var previousAssociatedProjects = args.Previous.GetAssociatedProjects();
                var currentAssociatedProjects = args.Entity.GetAssociatedProjects();

                if (previousAssociatedProjects.Count > 0)
                {
                    previousData = allProjects.FindAll(s => previousAssociatedProjects.Contains(s.Id)).Select(s => s.Name).ToDelimited(", ");
                }

                if (currentAssociatedProjects.Count > 0)
                {
                    currentData = allProjects.FindAll(s => currentAssociatedProjects.Contains(s.Id)).Select(s => s.Name).ToDelimited(", ");
                }

                changedValues.Add(new Triplet() { First = "projects", Second = previousData.TrimEnd(' ').TrimEnd(','), Third = currentData.TrimEnd(' ').TrimEnd(',') });
            }

            if (!args.Previous.Options.ToJson().Equals(args.Entity.Options.ToJson()))
            {
                changedValues.Add(new Triplet() { First = "templatedata", Second = args.Previous.Options.ToJson(), Third = args.Entity.Options.ToJson() });
            }



            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.SystemAlertTemplates;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Label;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion
    }

}
