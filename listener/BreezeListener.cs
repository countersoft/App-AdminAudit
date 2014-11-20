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
using Countersoft.Gemini.Commons.Meta;
using Countersoft.Gemini.Commons.Permissions;
using Countersoft.Gemini.Contracts.Business;
using Microsoft.Practices.Unity;

namespace AdminAudit
{
    [AppType(AppTypeEnum.Event),
    AppGuid("DA83F146-8C92-49AB-97C1-83EEDCCD3910"),
    AppControlGuid("F089D78B-08F7-4300-9026-3789B380E9BB"),
    AppAuthor("Countersoft"),
    AppName("Admin Audit"),
    AppDescription("View a history of all admin actions"),
    AppRequiresConfigScreen(true)]
    [OutputCache(Duration = 0, NoStore = true, Location = System.Web.UI.OutputCacheLocation.None)]
    public class BreezeListener : IAfterEnquiryQueueListener, IAfterEnquirySmtpServerListener, IAfterEnquiryMailboxListener, IAfterMatchExpressionListener, IAfterBreezeReplyOptionsListener
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string AppGuid { get; set; }

        #region Enquiry Queues

        public void AfterEnquiryQueueCreated(EnquiryQueueEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.BreezeQueue;
            audit.RowName = audit.ValueAfter = args.Entity.Name;
            
            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterEnquiryQueueDeleted(EnquiryQueueEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.BreezeQueue;
            audit.RowName = audit.ValueBefore = args.Previous.Name;
            
            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterEnquiryQueueUpdated(EnquiryQueueEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Name.Equals(args.Entity.Name))
            {
                changedValues.Add(new Triplet() { First = "name", Second = args.Previous.Name, Third = args.Entity.Name });
            }

            if (!args.Previous.Description.Equals(args.Entity.Description))
            {
                changedValues.Add(new Triplet() { First = "queuedescription", Second = args.Previous.Description, Third = args.Entity.Description });
            }

            if (!args.Previous.UserId.Equals(args.Entity.UserId))
            {
                changedValues.Add(new Triplet() { First = "userid", Second = args.Previous.UserId, Third = args.Entity.UserId });
            }

            if (!args.Previous.Color.Equals(args.Entity.Color))
            {
                changedValues.Add(new Triplet() { First = "colour", Second = args.Previous.Color, Third = args.Entity.Color });
            }

            if (!args.Previous.Sequence.Equals(args.Entity.Sequence))
            {
                changedValues.Add(new Triplet() { First = "sequence", Second = args.Previous.Sequence, Third = args.Entity.Sequence });
            }

            if (!args.Previous.Enabled.Equals(args.Entity.Enabled))
            {
                changedValues.Add(new Triplet() { First = "active", Second = args.Previous.Enabled, Third = args.Entity.Enabled });
            }

            if (!args.Previous.ProjectId.Equals(args.Entity.ProjectId))
            {
                changedValues.Add(new Triplet() { First = "projectid", Second = args.Context.Projects.Get(args.Previous.ProjectId).Name, Third = args.Context.Projects.Get(args.Entity.ProjectId).Name });
            }
           
            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.BreezeQueue;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Name;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion

        #region Enquiry Smtp

        public void AfterEnquirySmtpServerCreated(EnquirySmtpServerEventArgs args)
        {                   
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.BreezeSmtp;
            audit.RowName = audit.ValueAfter = args.Entity.Name;
           
            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterEnquirySmtpServerDeleted(EnquirySmtpServerEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.BreezeSmtp;
            audit.RowName = audit.ValueBefore = args.Previous.Name;
            
            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterEnquirySmtpServerUpdated(EnquirySmtpServerEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Name.Equals(args.Entity.Name))
            {
                changedValues.Add(new Triplet() { First = "smtpservername", Second = args.Previous.Name, Third = args.Entity.Name });
            }

            if (!args.Previous.Server.Equals(args.Entity.Server))
            {
                changedValues.Add(new Triplet() { First = "server", Second = args.Previous.Server, Third = args.Entity.Server });
            }

            if (!args.Previous.ServerPort.Equals(args.Entity.ServerPort))
            {
                changedValues.Add(new Triplet() { First = "serverport", Second = args.Previous.ServerPort, Third = args.Entity.ServerPort });
            }

            if (!args.Previous.Username.Equals(args.Entity.Username))
            {
                changedValues.Add(new Triplet() { First = "username", Second = args.Previous.Username, Third = args.Entity.Username });
            }

            if (!args.Previous.Password.Equals(args.Entity.Password))
            {
                changedValues.Add(new Triplet() { First = "password", Second = "***", Third = "***" });
            }

            if (!args.Previous.EncodingType.Equals(args.Entity.EncodingType))
            {
                changedValues.Add(new Triplet() { First = "encodingtype", Second = args.Previous.EncodingType, Third = args.Entity.EncodingType });
            }

            if (!args.Previous.UseSsl.Equals(args.Entity.UseSsl))
            {
                changedValues.Add(new Triplet() { First = "usessl", Second = args.Previous.UseSsl, Third = args.Entity.UseSsl });
            }

            if (!args.Previous.SslMode.Equals(args.Entity.SslMode))
            {
                changedValues.Add(new Triplet() { First = "sslmode", Second = args.Previous.SslMode, Third = args.Entity.SslMode });
            }

            if (!args.Previous.AuthMode.Equals(args.Entity.AuthMode))
            {
                changedValues.Add(new Triplet() { First = "authmode", Second = args.Previous.AuthMode, Third = args.Entity.AuthMode });
            }

            if (!args.Previous.PopBeforeSmtp.Equals(args.Entity.PopBeforeSmtp))
            {
                changedValues.Add(new Triplet() { First = "popbeforesmtp", Second = args.Previous.PopBeforeSmtp, Third = args.Entity.PopBeforeSmtp });
            }
            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.BreezeSmtp;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Name;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion

        #region Enquiry Mailbox

        public void AfterEnquiryMailboxCreated(EnquiryMailboxEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.BreezeMailbox;
            audit.RowName = audit.ValueAfter = args.Entity.Name;
           
            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterEnquiryMailboxDeleted(EnquiryMailboxEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.BreezeMailbox;
            audit.ValueBefore = args.Previous.Name;
            audit.RowName = args.Previous.Name;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterEnquiryMailboxUpdated(EnquiryMailboxEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.QueueId.Equals(args.Entity.QueueId))
            {
                var oldQueueName = args.Context.EnquiryQueues.Get(args.Previous.QueueId).Name;
                var newQueueName = args.Context.EnquiryQueues.Get(args.Entity.QueueId).Name;

                changedValues.Add(new Triplet() { First = "queueid", Second = oldQueueName, Third = newQueueName });
            }

            if (!args.Previous.Name.Equals(args.Entity.Name))
            {
                changedValues.Add(new Triplet() { First = "mailboxname", Second = args.Previous.Name, Third = args.Entity.Name });
            }

            if (!args.Previous.Server.Equals(args.Entity.Server))
            {
                changedValues.Add(new Triplet() { First = "server", Second = args.Previous.Server, Third = args.Entity.Server });
            }

            if (!args.Previous.ServerPort.Equals(args.Entity.ServerPort))
            {
                changedValues.Add(new Triplet() { First = "serverport", Second = args.Previous.ServerPort, Third = args.Entity.ServerPort });
            }

            if (!args.Previous.Domain.Equals(args.Entity.Domain))
            {
                changedValues.Add(new Triplet() { First = "domain", Second = args.Previous.Domain, Third = args.Entity.Domain });
            }

            if (!args.Previous.ExchangeVersion.Equals(args.Entity.ExchangeVersion))
            {
                changedValues.Add(new Triplet() { First = "exchangeversion", Second = args.Previous.ExchangeVersion.ToString(), Third = args.Entity.ExchangeVersion.ToString() });
            }

            if (!args.Previous.EmailAddress.Equals(args.Entity.EmailAddress))
            {
                changedValues.Add(new Triplet() { First = "emailaddress", Second = args.Previous.EmailAddress, Third = args.Entity.EmailAddress });
            }

            if (!args.Previous.Username.Equals(args.Entity.Username))
            {
                changedValues.Add(new Triplet() { First = "username", Second = args.Previous.Username, Third = args.Entity.Username });
            }

            if (!args.Previous.Password.Equals(args.Entity.Password))
            {
                changedValues.Add(new Triplet() { First = "password", Second = "***", Third = "***" });
            }

            if (!args.Previous.AuthenticationMode.ToString().Equals(args.Entity.AuthenticationMode.ToString()))
            {
                changedValues.Add(new Triplet() { First = "authenticationmode", Second = args.Previous.AuthenticationMode.ToString(), Third = args.Entity.AuthenticationMode.ToString() });
            }

            if (!args.Previous.UseSsl.Equals(args.Entity.UseSsl))
            {
                changedValues.Add(new Triplet() { First = "usessl", Second = args.Previous.UseSsl, Third = args.Entity.UseSsl });
            }

            if (!args.Previous.DeleteMessages.Equals(args.Entity.DeleteMessages))
            {
                changedValues.Add(new Triplet() { First = "deletemessages", Second = args.Previous.DeleteMessages, Third = args.Entity.DeleteMessages });
            }

            if (!args.Previous.DebugMode.Equals(args.Entity.DebugMode))
            {
                changedValues.Add(new Triplet() { First = "debugmode", Second = args.Previous.DebugMode, Third = args.Entity.DebugMode });
            }

            if (!args.Previous.UserId.Equals(args.Entity.UserId))
            {
                changedValues.Add(new Triplet() { First = "userid", Second = args.Previous.UserId, Third = args.Entity.UserId });
            }

            if (!args.Previous.UseSenderAsSubmitter.Equals(args.Entity.UseSenderAsSubmitter))
            {
                changedValues.Add(new Triplet() { First = "usesenderassubmitter", Second = args.Previous.UseSenderAsSubmitter, Third = args.Entity.UseSenderAsSubmitter });
            }

            if (!args.Previous.Mode.Equals(args.Entity.Mode))
            {
                changedValues.Add(new Triplet() { First = "mode", Second = args.Previous.Mode.ToString(), Third = args.Entity.Mode.ToString() });
            }

            if (!args.Previous.ImapFolder.Equals(args.Entity.ImapFolder))
            {
                changedValues.Add(new Triplet() { First = "imapfolder", Second = args.Previous.ImapFolder, Third = args.Entity.ImapFolder });
            }

            if (!args.Previous.BlackList.Equals(args.Entity.BlackList))
            {
                changedValues.Add(new Triplet() { First = "blacklist", Second = args.Previous.BlackList, Third = args.Entity.BlackList });
            }

            if (!args.Previous.WhiteList.Equals(args.Entity.WhiteList))
            {
                changedValues.Add(new Triplet() { First = "whitelist", Second = args.Previous.WhiteList, Third = args.Entity.WhiteList });
            }

            if (!args.Previous.NoReplyList.Equals(args.Entity.NoReplyList))
            {
                changedValues.Add(new Triplet() { First = "noreplylist", Second = args.Previous.NoReplyList, Third = args.Entity.NoReplyList });
            }

            if (!args.Previous.AlertTemplateId.GetValueOrDefault().Equals(args.Entity.AlertTemplateId.GetValueOrDefault()))
            {
                IAlertTemplates alerts = GeminiApp.Container.Resolve<IAlertTemplates>();

                var previousAlert = args.Previous.AlertTemplateId.GetValueOrDefault() == 0 ? string.Empty : alerts.FindWhere(c => c.Id == args.Previous.AlertTemplateId.GetValueOrDefault()).First().Label;
                var currentAlert  = args.Entity.AlertTemplateId.GetValueOrDefault() == 0 ? string.Empty : alerts.FindWhere(c => c.Id == args.Entity.AlertTemplateId.GetValueOrDefault()).First().Label;

                changedValues.Add(new Triplet() { First = "alerttemplateid", Second = previousAlert, Third = currentAlert });
            }

            if (!args.Previous.SmtpServerId.GetValueOrDefault().Equals(args.Entity.SmtpServerId.GetValueOrDefault()))
            {
                var previousSmtpServer = args.Previous.SmtpServerId.GetValueOrDefault() == 0 ? string.Empty : args.Context.EnquirySmtpServers.Get(args.Previous.SmtpServerId.GetValueOrDefault()).Name;
                var currentSmtpServer = args.Entity.SmtpServerId.GetValueOrDefault() == 0 ? string.Empty : args.Context.EnquirySmtpServers.Get(args.Entity.SmtpServerId.GetValueOrDefault()).Name;

                changedValues.Add(new Triplet() { First = "smtpserverid", Second = previousSmtpServer, Third = currentSmtpServer });
            }

            if (!args.Previous.StripSignature.Equals(args.Entity.StripSignature))
            {
                changedValues.Add(new Triplet() { First = "stripsignature", Second = args.Previous.StripSignature, Third = args.Entity.StripSignature });
            }

            if (!args.Previous.IgnoreAttachments.Equals(args.Entity.IgnoreAttachments))
            {
                changedValues.Add(new Triplet() { First = "ignoreattachments", Second = args.Previous.IgnoreAttachments, Third = args.Entity.IgnoreAttachments });
            }

            if (!args.Previous.Options.ToJson().Equals(args.Entity.Options.ToJson()))
            {
                changedValues.Add(new Triplet() { First = "options", Second = args.Previous.Options.ToJson(), Third = args.Entity.Options.ToJson() });
            }

            if (!args.Previous.SubjectLikeExpressions.Equals(args.Entity.SubjectLikeExpressions))
            {
                var expressions = args.Context.MatchExpressions.GetAll();
                
                var previousData = string.Empty;
                var currentData = string.Empty;

                if (args.Previous.SubjectLikeExpressions.HasValue())
                {
                    var previousExprepression = args.Previous.SubjectLikeExpressions.Split('|');
                    previousData = expressions.FindAll(s => previousExprepression.Contains(s.Id.ToString())).Select(s => s.Name).ToDelimited(", ");
                }

                if (args.Entity.SubjectLikeExpressions.HasValue())
                {
                    var currentExprepression = args.Entity.SubjectLikeExpressions.Split('|');
                    currentData = expressions.FindAll(s => currentExprepression.Contains(s.Id.ToString())).Select(s => s.Name).ToDelimited(", ");
                }

                changedValues.Add(new Triplet() { First = "subjectlikeexp", Second = previousData.TrimEnd(' ').TrimEnd(','), Third = currentData.TrimEnd(' ').TrimEnd(',') });
            }

            if (!args.Previous.SubjectNotLikeExpressions.Equals(args.Entity.SubjectNotLikeExpressions))
            {
                var expressions = args.Context.MatchExpressions.GetAll();

                var previousData = string.Empty;
                var currentData = string.Empty;

                if (args.Previous.SubjectNotLikeExpressions.HasValue())
                {
                    var previousExprepression = args.Previous.SubjectNotLikeExpressions.Split('|');
                    previousData = expressions.FindAll(s => previousExprepression.Contains(s.Id.ToString())).Select(s => s.Name).ToDelimited(", ");
                }

                if (args.Entity.SubjectNotLikeExpressions.HasValue())
                {
                    var currentExprepression = args.Entity.SubjectNotLikeExpressions.Split('|');
                    currentData = expressions.FindAll(s => currentExprepression.Contains(s.Id.ToString())).Select(s => s.Name).ToDelimited(", ");
                }

                changedValues.Add(new Triplet() { First = "subjectnotlikeexp", Second = previousData.TrimEnd(' ').TrimEnd(','), Third = currentData.TrimEnd(' ').TrimEnd(',') });
            }

            if (!args.Previous.ReplaceExpressions.Equals(args.Entity.ReplaceExpressions))
            {
                var expressions = args.Context.MatchExpressions.GetAll();

                var previousData = string.Empty;
                var currentData = string.Empty;

                if (args.Previous.ReplaceExpressions.HasValue())
                {
                    var previousExprepression = args.Previous.ReplaceExpressions.Split('|');
                    previousData = expressions.FindAll(s => previousExprepression.Contains(s.Id.ToString())).Select(s => s.Name).ToDelimited(", ");
                }

                if (args.Entity.ReplaceExpressions.HasValue())
                {
                    var currentExprepression = args.Entity.ReplaceExpressions.Split('|');
                    currentData = expressions.FindAll(s => currentExprepression.Contains(s.Id.ToString())).Select(s => s.Name).ToDelimited(", ");
                }

                changedValues.Add(new Triplet() { First = "replaceexp", Second = previousData.TrimEnd(' ').TrimEnd(','), Third = currentData.TrimEnd(' ').TrimEnd(',') });
            }

            if (!args.Previous.TruncateExpressions.Equals(args.Entity.TruncateExpressions))
            {
                var expressions = args.Context.MatchExpressions.GetAll();

                var previousData = string.Empty;
                var currentData = string.Empty;

                if (args.Previous.TruncateExpressions.HasValue())
                {
                    var previousExprepression = args.Previous.TruncateExpressions.Split('|');
                    previousData = expressions.FindAll(s => previousExprepression.Contains(s.Id.ToString())).Select(s => s.Name).ToDelimited(", ");
                }

                if (args.Entity.TruncateExpressions.HasValue())
                {
                    var currentExprepression = args.Entity.TruncateExpressions.Split('|');
                    currentData = expressions.FindAll(s => currentExprepression.Contains(s.Id.ToString())).Select(s => s.Name).ToDelimited(", ");
                }

                changedValues.Add(new Triplet() { First = "truncateexp", Second = previousData.TrimEnd(' ').TrimEnd(','), Third = currentData.TrimEnd(' ').TrimEnd(',') });
            }

            if (!args.Previous.ProjectId.GetValueOrDefault().Equals(args.Entity.ProjectId.GetValueOrDefault()))
            {
                var previousData = args.Previous.ProjectId.GetValueOrDefault() == 0 ? string.Empty : args.Context.Projects.Get(args.Previous.ProjectId.GetValueOrDefault()).Name;
                var currentData = args.Entity.ProjectId.GetValueOrDefault() == 0 ? string.Empty : args.Context.Projects.Get(args.Entity.ProjectId.GetValueOrDefault()).Name;
                
                changedValues.Add(new Triplet() { First = "projectid", Second = previousData, Third = currentData });
            }

            if (!args.Previous.IssueTypeId.GetValueOrDefault().Equals(args.Entity.IssueTypeId.GetValueOrDefault()))
            {
                var previousData = args.Previous.IssueTypeId.GetValueOrDefault() == 0 ? string.Empty : args.Context.Meta.TypeGet(args.Previous.IssueTypeId.GetValueOrDefault()).Label;
                var currentData = args.Entity.IssueTypeId.GetValueOrDefault() == 0 ? string.Empty : args.Context.Meta.TypeGet(args.Entity.IssueTypeId.GetValueOrDefault()).Label;

                changedValues.Add(new Triplet() { First = "issuetypeid", Second = previousData, Third = currentData });
            }


            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.BreezeSmtp;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Name;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion

        #region Match Expression

        public void AfterMatchExpressionCreated(MatchExpressionEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.BreezeExpression;
            audit.RowName = audit.ValueAfter = args.Entity.Name;
            
            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterMatchExpressionDeleted(MatchExpressionEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.BreezeExpression;
            audit.ValueBefore = args.Previous.Name;
            audit.RowName = args.Previous.Name;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterMatchExpressionUpdated(MatchExpressionEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Name.Equals(args.Entity.Name))
            {
                changedValues.Add(new Triplet() { First = "name", Second = args.Previous.Name, Third = args.Entity.Name });
            }

            if (!args.Previous.Expression.Equals(args.Entity.Expression))
            {
                changedValues.Add(new Triplet() { First = "expression", Second = args.Previous.Expression, Third = args.Entity.Expression });
            }

            if (!args.Previous.ReplaceValue.Equals(args.Entity.ReplaceValue))
            {
                changedValues.Add(new Triplet() { First = "replacevalue", Second = args.Previous.ReplaceValue, Third = args.Entity.ReplaceValue });
            }
            
            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.BreezeExpression;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Name;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion

        #region Breeze Reply

        public void AfterBreezeReplyOptionsCreated(BreezeReplyOptionsEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.BreezeReplyFrom;
            audit.RowName = audit.ValueAfter = args.Entity.Email;
           
            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterBreezeReplyOptionsDeleted(BreezeReplyOptionsEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.BreezeReplyFrom;
            audit.ValueBefore = args.Previous.Email;
            audit.RowName = args.Previous.Email;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterBreezeReplyOptionsUpdated(BreezeReplyOptionsEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Email.Equals(args.Entity.Email))
            {
                changedValues.Add(new Triplet() { First = "email", Second = args.Previous.Email, Third = args.Entity.Email });
            }

            if (!args.Previous.Projects.ToJson().Equals(args.Entity.Projects.ToJson()))
            {
                var allProjects = args.Context.Projects.GetAll();
                var previousProjects = args.Previous.Projects.Count == 0 ? string.Empty : allProjects.FindAll(s => args.Previous.Projects.Contains(s.Id)).Select(p => p.Name).ToDelimited(", ");
                var currentProjects = args.Entity.Projects.Count == 0 ? string.Empty : allProjects.FindAll(s => args.Entity.Projects.Contains(s.Id)).Select(p => p.Name).ToDelimited(", ");

                changedValues.Add(new Triplet() { First = "projects", Second = previousProjects.TrimEnd(' ').TrimEnd(','), Third = currentProjects.TrimEnd(' ').TrimEnd(',') });
            }

            if (!args.Previous.Active.Equals(args.Entity.Active))
            {
                changedValues.Add(new Triplet() { First = "active", Second = args.Previous.Active, Third = args.Entity.Active });
            }

            if (!args.Previous.UseUserFullname.Equals(args.Entity.UseUserFullname))
            {
                changedValues.Add(new Triplet() { First = "UseUserFullname", Second = args.Previous.UseUserFullname, Third = args.Entity.UseUserFullname });
            }

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.BreezeReplyFrom;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Email;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion
    }

}
