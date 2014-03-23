using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DynamicSPARQLSpace
{
    /// <summary>
    /// Helps creating of SPARQL
    /// </summary>
    public static partial class SPARQL
    {
        /// <summary>
        /// Makes triple
        /// </summary>
        /// <param name="s">Subject</param>
        /// <param name="p">Predicate</param>
        /// <param name="o">Object</param>
        /// <returns>triple</returns>
        public static Triple Triple(string s = null, dynamic p = null, dynamic o = null)
        {
            return new Triple(s, p, o );
        }
        /// <summary>
        /// Makes triple
        /// </summary>
        /// <param name="triple">triple string</param>
        /// <returns>triple</returns>
        public static Triple Triple(string triple)
        {
            IList<string> list = triple.Split(new []{' '}, 3,StringSplitOptions.RemoveEmptyEntries);
            //if (list.Count < 3)
            //    throw new ArgumentException("triple should consist of three items separated by whitespaces", "triple");
            
            return new Triple(list[0], list[1], list[2] );
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
        /// Makes Union graph pattern
        /// </summary>
        /// <param name="left">Left part of Union pattern</param>
        /// <param name="right">Right part of Union pattern</param>
        /// <returns>Union graph pattern</returns>
        public static Union Union(IWhereItem left = null, IWhereItem right = null)
        {
            if (left == null)
                left = new Group();
            
            if (right == null)
                right = new Group();
            
            return Union(items: new IWhereItem[] { left, right });
        }
        /// <summary>
        /// Makes Union graph pattern
        /// </summary>
        /// <param name="items">left and right items</param>
        /// <returns>Union graph pattern</returns>
        public static Union Union(params IWhereItem[] items)
        {
            if (items == null)
                return null;

            IWhereItem left = items.Length > 0 ? items[0] : new Group();
            IWhereItem right = items.Length > 1 ? items[1] : new Group();

            if (left as Group == null)
                left = new Group(left);

            if (right as Group == null)
                right = new Group(right);

            return new Union { Items = new IWhereItem[] {left, right} };
        }
        /// <summary>
        /// Makes prefix
        /// </summary>
        /// <param name="prefix">prefix</param>
        /// <param name="iri">IRI</param>
        /// <returns>Prefix</returns>
        public static Prefix Prefix(string prefix, string iri)
        {
            if (string.IsNullOrEmpty(prefix))
                prefix = ":";

            else if (prefix[prefix.Length - 1] != ':')
                prefix += ':'; 

            return new Prefix { PREFIX = prefix, IRI = iri };
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
        /// Makes a "EXISTS" filter expression
        /// </summary>
        /// <param name="s">Subject of first triple</param>
        /// <param name="p">Predicate of first triple</param>
        /// <param name="o">Object of first triple</param>
        /// <returns>"EXISTS" filter expression</returns>
        public static Exists Exists(string s = null, string p = null, string o = null)
        {
            var exists = new Exists();
            exists.Items = new[] { new Triple(s, p, o) };
            return exists;
        }
        /// <summary>
        /// Makes a "EXISTS" filter expression
        /// </summary>
        /// <param name="triple">first triple</param>
        /// <returns>"EXISTS" filter expression</returns>
        public static Exists Exists(string triple)
        {
            IList<string> list = triple.Split(new []{' '}, 3,StringSplitOptions.RemoveEmptyEntries);
            //if (list.Count != 3)
            //    throw new ArgumentException("triple should consist of three items separated by whitespaces", "triple");

            return Exists(s: list[0], p: list[1], o: list[2]);
        }
        /// <summary>
        /// Makes a "EXISTS" filter expression
        /// </summary>
        /// <param name="items">items</param>
        /// <returns>"EXISTS" filter expression</returns>
        public static Exists Exists(params IWhereItem[] items)
        {
            return new Exists { Items = items };
        }
        /// <summary>
        /// Makes a "NOT EXISTS" filter expression
        /// </summary>
        /// <param name="s">Subject of first triple</param>
        /// <param name="p">Predicate of first triple</param>
        /// <param name="o">Object of first triple</param>
        /// <returns>"NOT EXISTS" filter expression</returns>
        public static NotExists NotExists(string s = null, string p = null, string o = null)
        {
            var notExists = new NotExists();
            notExists.Items = new[] { new Triple(s, p, o) };
            return notExists;
        }
        /// <summary>
        /// Makes a "NOT EXISTS" filter expression
        /// </summary>
        /// <param name="triple">first triple</param>
        /// <returns>"NOT EXISTS" filter expression</returns>
        public static NotExists NotExists(string triple)
        {
            IList<string> list = triple.Split(new []{' '}, 3,StringSplitOptions.RemoveEmptyEntries);
            //if (list.Count != 3)
            //    throw new ArgumentException("triple should consist of three items separated by whitespaces", "triple");

            return NotExists(s: list[0], p: list[1], o: list[2]);
        }
        /// <summary>
        /// Makes a "NOT EXISTS" filter expression
        /// </summary>
        /// <param name="items">items</param>
        /// <returns>"NOT EXISTS" filter expression</returns>
        public static NotExists NotExists(params IWhereItem[] items)
        {
            return new NotExists { Items = items };
        }

        /// <summary>
        /// Creates triple chain. Every object used as next triple subject
        /// </summary>
        /// <param name="chain">Triple parts</param>
        /// <returns>triple group. No brackets</returns>
        public static Group TripleChain(params string[] chain)
        {
            if (chain.Length % 2 != 1)
                throw new ArgumentException("wrong triple parts count", "chain");
            
            IList<IWhereItem> triples = new List<IWhereItem>(chain.Length/2);
            
            for (int i = 0; i < chain.Length-1; i+=2 )
            {
                triples.Add(new Triple(chain[i+0], chain[i+1], chain[i+2]));
            }

            return new Group(noBrackets:true) { Items = triples };
        }
        /// <summary>
        /// Creates triple chain. Every object used as next triple subject
        /// </summary>
        /// <param name="chain">Triple parts</param>
        /// <returns>triple group. No brackets</returns>
        public static Group TripleChain(string chain)
        {
            return TripleChain(chain.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries));
        }



        public static string ToString(this Group group, bool autoQuotation,
            bool skipTriplesWithEmptyObject, bool mindAsterisk)
        {
            return group.AppendToString(new StringBuilder(), 
                autoQuotation: autoQuotation, 
                skipTriplesWithEmptyObject:skipTriplesWithEmptyObject,
                mindAsterisk:mindAsterisk).ToString();
        }




    }



    
}
