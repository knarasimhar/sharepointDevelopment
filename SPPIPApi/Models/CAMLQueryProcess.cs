using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPPipAPi.Models
{
    public class CAMLQueryProcess
    {

        private static string MakeExpression(PropertyString nestClause, string value)
        {
            var expr = nestClause.New();
            expr["NestClauseValue"] = value;
            return expr.ToString();
        }

        /// <summary>
        /// Recursively nests the clause with the nesting expression, until nestClauseValue is empty.
        /// </summary>
        /// <param name="whereClause"> A property string in the following format: <Eq><FieldRef Name='Title'/><Value Type='Text'>{{NestClauseValue}}</Value></Eq>"; </param>
        /// <param name="nestingExpression"> A property string in the following format: <And>{{FirstExpression}}{{SecondExpression}}</And> </param>
        /// <param name="nestClauseValues">A string value which NestClauseValue will be filled in with.</param>
        public static string NestEq(PropertyString whereClause, PropertyString nestingExpression, string[] nestClauseValues, int pos = 0)
        {
            if (pos > nestClauseValues.Length)
            {
                return "";
            }

            if (nestClauseValues.Length == 1)
            {
                return MakeExpression(whereClause, nestClauseValues[0]);
            }

            var expr = nestingExpression.New();
            if (pos == nestClauseValues.Length - 2)
            {
                expr["FirstExpression"] = MakeExpression(whereClause, nestClauseValues[pos]);
                expr["SecondExpression"] = MakeExpression(whereClause, nestClauseValues[pos + 1]);
                return expr.ToString();
            }
            else
            {
                expr["FirstExpression"] = MakeExpression(whereClause, nestClauseValues[pos]);
                expr["SecondExpression"] = NestEq(whereClause, nestingExpression, nestClauseValues, pos + 1);
                return expr.ToString();
            }
        }

    }




    public class PropertyString
    {
        private string _propStr;

        public PropertyString New()
        {
            return new PropertyString(_propStr);
        }

        public PropertyString(string propStr)
        {
            _propStr = propStr;
            _properties = new Dictionary<string, string>();
        }

        private Dictionary<string, string> _properties;
        public string this[string key]
        {
            get
            {
                return _properties.ContainsKey(key) ? _properties[key] : string.Empty;
            }
            set
            {
                if (_properties.ContainsKey(key))
                {
                    _properties[key] = value;
                }
                else
                {
                    _properties.Add(key, value);
                }
            }
        }
    }
}