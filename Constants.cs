using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Countersoft.Gemini.Commons;
using Countersoft.Gemini.Commons.Dto;

namespace AdminAudit
{
    public class CustomConstants
    {
        public const string APPGUID = "DA83F146-8C92-49AB-97C1-83EEDCCD3910";
        public const string APPAUTHOR = "Countersoft";
        public const string APPNAME = "Administrator Log";
        public const string APPDESCRIPTION = "List of all administrative actions performed by Gemini Administrators. Filter by date range or individual users.";
        
    }

    public enum UserAction
    {
        Created = 1,
        Edited = 2,
        Deleted = 3
    }

    public enum AdminAreaVisibility
    {
        ProjectTemplate = 0,
        Process = 1,
        CustomField = 2,
        Status = 3,
        Priority = 4,
        Severity = 5,
        Resolution = 6,
        LinkType = 7,
        TimeType = 8,
        Project = 9,
        ProjectLabel = 10,
        GeminiConfiguration = 11,
        PeopleUser = 12,
        PeopleGroup = 13,
        PeoplePermission = 14,
        BreezeQueue = 15,
        BreezeSmtp = 16,
        BreezeMailbox = 17,
        BreezeExpression = 18,
        BreezeReplyFrom = 19,
        Rules = 20,
        Sla = 21,
        SystemOption = 22,
        SystemEmail = 26,
        SystemAlertTemplates = 27,
        SystemActiveDirectory = 28,
        SystemLicensing = 29,
        Organization = 30,
        
        //Non Admin Areas:
        Issue = 100
    }
}
