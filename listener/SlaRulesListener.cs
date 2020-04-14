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
using Countersoft.Gemini.Commons.Permissions;


namespace AdminAudit
{
    [AppType(AppTypeEnum.Event),
    AppControlGuid("B56E25B4-197E-4F7C-B3DB-B404C238B67B"),
    AppGuid(CustomConstants.APPGUID),
    AppAuthor(CustomConstants.APPAUTHOR),
    AppName(CustomConstants.APPNAME),
    AppDescription(CustomConstants.APPDESCRIPTION),
    AppRequiresConfigScreen(true)]
    [OutputCache(Duration = 0, NoStore = true, Location = System.Web.UI.OutputCacheLocation.None)]
    public class SlaRulesListener : IAfterSLAListener, IAfterRulesActionsListener
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string AppGuid { get; set; }


        #region SLA

        public void AfterSLACreated(SLAEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.Sla;
            audit.RowName = audit.ValueAfter = args.Entity.Name;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterSLADeleted(SLAEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.Sla;
            audit.RowName = audit.ValueBefore = args.Previous.Name;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterSLAUpdated(SLAEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Name.Equals(args.Entity.Name))
            {
                changedValues.Add(new Triplet() { First = "name", Second = args.Previous.Name, Third = args.Entity.Name });
            }

            if (!args.Previous.Description.Equals(args.Entity.Description))
            {
                changedValues.Add(new Triplet() { First = "sladesc", Second = args.Previous.Description, Third = args.Entity.Description });
            }

            if (!args.Previous.Sequence.Equals(args.Entity.Sequence))
            {
                changedValues.Add(new Triplet() { First = "sequence", Second = args.Previous.Sequence, Third = args.Entity.Sequence });
            }

            if (!args.Previous.EntryRules.GetValueOrDefault().Equals(args.Entity.EntryRules.GetValueOrDefault()))
            {
                changedValues.Add(new Triplet() { First = "startrules", Second = args.Previous.EntryRules.GetValueOrDefault(), Third = args.Entity.EntryRules.GetValueOrDefault() });
            }

            if (!args.Previous.PauseRules.GetValueOrDefault().Equals(args.Entity.PauseRules.GetValueOrDefault()))
            {
                changedValues.Add(new Triplet() { First = "pauserules", Second = args.Previous.PauseRules.GetValueOrDefault(), Third = args.Entity.PauseRules.GetValueOrDefault() });
            }

            if (!args.Previous.ResumeRules.GetValueOrDefault().Equals(args.Entity.ResumeRules.GetValueOrDefault()))
            {
                changedValues.Add(new Triplet() { First = "resumerules", Second = args.Previous.ResumeRules.GetValueOrDefault(), Third = args.Entity.ResumeRules.GetValueOrDefault() });
            }

            if (!args.Previous.StopRules.GetValueOrDefault().Equals(args.Entity.StopRules.GetValueOrDefault()))
            {
                changedValues.Add(new Triplet() { First = "stoprules", Second = args.Previous.StopRules.GetValueOrDefault(), Third = args.Entity.StopRules.GetValueOrDefault() });
            }

            if (!args.Previous.Time.Equals(args.Entity.Time))
            {
                changedValues.Add(new Triplet() { First = "interval", Second = args.Previous.Time, Third = args.Entity.Time });
            }

            if (!args.Previous.Is24x7.Equals(args.Entity.Is24x7))
            {
                changedValues.Add(new Triplet() { First = "is24x7", Second = args.Previous.Is24x7, Third = args.Entity.Is24x7 });
            }

            if (!args.Previous.StartDayHour.Equals(args.Entity.StartDayHour))
            {
                changedValues.Add(new Triplet() { First = "startdayhour", Second = args.Previous.StartDayHour, Third = args.Entity.StartDayHour });
            }

            if (!args.Previous.StartDayMinute.Equals(args.Entity.StartDayMinute))
            {
                changedValues.Add(new Triplet() { First = "startdayminute", Second = args.Previous.StartDayMinute, Third = args.Entity.StartDayMinute });
            }

            if (!args.Previous.EndDayHour.Equals(args.Entity.EndDayHour))
            {
                changedValues.Add(new Triplet() { First = "enddayhour", Second = args.Previous.EndDayHour, Third = args.Entity.EndDayHour });
            }

            if (!args.Previous.EndDayMinute.Equals(args.Entity.EndDayMinute))
            {
                changedValues.Add(new Triplet() { First = "enddayminute", Second = args.Previous.EndDayMinute, Third = args.Entity.EndDayMinute });
            }

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.Sla;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Name;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion

        #region Rules

        public void AfterRulesActionsCreated(RulesActionsEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.Rules;

            if (args.Entity.SLAId.HasValue && args.Entity.Name.IsEmpty())
            {
                var sla = args.Context.SLAs.Get(args.Entity.SLAId.Value);
                audit.RowName = audit.ValueAfter = string.Concat("Sla - ", sla.Name);
            }
            else
                audit.RowName = audit.ValueAfter = args.Entity.Name;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterRulesActionsDeleted(RulesActionsEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.Rules;
 
            if (args.Previous.SLAId.HasValue && args.Previous.Name.IsEmpty())
            {
                var sla = args.Context.SLAs.Get(args.Previous.SLAId.Value);
                audit.RowName = audit.ValueBefore = string.Concat("Sla - ", sla.Name);
            }
            else
                audit.RowName = audit.ValueBefore = args.Previous.Name;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterRulesActionsUpdated(RulesActionsEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Name.Equals(args.Entity.Name))
            {
                changedValues.Add(new Triplet() { First = "name", Second = args.Previous.Name, Third = args.Entity.Name });
            }

            if (!args.Previous.Description.Equals(args.Entity.Description))
            {
                changedValues.Add(new Triplet() { First = "longdesc", Second = args.Previous.Description, Third = args.Entity.Description });
            }

            if (!args.Previous.Sequence.Equals(args.Entity.Sequence))
            {
                changedValues.Add(new Triplet() { First = "sequence", Second = args.Previous.Sequence, Third = args.Entity.Sequence });
            }

            if (!args.Previous.StopProcessing.Equals(args.Entity.StopProcessing))
            {
                changedValues.Add(new Triplet() { First = "stopprocessing", Second = args.Previous.StopProcessing, Third = args.Entity.StopProcessing });
            }

            if (!args.Previous.SLAId.GetValueOrDefault().Equals(args.Entity.SLAId.GetValueOrDefault()))
            {
                changedValues.Add(new Triplet() { First = "slaid", Second = args.Previous.SLAId.GetValueOrDefault(), Third = args.Entity.SLAId.GetValueOrDefault() });
            }

            if (!args.Previous.OnBeforeUpdate.Equals(args.Entity.OnBeforeUpdate))
            {
                changedValues.Add(new Triplet() { First = "onbeforeupdate", Second = args.Previous.OnBeforeUpdate, Third = args.Entity.OnBeforeUpdate });
            }

            if (!args.Previous.OnBeforeCreate.Equals(args.Entity.OnBeforeCreate))
            {
                changedValues.Add(new Triplet() { First = "onbeforecreate", Second = args.Previous.OnBeforeCreate, Third = args.Entity.OnBeforeCreate });
            }

            if (!args.Previous.OnAfterUpdate.Equals(args.Entity.OnAfterUpdate))
            {
                changedValues.Add(new Triplet() { First = "onafterupdate", Second = args.Previous.OnAfterUpdate, Third = args.Entity.OnAfterUpdate });
            }

            if (!args.Previous.OnAfterCreate.Equals(args.Entity.OnAfterCreate))
            {
                changedValues.Add(new Triplet() { First = "onaftercreate", Second = args.Previous.OnAfterCreate, Third = args.Entity.OnAfterCreate });
            }

            if (!args.Previous.OnTimer.Equals(args.Entity.OnTimer))
            {
                changedValues.Add(new Triplet() { First = "ontimer", Second = args.Previous.OnTimer, Third = args.Entity.OnTimer });
            }

            if (!args.Previous.OneTimeAction.Equals(args.Entity.OneTimeAction))
            {
                changedValues.Add(new Triplet() { First = "onetimeaction", Second = args.Previous.OneTimeAction, Third = args.Entity.OneTimeAction });
            }

            if (!args.Previous.Rules.ToJson().Equals(args.Entity.Rules.ToJson()))
            {
                changedValues.Add(new Triplet() { First = "rules", Second = args.Previous.Rules.ToJson(), Third = args.Entity.Rules.ToJson() });
            }

            if (!args.Previous.Actions.ToJson().Equals(args.Entity.Actions.ToJson()))
            {
                changedValues.Add(new Triplet() { First = "actions", Second = args.Previous.Actions.ToJson(), Third = args.Entity.Actions.ToJson() });
            }

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.Rules;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();

                if (args.Entity.SLAId.HasValue && args.Entity.Name.IsEmpty())
                {
                    var sla = args.Context.SLAs.Get(args.Entity.SLAId.Value);
                    audit.RowName = string.Concat("Sla - ", sla.Name);
                }
                else
                {
                    audit.RowName = args.Entity.Name;
                }

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion
    }

}
