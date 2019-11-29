namespace LV.Db.Common
{
    /// <summary>
    /// The class serve for Lambda Expression converting. 
    /// Any time when we'd like to convert a MemberExpression into struct with 2 parts: Expresion and Alias, for example: 
    /// p=>new {Fullname = p.FirstName+" "+p.LastName} 
    /// will be generate AliasFieldExpression {
    ///     Exprssion='FirstName+" "+LastName,
    ///     Alias='Fullname'
    ///  }
    /// </summary>
    internal class AliasFieldExpression
    {
        public string Expresion { get; internal set; }
        public string Alias { get; internal set; }
    }
}