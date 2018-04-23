using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ClearFilesPermission
{
    public static class SecurityHelper
    {
        public static int FileCounter { get; set; }
        public static int FolderCounter { get; set; }

        public static FileAttributes SecureHiddenAttributes => FileAttributes.System |
                                                               FileAttributes.ReadOnly |
                                                               FileAttributes.ReparsePoint |
                                                               FileAttributes.Hidden;
        public static FileAttributes SecureAttributes => FileAttributes.System |
                                                         FileAttributes.ReadOnly |
                                                         FileAttributes.ReparsePoint;
        public static FileAttributes NormalAttributes => FileAttributes.Normal;

        public static DirectorySecurity FullDirectorySecurity
        {
            get
            {
                using (var cuser = WindowsIdentity.GetCurrent())
                {
                    var currentUser = cuser.User ??
                                      (IdentityReference)new NTAccount(Environment.UserDomainName,
                                          Environment.UserName);
                    var admin = new NTAccount(Identity.Administrators);
                    var system = new NTAccount(Identity.System);
                    var authUsers = new NTAccount(Identity.AuthenticatedUsers);
                    var users = new NTAccount(Identity.Users);

                    //
                    // Add access rules for sub folders and files
                    var security = new DirectorySecurity();
                    security.AddAccessRule(users.GetDirectoryFileSystemAllowRule(true, false));
                    security.AddAccessRule(admin.GetDirectoryFileSystemAllowRule(true, false));
                    security.AddAccessRule(system.GetDirectoryFileSystemAllowRule(true, false));
                    security.AddAccessRule(authUsers.GetDirectoryFileSystemAllowRule(true, false));
                    security.AddAccessRule(currentUser.GetDirectoryFileSystemAllowRule(true, false));
                    //
                    // Add access rules for this folder files
                    security.AddAccessRule(users.GetDirectoryFileSystemAllowRule(false, false));
                    security.AddAccessRule(admin.GetDirectoryFileSystemAllowRule(false, false));
                    security.AddAccessRule(system.GetDirectoryFileSystemAllowRule(false, false));
                    security.AddAccessRule(authUsers.GetDirectoryFileSystemAllowRule(false, false));
                    security.AddAccessRule(currentUser.GetDirectoryFileSystemAllowRule(false, false));

                    return security;
                }
            }
        }

        public static void RemoveFileSecurity(this FileInfo file)
        {
            file.Refresh();

            var adminOwner = new NTAccount(Identity.Administrators);
            var security = file.GetAccessControl();
            security.SetOwner(adminOwner);

            foreach (FileSystemAccessRule acc in security.GetAccessRules(true, true, typeof(NTAccount)))
                security.RemoveAccessRuleAll(acc);

            file.SetAccessControl(security);
        }

        public static FileSecurity SetSystemAttributes(this FileInfo file)
        {
            file.RemoveFileSecurity();
            file.Refresh();
            var security = file.GetAccessControl();
            //
            // Add File Security as System ---> Read and Run

            var systemAccessRule =
                new FileSystemAccessRule(Identity.System, FileSystemRights.FullControl, AccessControlType.Allow);
            var adminAccessRule = new FileSystemAccessRule(Identity.Administrators, FileSystemRights.FullControl,
                AccessControlType.Allow);

            var adminAuditRule = new FileSystemAuditRule(Identity.Administrators, FileSystemRights.FullControl,
                AuditFlags.Success);
            var systemAuditRule =
                new FileSystemAuditRule(Identity.System, FileSystemRights.FullControl, AuditFlags.Success);

            // *** Add Access rule for the inheritance
            security.AddAuditRule(systemAuditRule);
            security.AddAuditRule(adminAuditRule);

            security.AddAccessRule(systemAccessRule);
            security.AddAccessRule(adminAccessRule);

            file.SetAccessControl(security);

            file.Refresh();

            return security;
        }

        public static void NormalAttributer(this FileInfo file)
        {
            try
            {
                file.Refresh();
                if (!file.Exists)
                    return;

                var security = file.SetSystemAttributes();

                using (var cuser = WindowsIdentity.GetCurrent())
                {
                    var usersAccessRule = new FileSystemAccessRule(Identity.Users, FileSystemRights.FullControl,
                        AccessControlType.Allow);
                    var authUsersAccessRule = new FileSystemAccessRule(Identity.AuthenticatedUsers,
                        FileSystemRights.FullControl, AccessControlType.Allow);
                    var currentUserAccessRule = new FileSystemAccessRule(
                        cuser.User ??
                        (IdentityReference)new NTAccount(Environment.UserDomainName, Environment.UserName),
                        FileSystemRights.FullControl, AccessControlType.Allow);

                    var usersAuditRule = new FileSystemAuditRule(Identity.Users, FileSystemRights.FullControl,
                        AuditFlags.Success);

                    security.AddAccessRule(usersAccessRule);
                    security.AddAccessRule(authUsersAccessRule);
                    security.AddAccessRule(currentUserAccessRule);

                    security.AddAuditRule(usersAuditRule);

                    file.SetAccessControl(security);
                    //
                    // Set file attribute's
                    file.Attributes = NormalAttributes;
                }
                FileCounter++;
            }
            catch (Exception ex)
            {
                ex.Catch();
            }
            finally
            {
                file.Refresh();
            }
        }

        public static void NormalAttributer(this DirectoryInfo dir)
        {
            try
            {
                dir.SetAccessControl(FullDirectorySecurity);
                FolderCounter++;

                foreach (var subDir in dir.GetDirectories())
                {
                    subDir.NormalAttributer();
                }

                foreach (var subFile in dir.GetFiles())
                {
                    subFile.NormalAttributer();
                }
            }
            catch (Exception ex)
            {
                ex.Catch();
            }
            finally
            {
                CommonHelper.ClearCurrentConsoleLine();
                Console.Write($"{FolderCounter} Folders\t-\t{FileCounter} Files is OK");
            }
        }
    }
}