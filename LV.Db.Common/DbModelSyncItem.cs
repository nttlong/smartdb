using System;
using System.Collections.Generic;

namespace LV.Db.Common
{
    public class DbModelSyncItem
    {
        public Type ModelType { get; internal set; }
        internal Dictionary<string, DBSyncItem> Databases { get; set; }
    }
}