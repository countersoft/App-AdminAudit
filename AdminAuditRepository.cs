using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Countersoft.Gemini.Commons;
using Countersoft.Gemini.Commons.Dto;
using Countersoft.Gemini.Extensibility;
using Countersoft.Gemini.Extensibility.Events;
using Countersoft.Foundation.Commons.Extensions;

namespace AdminAudit
{
    public class AdminAuditRepository
    {
        public static bool DoesTableExist()
        {
            var query = string.Format("SELECT count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'admin_audit'");

            var result = SQLService.Instance.RunQuery<int>(query);

            return result.Count() > 0 && result.First() != 0 ? true : false;
        }

        public static int CreateAdminAuditTable()
        {
            var createTableQuery = @"CREATE TABLE [dbo].[admin_audit](
	                                    [id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	                                	[userid] [numeric](10, 0) NOT NULL,
	                                    [rowid] [int] NOT NULL,
                                        [rowname] [nvarchar](max) NOT NULL,
	                                    [data][nvarchar](max),
	                                    [action] [int] NOT NULL,
	                                    [adminarea] [int] NOT NULL,                                        
	                                    [fieldchanged] [nvarchar](255) NOT NULL,
	                                    [valuebefore] [nvarchar](max) NOT NULL,
	                                    [valueafter] [nvarchar](max) NOT NULL,
	                                    [created] [datetime] NOT NULL default GETUTCDATE()
                                    )
                                    CREATE INDEX admin_audit_created
                                    ON admin_audit (created)

                                    CREATE INDEX admin_audit_userid
                                    ON admin_audit (userid)
                                    ";


            return SQLService.Instance.ExecuteQuery(createTableQuery);
        }

        public static int InsertAudit(AdminAuditDto audit)
        {
            var query = string.Format(@"INSERT INTO admin_audit (userid, rowid, rowname, data, action, adminarea, fieldchanged, valuebefore, valueafter, created) 
                values ({0},{1},'{2}','{3}',{4},{5},'{6}','{7}','{8}', @created) ", audit.UserId, audit.RowId, audit.RowName, audit.Data, (int)audit.Action, (int)audit.AdminArea, audit.FieldChanged, audit.ValueBefore, audit.ValueAfter);

            return SQLService.Instance.ExecuteQuery(query, new { created = DateTime.UtcNow });
        }

        public static List<AdminAuditDto> GetAll(DateTime fromDate, DateTime toDate, UserContext userContext, List<int> userids = null)
        {
            var query = string.Empty;

            if (userids == null || userids != null && userids.Count == 0)
            {
                query =   @"select a.id, a.action, a.rowid, a.adminarea, a.rowname, a.fieldchanged, a.valuebefore, a.valueafter, 
                          a.created, u.firstname + ' ' + u.surname as fullname 
                          from admin_audit a
                          left join gemini_users u ON u.userid = a.userid 
                          where a.created >= @fromDate AND a.created <= @toDate
                          ORDER BY a.created desc";
            }
            else
            {
                query = string.Format(@"select a.id, a.action, a.rowid, a.adminarea, a.rowname, a.fieldchanged, a.valuebefore, a.valueafter, 
                          a.created, u.firstname + ' ' + u.surname as fullname 
                          from admin_audit a
                          left join gemini_users u ON u.userid = a.userid 
                          where a.created >= @fromDate AND a.created <= @toDate AND a.userid in ({0})
                          ORDER BY a.created desc", string.Join(",", userids));
            }

            var result = SQLService.Instance.RunQuery<AdminAuditDto>(query, new { fromDate = fromDate, toDate = toDate }).ToList();

            if (result.Count > 0)
            {
                result.ForEach(m => m.Created = m.Created.ToLocal(userContext.User.TimeZone));
            }

            return result;
        }

        public static AdminAuditDto Get(int Id, UserContext userContext)
        {
            var query = string.Format(@"select a.rowid, a.rowname, a.data, a.id, a.action, a.adminarea, a.fieldchanged, a.valuebefore, a.valueafter, 
                          a.created, u.firstname + ' ' + u.surname as fullname 
                          from admin_audit a
                          left join gemini_users u ON u.userid = a.userid where a.id = {0} ", Id);

            var result = SQLService.Instance.RunQuery<AdminAuditDto>(query).ToList();

            if (result.Count > 0)
            {
                result.ForEach(m => m.Created = m.Created.ToLocal(userContext.User.TimeZone));
            }

            return result.Count > 0 ? result.First() : null;
        }

        public static void Delete(int Id)
        {
            var query = string.Format(@"delete from admin_audit where admin_audit.id = {0} ", Id);

            SQLService.Instance.ExecuteQuery(query);
        }

        public static bool Rollback(string query, int id)
        {
            if (SQLService.Instance.ExecuteQuery(query) > 0)
            {
                AdminAuditRepository.Delete(id);
                return true;
            }

            return false;
        }

    }


}
