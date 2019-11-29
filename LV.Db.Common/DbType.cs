using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LV.Db.Common
{
    /// <summary>
    /// The crucial database in which supports
    /// </summary>
    public enum DbTypes
    {
        None,
        /// <summary>
        /// PostgresSQL database engine 
        /// </summary>
        PostgreSQL,
        /// <summary>
        /// SqlServer database engine
        /// </summary>
        SqlServer,
        /// <summary>
        /// MySQlL database engine
        /// </summary>
        MySQL,
        /// <summary>
        /// Oracle database engine
        /// </summary>
        Oracle,
        Sqlite
    }
}