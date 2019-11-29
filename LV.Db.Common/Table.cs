//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace LV.Db.Common
//{
//    public class Table
//    {
//        private string name;

//        public string schema { get; private set; }

//        public Table(string schema, string name)
//        {
//            this.name = name;
//            this.schema = schema;
//        }

//        public DataField this[string name]
//        {
//            get
//            {
//                return new DataField(this.schema, this.name, name);
//            }
//        }
//    }
//}
