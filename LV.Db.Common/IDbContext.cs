using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace LV.Db.Common
{
    public interface IDbContext
    {
        IList<T> ExecToList<T>(BaseQuerySet qr, string SQL, params ParamConst[] pars);
        DataTable ExecToDataTable(string SQL, params ParamConst[] pars);
        InsertDataResult<T> InsertData<T>(string Schema, string TableName, object Data, bool ReturnError = false);
        UpdateDataResult<T> UpdateData<T>(string Schema,string TableName, Expression<Func<T, bool>> Where, object Data, bool ReturnError = false);
        DeleteDataResult<T> DeleteData<T>(string Schema, string TableName, Expression<Func<T, bool>> Where, bool ReturnError = false);
        DataActionResult ExcuteCommand(string SQL, params ParamConst[] pars);
        QuerySet<T> FromNothing<T>(Expression<Func<object, T>> Selector);
        //void BeginTran(DbIsolationLevel level);

        void BeginTran(DbIsolationLevel level = DbIsolationLevel.ReadCommitted);
        void Commit();

        void RollBack();

    }
}
