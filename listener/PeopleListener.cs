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
using System.Reflection;
using Countersoft.Gemini.Commons.Meta;
using Countersoft.Gemini.Commons.Permissions;
using Countersoft.Foundation.Commons.Core;


namespace AdminAudit
{
    [AppType(AppTypeEnum.Event),
    AppControlGuid("1D940864-E8FF-4307-A885-514D40BF4518"),
    AppGuid(CustomConstants.APPGUID),
    AppAuthor(CustomConstants.APPAUTHOR),
    AppName(CustomConstants.APPNAME),
    AppDescription(CustomConstants.APPDESCRIPTION),
    AppRequiresConfigScreen(true)]
    [OutputCache(Duration = 0, NoStore = true, Location = System.Web.UI.OutputCacheLocation.None)]
    public class PeopleListener : IAfterUserListener, IAfterProjectGroupListener, IAfterPermissionSetListener, IAfterGeminiConfigurationListener, IAfterOrganizationListener
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string AppGuid { get; set; }
        public static CustomTriplet LastAdGroupChanged { get; set; }
        public static CustomTriplet LastGroupChanged { get; set; }

        #region Users

        public void AfterUserCreated(UserEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.PeopleUser;
            audit.RowName = audit.ValueAfter = args.Entity.Fullname;
            
            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterUserDeleted(UserEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.PeopleUser;
            audit.RowName = audit.ValueBefore = args.Previous.Fullname;
            
            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterUserUpdated(UserEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Username.Equals(args.Entity.Username))
            {
                changedValues.Add(new Triplet() { First = "username", Second = args.Previous.Username, Third = args.Entity.Username });
            }

            if (!args.Previous.Firstname.Equals(args.Entity.Firstname))
            {
                changedValues.Add(new Triplet() { First = "firstname", Second = args.Previous.Firstname, Third = args.Entity.Firstname });
            }

            if (!args.Previous.Surname.Equals(args.Entity.Surname))
            {
                changedValues.Add(new Triplet() { First = "surname", Second = args.Previous.Surname, Third = args.Entity.Surname });
            }

            if (!args.Previous.Email.Equals(args.Entity.Email))
            {
                changedValues.Add(new Triplet() { First = "emailaddress", Second = args.Previous.Email, Third = args.Entity.Email });
            }

            if (!args.Previous.Password.ToJson().Equals(args.Entity.Password.ToJson()))
            {
                changedValues.Add(new Triplet() { First = "pwd", Second = "***", Third = "***" });
            }

            if (!args.Previous.Active.Equals(args.Entity.Active))
            {
                changedValues.Add(new Triplet() { First = "active", Second = args.Previous.Active, Third = args.Entity.Active });
            }

            if (!args.Previous.Locked.Equals(args.Entity.Locked))
            {
                changedValues.Add(new Triplet() { First = "locked", Second = args.Previous.Locked, Third = args.Entity.Locked });
            }

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.PeopleUser;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Fullname;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion

        #region Project Groups

        public void AfterProjectGroupCreated(ProjectGroupEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.PeopleGroup;
            audit.RowName = audit.ValueAfter = args.Entity.Name;            

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterProjectGroupDeleted(ProjectGroupEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.PeopleGroup;
            audit.RowName = audit.ValueBefore = args.Previous.Name;
            
            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterProjectGroupUpdated(ProjectGroupEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Name.Equals(args.Entity.Name))
            {
                changedValues.Add(new Triplet() { First = "projectgroupname", Second = args.Previous.Name, Third = args.Entity.Name });
            }

            if (!args.Previous.Description.Equals(args.Entity.Description))
            {
                changedValues.Add(new Triplet() { First = "projectgroupdesc", Second = args.Previous.Description, Third = args.Entity.Description });
            }

            if (!args.Previous.InteractionGroups.ToJson().Equals(args.Entity.InteractionGroups.ToJson()))
            {
                var projectGroups = args.Context.ProjectGroups.GetAll();
                var previousGroups = projectGroups.Count > 0 && args.Previous.InteractionGroups.Count > 0 ? projectGroups.FindAll(s => args.Previous.InteractionGroups.Contains(s.Id)).Select(a => a.Name).ToDelimited(", ") : string.Empty;
                var currentGroups = projectGroups.Count > 0 && args.Entity.InteractionGroups.Count > 0 ? projectGroups.FindAll(s => args.Entity.InteractionGroups.Contains(s.Id)).Select(a => a.Name).ToDelimited(", ") : string.Empty;

                changedValues.Add(new Triplet() { First = "interactiongroups", Second = previousGroups.TrimEnd(' ').TrimEnd(','), Third = currentGroups.TrimEnd(' ').TrimEnd(',') });
            }

            var dateNow = DateTime.Now;

            if (args.Previous.ADGroups.Count > 0) args.Previous.ADGroups.ForEach(s => s.Revised = dateNow);
            if (args.Entity.ADGroups.Count > 0)   args.Entity.ADGroups.ForEach(s => s.Revised = dateNow);

            if (!args.Previous.ADGroups.ToJson().Equals(args.Entity.ADGroups.ToJson()))
            {
                var allActiveDirectoryGroups = args.Context.ActiveDirectory.GetAll();
                
                var previousGroups = string.Empty;
                var currentGroups = string.Empty;

                if (args.Previous.ADGroups.Count > 0)
                {
                    var allPreviousGroups = allActiveDirectoryGroups.FindAll(s => args.Previous.ADGroups.Any(a => a.ADGroupId == s.Id));
                    if (allPreviousGroups.Count > 0) previousGroups = allPreviousGroups.Select(s => s.Name).ToDelimited(", ").TrimEnd(' ').TrimEnd(',');
                }

                if (args.Entity.ADGroups.Count > 0)
                {
                    var allCurrentGroups = allActiveDirectoryGroups.FindAll(s => args.Entity.ADGroups.Any( a => a.ADGroupId == s.Id));
                    if (allCurrentGroups.Count > 0) currentGroups = allCurrentGroups.Select(s => s.Name).ToDelimited(", ").TrimEnd(' ').TrimEnd(',');
                }

                if (LastAdGroupChanged != null)
                {
                    var difference = (DateTime.Now - LastAdGroupChanged.Created).Seconds;

                    if (difference <= 3 && LastAdGroupChanged.Id.ToString() == args.Previous.Id.ToString()
                        && LastAdGroupChanged.UserId.ToString() == args.User.Id.ToString() && LastAdGroupChanged.Data == previousGroups)
                    {
                        return;
                    }
                    else
                    {
                        LastAdGroupChanged = new CustomTriplet() { Id = args.Previous.Id, UserId = args.User.Id, Created = DateTime.Now, Data = previousGroups }; 
                    }
                }
                else
                {
                    LastAdGroupChanged = new CustomTriplet() { Id = args.Previous.Id, UserId = args.User.Id, Created = DateTime.Now, Data = previousGroups };   
                }

                changedValues.Add(new Triplet() { First = "ad_mappings", Second = currentGroups, Third = previousGroups });
            }

            if (args.Previous.Members.Count > 0) args.Previous.Members.ForEach(s => s.Revised = dateNow);
            if (args.Entity.Members.Count > 0) args.Entity.Members.ForEach(s => s.Revised = dateNow);

            if (!args.Previous.Members.ToJson().Equals(args.Entity.Members.ToJson()))
            {
                var allUsers = args.Context.Users.GetAll();
                var allProjects = args.Context.Projects.GetAll();

                Dictionary<string,List<string>> previousResult = new Dictionary<string,List<string>>();
                Dictionary<string, List<string>> currentResult = new Dictionary<string, List<string>>();
                var previousUsers = string.Empty;
                var currentUsers = string.Empty;

                foreach(var member in args.Previous.Members)
                {
                    var user = allUsers.Find(s => s.Id == member.UserId);
                    
                    if (user == null) continue;

                    string fullname = user.Fullname;
                    string projectName = member.ProjectId == null ? "All Projects" : allProjects.Find(s => s.Id == member.ProjectId).Name;
                                        
                    if (previousResult.Keys.Contains(projectName))                    
                        previousResult[projectName].Add(fullname);                    
                    else                    
                        previousResult.Add(projectName, new List<string> { fullname });                    
                }                

                foreach(var result in previousResult)
                {
                    previousUsers += string.Format("{0}: {1}, ", result.Key, result.Value.ToDelimited(", ").TrimEnd(' ').TrimEnd(','));
                }

                foreach (var member in args.Entity.Members)
                {
                    var user = allUsers.Find(s => s.Id == member.UserId);

                    if (user == null) continue;

                    string fullname = user.Fullname;
                    string projectName = member.ProjectId == null ? "All Projects" : allProjects.Find(s => s.Id == member.ProjectId).Name;

                    if (currentResult.Keys.Contains(projectName))
                        currentResult[projectName].Add(fullname);
                    else
                        currentResult.Add(projectName, new List<string> { fullname });
                }                

                foreach (var result in currentResult)
                {
                    currentUsers += string.Format("{0}: {1}, ", result.Key, result.Value.ToDelimited(", ").TrimEnd(' ').TrimEnd(','));
                }

                changedValues.Add(new Triplet() { First = "members", Second = currentUsers.TrimEnd(' ').TrimEnd(','), Third = previousUsers.TrimEnd(' ').TrimEnd(',') });
            }

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.PeopleGroup;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Name;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion
        
        #region Permissions

        public void AfterPermissionSetCreated(PermissionSetEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.PeoplePermission;
            audit.ValueAfter = args.Entity.Name;
            audit.RowName = args.Entity.Name;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterPermissionSetDeleted(PermissionSetEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.PeoplePermission;
            audit.ValueBefore = args.Previous.Name;
            audit.RowName = args.Previous.Name;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterPermissionSetUpdated(PermissionSetEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Name.Equals(args.Entity.Name))
            {
                changedValues.Add(new Triplet() { First = "schemename", Second = args.Previous.Name, Third = args.Entity.Name });
            }

            if (!args.Previous.Description.Equals(args.Entity.Description))
            {
                changedValues.Add(new Triplet() { First = "schemedesc", Second = args.Previous.Description, Third = args.Entity.Description });
            }

            var dateNow = DateTime.Now;
            //need to reset those properties because we always create new roles, on every new save
            if (args.Previous.Roles.Count > 0) args.Previous.Roles.ToList().ForEach(s => { s.Revised = dateNow; s.Id = 1; s.Timestamp = null; s.Created = dateNow; });
            if (args.Entity.Roles.Count > 0) args.Entity.Roles.ToList().ForEach(s => { s.Revised = dateNow; s.Id = 1; s.Timestamp = null; s.Created = dateNow; });

            if (!args.Previous.Roles.ToJson().Equals(args.Entity.Roles.ToJson()))
            {
                var allGroups = args.Context.ProjectGroups.GetAll();

                var data = new Triplet() { First = "members"};
                                
                Dictionary<string,string> previousRolesTranslated = new Dictionary<string,string>();
                Dictionary<string,string> currentRolesTranslated = new Dictionary<string, string>();

                foreach(var role in args.Previous.Roles)
                {
                    var roleName = ((Roles)role.Role).ToString();
                    var group = allGroups.Find(s => s.Id == role.MemberId);

                    if (previousRolesTranslated.Keys.Contains(roleName))
                    {
                        previousRolesTranslated[roleName] += string.Concat(", ", group.Name);
                    }
                    else
                    {
                        previousRolesTranslated.Add(roleName, string.Concat(roleName, ": ", group.Name));
                    }
                }

                foreach (var role in args.Entity.Roles)
                {
                    var roleName = ((Roles)role.Role).ToString();
                    var group = allGroups.Find(s => s.Id == role.MemberId);

                    if (currentRolesTranslated.Keys.Contains(roleName))
                    {
                        currentRolesTranslated[roleName] += string.Concat(", ", group.Name);
                    }
                    else
                    {
                        currentRolesTranslated.Add(roleName, string.Concat(roleName, ": ", group.Name));
                    }
                }
                
                data.Second = previousRolesTranslated.Values.ToDelimited(", ").TrimEnd(' ').TrimEnd(',');
                data.Third = currentRolesTranslated.Values.ToDelimited(", ").TrimEnd(' ').TrimEnd(',');

                changedValues.Add(data);
            }

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.PeoplePermission;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = args.Entity.Name;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion

        #region People User Options

        public void AfterGeminiConfigurationCreated(GeminiConfigurationEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.GeminiConfiguration;
            audit.ValueAfter = args.Entity.LicenseKeys;
            audit.RowName = audit.FieldChanged = GeminiConfigurationOption.LicenseKeys.ToString();           

            AdminAuditRepository.InsertAudit(audit);            
        }


        public void AfterGeminiConfigurationDeleted(GeminiConfigurationEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.PeoplePermission;
            audit.ValueBefore = args.Previous.LicenseKeys;
            audit.RowName = audit.FieldChanged = GeminiConfigurationOption.LicenseKeys.ToString();

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterGeminiConfigurationUpdated(GeminiConfigurationEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.DefaultCulture.Equals(args.Entity.DefaultCulture))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.DefaultCulture.ToString(), Second = args.Previous.DefaultCulture, Third = args.Entity.DefaultCulture });
            }

            if (!args.Previous.DefaultTimeZoneId.Equals(args.Entity.DefaultTimeZoneId))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.DefaultTimeZoneId.ToString(), Second = args.Previous.DefaultTimeZoneId, Third = args.Entity.DefaultTimeZoneId });
            }

            if (!args.Previous.AllowAnonymousAccess.Equals(args.Entity.AllowAnonymousAccess))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.AllowAnonymousAccess.ToString(), Second = args.Previous.AllowAnonymousAccess, Third = args.Entity.AllowAnonymousAccess });
            }

            if (!args.Previous.EnableOpenId.Equals(args.Entity.EnableOpenId))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.EnableOpenId.ToString(), Second = args.Previous.EnableOpenId, Third = args.Entity.EnableOpenId });
            }

            if (!args.Previous.EnableFacebookIntegration.Equals(args.Entity.EnableFacebookIntegration))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.EnableFacebookIntegration.ToString(), Second = args.Previous.EnableFacebookIntegration, Third = args.Entity.EnableFacebookIntegration });
            }

            if (!args.Previous.FacebookAppId.Equals(args.Entity.FacebookAppId))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.FacebookAppId.ToString(), Second = args.Previous.FacebookAppId, Third = args.Entity.FacebookAppId });
            }

            if (!args.Previous.WelcomeTitle.Equals(args.Entity.WelcomeTitle))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.WelcomeTitle.ToString(), Second = args.Previous.WelcomeTitle, Third = args.Entity.WelcomeTitle });
            }

            if (!args.Previous.WelcomeMessage.Equals(args.Entity.WelcomeMessage))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.WelcomeMessage.ToString(), Second = args.Previous.WelcomeMessage, Third = args.Entity.WelcomeMessage });
            }

            if (!args.Previous.GeminiAdmins.Equals(args.Entity.GeminiAdmins))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.GeminiAdmins.ToString(), Second = args.Previous.GeminiAdmins, Third = args.Entity.GeminiAdmins });
            }

            if (!args.Previous.ResetPasswordSubject.Equals(args.Entity.ResetPasswordSubject))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.ResetPasswordSubject.ToString(), Second = args.Previous.ResetPasswordSubject, Third = args.Entity.ResetPasswordSubject });
            }

            if (!args.Previous.ResetPasswordMessage.Equals(args.Entity.ResetPasswordMessage))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.ResetPasswordMessage.ToString(), Second = args.Previous.ResetPasswordMessage, Third = args.Entity.ResetPasswordMessage });
            }

            if (!args.Previous.SMTPServer.Equals(args.Entity.SMTPServer))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.SMTPServer.ToString(), Second = args.Previous.SMTPServer, Third = args.Entity.SMTPServer });
            }

            if (!args.Previous.SMTPServerPort.Equals(args.Entity.SMTPServerPort))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.SMTPServerPort.ToString(), Second = args.Previous.SMTPServerPort, Third = args.Entity.SMTPServerPort });
            }

            if (!args.Previous.SMTPAuthenticationUsername.Equals(args.Entity.SMTPAuthenticationUsername))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.SMTPAuthenticationUsername.ToString(), Second = args.Previous.SMTPAuthenticationUsername, Third = args.Entity.SMTPAuthenticationUsername });
            }

            if (!args.Previous.SMTPAuthenticationPassword.Equals(args.Entity.SMTPAuthenticationPassword))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.SMTPAuthenticationPassword.ToString(), Second = "***", Third = "***" });
            }

            if (!args.Previous.SMTPAuthenticationMode.Equals(args.Entity.SMTPAuthenticationMode))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.SMTPAuthenticationMode.ToString(), Second = args.Previous.SMTPAuthenticationMode, Third = args.Entity.SMTPAuthenticationMode });
            }

            if (!args.Previous.SMTPPOPBeforeSMTP.Equals(args.Entity.SMTPPOPBeforeSMTP))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.SMTPPOPBeforeSMTP.ToString(), Second = args.Previous.SMTPPOPBeforeSMTP, Third = args.Entity.SMTPPOPBeforeSMTP });
            }

            if (!args.Previous.SMTPUseSSL.Equals(args.Entity.SMTPUseSSL))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.SMTPUseSSL.ToString(), Second = args.Previous.SMTPUseSSL, Third = args.Entity.SMTPUseSSL });
            }

            if (!args.Previous.SMTPSSLMode.Equals(args.Entity.SMTPSSLMode))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.SMTPSSLMode.ToString(), Second = args.Previous.SMTPSSLMode, Third = args.Entity.SMTPSSLMode });
            }

            if (!args.Previous.SMTPEncodingType.Equals(args.Entity.SMTPEncodingType))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.SMTPEncodingType.ToString(), Second = args.Previous.SMTPEncodingType, Third = args.Entity.SMTPEncodingType });
            }

            if (!args.Previous.SMTPFromEmailAddress.Equals(args.Entity.SMTPFromEmailAddress))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.SMTPFromEmailAddress.ToString(), Second = args.Previous.SMTPFromEmailAddress, Third = args.Entity.SMTPFromEmailAddress });
            }

            if (!args.Previous.SMTPFromDisplayName.Equals(args.Entity.SMTPFromDisplayName))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.SMTPFromDisplayName.ToString(), Second = args.Previous.SMTPFromDisplayName, Third = args.Entity.SMTPFromDisplayName });
            }

            if (!args.Previous.EmailAlertsEnabled.Equals(args.Entity.EmailAlertsEnabled))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.EmailAlertsEnabled.ToString(), Second = args.Previous.EmailAlertsEnabled, Third = args.Entity.EmailAlertsEnabled });
            }

            if (!args.Previous.Debug.Equals(args.Entity.Debug))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.Debug.ToString(), Second = args.Previous.Debug, Third = args.Entity.Debug });
            }

            if (!args.Previous.OrganisationName.Equals(args.Entity.OrganisationName))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.OrganisationName.ToString(), Second = args.Previous.OrganisationName, Third = args.Entity.OrganisationName });
            }

            if (!args.Previous.RegistrationCode.Equals(args.Entity.RegistrationCode))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.RegistrationCode.ToString(), Second = args.Previous.RegistrationCode, Third = args.Entity.RegistrationCode });
            }

            if (!args.Previous.Theme.Equals(args.Entity.Theme))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.Theme.ToString(), Second = args.Previous.Theme, Third = args.Entity.Theme });
            }

            if (!args.Previous.DefaultCultureName.Equals(args.Entity.DefaultCultureName))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.DefaultCultureName.ToString(), Second = args.Previous.DefaultCultureName, Third = args.Entity.DefaultCultureName });
            }

            if (!args.Previous.TimeInWorkingDay.Equals(args.Entity.TimeInWorkingDay))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.TimeInWorkingDay.ToString(), Second = args.Previous.TimeInWorkingDay, Third = args.Entity.TimeInWorkingDay });
            }

            if (!args.Previous.IssueLinkQualifier.Equals(args.Entity.IssueLinkQualifier))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.IssueLinkQualifier.ToString(), Second = args.Previous.IssueLinkQualifier, Third = args.Entity.IssueLinkQualifier });
            }

            if (!args.Previous.AutoAlertForIssueCreator.Equals(args.Entity.AutoAlertForIssueCreator))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.AutoAlertForIssueCreator.ToString(), Second = args.Previous.AutoAlertForIssueCreator, Third = args.Entity.AutoAlertForIssueCreator });
            }

            if (!args.Previous.AutoAlertForIssueResource.Equals(args.Entity.AutoAlertForIssueResource))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.AutoAlertForIssueResource.ToString(), Second = args.Previous.AutoAlertForIssueResource, Third = args.Entity.AutoAlertForIssueResource });
            }

            if (!args.Previous.ShowUserRegistrationLink.Equals(args.Entity.ShowUserRegistrationLink))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.ShowUserRegistrationLink.ToString(), Second = args.Previous.ShowUserRegistrationLink, Third = args.Entity.ShowUserRegistrationLink });
            }

            if (!args.Previous.AlwaysShowGeminiStats.Equals(args.Entity.AlwaysShowGeminiStats))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.AlwaysShowGeminiStats.ToString(), Second = args.Previous.AlwaysShowGeminiStats, Third = args.Entity.AlwaysShowGeminiStats });
            }

            if (!args.Previous.NewUserResetPassword.Equals(args.Entity.NewUserResetPassword))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.NewUserResetPassword.ToString(), Second = args.Previous.NewUserResetPassword, Third = args.Entity.NewUserResetPassword });
            }

            if (!args.Previous.HelpDeskModeGroup.Equals(args.Entity.HelpDeskModeGroup))
            {
                var allGroups = args.Context.ProjectGroups.GetAll();

                var previousData = args.Previous.HelpDeskModeGroup > 0 ? args.Context.ProjectGroups.Get(args.Previous.HelpDeskModeGroup).Name : string.Empty;
                var currentData = args.Entity.HelpDeskModeGroup > 0 ? args.Context.ProjectGroups.Get(args.Entity.HelpDeskModeGroup).Name : string.Empty;

                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.HelpDeskModeGroup.ToString(), Second = previousData, Third = currentData });
            }

            if (!args.Previous.DefaultNewUserGlobalGroups.Equals(args.Entity.DefaultNewUserGlobalGroups))
            {
                var allGroups = args.Context.ProjectGroups.GetAll();

                var previousData = string.Empty;
                var currentData = string.Empty;

                if (args.Previous.DefaultNewUserGlobalGroups.HasValue())
                {
                    var previousGroupsSplitted = args.Previous.DefaultNewUserGlobalGroups.Split('|');
                    previousData = allGroups.FindAll(s => previousGroupsSplitted.Contains(s.Id.ToString())).Select(s => s.Name).ToDelimited(", ");
                }

                if (args.Entity.DefaultNewUserGlobalGroups.HasValue())
                {
                    var currentGroupsSplitted = args.Entity.DefaultNewUserGlobalGroups.Split('|');
                    currentData = allGroups.FindAll(s => currentGroupsSplitted.Contains(s.Id.ToString())).Select(s => s.Name).ToDelimited(", ");
                }

                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.DefaultNewUserGlobalGroups.ToString(), Second = previousData.TrimEnd(' ').TrimEnd(','), Third = currentData.TrimEnd(' ').TrimEnd(',') });
            }

            if (!args.Previous.HelpDeskWelcomeMessage.Equals(args.Entity.HelpDeskWelcomeMessage))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.HelpDeskWelcomeMessage.ToString(), Second = args.Previous.HelpDeskWelcomeMessage, Third = args.Entity.HelpDeskWelcomeMessage });
            }

            if (!args.Previous.HelpDeskWelcomeTitle.Equals(args.Entity.HelpDeskWelcomeTitle))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.HelpDeskWelcomeTitle.ToString(), Second = args.Previous.HelpDeskWelcomeTitle, Third = args.Entity.HelpDeskWelcomeTitle });
            }

            if (!args.Previous.SyncWithActiveDirectory.Equals(args.Entity.SyncWithActiveDirectory))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.SyncWithActiveDirectory.ToString(), Second = args.Previous.SyncWithActiveDirectory, Third = args.Entity.SyncWithActiveDirectory });
            }

            if (!args.Previous.ActiveDirectoryConnectionString.Equals(args.Entity.ActiveDirectoryConnectionString))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.ActiveDirectoryConnectionString.ToString(), Second = args.Previous.ActiveDirectoryConnectionString, Third = args.Entity.ActiveDirectoryConnectionString });
            }

            if (!args.Previous.ActiveDirectoryUserName.Equals(args.Entity.ActiveDirectoryUserName))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.ActiveDirectoryUserName.ToString(), Second = args.Previous.ActiveDirectoryUserName, Third = args.Entity.ActiveDirectoryUserName });
            }

            if (!args.Previous.ActiveDirectoryPassword.Equals(args.Entity.ActiveDirectoryPassword))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.ActiveDirectoryPassword.ToString(), Second = "***", Third = "***" });
            }

            if (!args.Previous.ActiveDirectoryAddNewUsers.Equals(args.Entity.ActiveDirectoryAddNewUsers))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.ActiveDirectoryAddNewUsers.ToString(), Second = args.Previous.ActiveDirectoryAddNewUsers, Third = args.Entity.ActiveDirectoryAddNewUsers });
            }

            if (!args.Previous.ActiveDirectoryDomain.Equals(args.Entity.ActiveDirectoryDomain))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.ActiveDirectoryDomain.ToString(), Second = args.Previous.ActiveDirectoryDomain, Third = args.Entity.ActiveDirectoryDomain });
            }

            if (!args.Previous.ActiveDirectoryValidateLogon.Equals(args.Entity.ActiveDirectoryValidateLogon))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.ActiveDirectoryValidateLogon.ToString(), Second = args.Previous.ActiveDirectoryValidateLogon, Third = args.Entity.ActiveDirectoryValidateLogon });
            }

            if (!args.Previous.ActiveDirectoryValidateEmail.Equals(args.Entity.ActiveDirectoryValidateEmail))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.ActiveDirectoryValidateEmail.ToString(), Second = args.Previous.ActiveDirectoryValidateEmail, Third = args.Entity.ActiveDirectoryValidateEmail });
            }

            if (!args.Previous.ActiveDirectoryMapping.Equals(args.Entity.ActiveDirectoryMapping))
            {
                changedValues.Add(new Triplet() { First = "ActiveDirectoryMappings", Second = args.Previous.ActiveDirectoryMapping, Third = args.Entity.ActiveDirectoryMapping });
            }

            if (!args.Previous.SessionTimeOut.Equals(args.Entity.SessionTimeOut))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.SessionTimeOut.ToString(), Second = args.Previous.SessionTimeOut, Third = args.Entity.SessionTimeOut });
            }

            if (!args.Previous.ForceNewPasswordInDays.Equals(args.Entity.ForceNewPasswordInDays))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.ForceNewPasswordInDays.ToString(), Second = args.Previous.ForceNewPasswordInDays, Third = args.Entity.ForceNewPasswordInDays });
            }

            if (!args.Previous.PreventPasswordReuse.Equals(args.Entity.PreventPasswordReuse))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.PreventPasswordReuse.ToString(), Second = args.Previous.PreventPasswordReuse, Third = args.Entity.PreventPasswordReuse });
            }

            if (!args.Previous.AccountLockoutThreshold.Equals(args.Entity.AccountLockoutThreshold))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.AccountLockoutThreshold.ToString(), Second = args.Previous.AccountLockoutThreshold, Third = args.Entity.AccountLockoutThreshold });
            }

            if (!args.Previous.EnforcePasswordPolicy.Equals(args.Entity.EnforcePasswordPolicy))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.EnforcePasswordPolicy.ToString(), Second = args.Previous.EnforcePasswordPolicy, Third = args.Entity.EnforcePasswordPolicy });
            }

            if (!args.Previous.MinimumLetters.Equals(args.Entity.MinimumLetters))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.MinimumLetters.ToString(), Second = args.Previous.MinimumLetters, Third = args.Entity.MinimumLetters });
            }

            if (!args.Previous.NumberRequired.Equals(args.Entity.NumberRequired))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.NumberRequired.ToString(), Second = args.Previous.NumberRequired, Third = args.Entity.NumberRequired });
            }

            if (!args.Previous.UppercaseRequired.Equals(args.Entity.UppercaseRequired))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.UppercaseRequired.ToString(), Second = args.Previous.UppercaseRequired, Third = args.Entity.UppercaseRequired });
            }

            if (!args.Previous.LowercaseRequired.Equals(args.Entity.LowercaseRequired))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.LowercaseRequired.ToString(), Second = args.Previous.LowercaseRequired, Third = args.Entity.LowercaseRequired });
            }

            if (!args.Previous.SymbolRequired.Equals(args.Entity.SymbolRequired))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.SymbolRequired.ToString(), Second = args.Previous.SymbolRequired, Third = args.Entity.SymbolRequired });
            }

            if (!args.Previous.EnableGoogleIntegration.Equals(args.Entity.EnableGoogleIntegration))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.EnableGoogleIntegration.ToString(), Second = args.Previous.EnableGoogleIntegration, Third = args.Entity.EnableGoogleIntegration });
            }

            if (!args.Previous.GoogleClientId.Equals(args.Entity.GoogleClientId))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.GoogleClientId.ToString(), Second = args.Previous.GoogleClientId, Third = args.Entity.GoogleClientId });
            }

            if (!args.Previous.GoogleClientSecret.Equals(args.Entity.GoogleClientSecret))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.GoogleClientSecret.ToString(), Second = "***", Third = "***" });
            }

            if (!args.Previous.Logo.Equals(args.Entity.Logo))
            {
                changedValues.Add(new Triplet() { First = GeminiConfigurationOption.Logo.ToString(), Second = args.Previous.Logo, Third = args.Entity.Logo });
            }

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.GeminiConfiguration;
                audit.FieldChanged = value.First.ToString();
                audit.ValueBefore = value.Second.ToString();
                audit.ValueAfter = value.Third.ToString();
                audit.RowName = string.Empty;

                AdminAuditRepository.InsertAudit(audit);
            }
        }

        #endregion


        #region Organizations

        public void AfterOrganizationCreated(OrganizationEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Entity.Id;
            audit.Action = UserAction.Created;
            audit.AdminArea = AdminAreaVisibility.Organization;
            audit.RowName = audit.ValueAfter = args.Entity.Name;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterOrganizationDeleted(OrganizationEventArgs args)
        {
            AdminAuditDto audit = new AdminAuditDto();

            audit.UserId = args.User.Id;
            audit.RowId = args.Previous.Id;
            audit.Data = args.Previous.ToJson();
            audit.Action = UserAction.Deleted;
            audit.AdminArea = AdminAreaVisibility.Organization;
            audit.RowName = audit.ValueBefore = args.Previous.Name;

            AdminAuditRepository.InsertAudit(audit);
        }

        public void AfterOrganizationUpdated(OrganizationEventArgs args)
        {
            List<Triplet> changedValues = new List<Triplet>();

            if (!args.Previous.Name.Equals(args.Entity.Name))
            {
                changedValues.Add(new Triplet() { First = "name", Second = args.Previous.Name, Third = args.Entity.Name });
            }

            if (!args.Previous.Description.Equals(args.Entity.Description))
            {
                changedValues.Add(new Triplet() { First = "organizationdesc", Second = args.Previous.Description, Third = args.Entity.Description });
            }

            var dateNow = DateTime.Now;

            if (args.Previous.Members.Count > 0) args.Previous.Members.ForEach(s => s.Revised = dateNow);
            if (args.Entity.Members.Count > 0) args.Entity.Members.ForEach(s => s.Revised = dateNow);

            if (!args.Previous.Members.ToJson().Equals(args.Entity.Members.ToJson()))
            {
                var allUsers = args.Context.Users.GetAll();

                var previousUsers = allUsers.Count > 0 && args.Previous.Members.Count > 0 ? allUsers.FindAll(s => args.Previous.Members.Find(a => a.UserId == s.Id) != null).Select(a => a.Fullname).ToDelimited(", ").TrimEnd(' ').TrimEnd(',') : string.Empty;
                var currentUsers = allUsers.Count > 0 && args.Entity.Members.Count > 0 ? allUsers.FindAll(s => args.Entity.Members.Find(a => a.UserId == s.Id) != null).Select(a => a.Fullname).ToDelimited(", ").TrimEnd(' ').TrimEnd(',') : string.Empty;


                changedValues.Add(new Triplet() { First = "members", Second = currentUsers, Third = previousUsers });
            }

            foreach (var value in changedValues)
            {
                AdminAuditDto audit = new AdminAuditDto();

                audit.UserId = args.User.Id;
                audit.RowId = args.Previous.Id;
                audit.Action = UserAction.Edited;
                audit.AdminArea = AdminAreaVisibility.Organization;
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
