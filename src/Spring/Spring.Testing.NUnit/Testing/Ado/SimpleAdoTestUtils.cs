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

using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;

using Common.Logging;

using Spring.Core.IO;
using Spring.Dao;
using Spring.Data;
using Spring.Data.Common;
using Spring.Data.Core;
using Spring.Transaction;
using Spring.Util;

namespace Spring.Testing.Ado
{
    /// <summary>
    /// TBD
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class SimpleAdoTestUtils
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly RegexOptions REGEX_OPTIONS = RegexOptions.Multiline | RegexOptions.ECMAScript | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;

        /// <summary>
        /// TBD
        /// </summary>
        public static readonly string BLOCKDELIM_GO = @"^[\s\n\r]*GO[\s\n\r]*$";
        /// <summary>
        /// TBD
        /// </summary>
        public static readonly Regex BLOCKDELIM_GO_EXP = new Regex(BLOCKDELIM_GO, REGEX_OPTIONS);
        /// <summary>
        /// TBD
        /// </summary>
        public static readonly string BLOCKDELIM_SEMICOLON = @";";
        /// <summary>
        /// TBD
        /// </summary>
        public static readonly Regex BLOCKDELIM_SEMICOLON_EXP = new Regex(BLOCKDELIM_SEMICOLON, REGEX_OPTIONS);
        /// <summary>
        /// TBD
        /// </summary>
        public static readonly string BLOCKDELIM_NEWLINE = @"\n";
        /// <summary>
        /// TBD
        /// </summary>
        public static readonly Regex BLOCKDELIM_NEWLINE_EXP = new Regex(BLOCKDELIM_NEWLINE, REGEX_OPTIONS);

        /// <summary>
        /// TBD
        /// </summary>
        public static Regex BLOCKDELIM_DEFAULT_EXP = BLOCKDELIM_GO_EXP;

        /// <summary>
        /// TBD
        /// </summary>
        public static Regex[] BLOCKDELIM_ALL_EXP = { BLOCKDELIM_GO_EXP, BLOCKDELIM_SEMICOLON_EXP, BLOCKDELIM_NEWLINE_EXP };

        static SimpleAdoTestUtils()
        { }

        /// <summary>
        /// TBD
        /// </summary>
        public static IPlatformTransaction CreateTransaction(IDbProvider dbProvider, ITransactionDefinition txDefinition)
        {
            AdoPlatformTransactionManager txMgr = new AdoPlatformTransactionManager(dbProvider);
            ITransactionStatus txStatus = txMgr.GetTransaction(txDefinition);
            return new PlatformTransactionHolder(txStatus, txMgr);
        }

        /// <summary>
        /// Execute the given script
        /// </summary>
        public static void ExecuteSqlScript(IAdoOperations adoTemplate, string script, params Regex[] blockDelimiter)
        {
            ExecuteSqlScriptInternal(adoTemplate, new EncodedResource(new StringResource(script)), false, blockDelimiter);
        }

        /// <summary>
        /// Execute the given script
        /// </summary>
        public static void ExecuteSqlScript(IAdoOperations adoTemplate, string script, bool continueOnError, params Regex[] blockDelimiter)
        {
            ExecuteSqlScriptInternal(adoTemplate, new EncodedResource(new StringResource(script)), continueOnError, blockDelimiter);
        }

        /// <summary>
        /// Execute the given script
        /// </summary>
        public static void ExecuteSqlScript(IAdoOperations adoTemplate, IResourceLoader resourceLoader, string scriptResourcePath, bool continueOnError, params Regex[] blockDelimiter)
        {
            ExecuteSqlScriptInternal(adoTemplate, new EncodedResource(resourceLoader.GetResource(scriptResourcePath)), continueOnError, blockDelimiter);
        }

        /// <summary>
        /// Execute the given script
        /// </summary>
        public static void ExecuteSqlScript(IAdoOperations adoTemplate, IResource resource, params Regex[] blockDelimiter)
        {
            ExecuteSqlScriptInternal(adoTemplate, new EncodedResource(resource), false, blockDelimiter);
        }

        /// <summary>
        /// Execute the given script
        /// </summary>
        public static void ExecuteSqlScript(IAdoOperations adoTemplate, IResource resource, bool continueOnError, params Regex[] blockDelimiter)
        {
            ExecuteSqlScriptInternal(adoTemplate, new EncodedResource(resource), continueOnError, blockDelimiter);
        }

        /// <summary>
        /// Execute the given script
        /// </summary>
        public static void ExecuteSqlScript(IAdoOperations adoTemplate, EncodedResource resource, bool continueOnError, params Regex[] blockDelimiter)
        {
            ExecuteSqlScriptInternal(adoTemplate, resource, continueOnError, blockDelimiter);
        }

        /// <summary>
        /// Execute the given script
        /// </summary>
        private static void ExecuteSqlScriptInternal(IAdoOperations adoTemplate, EncodedResource resource, bool continueOnError, params Regex[] blockDelimiter)
        {
            AssertUtils.ArgumentNotNull(adoTemplate, "adoTemplate");
            AssertUtils.ArgumentNotNull(resource, "resource");

            if (!CollectionUtils.HasElements(blockDelimiter))
            {
                blockDelimiter = BLOCKDELIM_ALL_EXP;
            }

            List<string> statements = new List<string>();
            try
            {
                GetScriptBlocks(resource, statements, blockDelimiter);
            }
            catch (Exception ex)
            {
                throw new DataAccessResourceFailureException("Failed to open SQL script from " + resource, ex);
            }

            foreach (string statement in statements)
            {
                try
                {
                    adoTemplate.ExecuteNonQuery(CommandType.Text, statement);
                }
                catch (DataAccessException dae)
                {
                    if (!continueOnError)
                    {
                        throw;
                    }
                    Log.Warn(string.Format("SQL statement failed:{0}", statement), dae);
                }
            }
        }

        /// <summary>
        /// TBD
        /// </summary>
        public static void GetScriptBlocks(EncodedResource encodedResource, IList<string> blockCollector, params Regex[] blockDelimiterPatterns)
        {
            AssertUtils.ArgumentNotNull(blockCollector, "blockCollector");

            using (TextReader sr = encodedResource.OpenReader())
            {
                string script = sr.ReadToEnd();

                // the first pattern that finds a match will be used, if any
                Regex patternToUse = BLOCKDELIM_DEFAULT_EXP;
                if (blockDelimiterPatterns != null)
                {
                    foreach (Regex pattern in blockDelimiterPatterns)
                    {
                        if (pattern.IsMatch(script))
                        {
                            patternToUse = pattern;
                            break;
                        }
                    }
                }

                Split(script, patternToUse, blockCollector);
            }
        }

        private static void Split(string text, Regex exp, IList<string> blockCollector)
        {
            //            string[] blocks = exp.Split(text);
            //            foreach(string block in blocks)
            //            {
            //                if (StringUtils.HasText(block))
            //                {
            //                    blockCollector.Add(block);
            //                }
            //            }
            MatchCollection matches = exp.Matches(text);
            int curIndexStart = 0;
            string tmp;
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                Group group = match.Groups[0];
                //                Capture cap = group.Captures[0];
                tmp = text.Substring(curIndexStart, match.Index - curIndexStart);
                if (tmp.Trim().Length > 0)
                    blockCollector.Add(tmp);
                curIndexStart = match.Index + match.Length;
            }

            tmp = text.Substring(curIndexStart);
            if (tmp.Trim().Length > 0)
                blockCollector.Add(tmp);
        }

        #region To be probably added in a future version

        //        public static void ExecuteSqlScript( AdoTemplate adoTemplate, IResource scriptResource, string blockDelimiter, bool continueOnError )
        //        {
        //            ExecuteSqlScript( adoTemplate, new EncodedResource(scriptResource), new Regex( Regex.Escape(blockDelimiter), REGEX_OPTIONS ), continueOnError );
        //        }
        //
        //        public static void ExecuteSqlScript( AdoTemplate adoTemplate, EncodedResource resource, string blockDelimiter, bool continueOnError )
        //        {
        //            ExecuteSqlScript( adoTemplate, resource, new Regex( Regex.Escape(blockDelimiter), REGEX_OPTIONS ), continueOnError );
        //        }

        //        /// <summary>
        //        /// Execute the given script
        //        /// </summary>
        //        public static void ExecuteSqlScript(AdoTemplate adoTemplate, string script, params string[] blockDelimiter)
        //        {
        //            Regex[] exps = CreateRegexpsFromStrings(blockDelimiter);
        //            ExecuteSqlScriptInternal(adoTemplate, new EncodedResource(new StringResource(script)), false, exps);
        //        }

        //        /// <summary>
        //        /// Creates an array of <see cref="Regex"/> from the given <paramref name="blockDelimiter"/>
        //        /// </summary>
        //        private static Regex[] CreateRegexpsFromStrings(string[] blockDelimiter)
        //        {
        //            Regex[] exps = null;
        //            if (!CollectionUtils.IsEmpty(blockDelimiter))
        //            {
        //                ArrayList expsList = new ArrayList();
        //                foreach (string delim in blockDelimiter)
        //                {
        //                    expsList.Add(new Regex(Regex.Escape(delim), REGEX_OPTIONS));
        //                }
        //                exps = (Regex[])expsList.ToArray(typeof(Regex));
        //            }
        //            return exps;
        //        }
        //

        #endregion

        private class PlatformTransactionHolder : IPlatformTransaction
        {
            private ITransactionStatus txStatus;
            private IPlatformTransactionManager txMgr;
            private bool commit;

            public PlatformTransactionHolder(ITransactionStatus txStatus, IPlatformTransactionManager txMgr)
            {
                AssertUtils.ArgumentNotNull(txStatus, "txStatus");
                AssertUtils.ArgumentNotNull(txMgr, "txMgr");
                this.txStatus = txStatus;
                this.txMgr = txMgr;
                this.commit = false;
            }

            public void Dispose()
            {
                try
                {
                    if (txStatus == null)
                        return;

                    if (commit)
                    {
                        txMgr.Commit(txStatus);
                        return;
                    }
                    txMgr.Rollback(txStatus);
                }
                finally
                {
                    txMgr = null;
                    txStatus = null;
                }
            }

            public void Commit()
            {
                commit = true;
            }

            public void Rollback()
            {
                try
                {
                    txMgr.Rollback(txStatus);
                }
                finally
                {
                    txStatus = null;
                }
            }
        }
    }
}
