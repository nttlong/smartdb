//namespace LV.Db.Common
//{
//    public class DataField
//    {
//        internal string name;
       

//        public DataField(string schema, string table_name, string name)
//        {
//            this.name = Cmp.GetFieldName(schema,table_name, name);
//        }

//        public DataField(string name)
//        {
//            this.name = name;
//        }

//        public static bool operator==(DataField owner, object other)
//        {
//            return true;
//        }
//        public static bool operator !=(DataField owner, object other)
//        {
//            return true;
//        }
//        public static DataField operator +(DataField owner,object other)
//        {
//            return null;
//        }
//        public static DataField operator +(object other, DataField owner)
//        {
//            return null;
//        }
//        public static DataField operator +(DataField other, DataField owner)
//        {
//            return null;
//        }
//        public DataField Alias(string name)
//        {
//            return null;
//        }
//    }
//}