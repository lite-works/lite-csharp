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
public partial class Result{
public object data;
public string text;
public string permission = "public";
public bool is_virtual;
public bool isDefine;
public bool isMutable;
public string rootID = "";
}
public partial class FeelLangVisitor:FeelParserBaseVisitor<object>{
public string self_ID = "";
public string super_ID = "";
public string set_ID = "";
public string get_ID = "";
public string self_property_ID = "";
public List<string> self_property_content = (new List<string>());
public bool self_property_variable = false;
public HashSet<string> all_ID_set = (new HashSet<string>());
public Stack<HashSet<string>> cuttent_ID_set = (new Stack<HashSet<string>>());
public HashSet<string> type_Id_set = (new HashSet<string>());
public Stack<bool> func_async_stack = (new Stack<bool>());
public  virtual  bool Has_ID( string id ){
return this.all_ID_set.Contains(id)||this.cuttent_ID_set.Peek().Contains(id);
}
public  virtual  void Add_ID( string id ){
this.cuttent_ID_set.Peek().Add(id);
}
public  virtual  void Add_current_set(){
foreach (var item in cuttent_ID_set.Peek()){
all_ID_set.Add(item);
}
this.cuttent_ID_set.Push((new HashSet<string>()));
}
public  virtual  void Delete_current_set(){
this.all_ID_set.ExceptWith(this.cuttent_ID_set.Peek());
this.cuttent_ID_set.Pop();
}
public  virtual  bool Is_type( string id ){
return this.type_Id_set.Contains(id);
}
public  virtual  void Add_type( string id ){
this.type_Id_set.Add(id);
}
public  virtual  void Add_func_stack(){
func_async_stack.Push(false);
}
public  virtual  void Delete_func_stack(){
func_async_stack.Pop();
}
public  virtual  bool Get_func_async(){
return func_async_stack.Peek();
}
public  virtual  void Set_func_async(){
if ( func_async_stack.Peek() ) {
return;
}
func_async_stack.Pop();
func_async_stack.Push(true);
}
}
public partial class FeelLangVisitor{
public FeelLangVisitor (){this.cuttent_ID_set.Push((new HashSet<string>()));
func_async_stack.Push(false);
}
}
public partial class FeelLangVisitor{
public  override  object VisitProgram( ProgramContext context ){
var StatementList = context.statement();
var result = "";
foreach (var item in StatementList){
result+=VisitStatement(item);
}
return result;
}
public  override  object VisitId( IdContext context ){
var r = (new Result(){data = "var"});
var first = (Result)(Visit(context.GetChild(0)));
r.permission=first.permission;
r.text=first.text;
r.is_virtual=first.is_virtual;
if ( context.ChildCount>=2 ) {
foreach (var i in Range(1, context.ChildCount, 1)){
var other = (Result)(Visit(context.GetChild(i)));
r.text+=(new System.Text.StringBuilder().Append("_").Append(other.text)).To_Str();
}
}
if ( keywords.Exists((t)=>t==r.text) ) {
r.text=(new System.Text.StringBuilder().Append("@").Append(r.text)).To_Str();
}
if ( r.text==self_ID ) {
r.text="this";
}
else if ( r.text==super_ID ) {
r.text="base";
}
else if ( r.text==set_ID ) {
r.text="value";
}
else if ( r.text==get_ID ) {
r.text="_"+self_property_ID;
}
r.rootID=r.text;
return r;
}
public  override  object VisitIdItem( IdItemContext context ){
var r = (new Result(){data = "var"});
if ( context.typeAny()!=null ) {
r.text+=context.typeAny().GetText();
r.is_virtual=true;
return r;
}
var id = context.Identifier().GetText();
r.text+=id;
r.is_virtual=true;
if ( id[0]=='_' ) {
r.permission="protected internal";
if ( id[1].Is_lower() ) {
r.isMutable=true;
}
}
else if ( id[0].Is_lower() ) {
r.isMutable=true;
}
return r;
}
public  override  object VisitVarId( VarIdContext context ){
if ( context.Discard()!=null ) {
return "_";
}
else {
var id = ((Result)(Visit(context.id()))).text;
if ( this.Has_ID(id) ) {
return id;
}
else {
this.Add_ID(id);
return "var "+id;
}
}
}
public  override  object VisitVarIdType( VarIdTypeContext context ){
if ( context.Discard()!=null ) {
return "_";
}
else {
var id = ((Result)(Visit(context.id()))).text;
if ( !this.Has_ID(id) ) {
this.Add_ID(id);
}
return Visit(context.typeType())+" "+id;
}
}
public  override  object VisitBoolExpr( BoolExprContext context ){
var r = (new Result());
if ( context.t.Type==TrueLiteral ) {
r.data=TargetTypeBool;
r.text=T;
}
else if ( context.t.Type==FalseLiteral ) {
r.data=TargetTypeBool;
r.text=F;
}
return r;
}
public  override  object VisitAnnotationSupport( AnnotationSupportContext context ){
return (string)(Visit(context.annotation()));
}
public  override  object VisitAnnotation( AnnotationContext context ){
var obj = "";
var r = (string)(Visit(context.annotationList()));
if ( r!="" ) {
obj+=r;
}
return obj;
}
public  override  object VisitAnnotationList( AnnotationListContext context ){
var obj = "";
foreach (var (i, v) in Range(context.annotationItem())){
var txt = (string)(this.Visit(v));
if ( txt!="" ) {
obj+=txt;
}
}
return obj;
}
public  override  object VisitAnnotationItem( AnnotationItemContext context ){
var obj = "";
var id = "";
if ( context.id().Length==2 ) {
id = (new System.Text.StringBuilder().Append(((Result)(Visit(context.id(0)))).text).Append(":")).To_Str();
obj+=((Result)(this.Visit(context.id(1)))).text;
}
else {
obj+=((Result)(this.Visit(context.id(0)))).text;
}
switch (obj) {
case "get" :
{ if ( context.lambda()==null ) {
this.self_property_variable=true;
this.self_property_content.Append((new System.Text.StringBuilder().Append("get{return _").Append(this.self_property_ID).Append("; }")).To_Str());
}
else {
this.self_property_content.Append((new System.Text.StringBuilder().Append("get{").Append(this.VisitPropertyLambda(context.lambda(), true)).Append("}")).To_Str());
}
return "";
} break;
case "set" :
{ if ( context.lambda()==null ) {
this.self_property_variable=true;
this.self_property_content.Append((new System.Text.StringBuilder().Append("set{_").Append(this.self_property_ID).Append("=value;}")).To_Str());
}
else {
this.self_property_content.Append((new System.Text.StringBuilder().Append("set{").Append(this.VisitPropertyLambda(context.lambda(), false)).Append("}")).To_Str());
}
return "";
} break;
case "get_" :
{ if ( context.lambda()==null ) {
this.self_property_variable=true;
this.self_property_content.Append((new System.Text.StringBuilder().Append("private get{return _").Append(this.self_property_ID).Append("; }")).To_Str());
}
else {
this.self_property_content.Append((new System.Text.StringBuilder().Append("private get{").Append(this.VisitPropertyLambda(context.lambda(), true)).Append("}")).To_Str());
}
return "";
} break;
case "set_" :
{ if ( context.lambda()==null ) {
this.self_property_variable=true;
this.self_property_content.Append((new System.Text.StringBuilder().Append("private set{_").Append(this.self_property_ID).Append("=value;}")).To_Str());
}
else {
this.self_property_content.Append((new System.Text.StringBuilder().Append("private set{").Append(this.VisitPropertyLambda(context.lambda(), false)).Append("}")).To_Str());
}
return "";
} break;
case "add" :
{ Todo("not yet");
return "";
} break;
case "remove" :
{ Todo("not yet");
return "";
} break;
}
if ( context.tuple()!=null ) {
obj+=((Result)(this.Visit(context.tuple()))).text;
}
else if ( context.lambda()!=null ) {
obj+=(new System.Text.StringBuilder().Append("(").Append(((Result)(this.Visit(context.lambda()))).text).Append(")")).To_Str();
}
else {
obj+="";
}
if ( id!="" ) {
obj = id+obj;
}
obj = "["+obj+"]";
return obj;
}
public  virtual  string VisitPropertyLambda( LambdaContext context ,  bool is_get ){
this.Add_current_set();
var obj = "";
if ( context.lambdaIn()!=null ) {
this.VisitPropertyLambdaIn(context.lambdaIn(), is_get);
}
if ( context.tupleExpression()!=null ) {
obj+=((Result)(Visit(context.tupleExpression()))).text;
if ( is_get ) {
obj = "return "+obj;
}
obj+=Terminate;
}
else {
obj+=ProcessFunctionSupport(context.functionSupportStatement());
}
this.get_ID="";
this.set_ID="";
this.Delete_current_set();
return obj;
}
public  virtual  void VisitPropertyLambdaIn( LambdaInContext context ,  bool is_get ){
switch (context.id().Length) {
case 1 :
{ var id0 = (Result)(this.Visit(context.id(0)));
this.Add_ID(id0.text);
if ( is_get ) {
this.self_property_variable=true;
this.Add_ID("_"+this.self_property_ID);
this.get_ID=id0.text;
}
else {
this.set_ID=id0.text;
}
} break;
case 2 :
{ this.self_property_variable=true;
this.Add_ID("_"+this.self_property_ID);
var id0 = (Result)(this.Visit(context.id(0)));
var id1 = (Result)(this.Visit(context.id(1)));
this.Add_ID(id0.text);
this.Add_ID(id1.text);
this.get_ID=id0.text;
this.set_ID=id1.text;
} break;
}
}
}
public partial class Compiler_static {
public const string Terminate = ";";
public const string Wrap = "\r\n";
public const string TargetTypeAny = "object";
public const string TargetTypeInt = "int";
public const string TargetTypeNum = "double";
public const string TargetTypeI8 = "sbyte";
public const string TargetTypeI16 = "short";
public const string TargetTypeI32 = "int";
public const string TargetTypeI64 = "long";
public const string TargetTypeU8 = "byte";
public const string TargetTypeU16 = "ushort";
public const string TargetTypeU32 = "uint";
public const string TargetTypeU64 = "ulong";
public const string TargetTypeF32 = "float";
public const string TargetTypeF64 = "double";
public const string TargetTypeBool = "bool";
public const string T = "true";
public const string F = "false";
public const string TargetTypeChr = "char";
public const string TargetTypeStr = "string";
public const string TargetTypeLst = "List";
public const string TargetTypeSet = "Hashset";
public const string TargetTypeDic = "Dictionary";
public const string BlockLeft = "{";
public const string BlockRight = "}";
public const string Task = "System.Threading.Tasks.Task";
}
}
