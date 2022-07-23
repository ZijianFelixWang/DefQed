// This file is an example of the JS statement format.
// EXPERIMENTAL FEATURE!
// Use the defqed diagnostic database for this

$.Connect("DefQed", "oClg2%[TenbL86V+rsC3", "defqed");
$.Enroll("item", "x");
$.Enroll("item", "y");
$.Enroll("item", "z");
$.LeftPool([
    $.MicroStatement($.SymbolHolder($.Symbol("x")), $.Notation("=="), $.SymbolHolder($.Symbol("y"))),
    $.MicroStatement($.SymbolHolder($.Symbol("y")), $.Notation("=="), $.SymbolHolder($.Symbol("z")))
]);
$.RightPool([
    $.MicroStatement($.SymbolHolder($.Symbol("x")), $.Notation("=="), $.SymbolHolder($.Symbol("z")))
]);