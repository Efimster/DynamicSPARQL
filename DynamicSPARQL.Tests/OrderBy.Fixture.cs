﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;
using Should.Fluent;

namespace DynamicSPARQLSpace.Tests
{
    public class OrderByFixture
    {

        [Theory(DisplayName = "Order by "),
         Xunit.Trait("SPARQL Query", ""),
         InlineData(@"@prefix dc:   <http://purl.org/dc/elements/1.1/> .
                @prefix :     <http://example.org/book/> .
                @prefix ns:   <http://example.org/ns#> .

                :book1  dc:title  ""SPARQL Tutorial"" .
                :book1  ns:price  42 .
                :book2  dc:title  ""The Semantic Web"" .
                :book2  ns:price  23 .")]
        public void TestOrderBy1(string data)
        {
            var dyno = TestDataProvider.GetDyno(data);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { 
                    SPARQL.Prefix("dc:", "http://purl.org/dc/elements/1.1/"),
                    SPARQL.Prefix("ns:", "http://example.org/ns#"),
                },
                projection: "?title ?price",
                where: SPARQL.Triple(s: "?x", p: new[] { "ns:price ?price", "dc:title ?title" }),
                orderBy: "desc(?price)"
            );

            var list = res.ToList();
            list.Count.Should().Equal(2);
            ((int)list.First().price).Should().Equal(42);

        }
    }
}
