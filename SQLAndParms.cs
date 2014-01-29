using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;

namespace KCS.Common.Shared
{
    [Serializable()]
    public class SQLAndParms
    {
        private string _sql = "";
        private Hashtable _parms = new Hashtable(10);

        public string Sql
        {
            get { return _sql; }
            set { _sql = value; }
        }

        /// <summary>
        /// Deep Clone this object, including data
        /// </summary>
        /// <returns></returns>
        public SQLAndParms Clone()
        {
            SQLAndParms newSAP = new SQLAndParms();
            newSAP.Sql = Sql;
            foreach (string s in _parms.Keys) newSAP._parms[s] = _parms[s];
            return newSAP;
        }

        public void AddParm(string keyword, string value)
        {          
            _parms[keyword] = value;
        }

        /// <summary>
        /// Add parms from parm value pairs
        /// </summary>
        /// <param name="parmValuePairs">Dictionary of Value Pair</param>
        public void AddParms(string parmValuePairs)
        {
            Dictionary<string, string> dParmValuePairs = Utility.SplitSqlParms(parmValuePairs);
            foreach (string s in dParmValuePairs.Keys)
            {
                AddParm(s, dParmValuePairs[s]);
            }
        }
        /// <summary>
        /// Converts IN condition strings into parameterized IN Clauses
        /// </summary>
        /// <param name="paramsList">The list of values for the IN condition</param>
        /// <param name="delimiter">The delimiting character for the IN clause : comma, etc</param>
        /// <param name="clauseType">clauseType name to ensure parmeter uniqueness</param>
        /// <returns>Parametarized IN Clause string for the invoking SQL statement</returns>
        public string AddParms(string paramsList, char delimiter, string clauseType)
        {
            string returnString = "";
            string param = "";
            int idx = 0;

            string[] parameterValues = paramsList.Split(delimiter);

            foreach (string parameterValue in parameterValues)
            {
                param = "{PARAM_" + clauseType.ToUpper()+"_"+idx.ToString() + "}";
                returnString += param.ToUpper() + ",";
                AddParm(param, parameterValue.ToUpper());
                idx++;
            }

            returnString = returnString.TrimEnd(',');

            return returnString;
        }

        public byte[] GetParmsByteArray()
        {
            DataSet ds = new DataSet();
            ds.Merge(GetParmsDataTable());
            return KCS.Common.Shared.Compression.CompressDataSet(ds);
        }

        public DataTable GetParmsDataTable()
        {
            DataTable dt = new DataTable("parms");
            dt.Columns.Add("keyword");
            dt.Columns.Add("value", typeof(object));

            foreach (string s in _parms.Keys)
            {
                DataRow dr = dt.NewRow();
                dr["keyword"] = s;
                dr["value"] = _parms[s];
                dt.Rows.Add(dr);
            }
            return dt;
        }


        public bool IsKeywordSet(string keyword)
        {
            return _parms.Contains(keyword);
        }

        public bool IsKeywordEmpty(string keyword)
        {
            return Utility.IsEmpty2(_parms[keyword]);
        }

        public bool IsKeywordSetAndEmpty(string keyword)
        {
            return IsKeywordSet(keyword) && IsKeywordEmpty(keyword);
        }

        public void RemoveKeyword(string keyword)
        {
            if (IsKeywordSet(keyword)) _parms.Remove(keyword);
        }
    }
}
