using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Countersoft.Gemini.Commons.Dto;
using Countersoft.Gemini.Extensibility.Apps;
using Countersoft.Gemini.Extensibility.Events;
using Countersoft.Gemini.Infrastructure.Helpers;

namespace AdminAudit.listener
{
    [AppType( AppTypeEnum.Event ),
    AppControlGuid( "4D2773B4-DE1D-4BCA-AF34-7F30E2C90828" ),
    AppGuid( CustomConstants.APPGUID ),
    AppAuthor( CustomConstants.APPAUTHOR ),
    AppName( CustomConstants.APPNAME ),
    AppDescription( CustomConstants.APPDESCRIPTION ),
    AppRequiresConfigScreen( true )]
    [OutputCache( Duration = 0, NoStore = true, Location = System.Web.UI.OutputCacheLocation.None )]
    public class ItemListener: Countersoft.Gemini.Extensibility.Events.AbstractIssueListener
    {
        
        #region Deleted Items

        public override void AfterDelete( IssueEventArgs args )
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.Issue;
            IssueDto dto = new IssueDto(args.Entity, args.Context.Projects.Get(args.Entity.ProjectId));
            audit.RowName = audit.ValueBefore =  string.Concat( dto.IssueKey, " ", dto.Title );

            AdminAuditRepository.InsertAudit( audit );
        }

        #endregion
    }
}
