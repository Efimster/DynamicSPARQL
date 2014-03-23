using System;
using System.Collections.Generic;


namespace DynamicSPARQLSpace
{
    /// <summary>
    /// Helps creating of SPARQL
    /// </summary>
    public static partial class SPARQL
    {
        /// <summary>
        /// Makes a "MINUS" filter expression
        /// </summary>
        /// <param name="s">Subject of first triple</param>
        /// <param name="p">Predicate of first triple</param>
        /// <param name="o">Object of first triple</param>
        /// <returns>"MINUS" filter expression</returns>
        public static Minus Minus(string s = null, string p = null, string o = null)
        {
            var minus = new Minus();
            minus.Items = new[] { new Triple(s, p, o) };
            return minus;
        }
        /// <summary>
        /// Makes an "MINUS" filter expression
        /// </summary>
        /// <param name="triple">first triple</param>
        /// <returns>"MINUS" filter expression</returns>
        public static Minus Minus(string triple)
        {
            IList<string> list = triple.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
            //if (list.Count != 3)
            //    throw new ArgumentException("triple should consist of three items separated by whitespaces", "triple");

            return Minus(s: list[0], p: list[1], o: list[2]);
        }
        /// <summary>
        /// Makes a "MINUS" filter expression
        /// </summary>
        /// <param name="items">items</param>
        /// <returns>"MINUS" filter expression</returns>
        public static Minus Minus(params IWhereItem[] items)
        {
            return new Minus { Items = items };
        }
    }
}
