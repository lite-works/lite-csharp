using Library;
using static Library.Lib;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using static Compiler.FeelParser;
using static Compiler.Compiler_static;

namespace Compiler
{
public partial class FeelLangVisitorLoop:FeelLangVisitorJudge{
public FeelLangVisitorLoop(){}
public  override  object VisitRangeExpression( RangeExpressionContext context ){
Func<Result, Result> fn = (e1)=>{var e2 = ((Result)Visit(context.expression(0)));
var r = (new Result());
r.data="IEnumerable<int>";
var rangeName = "";
switch (context.n.Type) {
case FeelParser.To :
{ rangeName="Up_to";
} break;
case FeelParser.Downto :
{ rangeName="Down_to";
} break;
case FeelParser.Until :
{ rangeName="Up_until";
} break;
case FeelParser.Downuntil :
{ rangeName="Down_until";
} break;
}
r.text=(new System.Text.StringBuilder().Append(e1.text).Append(".").Append(rangeName).Append("(").Append(e2.text).Append(")")).To_Str();
if ( context.expression(1)!=null ) {
var step = ((Result)Visit(context.expression(1)));
r.text+=(new System.Text.StringBuilder().Append(".Step(").Append(step.text).Append(")")).To_Str();
}
return r;
};
return fn;
}
public  override  object VisitLoopStatement( LoopStatementContext context ){
var obj = "";
var arr = ((Result)Visit(context.expression()));
var target = arr.text;
var ids = "";
foreach (var (i,v) in context.loopId().WithIndex()){
if ( i!=0 ) {
ids+=","+Visit(v);
}
else {
ids+=Visit(v);
}
}
if ( context.loopId().Length>1 ) {
ids="("+ids+")";
}
obj+=(new System.Text.StringBuilder().Append("foreach (var ").Append(ids).Append(" in ").Append(target).Append(")")).To_Str();
obj+=BlockLeft+Wrap;
Add_current_set();
obj+=ProcessFunctionSupport(context.functionSupportStatement());
Delete_current_set();
obj+=BlockRight+Wrap;
return obj;
}
public  override  object VisitLoopId( LoopIdContext context ){
var id = ((Result)Visit(context.id())).text;
if ( Has_ID(id) ) {
return id;
}
else {
Add_ID(id);
return id;
}
}
public  override  object VisitLoopCaseStatement( LoopCaseStatementContext context ){
var obj = "";
var expr = ((Result)Visit(context.expression()));
obj+=(new System.Text.StringBuilder().Append("while (true) { ").Append(Wrap).Append(" if (").Append(expr.text).Append(") ")).To_Str();
obj+=BlockLeft+Wrap;
Add_current_set();
obj+=ProcessFunctionSupport(context.functionSupportStatement());
Delete_current_set();
obj+=BlockRight+Wrap;
obj+=(new System.Text.StringBuilder().Append(" else { ").Append(Wrap)).To_Str();
if ( context.loopElseStatement()!=null ) {
obj+=Visit(context.loopElseStatement());
}
obj+=(new System.Text.StringBuilder().Append(" break; ").Append(Wrap).Append(" } }")).To_Str();
return obj;
}
public  override  object VisitLoopElseStatement( LoopElseStatementContext context ){
var obj = "";
Add_current_set();
obj+=ProcessFunctionSupport(context.functionSupportStatement());
Delete_current_set();
return obj;
}
public  override  object VisitLoopJumpStatement( LoopJumpStatementContext context ){
return (new System.Text.StringBuilder().Append("break").Append(Terminate).Append(Wrap)).To_Str();
}
public  override  object VisitLoopContinueStatement( LoopContinueStatementContext context ){
return (new System.Text.StringBuilder().Append("continue").Append(Terminate).Append(Wrap)).To_Str();
}
}
}
