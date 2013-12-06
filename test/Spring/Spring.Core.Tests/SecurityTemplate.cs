#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;

#pragma warning disable 618

namespace Spring
{
    /// <summary>
    /// Allows to invoke parts of your code within the context of a certain PermissionSet.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You may also use the static method <see cref="LoadDomainPolicyFromAppConfig"/> and 
    /// <see cref="LoadDomainPolicyFromUri"/> to load a <see cref="PolicyLevel"/> instance 
    /// yourself and apply it on your application domain using <see cref="AppDomain.SetAppDomainPolicy"/>. Note,
    /// that you must set the policy *before* an assembly gets loaded to apply that policy on that assembly!
    /// </para>
    /// <para>
    /// The policy file format is the one used by <see cref="SecurityManager.LoadPolicyLevelFromString"/>.
    /// You get good examples from your %FrameworkDir%/CONFIG/ directory (e.g. 'web_mediumtrust.config').
    /// </para>
    /// </remarks>
    /// <author>Erich Eichinger</author>
    public class SecurityTemplate
    {
        /// <summary>
        /// The default full trust permission set name ("FullTrust")
        /// </summary>
        public static readonly string PERMISSIONSET_FULLTRUST = "FullTrust";
        /// <summary>
        /// The default no trust permission set name ("Nothing")
        /// </summary>
        public static readonly string PERMISSIONSET_NOTHING = "Nothing";
        /// <summary>
        /// The default medium trust permission set name ("MediumTrust")
        /// </summary>
        public static readonly string PERMISSIONSET_MEDIUMTRUST = "MediumTrust";
        /// <summary>
        /// The default low trust permission set name ("LowTrust")
        /// </summary>
        public static readonly string PERMISSIONSET_LOWTRUST = "LowTrust";
        /// <summary>
        /// The default asp.net permission set name ("ASP.NET")
        /// </summary>
        public static readonly string PERMISSIONSET_ASPNET = "ASP.Net";

        private readonly PolicyLevel _domainPolicy;
//        private readonly Dictionary<string, SecurityContext> securityContextCache = new Dictionary<string, SecurityContext>();
        private bool throwOnUnknownPermissionSet = true;

        /// <summary>
        /// Avoid beforeFieldInit
        /// </summary>
        static SecurityTemplate()
        {}

        /// <summary>
        /// Invoke the specified callback in a medium trusted context
        /// </summary>
        public static void MediumTrustInvoke( ThreadStart callback )
        {
            SecurityTemplate template = new SecurityTemplate(true);
            template.PartialTrustInvoke(PERMISSIONSET_MEDIUMTRUST, callback);
        }

        /// <summary>
        /// Access the domain <see cref="PolicyLevel"/> of this <see cref="SecurityTemplate"/> instance.
        /// </summary>
        public PolicyLevel DomainPolicy
        {
            get { return _domainPolicy; }
        }

        /// <summary>
        /// Whether to throw an <see cref="ArgumentOutOfRangeException"/> in case
        /// the permission set name is not found when invoking <see cref="PartialTrustInvoke(string,ThreadStart)"/>.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool ThrowOnUnknownPermissionSet
        {
            get { return throwOnUnknownPermissionSet; }
            set
            {
                throwOnUnknownPermissionSet = value;
//                securityContextCache.Clear(); // clear cache
            }
        }

        /// <summary>
        /// Creates a new instance providing default "FullTrust", "Nothing", "MediumTrust" and "LowTrust" permissionsets
        /// </summary>
        /// <param name="allowUnmanagedCode">NCover requires unmangaged code permissions, set this flag <c>true</c> in this case.</param>
        public SecurityTemplate(bool allowUnmanagedCode)
        {
            PolicyLevel pLevel = PolicyLevel.CreateAppDomainLevel();

            // NOTHING permissionset
            if (null == pLevel.GetNamedPermissionSet(PERMISSIONSET_NOTHING) )
            {
                NamedPermissionSet noPermissionSet = new NamedPermissionSet(PERMISSIONSET_NOTHING, PermissionState.None);
                noPermissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.NoFlags));
                pLevel.AddNamedPermissionSet(noPermissionSet);
            }            

            // FULLTRUST permissionset
            if (null == pLevel.GetNamedPermissionSet(PERMISSIONSET_FULLTRUST))
            {
                NamedPermissionSet fulltrustPermissionSet = new NamedPermissionSet(PERMISSIONSET_FULLTRUST, PermissionState.Unrestricted);
                pLevel.AddNamedPermissionSet(fulltrustPermissionSet);
            }
            // MEDIUMTRUST permissionset (corresponds to ASP.Net permission set in web_mediumtrust.config)
            NamedPermissionSet mediumTrustPermissionSet = new NamedPermissionSet(PERMISSIONSET_MEDIUMTRUST, PermissionState.None);
            mediumTrustPermissionSet.AddPermission(new AspNetHostingPermission(AspNetHostingPermissionLevel.Medium));
            mediumTrustPermissionSet.AddPermission(new DnsPermission(PermissionState.Unrestricted));
            mediumTrustPermissionSet.AddPermission(new EnvironmentPermission(EnvironmentPermissionAccess.Read,
                                                                             "TEMP;TMP;USERNAME;OS;COMPUTERNAME"));
            mediumTrustPermissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess,
                                                                        AppDomain.CurrentDomain.BaseDirectory));
            IsolatedStorageFilePermission isolatedStorageFilePermission = new IsolatedStorageFilePermission(PermissionState.None);
            isolatedStorageFilePermission.UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByUser;
            isolatedStorageFilePermission.UserQuota = 9223372036854775807;
            mediumTrustPermissionSet.AddPermission(isolatedStorageFilePermission);
            mediumTrustPermissionSet.AddPermission(new PrintingPermission(PrintingPermissionLevel.DefaultPrinting));
            SecurityPermissionFlag securityPermissionFlag = SecurityPermissionFlag.Assertion | SecurityPermissionFlag.Execution |
                                           SecurityPermissionFlag.ControlThread | SecurityPermissionFlag.ControlPrincipal |
                                           SecurityPermissionFlag.RemotingConfiguration;
            if (allowUnmanagedCode)
            {
                securityPermissionFlag |= SecurityPermissionFlag.UnmanagedCode;
            }
            mediumTrustPermissionSet.AddPermission(new SecurityPermission(securityPermissionFlag));
            mediumTrustPermissionSet.AddPermission(new System.Net.Mail.SmtpPermission(System.Net.Mail.SmtpAccess.Connect));
            mediumTrustPermissionSet.AddPermission(new SqlClientPermission(PermissionState.Unrestricted));
            mediumTrustPermissionSet.AddPermission(new WebPermission());
            pLevel.AddNamedPermissionSet(mediumTrustPermissionSet);

            // LOWTRUST permissionset (corresponds to ASP.Net permission set in web_mediumtrust.config)
            NamedPermissionSet lowTrustPermissionSet = new NamedPermissionSet(PERMISSIONSET_LOWTRUST, PermissionState.None);
            lowTrustPermissionSet.AddPermission(new AspNetHostingPermission(AspNetHostingPermissionLevel.Low));
            lowTrustPermissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read|FileIOPermissionAccess.PathDiscovery,
                                                                        AppDomain.CurrentDomain.BaseDirectory));
            IsolatedStorageFilePermission isolatedStorageFilePermissionLow = new IsolatedStorageFilePermission(PermissionState.None);
            isolatedStorageFilePermissionLow.UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByUser;
            isolatedStorageFilePermissionLow.UserQuota = 1048576;
            lowTrustPermissionSet.AddPermission(isolatedStorageFilePermissionLow);
            SecurityPermissionFlag securityPermissionFlagLow = SecurityPermissionFlag.Execution;
            if (allowUnmanagedCode)
            {
                securityPermissionFlagLow |= SecurityPermissionFlag.UnmanagedCode;
            }
            lowTrustPermissionSet.AddPermission(new SecurityPermission(securityPermissionFlagLow));
            pLevel.AddNamedPermissionSet(lowTrustPermissionSet);

//            UnionCodeGroup rootCodeGroup = new UnionCodeGroup(new AllMembershipCondition(), new PolicyStatement(noPermissionSet, PolicyStatementAttribute.Nothing));
//            pLevel.RootCodeGroup = rootCodeGroup;
            _domainPolicy = pLevel;
        }

        /// <summary>
        /// Loads domain policy from specified file
        /// </summary>
        /// <param name="securityConfigurationFile"></param>
        public SecurityTemplate(FileInfo securityConfigurationFile)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _domainPolicy = LoadDomainPolicyFromUri(new Uri(securityConfigurationFile.FullName), appDirectory, string.Empty);
        }

        /// <summary>
        /// Create a security tool from the specified domainPolicy
        /// </summary>
        public SecurityTemplate(PolicyLevel domainPolicy)
        {
            this._domainPolicy = domainPolicy;
        }

        /// <summary>
        /// Invokes the given callback using the policy's default
        /// partial trust permissionset ("ASP.Net" <see cref="PERMISSIONSET_ASPNET"/>).
        /// </summary>
//        [SecurityTreatAsSafe, SecurityCritical]
        public void PartialTrustInvoke(ThreadStart callback)
        {
            string defaultPermissionSetName = PERMISSIONSET_MEDIUMTRUST;
            if (null != GetNamedPermissionSet(PERMISSIONSET_ASPNET))
            {
                defaultPermissionSetName = PERMISSIONSET_ASPNET;
            }

            PartialTrustInvoke(defaultPermissionSetName, callback);
        }

        /// <summary>
        /// Invokes the given callback using the specified permissionset.
        /// </summary>
//        [SecurityTreatAsSafe, SecurityCritical]
        public void PartialTrustInvoke(string permissionSetName, ThreadStart callback)
        {
            PermissionSet ps = null;
            ps = GetNamedPermissionSet(permissionSetName);
            if (ps == null && throwOnUnknownPermissionSet)
            {
                throw new ArgumentOutOfRangeException("permissionSetName", permissionSetName,
                                                      string.Format("unknown PermissionSet name '{0}'",
                                                                    permissionSetName));
            }

            if (!IsFullTrust(ps))
            {
                ps.PermitOnly();
                callback();
                CodeAccessPermission.RevertPermitOnly();
            }
            else
            {
                callback();
            }
        }

        private PermissionSet GetNamedPermissionSet(string name)
        {
            if (_domainPolicy != null)
            {
                return _domainPolicy.GetNamedPermissionSet(name);
            }
            return null;
        }

#if !MONO
        /// <summary>
        /// Loads the policy configuration from app.config configuration section
        /// </summary>
        /// <example>
        /// Configuration is identical to web.config:
        /// <code>
        /// &lt;configuration&gt;
        ///     &lt;system.web&gt;
        ///         &lt;securityPolicy&gt;
        ///             &lt;trustLevel name=&quot;Full&quot; policyFile=&quot;internal&quot;/&gt;
        ///             &lt;trustLevel name=&quot;High&quot; policyFile=&quot;web_hightrust.config&quot;/&gt;
        ///             &lt;trustLevel name=&quot;Medium&quot; policyFile=&quot;web_mediumtrust.config&quot;/&gt;
        ///             &lt;trustLevel name=&quot;Low&quot; policyFile=&quot;web_lowtrust.config&quot;/&gt;
        ///             &lt;trustLevel name=&quot;Minimal&quot; policyFile=&quot;web_minimaltrust.config&quot;/&gt;
        ///         &lt;/securityPolicy&gt;
        ///         &lt;trust level=&quot;Medium&quot; originUrl=&quot;&quot;/&gt;
        ///     &lt;/system.web&gt;
        /// &lt;/configuration&gt;
        /// </code>
        /// </example>
        /// <param name="throwOnError"></param>
        /// <returns></returns>
        public static PolicyLevel LoadDomainPolicyFromAppConfig(bool throwOnError)
        {
            TrustSection trustSection = (TrustSection)ConfigurationManager.GetSection("system.web/trust");
            SecurityPolicySection securityPolicySection = (SecurityPolicySection)ConfigurationManager.GetSection("system.web/securityPolicy");

            if ((trustSection == null) || string.IsNullOrEmpty(trustSection.Level))
            {
                if (!throwOnError) return null;
                throw new ConfigurationErrorsException("Configuration section <system.web/trust> not found ");
            }

            if (trustSection.Level == "Full")
            {   
                return null;
            }

            if ((securityPolicySection == null) || (securityPolicySection.TrustLevels[trustSection.Level] == null))
            {
                if (!throwOnError) return null;
                throw new ConfigurationErrorsException(string.Format("configuration <system.web/securityPolicy/trustLevel@name='{0}'> not found", trustSection.Level));
            }

            string policyFileExpanded = GetPolicyFilenameExpanded(securityPolicySection.TrustLevels[trustSection.Level]);
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            PolicyLevel domainPolicy = LoadDomainPolicyFromUri(new Uri(policyFileExpanded), appDirectory, trustSection.OriginUrl);
            return domainPolicy;
        }
#endif
        /// <summary>
        /// Loads a policy from a file (<see cref="SecurityManager.LoadPolicyLevelFromFile"/>), 
        /// replacing placeholders  
        /// <list>
        ///   <item>$AppDir$, $AppDirUrl$ => <paramref name="appDirectory"/></item>
        ///   <item>$CodeGen$ => (TODO)</item>
        ///   <item>$OriginHost$ => <paramref name="originUrl"/></item>
        ///   <item>$Gac$ => the current machine's GAC path</item>
        /// </list>
        /// </summary>
        /// <param name="policyFileLocation"></param>
        /// <param name="originUrl"></param>
        /// <param name="appDirectory"></param>
        /// <returns></returns>
        public static PolicyLevel LoadDomainPolicyFromUri(Uri policyFileLocation, string appDirectory, string originUrl)
        {
            bool foundGacToken = false;
            PolicyLevel domainPolicy = CreatePolicyLevel(policyFileLocation, appDirectory, appDirectory, originUrl, out foundGacToken);
            if (foundGacToken)
            {
                CodeGroup rootCodeGroup = domainPolicy.RootCodeGroup;
                bool hasGacMembershipCondition = false;
                foreach (CodeGroup childCodeGroup in rootCodeGroup.Children)
                {
                    if (childCodeGroup.MembershipCondition is GacMembershipCondition)
                    {
                        hasGacMembershipCondition = true;
                        break;
                    }
                }
                if (!hasGacMembershipCondition && (rootCodeGroup is FirstMatchCodeGroup))
                {
                    FirstMatchCodeGroup firstMatchCodeGroup = (FirstMatchCodeGroup)rootCodeGroup;
                    if ((firstMatchCodeGroup.MembershipCondition is AllMembershipCondition) && (firstMatchCodeGroup.PermissionSetName == PERMISSIONSET_NOTHING))
                    {
                        PermissionSet unrestrictedPermissionSet = new PermissionSet(PermissionState.Unrestricted);
                        CodeGroup gacGroup = new UnionCodeGroup(new GacMembershipCondition(), new PolicyStatement(unrestrictedPermissionSet));
                        CodeGroup rootGroup = new FirstMatchCodeGroup(rootCodeGroup.MembershipCondition, rootCodeGroup.PolicyStatement);
                        foreach (CodeGroup childGroup in rootCodeGroup.Children)
                        {
                            if (((childGroup is UnionCodeGroup) && (childGroup.MembershipCondition is UrlMembershipCondition)) && (childGroup.PolicyStatement.PermissionSet.IsUnrestricted() && (gacGroup != null)))
                            {
                                rootGroup.AddChild(gacGroup);
                                gacGroup = null;
                            }
                            rootGroup.AddChild(childGroup);
                        }
                        domainPolicy.RootCodeGroup = rootGroup;
                    }
                }
            }
            return domainPolicy;
        }

        private static PolicyLevel CreatePolicyLevel(Uri configFile, string appDir, string binDir, string strOriginUrl, out bool foundGacToken)
        {
            WebClient webClient = new WebClient();
            string strXmlPolicy = webClient.DownloadString(configFile);
            appDir = FileUtil.RemoveTrailingDirectoryBackSlash(appDir);
            binDir = FileUtil.RemoveTrailingDirectoryBackSlash(binDir);
            strXmlPolicy = strXmlPolicy.Replace("$AppDir$", appDir).Replace("$AppDirUrl$", MakeFileUrl(appDir)).Replace("$CodeGen$", MakeFileUrl(binDir));
            if (strOriginUrl == null)
            {
                strOriginUrl = string.Empty;
            }
            strXmlPolicy = strXmlPolicy.Replace("$OriginHost$", strOriginUrl);
            if (strXmlPolicy.IndexOf("$Gac$", StringComparison.Ordinal) != -1)
            {
                string gacLocation = GetGacLocation();
                if (gacLocation != null)
                {
                    gacLocation = MakeFileUrl(gacLocation);
                }
                if (gacLocation == null)
                {
                    gacLocation = string.Empty;
                }
                strXmlPolicy = strXmlPolicy.Replace("$Gac$", gacLocation);
                foundGacToken = true;
            }
            else
            {
                foundGacToken = false;
            }
            return SecurityManager.LoadPolicyLevelFromString(strXmlPolicy, PolicyLevelType.AppDomain);
        }
#if !MONO
        private static string GetPolicyFilenameExpanded(TrustLevel trustLevel)
        {
            bool isRelative = true;
            if (trustLevel.PolicyFile.Length > 1)
            {
                char ch = trustLevel.PolicyFile[1];
                char ch2 = trustLevel.PolicyFile[0];
                if (ch == ':')
                {
                    isRelative = false;
                }
                else if ((ch2 == '\\') && (ch == '\\'))
                {
                    isRelative = false;
                }
            }

            if (isRelative)
            {
                string configurationElementFileSource = trustLevel.ElementInformation.Properties["policyFile"].Source;
                string path = configurationElementFileSource.Substring(0, configurationElementFileSource.LastIndexOf('\\') + 1);
                return path + trustLevel.PolicyFile;
            }
            return trustLevel.PolicyFile;
        }
#endif
        [DllImport("mscorwks.dll", CharSet = CharSet.Unicode)]
        private static extern int GetCachePath(int dwCacheFlags, StringBuilder pwzCachePath, ref int pcchPath);

        private static string GetGacLocation()
        {
            int capacity = 0x106;
            StringBuilder pwzCachePath = new StringBuilder(capacity);
            int pcchPath = capacity - 2;
            int hRes = GetCachePath(2, pwzCachePath, ref pcchPath);
            if (hRes < 0)
            {
                throw new HttpException("failed obtaining GAC path", hRes);
            }
            return pwzCachePath.ToString();
        }

        private static string MakeFileUrl(string path)
        {
            Uri uri = new Uri(path);
            return uri.ToString();
        }

        private class FileUtil
        {
            internal static string RemoveTrailingDirectoryBackSlash(string path)
            {
                if (path == null)
                {
                    return null;
                }
                int length = path.Length;
                if ((length > 3) && (path[length - 1] == '\\'))
                {
                    path = path.Substring(0, length - 1);
                }
                return path;
            }
        }

        private static bool IsFullTrust(PermissionSet perms)
        {
            if (perms != null)
            {
                return perms.IsUnrestricted();
            }
            return true;
        }

        //        [SecurityTreatAsSafe, SecurityCritical]
        //        private bool NeedPartialTrustInvoke(string permissionSetName)
        //        {
        //            if (_domainPolicy == null) return false;
        //
        //            SecurityContext securityContext = null;
        //            if (!securityContextCache.ContainsKey(permissionSetName))
        //            {
        //                NamedPermissionSet permissionSet = _domainPolicy.GetNamedPermissionSet(permissionSetName);
        //                if (permissionSet == null && throwOnUnknownPermissionSet)
        //                {
        //                    throw new ArgumentOutOfRangeException("permissionSetName", permissionSetName, "Undefined permission set");
        //                }
        //                if (!IsFullTrust(permissionSet))
        //                {
        //                    try
        //                    {
        //                        permissionSet.PermitOnly();
        //                        securityContext = CaptureSecurityContextNoIdentityFlow();
        //                    }
        //                    finally
        //                    {
        //                        CodeAccessPermission.RevertPermitOnly();
        //                    }
        //                }
        //                securityContextCache[permissionSetName] = securityContext;
        //            }
        //            else
        //            {
        //                securityContext = securityContextCache[permissionSetName];
        //            }
        //            return (securityContext != null);
        //        }


//        [SecurityCritical]
//        private static SecurityContext CaptureSecurityContextNoIdentityFlow()
//        {
//            if (SecurityContext.IsWindowsIdentityFlowSuppressed())
//            {
//                return SecurityContext.Capture();
//            }
//            using (SecurityContext.SuppressFlowWindowsIdentity())
//            {
//                return SecurityContext.Capture();
//            }
//        }
    }
}
#pragma warning restore 618
