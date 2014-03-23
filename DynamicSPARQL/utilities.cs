using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSPARQLSpace
{
    public static class Utilities
    {
        /// <summary>
        /// Generates string GUID
        /// </summary>
        /// <param name="noDashes"></param>
        /// <returns></returns>
        public static string NewStringGuid(bool noDashes = true)
        {
            return noDashes ? Guid.NewGuid().ToString().Replace("-", string.Empty) : Guid.NewGuid().ToString();

        }

        /// <summary>
        /// Chaecks whether string is IRI
        /// </summary>
        /// <param name="str"></param>
        /// <returns>true - string is IRI, otherwise false</returns>
        public static IRIType GetIRIType(this string str)
        {
            var test = str.Trim();
            if (test[0] == '<')
                return IRIType.FullBracketed;
            //check the url pattern 
            if (System.Text.RegularExpressions.Regex.IsMatch(test,
                @"(((http(?s):(?:\/\/)?)(?:[-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w-_]*)?\??(?:[-\+=&;%@.\w_]*)#?(?:[\w]*))?)"
                , System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                )
                return IRIType.FullUnbracketed;

            if (test.IndexOf(':') > -1)
                return IRIType.Prefixed;


            return IRIType.None;
        }

        public static string MindAsterisk(this string str)
        {
            int idx = 0;
            string res = string.Empty;

            while ((idx = str.IndexOf("?*")) >= 0)
            {
                res += str.Substring(0, idx) + "?" + NewStringGuid();
                str = str.Substring(idx + 2);
            }

            res += str;

            return res;
        }

        public static string AutoquoteSPARQL(this string val)
        {
            return System.Text.RegularExpressions.Regex.Replace(val, "((?<![:\"])\\b)(?!(\\d+|exists|not|a|in)\\b)(?<!\\?)\\w+(?![:\\(])\\b", "\"$&\"",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        public enum IRIType
        {
            None,
            FullBracketed,
            FullUnbracketed,
            Prefixed
        }

    }
}
