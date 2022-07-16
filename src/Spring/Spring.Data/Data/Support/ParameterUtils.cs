#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.Collections;
using System.Data;
using Common.Logging;
using Spring.Data.Common;

namespace Spring.Data.Support
{
	/// <summary>
	/// Miscellaneous utility methods for manipulating parameter objects.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public class ParameterUtils
	{
		#region Fields

		#endregion

		#region Constants

		/// <summary>
		/// The shared log instance for this class (and derived classes).
		/// </summary>
		protected static readonly ILog log =
			LogManager.GetLogger(typeof (ParameterUtils));

		#endregion

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="ParameterUtils"/> class.
                /// </summary>
		public 	ParameterUtils()
		{

		}

		#endregion

		#region Properties

		#endregion

		#region Methods


        /// <summary>
        /// Copies the parameters from IDbParameters to the parameter collection in IDbCommand
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="springParamCollection">The spring param collection.</param>
        public static void CopyParameters(IDbCommand command, IDbParameters springParamCollection)
        {
            if (springParamCollection != null)
            {
                IDataParameterCollection collection = springParamCollection.DataParameterCollection;

                foreach (IDbDataParameter parameter in collection)
                {
                    IDbDataParameter pClone = (IDbDataParameter)((ICloneable)parameter).Clone();
                    command.Parameters.Add(pClone);
                }
            }
        }

	    public static IDataParameter[] CloneParameters(IDbCommand command)
	    {
            IDataParameter[] returnParameters = new IDataParameter[command.Parameters.Count];
            for (int i = 0; i < command.Parameters.Count; i++)
            {
                IDbDataParameter pClone = (IDbDataParameter)((ICloneable)command.Parameters[i]).Clone();
                returnParameters[i] = pClone;
            }
	        return returnParameters;
	    }


        /// <summary>
        /// Copies the parameters in IDbCommand to IDbParameters
        /// </summary>
        /// <param name="springParamCollection">The spring param collection.</param>
        /// <param name="command">The command.</param>
        public static void CopyParameters(IDbParameters springParamCollection,
                                          IDbCommand command)
        {
            if (springParamCollection != null)
            {
                IDataParameterCollection cmdParameterCollection =  command.Parameters;
                //TODO investigate copying only the values over, not a full clone.
                int count = 0;
                foreach (IDbDataParameter dbDataParameter in cmdParameterCollection)
                {
                    springParamCollection[count] = (IDbDataParameter)((ICloneable)dbDataParameter).Clone();;
                    count++;
                }
            }
        }

		#endregion

	    public static void ExtractOutputParameters(IDictionary returnedParameters, IDbCommand command)
	    {
	        if (returnedParameters == null)
	        {
	            return;
	        }
	        IDataParameterCollection paramCollection = command.Parameters;
	        int count = 0;
	        foreach (IDbDataParameter dbDataParameter in paramCollection)
	        {
                if (dbDataParameter.Direction == ParameterDirection.Output
                    || dbDataParameter.Direction == ParameterDirection.InputOutput)
                {
                    if (dbDataParameter.ParameterName == null)
                    {
                        returnedParameters.Add("P" + count, dbDataParameter.Value);
                    }
                    else
                    {
                        returnedParameters.Add(dbDataParameter.ParameterName, dbDataParameter.Value);
                    }
                }
                if (dbDataParameter.Direction == ParameterDirection.ReturnValue)
                {
                    returnedParameters.Add("RETURN_VALUE", dbDataParameter.Value);
                }
	            count++;
	        }
            /*
	        for (int i = 0; i < declaredParameters.Count; i++)
	        {
	            IDataParameter declaredParameter = declaredParameters[i];
	            if (declaredParameter.Direction == ParameterDirection.Output
	                || declaredParameter.Direction == ParameterDirection.InputOutput)
	            {
	                IDataParameter outParameter = (IDataParameter)command.Parameters[i];
	                if (declaredParameter.ParameterName == null)
	                {
	                    returnedParameters.Add("P" + i, outParameter.Value);
	                }
	                else
	                {
	                    returnedParameters.Add(outParameter.ParameterName, outParameter.Value);
	                }
	            }
	        }*/
	    }
	}
}
