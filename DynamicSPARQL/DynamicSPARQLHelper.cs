using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelperExtensionsLibrary.Strings;

namespace DynamicSPARQLSpace
{
    /// <summary>
    /// Helps creating of SPARQL
    /// </summary>
    public static class SPARQL
    {
        /// <summary>
        /// Makes triple
        /// </summary>
        /// <param name="S">Subject</param>
        /// <param name="P">Predicate</param>
        /// <param name="O">Object</param>
        /// <returns>triple</returns>
        public static Triple Tripple(string S = null, dynamic P = null, dynamic O = null)
        {
            return new Triple() { Subject = S, Property = P, Object = O };
        }
        /// <summary>
        /// Makes triple
        /// </summary>
        /// <param name="triple">triple string</param>
        /// <returns>triple</returns>
        public static Triple Tripple(string triple)
        {
            IList<string> list = triple.SplitExt(" ").ToArray();
            if (list.Count != 3)
                throw new ArgumentException("triple should consist of three items separated by whitespaces", "triple");
            
            return new Triple() { Subject = list[0], Property = list[1], Object = list[2] };
        }
        /// <summary>
        /// Makes a group graph pattern
        /// </summary>
        /// <param name="Items">group pattern items</param>
        /// <returns>group graph pattern</returns>
        public static Group Group(params IWhereItem[] Items)
        {
            return new Group { Items = Items };
        }
        /// <summary>
        /// Makes an optional graph pattern
        /// </summary>
        /// <param name="S">Subject of first triple</param>
        /// <param name="P">Predicate of first triple</param>
        /// <param name="O">Object of first triple</param>
        /// <returns>optional graph pattern</returns>
        public static Optional Optional(string S = null, string P = null, string O = null)
        {
            var optional = new Optional();
            optional.Items = new[] { new Triple { Subject = S, Property = P, Object = O } };
            return optional;
        }
        /// <summary>
        /// Makes an optional graph pattern
        /// </summary>
        /// <param name="triple">first triple</param>
        /// <returns>optional graph pattern</returns>
        public static Optional Optional(string triple)
        {
            IList<string> list = triple.SplitExt(" ").ToArray();
            if (list.Count != 3)
                throw new ArgumentException("triple should consist of three items separated by whitespaces", "triple");

            return Optional ( S: list[0], P: list[1], O: list[2] );
        }
        /// <summary>
        /// Makes an optional graph pattern
        /// </summary>
        /// <param name="Items">optional graph pattern items</param>
        /// <returns>optional graph pattern</returns>
        public static Optional Optional(params IWhereItem[] Items)
        {
            return new Optional { Items = Items };
        }
        /// <summary>
        /// Makes Union graph pattern
        /// </summary>
        /// <param name="Left">Left part of Union pattern</param>
        /// <param name="Right">Right part of Union pattern</param>
        /// <returns>Union graph pattern</returns>
        public static Union Union(IWhereItem Left = null, IWhereItem Right = null)
        {
            if (Left == null)
                Left = new Group();
            
            if (Right == null)
                Right = new Group();
            
            return Union(Items: new IWhereItem[] { Left, Right });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Items"></param>
        /// <returns></returns>
        public static Union Union(params IWhereItem[] Items)
        {
            if (Items == null)
                return null;

            IWhereItem left = Items.Length > 0 ? Items[0] : new Group();
            IWhereItem right = Items.Length > 1 ? Items[1] : new Group();

            if (left as Group == null)
                left = new Group(left);

            if (right as Group == null)
                right = new Group(right);

            return new Union { Items = new IWhereItem[] {left, right} };
        }
        /// <summary>
        /// Makes prefix
        /// </summary>
        /// <param name="Prefix">prefix</param>
        /// <param name="IRI">IRI</param>
        /// <returns>Prefix</returns>
        public static Prefix Prefix(string Prefix, string IRI)
        {
            return new Prefix { PREFIX = Prefix, IRI = IRI };
        }
        /// <summary>
        /// Makes filter graph pattern
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Filter Filter(string filter)
        {
            return new Filter { FILTER = filter };
        }
        /// <summary>
        /// Makes bind pattern
        /// </summary>
        /// <param name="bind">bind expression</param>
        /// <returns>bind pattern</returns>
        public static Bind Bind(string bind)
        {
            return new Bind { BIND = bind };
        }
        /// <summary>
        /// Makes a "MINUS" filter expression
        /// </summary>
        /// <param name="S">Subject of first triple</param>
        /// <param name="P">Predicate of first triple</param>
        /// <param name="O">Object of first triple</param>
        /// <returns>"MINUS" filter expression</returns>
        public static Minus Minus(string S = null, string P = null, string O = null)
        {
            var minus = new Minus();
            minus.Items = new[] { new Triple { Subject = S, Property = P, Object = O } };
            return minus;
        }
        /// <summary>
        /// Makes an "MINUS" filter expression
        /// </summary>
        /// <param name="triple">first triple</param>
        /// <returns>"MINUS" filter expression</returns>
        public static Minus Minus(string triple)
        {
            IList<string> list = triple.SplitExt(" ").ToArray();
            if (list.Count != 3)
                throw new ArgumentException("triple should consist of three items separated by whitespaces", "triple");

            return Minus(S: list[0], P: list[1], O: list[2]);
        }
        /// <summary>
        /// Makes a "MINUS" filter expression
        /// </summary>
        /// <param name="Items">items</param>
        /// <returns>"MINUS" filter expression</returns>
        public static Minus Minus(params IWhereItem[] Items)
        {
            return new Minus { Items = Items };
        }

        /// <summary>
        /// Makes a "EXISTS" filter expression
        /// </summary>
        /// <param name="S">Subject of first triple</param>
        /// <param name="P">Predicate of first triple</param>
        /// <param name="O">Object of first triple</param>
        /// <returns>"EXISTS" filter expression</returns>
        public static Exists Exists(string S = null, string P = null, string O = null)
        {
            var exists = new Exists();
            exists.Items = new[] { new Triple { Subject = S, Property = P, Object = O } };
            return exists;
        }
        /// <summary>
        /// Makes a "EXISTS" filter expression
        /// </summary>
        /// <param name="triple">first triple</param>
        /// <returns>"EXISTS" filter expression</returns>
        public static Exists Exists(string triple)
        {
            IList<string> list = triple.SplitExt(" ").ToArray();
            if (list.Count != 3)
                throw new ArgumentException("triple should consist of three items separated by whitespaces", "triple");

            return Exists(S: list[0], P: list[1], O: list[2]);
        }
        /// <summary>
        /// Makes a "EXISTS" filter expression
        /// </summary>
        /// <param name="Items">items</param>
        /// <returns>"EXISTS" filter expression</returns>
        public static Exists Exists(params IWhereItem[] Items)
        {
            return new Exists { Items = Items };
        }
        /// <summary>
        /// Makes a "NOT EXISTS" filter expression
        /// </summary>
        /// <param name="S">Subject of first triple</param>
        /// <param name="P">Predicate of first triple</param>
        /// <param name="O">Object of first triple</param>
        /// <returns>"NOT EXISTS" filter expression</returns>
        public static NotExists NotExists(string S = null, string P = null, string O = null)
        {
            var notExists = new NotExists();
            notExists.Items = new[] { new Triple { Subject = S, Property = P, Object = O } };
            return notExists;
        }
        /// <summary>
        /// Makes a "NOT EXISTS" filter expression
        /// </summary>
        /// <param name="triple">first triple</param>
        /// <returns>"NOT EXISTS" filter expression</returns>
        public static NotExists NotExists(string triple)
        {
            IList<string> list = triple.SplitExt(" ").ToArray();
            if (list.Count != 3)
                throw new ArgumentException("triple should consist of three items separated by whitespaces", "triple");

            return NotExists(S: list[0], P: list[1], O: list[2]);
        }
        /// <summary>
        /// Makes a "NOT EXISTS" filter expression
        /// </summary>
        /// <param name="Items">items</param>
        /// <returns>"NOT EXISTS" filter expression</returns>
        public static NotExists NotExists(params IWhereItem[] Items)
        {
            return new NotExists { Items = Items };
        }


        public static string AutoquoteSPARQL(this string val)
        {
            return System.Text.RegularExpressions.Regex.Replace(val, "((?<![:\"])\\b)(?!(\\d+|exists|not|a)\\b)(?<!\\?)\\w+(?![:\\(])\\b", "\"$&\"");
        }

        public static string GetPrefixesString(this IEnumerable<Prefix> prefixes)
        {
            var sb = new StringBuilder();

            foreach (var prefix in prefixes)
            {
                sb = prefix.AppendToString(sb);
            }

            return sb.ToString();
        }


    }



    
}
