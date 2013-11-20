DynamicSPARQL connector for BrightstarDB

General use:
	var func = Connector.GetQueringFunction("type=embedded;storesdirectory=brightstar;storename=" + storeName);
	var dyno = DynamicSPARQL.CreateDyno(func);