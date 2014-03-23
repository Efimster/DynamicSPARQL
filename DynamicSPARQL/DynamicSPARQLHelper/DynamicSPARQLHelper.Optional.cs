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
        /// Makes an optional graph pattern
        /// </summary>
        /// <param name="s">Subject of first triple</param>
        /// <param name="p">Predicate of first triple</param>
        /// <param name="o">Object of first triple</param>
        /// <returns>optional graph pattern</returns>
        public static Optional Optional(string s = null, string p = null, string o = null)
        {
            var optional = new Optional();
            optional.Items = new[] { new Triple(s, p, o) };
            return optional;
        }
        /// <summary>
        /// Makes an optional graph pattern
        /// </summary>
        /// <param name="triple">first triple</param>
        /// <returns>optional graph pattern</returns>
        public static Optional Optional(string triple)
        {
            IList<string> list = triple.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
            //if (list.Count != 3)
            //    throw new ArgumentException("triple should consist of three items separated by whitespaces", "triple");

            return Optional(s: list[0], p: list[1], o: list[2]);
        }
        /// <summary>
        /// Makes an optional graph pattern
        /// </summary>
        /// <param name="items">optional graph pattern items</param>
        /// <returns>optional graph pattern</returns>
        public static Optional Optional(params IWhereItem[] items)
        {
            return new Optional { Items = items };
        }
    }
}
