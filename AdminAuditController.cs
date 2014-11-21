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
using System.Linq;
using Countersoft.Gemini.Extensibility.Apps;
using Countersoft.Gemini.Infrastructure.Apps;
using Countersoft.Gemini.Commons.Dto;
using System.Configuration;
using NHibernate.Cfg.ConfigurationSchema;
using System.Data.SqlClient;
using System.Diagnostics;
using Countersoft.Gemini.Commons.Meta;
using Countersoft.Foundation.Commons.Core;


namespace AdminAudit
{
    [AppType(AppTypeEnum.Config),
    AppControlGuid("FF74C706-49BA-4D81-8F4D-A55C26D2CADF"),
    AppKey("AdminAudit"),
    AppGuid(CustomConstants.APPGUID),
    AppAuthor(CustomConstants.APPAUTHOR),
    AppName(CustomConstants.APPNAME),
    AppDescription(CustomConstants.APPDESCRIPTION),
    AppRequiresConfigScreen(true)]
    [OutputCache(Duration = 0, NoStore = true, Location = System.Web.UI.OutputCacheLocation.None)]
    public class AdminAuditController : BaseAppController
    {
        public override WidgetResult Caption(IssueDto issue)
        {
            WidgetResult result = new WidgetResult();

            result.Success = true;

            result.Markup.Html = AppName;
          
            return result;
        }

        public override WidgetResult Configuration()
        {
            var model = new AdminAuditModel();
            model.data = AdminAuditRepository.GetAll(DateTime.Today.AddDays(-14), DateTime.Now);
            model.DateFormat = CurrentUser.GeminiDateFormat;

            List<UserDto> allUsers = new List<UserDto>();
            var allUserDto = new UserDto();
            allUserDto.Fullname = "All Users";                       
            allUserDto.Entity.Id = 0;

            allUsers.Add(allUserDto);
            allUsers.AddRange(UserManager.GetActiveUsers());
            model.Users = new MultiSelectList(allUsers, "Entity.Id", "Fullname", new List<int> {0}); ;

            var result = new WidgetResult();

            result.Success = true;

            result.Markup = new WidgetMarkup("views/settings.cshtml", model);

            return result;
        }

        public override WidgetResult Show(IssueDto args)
        {
            throw new NotImplementedException();
        }


        [AppUrl("rollback")]
        public ActionResult Rollback(int id)
        {
            try
            {
                var data = AdminAuditRepository.Get(id);

                var query = Helper.CreateRollbackQuery(data);

                if (query.HasValue() && AdminAuditRepository.Rollback(query, id))
                {
                    switch(data.AdminArea)
                    {
                        case AdminAreaVisibility.Process:
                            Cache.Meta.TypeRemoveAll();
                            break;
                        case AdminAreaVisibility.ProjectTemplate:
                            Cache.ProjectTemplates.RemoveAll();                            
                            break;
                        case AdminAreaVisibility.CustomField:
                            Cache.CustomFields.RemoveAll();
                            break;
                        case AdminAreaVisibility.Status:
                            Cache.Meta.StatusRemoveAll();
                            break;
                        case AdminAreaVisibility.Priority:
                            Cache.Meta.PriorityRemoveAll();
                            break;
                        case AdminAreaVisibility.Severity:
                            Cache.Meta.SeverityRemoveAll();
                            break;
                        case AdminAreaVisibility.Resolution:
                            Cache.Meta.ResolutionRemoveAll();
                            break;
                        case AdminAreaVisibility.LinkType:
                            Cache.Meta.LinkTypeRemoveAll();
                            break;
                        case AdminAreaVisibility.TimeType:
                            Cache.Meta.TimeTypeRemoveAll();
                            break;
                        case AdminAreaVisibility.Project:
                            Cache.Projects.RemoveAll();
                            break;
                        case AdminAreaVisibility.ProjectLabel:
                            Cache.ProjectLabels.RemoveAll();
                            break;
                        case AdminAreaVisibility.GeminiConfiguration:
                            //Update user property from session/cache?
                            break;
                        case AdminAreaVisibility.PeopleUser:
                            Cache.Users.RemoveAll();
                            break;
                        case AdminAreaVisibility.PeopleGroup:
                            Cache.ProjectGroups.RemoveAll();
                            break;
                        case AdminAreaVisibility.PeoplePermission:
                            Cache.Permissions.RemoveAll();
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
                            Cache.RulesActions.RemoveAll();
                            break;
                        case AdminAreaVisibility.Sla:
                            Cache.SLA.RemoveAll();
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

                    return JsonSuccess(new { id = id });
                }

                return JsonError();
            }
            catch(Exception ex)
            {
                return JsonError(ex);
            }           
        }

        [AppUrl("getdata")]
        public ActionResult GetData(string dateFrom, string dateTo, List<int> userids)
        {
            var dateFromFormatted = ParseDateString.GetDateForString(dateFrom);
            var dateToFormatted = ParseDateString.GetDateForString(dateTo);

            if (!dateFromFormatted.HasValue) dateFromFormatted = DateTime.Today.AddDays(-14);
            if (!dateToFormatted.HasValue) dateToFormatted = DateTime.Now;

            var model = new AdminAuditModel();

            if (userids.Contains(0))
            {
                userids = UserManager.GetActiveUsers().Select(s => s.Entity.Id).ToList();
            }

            model.data = AdminAuditRepository.GetAll(dateFromFormatted.Value, dateToFormatted.Value.AddHours(24), userids);


            return JsonSuccess(new { html = RenderPartialViewToString(this, AppManager.Instance.GetAppUrl(AppGuid, "views/_tableData.cshtml"), model) });
        }
    }

    public class AdminAuditRoutes : IAppRoutes
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            if (!AdminAuditRepository.DoesTableExist())
            {
                AdminAuditRepository.CreateAdminAuditTable();
            }
        }
    }

}
