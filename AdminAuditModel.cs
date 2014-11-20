using System;
using System.Collections.Generic;
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
    public class AdminAuditDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RowId { get; set; }        
        public string Fullname { get; set; }
        public UserAction Action { get; set; }
        public AdminAreaVisibility AdminArea { get; set; }
        public string FieldChanged { get; set; }
        
        public string FieldChangedDisplay { 
            get 
            {
                if (FieldChanged.IsEmpty()) return string.Empty;

                if (FieldChanged.Equals("name",StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("projectname", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("labelname", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("Schemename", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("Smtpservername", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("Mailboxname", StringComparison.InvariantCultureIgnoreCase)) return "Name";

                if (FieldChanged.Equals("queuedescription", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("projectdesc", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("Projectgroupdesc", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("Schemedesc", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("Longdesc", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("Sladesc", StringComparison.InvariantCultureIgnoreCase)) return "Description";

                if (FieldChanged.Equals("projectid", StringComparison.InvariantCultureIgnoreCase)) return "Project";
                if (FieldChanged.Equals("projectlabelid", StringComparison.InvariantCultureIgnoreCase) &&
                    AdminArea == AdminAreaVisibility.ProjectLabel) return "Projects";

                if (FieldChanged.Equals("typedesc", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("prioritydesc", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("severitydesc", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("resdesc", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("statusdesc", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("linkname", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("timetypename", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("projectlabelid", StringComparison.InvariantCultureIgnoreCase)) return "Label";

                if (FieldChanged.Equals("imagepath", StringComparison.InvariantCultureIgnoreCase)) return "Icon";
                if (FieldChanged.Equals("templateid", StringComparison.InvariantCultureIgnoreCase)) return "Template";
                if (FieldChanged.Equals("seq", StringComparison.InvariantCultureIgnoreCase)) return "Sequence";
                
                if (FieldChanged.Equals("isfinal", StringComparison.InvariantCultureIgnoreCase) &&
                    AdminArea == AdminAreaVisibility.Resolution) return "Resolved?";
                
                if (FieldChanged.Equals("isfinal", StringComparison.InvariantCultureIgnoreCase) &&
                    AdminArea == AdminAreaVisibility.Status) return "Final?";

                if (FieldChanged.Equals("linkdesc", StringComparison.InvariantCultureIgnoreCase) ||
                    FieldChanged.Equals("timetypedesc", StringComparison.InvariantCultureIgnoreCase)) return "Comment";

                if (FieldChanged.Equals("customfieldname", StringComparison.InvariantCultureIgnoreCase)) return "Custom Field Name";
                if (FieldChanged.Equals("screenlabel", StringComparison.InvariantCultureIgnoreCase)) return "Screen Label";
                if (FieldChanged.Equals("screentooltip", StringComparison.InvariantCultureIgnoreCase)) return "Screen Tooltip";
                if (FieldChanged.Equals("canfilter", StringComparison.InvariantCultureIgnoreCase)) return "Can Filter";
                if (FieldChanged.Equals("maxlen", StringComparison.InvariantCultureIgnoreCase)) return "Maximum Length";
                if (FieldChanged.Equals("regularexp", StringComparison.InvariantCultureIgnoreCase)) return "Regular Expression";
                if (FieldChanged.Equals("showinline", StringComparison.InvariantCultureIgnoreCase)) return "Show with Attributes";

                if (FieldChanged.Equals("customfieldtype", StringComparison.InvariantCultureIgnoreCase)) return "Custom Field Type";
                if (FieldChanged.Equals("minvalue", StringComparison.InvariantCultureIgnoreCase)) return "Minimum Value";
                if (FieldChanged.Equals("maxvalue", StringComparison.InvariantCultureIgnoreCase)) return "Maximum Value";
                if (FieldChanged.Equals("usestaticdata", StringComparison.InvariantCultureIgnoreCase)) return "Static";

                if (FieldChanged.Equals("allownoselection", StringComparison.InvariantCultureIgnoreCase)) return "Add Blank Entry";
                if (FieldChanged.Equals("autocomplete", StringComparison.InvariantCultureIgnoreCase)) return "Auto-complete";
                if (FieldChanged.Equals("lookupdata", StringComparison.InvariantCultureIgnoreCase)) return "Lookup Data";
                if (FieldChanged.Equals("Lookupsortfield", StringComparison.InvariantCultureIgnoreCase)) return "Lookup Sort Field Name";
                if (FieldChanged.Equals("Lookuptextfield", StringComparison.InvariantCultureIgnoreCase)) return "Lookup Description Field Name";
                if (FieldChanged.Equals("Cascadinglookupvaluefield", StringComparison.InvariantCultureIgnoreCase)) return "Cascading Lookup Field Name";
                if (FieldChanged.Equals("Cascadingparentfield", StringComparison.InvariantCultureIgnoreCase)) return "Cascading Parent Field";
                if (FieldChanged.Equals("Projectidfilter", StringComparison.InvariantCultureIgnoreCase)) return "Filter by Project";
                if (FieldChanged.Equals("Canmultiselect", StringComparison.InvariantCultureIgnoreCase)) return "Can Multi-Select";
                if (FieldChanged.Equals("Listlimiter", StringComparison.InvariantCultureIgnoreCase)) return "Limit Version";
                if (FieldChanged.Equals("lookupname", StringComparison.InvariantCultureIgnoreCase)) return "Lookup Table Name";


                if (FieldChanged.Equals("projectcode", StringComparison.InvariantCultureIgnoreCase)) return "Code";
                if (FieldChanged.Equals("resourcemode", StringComparison.InvariantCultureIgnoreCase)) return "Resource Assignment Mode";
                if (FieldChanged.Equals("componentmode", StringComparison.InvariantCultureIgnoreCase)) return "Component Assignment Mode";
                if (FieldChanged.Equals("globalschemeid", StringComparison.InvariantCultureIgnoreCase)) return "Permissions";
                if (FieldChanged.Equals("userid", StringComparison.InvariantCultureIgnoreCase) &&
                    AdminArea == AdminAreaVisibility.Project) return "Lead";

                if (FieldChanged.Equals("Emailaddress", StringComparison.InvariantCultureIgnoreCase)) return "Email";
                if (FieldChanged.Equals("pwd", StringComparison.InvariantCultureIgnoreCase)) return "Password";

                if (FieldChanged.Equals("Ad_mappings", StringComparison.InvariantCultureIgnoreCase)) return "Active Directory Groups";
                if (FieldChanged.Equals("Projectgroupname", StringComparison.InvariantCultureIgnoreCase)) return "Group Name";
                if (FieldChanged.Equals("Interactiongroups", StringComparison.InvariantCultureIgnoreCase)) return "Can Share/Chat With";

                if (FieldChanged.Equals("Serverport", StringComparison.InvariantCultureIgnoreCase)) return "Server Port";
                if (FieldChanged.Equals("Encodingtype", StringComparison.InvariantCultureIgnoreCase)) return "Encoding";
                if (FieldChanged.Equals("Usessl", StringComparison.InvariantCultureIgnoreCase)) return "Use SSL";
                if (FieldChanged.Equals("Authmode", StringComparison.InvariantCultureIgnoreCase)) return "Authentication Mode";
                if (FieldChanged.Equals("Sslmode", StringComparison.InvariantCultureIgnoreCase)) return "SSL Mode";

                if (FieldChanged.Equals("Queueid", StringComparison.InvariantCultureIgnoreCase)) return "Queue";
                if (FieldChanged.Equals("Deletemessages", StringComparison.InvariantCultureIgnoreCase)) return "Delete Messages";
                if (FieldChanged.Equals("Issuetypeid", StringComparison.InvariantCultureIgnoreCase)) return "Item Type";
                if (FieldChanged.Equals("Truncateexp", StringComparison.InvariantCultureIgnoreCase)) return "Content 'Truncation' Match Expressions";
                if (FieldChanged.Equals("Replaceexp", StringComparison.InvariantCultureIgnoreCase)) return "Content 'Replacement' Match Expressions";
                if (FieldChanged.Equals("Subjectnotlikeexp", StringComparison.InvariantCultureIgnoreCase)) return "Subject 'Not Like' Match Expressions";
                if (FieldChanged.Equals("Subjectlikeexp", StringComparison.InvariantCultureIgnoreCase)) return "Subject Like Match Expressions";
                if (FieldChanged.Equals("Ignoreattachments", StringComparison.InvariantCultureIgnoreCase)) return "Ignore Attachments";
                if (FieldChanged.Equals("Stripsignature", StringComparison.InvariantCultureIgnoreCase)) return "Strip Signature";
                if (FieldChanged.Equals("Smtpserverid", StringComparison.InvariantCultureIgnoreCase)) return "Smtp Server";
                if (FieldChanged.Equals("Alerttemplateid", StringComparison.InvariantCultureIgnoreCase)) return "Alert Template";
                if (FieldChanged.Equals("Noreplylist", StringComparison.InvariantCultureIgnoreCase)) return "No-Reply List";
                if (FieldChanged.Equals("Whitelist", StringComparison.InvariantCultureIgnoreCase)) return "White List";
                if (FieldChanged.Equals("Blacklist", StringComparison.InvariantCultureIgnoreCase)) return "Black List";
                if (FieldChanged.Equals("Usesenderassubmitter", StringComparison.InvariantCultureIgnoreCase)) return "Use Sender as Reporter";
                if (FieldChanged.Equals("Exchangeversion", StringComparison.InvariantCultureIgnoreCase)) return "Exchange Version";
                if (FieldChanged.Equals("mode", StringComparison.InvariantCultureIgnoreCase) && 
                    AdminArea == AdminAreaVisibility.BreezeSmtp) return "Connection Type";

                if (FieldChanged.Equals("Imapfolder", StringComparison.InvariantCultureIgnoreCase)) return "IMAP Folder";

                if (FieldChanged.Equals("Startdayminute", StringComparison.InvariantCultureIgnoreCase)) return "Day starts";
                if (FieldChanged.Equals("Enddayminute", StringComparison.InvariantCultureIgnoreCase)) return "Day ends";
                if (FieldChanged.Equals("Interval", StringComparison.InvariantCultureIgnoreCase)) return "Response Time";

                if (FieldChanged.Equals("Alerttype", StringComparison.InvariantCultureIgnoreCase)) return "Alert Type";
                if (FieldChanged.Equals("Alertcontent", StringComparison.InvariantCultureIgnoreCase)) return "Content";
                if (FieldChanged.Equals("Label", StringComparison.InvariantCultureIgnoreCase) &&
                    AdminArea == AdminAreaVisibility.SystemAlertTemplates) return "Name";

                return FieldChanged.Length > 2 ? char.ToUpper(FieldChanged[0]) + FieldChanged.Substring(1) : char.ToUpper(FieldChanged[0]).ToString(); 
            }
        }

        public string ValueBefore { get; set; }
        public string ValueAfter { get; set; }
        public string Data { get; set; }       
        public DateTime Created { get; set; }
        public string RowName { get; set; }
        public string RowNameDisplay
        {
            get
            {
                if (AdminArea == AdminAreaVisibility.GeminiConfiguration)
                {
                    if (FieldChanged.Equals(GeminiConfigurationOption.DefaultCulture.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Language";
                    if (FieldChanged.Equals(GeminiConfigurationOption.DefaultTimeZoneId.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Timezone";
                    if (FieldChanged.Equals(GeminiConfigurationOption.GeminiAdmins.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Administrator Email(s)";
                    if (FieldChanged.Equals(GeminiConfigurationOption.DefaultCultureName.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Timezone Name";
                    if (FieldChanged.Equals(GeminiConfigurationOption.AllowAnonymousAccess.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Browse Gemini without logging in";
                    if (FieldChanged.Equals(GeminiConfigurationOption.EnableOpenId.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Enable OpenId authentication";
                    if (FieldChanged.Equals(GeminiConfigurationOption.EnableFacebookIntegration.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Enable Facebook Authentication";
                    if (FieldChanged.Equals(GeminiConfigurationOption.FacebookAppId.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Facebook App Id";
                    if (FieldChanged.Equals(GeminiConfigurationOption.WelcomeTitle.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Welcome Title";
                    if (FieldChanged.Equals(GeminiConfigurationOption.WelcomeMessage.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Welcome Message";
                    if (FieldChanged.Equals(GeminiConfigurationOption.SMTPServer.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "SMTP Server";
                    if (FieldChanged.Equals(GeminiConfigurationOption.SMTPServerPort.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "SMTP Port";
                    if (FieldChanged.Equals(GeminiConfigurationOption.SMTPAuthenticationUsername.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Authentication Username";
                    if (FieldChanged.Equals(GeminiConfigurationOption.SMTPAuthenticationPassword.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Authentication Password";
                    if (FieldChanged.Equals(GeminiConfigurationOption.SMTPAuthenticationMode.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Authentication Mode";
                    if (FieldChanged.Equals(GeminiConfigurationOption.SMTPPOPBeforeSMTP.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Call POP3 Before Sending Emails";
                    if (FieldChanged.Equals(GeminiConfigurationOption.SMTPUseSSL.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Use SSL";
                    if (FieldChanged.Equals(GeminiConfigurationOption.SMTPSSLMode.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "SSL Protocol";
                    if (FieldChanged.Equals(GeminiConfigurationOption.SMTPEncodingType.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Encoding";
                    if (FieldChanged.Equals(GeminiConfigurationOption.SMTPFromEmailAddress.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "From Email Address";
                    if (FieldChanged.Equals(GeminiConfigurationOption.SMTPFromDisplayName.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "From Display Name";
                    if (FieldChanged.Equals(GeminiConfigurationOption.EmailAlertsEnabled.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Send email alerts";
                    if (FieldChanged.Equals(GeminiConfigurationOption.TimeInWorkingDay.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Working Hours in Day";
                    if (FieldChanged.Equals(GeminiConfigurationOption.AutoAlertForIssueCreator.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Auto-email creator/reporter";
                    if (FieldChanged.Equals(GeminiConfigurationOption.AutoAlertForIssueResource.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Auto-email assigned resource";
                    if (FieldChanged.Equals(GeminiConfigurationOption.ShowUserRegistrationLink.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Allow users to self-register";
                    if (FieldChanged.Equals(GeminiConfigurationOption.NewUserResetPassword.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Force New User Password Reset";
                    if (FieldChanged.Equals(GeminiConfigurationOption.ResetPasswordMessage.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Reset Password Email Text";
                    if (FieldChanged.Equals(GeminiConfigurationOption.ResetPasswordSubject.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Reset Password Email Subject";
                    if (FieldChanged.Equals(GeminiConfigurationOption.HelpDeskModeGroup.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Customer Portal User Group";
                    if (FieldChanged.Equals(GeminiConfigurationOption.DefaultNewUserGlobalGroups.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "New User Default Groups";
                    if (FieldChanged.Equals(GeminiConfigurationOption.HelpDeskWelcomeMessage.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Customer Portal Welcome Message";
                    if (FieldChanged.Equals(GeminiConfigurationOption.HelpDeskWelcomeTitle.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Customer Portal Welcome Title";
                    if (FieldChanged.Equals(GeminiConfigurationOption.SyncWithActiveDirectory.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Enable Active Directory Integration";
                    if (FieldChanged.Equals(GeminiConfigurationOption.ActiveDirectoryConnectionString.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Active Directory Connection String";
                    if (FieldChanged.Equals(GeminiConfigurationOption.ActiveDirectoryUserName.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Active Directory Username";
                    if (FieldChanged.Equals(GeminiConfigurationOption.ActiveDirectoryPassword.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Active Directory Password";
                    if (FieldChanged.Equals(GeminiConfigurationOption.ActiveDirectoryAddNewUsers.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Automatically create users in Gemini";
                    if (FieldChanged.Equals(GeminiConfigurationOption.ActiveDirectoryDomain.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Active Directory Domain";
                    if (FieldChanged.Equals(GeminiConfigurationOption.ActiveDirectoryValidateLogon.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Ignore AD users who have never logged in";
                    if (FieldChanged.Equals(GeminiConfigurationOption.ActiveDirectoryValidateEmail.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Ignore AD users with no email address";
                    if (FieldChanged.Equals("ActiveDirectoryMappings", StringComparison.InvariantCultureIgnoreCase)) return "ActiveDirectory Existing Mappings";
                    if (FieldChanged.Equals(GeminiConfigurationOption.LicenseKeys.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "LicenseKeys";
                    if (FieldChanged.Equals(GeminiConfigurationOption.SessionTimeOut.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Browser Session Timeout";
                    if (FieldChanged.Equals(GeminiConfigurationOption.ForceNewPasswordInDays.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Force New Password Every";
                    if (FieldChanged.Equals(GeminiConfigurationOption.PreventPasswordReuse.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Do Not Allow Password Reuse";
                    if (FieldChanged.Equals(GeminiConfigurationOption.AccountLockoutThreshold.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Maximum Login Attempts";
                    if (FieldChanged.Equals(GeminiConfigurationOption.EnforcePasswordPolicy.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Enforce Password Format";
                    if (FieldChanged.Equals(GeminiConfigurationOption.MinimumLetters.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Minimum Password Length";
                    if (FieldChanged.Equals(GeminiConfigurationOption.NumberRequired.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Number Required";
                    if (FieldChanged.Equals(GeminiConfigurationOption.UppercaseRequired.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Uppercase Required";
                    if (FieldChanged.Equals(GeminiConfigurationOption.LowercaseRequired.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Lowercase Required";
                    if (FieldChanged.Equals(GeminiConfigurationOption.SymbolRequired.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Symbol Required";
                    if (FieldChanged.Equals(GeminiConfigurationOption.EnableGoogleIntegration.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Enable Google Authentication";
                    if (FieldChanged.Equals(GeminiConfigurationOption.GoogleClientId.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Google Client Id";
                    if (FieldChanged.Equals(GeminiConfigurationOption.GoogleClientSecret.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Google Client Secret";
                    if (FieldChanged.Equals(GeminiConfigurationOption.Logo.ToString(), StringComparison.InvariantCultureIgnoreCase)) return "Logo";
                }

                return RowName;
            }
        }

        public AdminAuditDto()
        {
            FieldChanged = string.Empty;
            ValueBefore = string.Empty;
            ValueAfter = string.Empty;
            Data = string.Empty;
        }
    }

    public class AdminAuditModel
    {
        public List<AdminAuditDto> data { get; set; }
        public string DateFormat { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }
    }

    public class CustomTriplet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Data { get; set; }
        public DateTime Created { get; set; }
    }

}
