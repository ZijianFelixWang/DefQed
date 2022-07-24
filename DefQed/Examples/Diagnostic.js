// This file is an example of the JS statement format.
// EXPERIMENTAL FEATURE!
// Use the defqed diagnostic database for this

JSDriver.Connect("DtefQed", "oClg2%[TenbL86V+rsC3", "defqed");
JSDriver.Enroll("item", "x");
JSDriver.Enroll("item", "y");
JSDriver.Enroll("item", "z");

JSDriver.Left(JSDriver.MicroStatement(JSDriver.SymbolHolder(JSDriver.Symbol("x")), JSDriver.Notation("=="), JSDriver.SymbolHolder(JSDriver.Symbol("y"))));
JSDriver.Left(JSDriver.MicroStatement(JSDriver.SymbolHolder(JSDriver.Symbol("y")), JSDriver.Notation("=="), JSDriver.SymbolHolder(JSDriver.Symbol("z"))));

JSDriver.Right(JSDriver.MicroStatement(JSDriver.SymbolHolder(JSDriver.Symbol("x")), JSDriver.Notation("=="), JSDriver.SymbolHolder(JSDriver.Symbol("z"))));