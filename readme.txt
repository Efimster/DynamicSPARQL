DynamicSPARQL dynamically scaffolds SPARQL queries

Please visit project wiki pages https://github.com/Efimster/DynamicSPARQL/wiki  

Not yet documented:

 Typed projection:

  DynamicSPARQL dyno = DynamicSPARQL.CreateDyno(QueringFunc);

	Book book = dyno.Select<Book>(
		prefixes: new[] { 
			SPARQL.Prefix("dc", "http://purl.org/dc/elements/1.1/"),
			SPARQL.Prefix("ns", "http://example.org/ns#"),
		},
		projection: "?title ?price",
		where: SPARQL.Group(
			SPARQL.Triple("?x ns:price ?price"),
			SPARQL.Triple("?x dc:title ?title")
		),
		orderBy: "desc(?price)"
	)
	.First();

SPARQL Update:

1)Create function for update RDF source that takes update query string
   Func<string, object> UpdateFunc;
2) Create a dynamic object 
    dynamic dyno = DynamicSPARQL.CreateDyno(.., updateFunc,...);
3) Make a query
    //DELETE DATA
    dyno.Delete(
                prefixes: new[] {
                    SPARQL.Prefix("dc", "http://purl.org/dc/elements/1.1/")
                },
                delete: SPARQL.Triple(s: "<http://example/book2>", p: new[] { @"dc:title ""David Copperfield""",
                                                                              @"dc:creator ""Edmund Wells""" })
            );
	//INSERT DATA
	 dyno.Insert(
                prefixes: new[] {
                    SPARQL.Prefix("dc", "http://purl.org/dc/elements/1.1/")
                },
                insert: SPARQL.Triple(s: "<http://example/book1>", p: new[] { @"dc:title ""David Copperfield""",
                                                                              @"dc:creator ""Edmund Wells""" })
            );
	//DELETE/INSERT
	dyno.Update(
                prefixes: new[] {
                    SPARQL.Prefix("foaf", "http://xmlns.com/foaf/0.1/")
                },
                delete: SPARQL.Triple("?person foaf:givenName Bill"),
                insert: SPARQL.Triple("?person foaf:givenName William"),
                where: SPARQL.Triple("?person foaf:givenName Bill")

            );
	//DELETE(informative)
	dyno.Delete(
                prefixes: new[] {
                    SPARQL.Prefix("dc", "http://purl.org/dc/elements/1.1/"),
                    SPARQL.Prefix("xsd","http://www.w3.org/2001/XMLSchema#")
                },
                delete: SPARQL.Triple("?book ?p ?v"),
                where: SPARQL.Group( 
                    SPARQL.Triple("?book dc:date ?date"),
                    SPARQL.Filter(@"?date > ""1970-01-01T00:00:00-02:00""^^xsd:dateTime"),
                    SPARQL.Triple("?book ?p ?v")
                )
            );
	//INSERT(informative)
	dyno.Insert(
                prefixes: new[] {
                    SPARQL.Prefix("foaf","http://xmlns.com/foaf/0.1/")
                },
                insert: SPARQL.Group( 
                    SPARQL.Triple("?person  foaf:name2  ?name"),
                    SPARQL.Triple("?person  foaf:mbox2  ?email")
                ),
                where: SPARQL.Group(
                    SPARQL.Triple("?person  foaf:name  ?name"),
                    SPARQL.Optional("?person  foaf:mbox  ?email")
                )

            );
	//DELETE WHERE
	dyno.Delete(
                prefixes: new[] {
                    SPARQL.Prefix("foaf", "http://xmlns.com/foaf/0.1/")
                },
                where: SPARQL.Triple(s:"?person",p:new[]{"foaf:givenName Fred", "?property ?value"})
            );
