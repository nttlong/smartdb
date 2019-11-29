using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotLiquid;

namespace ReEngine.Web
{
    public class RawHtml : DotLiquid.Tag
    {
        private int _max;

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            base.Initialize(tagName, markup, tokens);
            //_max = Convert.ToInt32(markup);
        }
        public override void Render(DotLiquid.Context context, TextWriter result)
        {
            result.Write(new Random().Next(_max).ToString());
        }
        //public void Render(Context context, TextWriter result)
        //{
        //    result.Write(new Random().Next(_max).ToString());
        //}
    }
}
