﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSPARQLSpace
{
    public class Triple : IWhereItem
    {
        public string Subject { get; set; }
        public dynamic Property { get; set; }
        public dynamic Object { get; set; }

        const string PROPERTY_DELIMITER = "; ";
        const string OBJECT_DELIMITER = ", ";
        static readonly char[] separator = new[] { ' ' };
        static readonly char[] NON_AUTOQUTE = new[] { '"', '?', ':' };


        public Triple(string s, dynamic p, dynamic o)
        {
            Subject = s;
            Property = p;
            Object = (o as string) != null ? o.Trim() : o;
        }


        public override string ToString()
        {
            return ToStringBuilder().ToString();
        }

        public string ToString(bool autoQuotation, bool skipTriplesWithEmptyObject, bool mindAsterisk)
        {
            return ToStringBuilder(autoQuotation, skipTriplesWithEmptyObject, mindAsterisk).ToString();
        }

        public virtual StringBuilder AppendToString(StringBuilder sb, bool autoQuotation = false,
            bool skipTriplesWithEmptyObject = false, bool mindAsterisk = false)
        {
            var str = ToStringBuilder(autoQuotation, skipTriplesWithEmptyObject, mindAsterisk).ToString();
            return string.IsNullOrEmpty(str) ? sb : sb.AppendLine(str); 
            //if (autoQuotation)
            //    str = str.AutoquoteSPARQL();
    
        }

        private StringBuilder ToStringBuilder(bool autoQuotation = false, bool skipTriplesWithEmptyObject = false, bool mindAsterisk = false)
        {
            var sb = new StringBuilder();
            string subject = ApplyGems(Subject, autoQuotation, mindAsterisk);
            string property = ApplyGems(Property as string, autoQuotation, mindAsterisk);
            string obj = ApplyGems(Object as string,autoQuotation, mindAsterisk);
            

            if (!string.IsNullOrEmpty(property))
            {
                if ((Object as IList) == null)
                {
                    if (skipTriplesWithEmptyObject && IsEmptyLiteral(obj))
                        return sb;

                    return sb.Append(string.Concat(subject, " ", property, " ", obj, " ."));
                }

                
                if (Object.Length > 0)
                {
                    sb.Append(string.Concat(subject, " ", property, " "));

                    foreach (var obj2 in Object)
                    {
                        if (!skipTriplesWithEmptyObject || !IsEmptyLiteral(obj2))
                            sb.Append(obj2 + OBJECT_DELIMITER);
                    }


                    sb.Remove(sb.Length - OBJECT_DELIMITER.Length, OBJECT_DELIMITER.Length);
                    sb.Append(" .");
                }

                return sb;
            }
            
            var prop = Property as IEnumerable<string>;
            if (prop != null /*&& prop.Count > 0*/)
            {
                var propWithObjList = prop.Select(x => x.Split(separator, 2, StringSplitOptions.RemoveEmptyEntries));
                
                if (skipTriplesWithEmptyObject)
                    propWithObjList = propWithObjList.Where(x => !IsEmptyLiteral(x[1]));

                var list = propWithObjList.Select(x => 
                    Tuple.Create(ApplyGems(x[0], false, mindAsterisk), ApplyGems(x[1], autoQuotation, mindAsterisk))).ToList();

                if (list.Count == 0)
                    return sb;

                if (list.Count == 1)
                {
                    return sb.Append(string.Concat(subject, " ", list[0].Item1, " ", list[0].Item2, " ."));
                }

                sb.Append(subject + " ");
                               
                foreach (var item in list)
                    sb.Append(string.Concat(item.Item1, " ", item.Item2, PROPERTY_DELIMITER));
                

                sb.Remove(sb.Length - PROPERTY_DELIMITER.Length, PROPERTY_DELIMITER.Length);
                sb.Append(" .");
            }

            return sb;
        }

        private bool IsEmptyLiteral(dynamic literal)
        {
            var str = literal as string;
            if (str == null)
                return false;

            return string.IsNullOrWhiteSpace(str.Trim('"','\''));
        }

        private string ApplyGems(string str, bool autoQuotation, bool mindAsterisk)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            if (mindAsterisk)
                str = str.MindAsterisk();

            if (autoQuotation 
                && str[0]!='"' 
                && NON_AUTOQUTE.Where(x=>x == str[0]).Count()==0
                && str.IndexOf(':') < 0
                && !System.Text.RegularExpressions.Regex.IsMatch(str,@"\b[\d\.]+\b"))
                //str = System.Text.RegularExpressions.Regex.Replace(str, @"((?<!([:""\?]|(<\w+:\S*\b)))\b)(?!([\d\.]+|a)\b)[<\w\x20]+(?![:])\b", @"""$&""");
                str = string.Concat("\"", str, "\"");

            return str;

        }


        public WhreItemType ItemType { get { return WhreItemType.Triple; } }
    }
}
