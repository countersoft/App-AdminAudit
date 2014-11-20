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

namespace AdminAudit
{
    [AppType(AppTypeEnum.Event),
    AppGuid("DA83F146-8C92-49AB-97C1-83EEDCCD3910"),
    AppControlGuid("217A7936-B38C-42EB-99FE-55067BEE0057"),
    AppAuthor("Countersoft"),
    AppName("Admin Audit"),
    AppDescription("View a history of all admin actions"),
    AppRequiresConfigScreen(true)]
    [OutputCache(Duration = 0, NoStore = true, Location = System.Web.UI.OutputCacheLocation.None)]
    public class TemplatesListener : IAfterIssueTypeListener, IAfterCustomFieldListener, IAfterIssueStatusListener, IAfterIssuePriorityListener, IAfterIssueSeverityListener, IAfterIssueResolutionListener, IAfterLinkTypeListener, IAfterTimeTypeListener, IAfterProjectTemplateListener
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string AppGuid { get; set; }

        #region PROJECT TEMPLATES

        public void AfterProjectTemplateCreated(ProjectTemplateEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.ProjectTemplate;
            audit.RowName = audit.ValueAfter = args.Entity.Name;

            AdminAuditRepository.InsertAudit(audit);   
        }

        public void AfterProjectTemplateDeleted(ProjectTemplateEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.ProjectTemplate;
            audit.RowName = audit.ValueBefore = args.Previous.Name;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterProjectTemplateUpdated(ProjectTemplateEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Name.Equals(args.Entity.Name))
            {
                changedValues.Add(new Triplet() { First = "title", Second = args.Previous.Name, Third = args.Entity.Name });
            }

            if (!args.Previous.Description.Equals(args.Entity.Description))
            {
                changedValues.Add(new Triplet() { First = "descrip", Second = args.Previous.Description, Third = args.Entity.Description });
            }

            if (!args.Previous.Publisher.Equals(args.Entity.Publisher))
            {
                changedValues.Add(new Triplet() { First = "author", Second = args.Previous.Publisher, Third = args.Entity.Publisher });
            }

            if (!args.Previous.Published.Equals(args.Entity.Published))
            {
                changedValues.Add(new Triplet() { First = "published", Second = args.Previous.Published, Third = args.Entity.Published });
            }

            if (!args.Previous.VersionNumber.Equals(args.Entity.VersionNumber))
            {
                changedValues.Add(new Triplet() { First = "versionnumber", Second = args.Previous.VersionNumber, Third = args.Entity.VersionNumber });
            }

            if (!args.Previous.Template.Equals(args.Entity.Template))
            {
                changedValues.Add(new Triplet() { First = "template", Second = args.Previous.Template, Third = args.Entity.Template });
            }

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.ProjectTemplate;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Name;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion

        #region TYPES

        public void AfterIssueTypeCreated(IssueTypeEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.Process;
            audit.RowName = audit.ValueAfter = args.Entity.Label;

            AdminAuditRepository.InsertAudit(audit);           
        }

        public void AfterIssueTypeDeleted(IssueTypeEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.Process;
            audit.RowName = audit.ValueBefore = args.Previous.Label;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterIssueTypeUpdated(IssueTypeEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Color.Equals(args.Entity.Color))
            {
                changedValues.Add(new Triplet() { First = "color", Second = args.Previous.Color, Third = args.Entity.Color });
            }

            if (!args.Previous.Image.Equals(args.Entity.Image))
            {
                changedValues.Add(new Triplet() { First = "imagepath", Second = args.Previous.Image, Third = args.Entity.Image });
            }

            if (!args.Previous.Label.Equals(args.Entity.Label))
            {
                changedValues.Add(new Triplet() { First = "typedesc", Second = args.Previous.Label, Third = args.Entity.Label });
            }

            if (args.Previous.Sequence != args.Entity.Sequence)
            {
                changedValues.Add(new Triplet() { First = "seq", Second = args.Previous.Sequence, Third = args.Entity.Sequence });
            }
           
            //These date change on every save but don't mean anything in screens was changed
            var previousRevised = args.Previous.Screen.Revised;
            var previousCreated = args.Previous.Screen.Created;

            var currentRevised = args.Entity.Screen.Revised;
            var currentCreated = args.Entity.Screen.Created;

            args.Previous.Screen.Revised = DateTime.Today;
            args.Entity.Screen.Revised = DateTime.Today;

            args.Previous.Screen.Created = DateTime.Today;
            args.Entity.Screen.Created = DateTime.Today;

            if ((args.Previous.Screen.ReferenceId == 0 && args.Entity.Screen.ReferenceId == 0 && !args.Previous.Screen.ToJson().Equals(args.Entity.Screen.ToJson())) || args.Previous.Screen.ReferenceId != args.Entity.Screen.ReferenceId)
            {
                args.Previous.Screen.Revised = previousRevised;
                args.Previous.Screen.Created = previousCreated;

                args.Entity.Screen.Revised = currentRevised;
                args.Entity.Screen.Created = currentCreated;
                
                changedValues.Add(new Triplet() { First = "screen", Second = args.Previous.Screen.ToJson(), Third = args.Entity.Screen.ToJson() });
            }

            if ((args.Previous.Workflow.ReferenceId == 0 && args.Entity.Workflow.ReferenceId == 0 && !args.Previous.Workflow.ToJson().Equals(args.Entity.Workflow.ToJson())) || args.Previous.Workflow.ReferenceId != args.Entity.Workflow.ReferenceId)
            {
                changedValues.Add(new Triplet() { First = "workflow", Second = args.Previous.Workflow.ToJson(), Third = args.Entity.Workflow.ToJson() });
            }           

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.Process;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Label;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion

        #region CUSTOM FIELDS

        public void AfterCustomFieldCreated(CustomFieldEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.CustomField;
            audit.RowName = audit.ValueAfter = args.Entity.Name;

            AdminAuditRepository.InsertAudit(audit); 
        }

        public void AfterCustomFieldDeleted(CustomFieldEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.CustomField;
            audit.RowName = audit.ValueBefore = args.Previous.Name;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterCustomFieldUpdated(CustomFieldEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Name.Equals(args.Entity.Name))
            {
                changedValues.Add(new Triplet() { First = "customfieldname", Second = args.Previous.Name, Third = args.Entity.Name });
            }

            if (!args.Previous.ScreenLabel.Equals(args.Entity.ScreenLabel))
            {
                changedValues.Add(new Triplet() { First = "screenlabel", Second = args.Previous.ScreenLabel, Third = args.Entity.ScreenLabel });
            }

            if (!args.Previous.ScreenDescription.Equals(args.Entity.ScreenDescription))
            {
                changedValues.Add(new Triplet() { First = "screendescription", Second = args.Previous.ScreenDescription, Third = args.Entity.ScreenDescription });
            }

            if (!args.Previous.ScreenTooltip.Equals(args.Entity.ScreenTooltip))
            {
                changedValues.Add(new Triplet() { First = "screentooltip", Second = args.Previous.ScreenTooltip, Third = args.Entity.ScreenTooltip });
            }

            if (!args.Previous.MaxLen.Equals(args.Entity.MaxLen))
            {
                changedValues.Add(new Triplet() { First = "maxlen", Second = args.Previous.MaxLen, Third = args.Entity.MaxLen });
            }

            if (!args.Previous.CanMultiSelect.Equals(args.Entity.CanMultiSelect))
            {
                changedValues.Add(new Triplet() { First = "canmultiselect", Second = args.Previous.CanMultiSelect, Third = args.Entity.CanMultiSelect });
            }

            if (!args.Previous.CanFilter.Equals(args.Entity.CanFilter))
            {
                changedValues.Add(new Triplet() { First = "canfilter", Second = args.Previous.CanFilter, Third = args.Entity.CanFilter });
            }

            if (!args.Previous.RegularExpression.Equals(args.Entity.RegularExpression))
            {
                changedValues.Add(new Triplet() { First = "regularexp", Second = args.Previous.RegularExpression, Third = args.Entity.RegularExpression });
            }

            if (!args.Previous.Type.Equals(args.Entity.Type))
            {
                changedValues.Add(new Triplet() { First = "customfieldtype", Second = Helper.GetCustomFieldFullName(args.Previous.Type), Third = Helper.GetCustomFieldFullName(args.Entity.Type) });
            }

            if (!args.Previous.UseStaticData.Equals(args.Entity.UseStaticData))
            {
                changedValues.Add(new Triplet() { First = "usestaticdata", Second = args.Previous.UseStaticData, Third = args.Entity.UseStaticData });
            }

            if (!args.Previous.LookupName.Equals(args.Entity.LookupName))
            {
                changedValues.Add(new Triplet() { First = "lookupname", Second = args.Previous.LookupName, Third = args.Entity.LookupName });
            }

            if (!args.Previous.CascadingParentField.Equals(args.Entity.CascadingParentField))
            {
                changedValues.Add(new Triplet() { First = "cascadingparentfield", Second = args.Previous.CascadingParentField, Third = args.Entity.CascadingParentField });
            }

            if (!args.Previous.CascadingLookupValueField.Equals(args.Entity.CascadingLookupValueField))
            {
                changedValues.Add(new Triplet() { First = "cascadinglookupvaluefield", Second = args.Previous.CascadingLookupValueField, Third = args.Entity.CascadingLookupValueField });
            }

            if (!args.Previous.LookupTextField.Equals(args.Entity.LookupTextField))
            {
                changedValues.Add(new Triplet() { First = "lookuptextfield", Second = args.Previous.LookupTextField, Third = args.Entity.LookupTextField });
            }

            if (!args.Previous.LookupSortField.Equals(args.Entity.LookupSortField))
            {
                changedValues.Add(new Triplet() { First = "lookupsortfield", Second = args.Previous.LookupSortField, Third = args.Entity.LookupSortField });
            }

            if (!args.Previous.ProjectIdFilter.Equals(args.Entity.ProjectIdFilter))
            {
                changedValues.Add(new Triplet() { First = "projectidfilter", Second = args.Previous.ProjectIdFilter, Third = args.Entity.ProjectIdFilter });
            }

            if (!args.Previous.ShowInAttributes.Equals(args.Entity.ShowInAttributes))
            {
                changedValues.Add(new Triplet() { First = "showinline", Second = args.Previous.ShowInAttributes, Third = args.Entity.ShowInAttributes });
            }

            if (!args.Previous.MaxValue.Equals(args.Entity.MaxValue))
            {
                changedValues.Add(new Triplet() { First = "maxvalue", Second = args.Previous.MaxValue, Third = args.Entity.MaxValue });
            }

            if (!args.Previous.MinValue.Equals(args.Entity.MinValue))
            {
                changedValues.Add(new Triplet() { First = "minvalue", Second = args.Previous.MinValue, Third = args.Entity.MinValue });
            }

            if (!args.Previous.LookupStaticData.Equals(args.Entity.LookupStaticData))
            {
                changedValues.Add(new Triplet() { First = "lookupdata", Second = args.Previous.LookupStaticData, Third = args.Entity.LookupStaticData });
            }

            if (!args.Previous.ListLimiter.Equals(args.Entity.ListLimiter))
            {
                changedValues.Add(new Triplet() { First = "listlimiter", Second = Helper.GetListLimiterFullName(args.Previous, args), Third = Helper.GetListLimiterFullName(args.Entity, args) });
            }

            if (!args.Previous.UsedIn.Equals(args.Entity.UsedIn))
            {
                changedValues.Add(new Triplet() { First = "usedin", Second = args.Previous.UsedIn, Third = args.Entity.UsedIn });
            }

            if (!args.Previous.TemplateId.Equals(args.Entity.TemplateId))
            {
                changedValues.Add(new Triplet() { First = "templateid", Second = args.Previous.TemplateId, Third = args.Entity.TemplateId });
            }

            if (!args.Previous.AllowNoSelection.Equals(args.Entity.AllowNoSelection))
            {
                changedValues.Add(new Triplet() { First = "allownoselection", Second = args.Previous.AllowNoSelection, Third = args.Entity.AllowNoSelection });
            }

            if (!args.Previous.AutoComplete.Equals(args.Entity.AutoComplete))
            {
                changedValues.Add(new Triplet() { First = "autocomplete", Second = args.Previous.AutoComplete, Third = args.Entity.AutoComplete });
            }


            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.CustomField;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Name;

                AdminAuditRepository.InsertAudit(audit);
            }
        }
        #endregion

        #region STATUS

        public void AfterIssueStatusCreated(IssueStatusEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.Status;
            audit.RowName = audit.ValueAfter = args.Entity.Label;

            AdminAuditRepository.InsertAudit(audit);  
        }

        public void AfterIssueStatusDeleted(IssueStatusEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.Status;
            audit.RowName = audit.ValueBefore = args.Previous.Label;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterIssueStatusUpdated(IssueStatusEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Label.Equals(args.Entity.Label))
            {
                changedValues.Add(new Triplet() { First = "statusdesc", Second = args.Previous.Label, Third = args.Entity.Label });
            }

            if (!args.Previous.Image.Equals(args.Entity.Image))
            {
                changedValues.Add(new Triplet() { First = "imagepath", Second = args.Previous.Image, Third = args.Entity.Image });
            }

            if (!args.Previous.Type.Equals(args.Entity.Type))
            {
                changedValues.Add(new Triplet() { First = "statustype", Second = args.Previous.Type, Third = args.Entity.Type });
            }

            if (!args.Previous.Comment.Equals(args.Entity.Comment))
            {
                changedValues.Add(new Triplet() { First = "comment", Second = args.Previous.Comment, Third = args.Entity.Comment });
            }

            if (!args.Previous.IsFinal.Equals(args.Entity.IsFinal))
            {
                changedValues.Add(new Triplet() { First = "isfinal", Second = args.Previous.IsFinal, Third = args.Entity.IsFinal });
            }

            if (!args.Previous.Color.Equals(args.Entity.Color))
            {
                changedValues.Add(new Triplet() { First = "color", Second = args.Previous.Color, Third = args.Entity.Color });
            }
            if (args.Previous.Sequence != args.Entity.Sequence)
            {
                changedValues.Add(new Triplet() { First = "seq", Second = args.Previous.Sequence, Third = args.Entity.Sequence });
            }           

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.Status;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Label;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion

        #region PRIORITY

        public void AfterIssuePriorityCreated(IssuePriorityEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.Priority;
            audit.RowName = audit.ValueAfter = args.Entity.Label;

            AdminAuditRepository.InsertAudit(audit);  
        }

        public void AfterIssuePriorityDeleted(IssuePriorityEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.Priority;
            audit.RowName = audit.ValueBefore = args.Previous.Label;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterIssuePriorityUpdated(IssuePriorityEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Label.Equals(args.Entity.Label))
            {
                changedValues.Add(new Triplet() { First = "prioritydesc", Second = args.Previous.Label, Third = args.Entity.Label });
            }

            if (!args.Previous.Image.Equals(args.Entity.Image))
            {
                changedValues.Add(new Triplet() { First = "imagepath", Second = args.Previous.Image, Third = args.Entity.Image });
            }

            if (!args.Previous.Color.Equals(args.Entity.Color))
            {
                changedValues.Add(new Triplet() { First = "color", Second = args.Previous.Color, Third = args.Entity.Color });
            }

            if (args.Previous.Sequence != args.Entity.Sequence)
            {
                changedValues.Add(new Triplet() { First = "seq", Second = args.Previous.Sequence, Third = args.Entity.Sequence });
            }

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.Priority;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Label;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion

        #region SEVERITY

        public void AfterIssueSeverityCreated(IssueSeverityEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.Severity;
            audit.RowName = audit.ValueAfter = args.Entity.Label;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterIssueSeverityDeleted(IssueSeverityEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.Severity;
            audit.RowName = audit.ValueBefore = args.Previous.Label;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterIssueSeverityUpdated(IssueSeverityEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Label.Equals(args.Entity.Label))
            {
                changedValues.Add(new Triplet() { First = "severitydesc", Second = args.Previous.Label, Third = args.Entity.Label });
            }

            if (!args.Previous.Image.Equals(args.Entity.Image))
            {
                changedValues.Add(new Triplet() { First = "imagepath", Second = args.Previous.Image, Third = args.Entity.Image });
            }

            if (!args.Previous.Color.Equals(args.Entity.Color))
            {
                changedValues.Add(new Triplet() { First = "color", Second = args.Previous.Color, Third = args.Entity.Color });
            }

            if (args.Previous.Sequence != args.Entity.Sequence)
            {
                changedValues.Add(new Triplet() { First = "seq", Second = args.Previous.Sequence, Third = args.Entity.Sequence });
            }

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.Severity;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Label;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion

        #region RESOLUTION

        public void AfterIssueResolutionCreated(IssueResolutionEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.Resolution;
            audit.RowName = audit.ValueAfter = args.Entity.Label;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterIssueResolutionDeleted(IssueResolutionEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.Resolution;
            audit.RowName = audit.ValueBefore = args.Previous.Label;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterIssueResolutionUpdated(IssueResolutionEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Label.Equals(args.Entity.Label))
            {
                changedValues.Add(new Triplet() { First = "resdesc", Second = args.Previous.Label, Third = args.Entity.Label });
            }

            if (!args.Previous.IsResolved.Equals(args.Entity.IsResolved))
            {
                changedValues.Add(new Triplet() { First = "isfinal", Second = args.Previous.IsResolved, Third = args.Entity.IsResolved });
            }

            if (args.Previous.Sequence != args.Entity.Sequence)
            {
                changedValues.Add(new Triplet() { First = "seq", Second = args.Previous.Sequence, Third = args.Entity.Sequence });
            }

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.Resolution;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Label;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion

        #region LINK TYPES

        public void AfterLinkTypeCreated(LinkTypeEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.LinkType;
            audit.RowName = audit.ValueAfter = args.Entity.Label;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterLinkTypeDeleted(LinkTypeEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.LinkType;
            audit.RowName = audit.ValueBefore = args.Previous.Label;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterLinkTypeUpdated(LinkTypeEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Label.Equals(args.Entity.Label))
            {
                changedValues.Add(new Triplet() { First = "linkname", Second = args.Previous.Label, Third = args.Entity.Label });
            }

            if (!args.Previous.Description.Equals(args.Entity.Description))
            {
                changedValues.Add(new Triplet() { First = "linkdesc", Second = args.Previous.Description, Third = args.Entity.Description });
            }

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.LinkType;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Label;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion

        #region TIME

        public void AfterTimeTypeCreated(TimeTypeEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.TimeType;
            audit.RowName = audit.ValueAfter = args.Entity.Label;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterTimeTypeDeleted(TimeTypeEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.TimeType;
            audit.RowName = audit.ValueBefore = args.Previous.Label;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterTimeTypeUpdated(TimeTypeEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Label.Equals(args.Entity.Label))
            {
                changedValues.Add(new Triplet() { First = "timetypename", Second = args.Previous.Label, Third = args.Entity.Label });
            }

            if (!args.Previous.Description.Equals(args.Entity.Description))
            {
                changedValues.Add(new Triplet() { First = "timetypedesc", Second = args.Previous.Description, Third = args.Entity.Description });
            }

            if (args.Previous.Sequence != args.Entity.Sequence)
            {
                changedValues.Add(new Triplet() { First = "seq", Second = args.Previous.Sequence, Third = args.Entity.Sequence });
            }

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.TimeType;
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
