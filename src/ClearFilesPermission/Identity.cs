using System.Security.AccessControl;
using System.Security.Principal;

namespace ClearFilesPermission
{
    public static class Identity
    {
        public const string Users = "Users";
        public const string Administrators = "Administrators";
        public const string System = "SYSTEM";
        public const string AuthenticatedUsers = "Authenticated Users";

        public static FileSystemAccessRule GetDirectoryFileSystemAllowRule(this IdentityReference identity,
            bool applyOnSubFolders, bool isReadonly)
        {
            return new FileSystemAccessRule(identity,
                isReadonly ? FileSystemRights.ReadAndExecute : FileSystemRights.FullControl,
                applyOnSubFolders
                    ? InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit
                    : InheritanceFlags.None,
                applyOnSubFolders ? PropagationFlags.InheritOnly : PropagationFlags.NoPropagateInherit,
                AccessControlType.Allow);
        }

        public static FileSystemAccessRule GetDirectoryFileSystemDenyRule(this IdentityReference identity,
            bool applyOnSubFolders, FileSystemRights rights)
        {
            return new FileSystemAccessRule(identity, rights,
                applyOnSubFolders
                    ? InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit
                    : InheritanceFlags.None,
                applyOnSubFolders ? PropagationFlags.InheritOnly : PropagationFlags.NoPropagateInherit,
                AccessControlType.Deny);
        }
    }
}