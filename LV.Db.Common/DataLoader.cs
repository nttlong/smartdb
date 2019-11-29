using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LV.Db.Common
{
    public class DataLoader
    {
        public static IList<T> ToList<T>(DataTable dataTable)
        {
            var isAnonymousType = utils.CheckIfAnonymousType(typeof(T));
            var isPrimitiveType = utils.CheckIfPrimitiveType(typeof(T));
            var ret = new List<T>();
            var retObjs = new List<object>();
            var cols = dataTable.Columns.Cast<DataColumn>().Join(typeof(T).GetProperties().Cast<PropertyInfo>(), p => p.ColumnName, q => q.Name, (p, q) => new
            {
                name = p.ColumnName,
                property = q,
                PropertyType = System.Nullable.GetUnderlyingType(q.PropertyType) ?? q.PropertyType
            }).ToList();
            var pType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            foreach (DataRow r in dataTable.Rows)
            {
                if (isAnonymousType)
                {
                    var cIndex = 0;
                    var args = new List<object>();
                    foreach (var col in cols)
                    {
                        if (r[col.name] != DBNull.Value)
                        {
                            try
                            {
                                args.Add(System.Convert.ChangeType(r[col.name], col.PropertyType));
                            }
                            catch (ArgumentException ex)
                            {
                                throw new Exception(ex.Message + ". ColumnName '" + col.name + "'");
                            }
                            catch (Exception ex)
                            {
                                throw (ex);
                            }

                            //col.property.SetValue(item, r[col.name]);
                        }
                        else
                        {
                            args.Add(null);
                        }
                        cIndex++;
                    }

                    var item = (T)Activator.CreateInstance(typeof(T), args.ToArray());
                    ret.Add(item);
                }
                else if (isPrimitiveType)
                {
                    retObjs.Add(System.Convert.ChangeType(r[dataTable.Columns[0]], pType));
                    
                        
                }
                else
                {
                    var item = (T)Activator.CreateInstance(typeof(T));
                    var cIndex = 0;
                    foreach (var col in cols)
                    {
                        if (r[col.name] != DBNull.Value)
                        {
                            try
                            {
#if NET_CORE
                                col.property.SetValue(item, System.Convert.ChangeType(r[col.name], col.PropertyType));
#else
                                col.property.SetValue(item, System.Convert.ChangeType(r[col.name], col.PropertyType),new object[] { });
#endif
                                
                            }
                            catch (ArgumentException ex)
                            {
                                throw new Exception(ex.Message + ". ColumnName '" + col.name + "'");
                            }
                            catch (Exception ex)
                            {
                                throw (ex);
                            }
                        }
                        else
                        {
#if NET_CORE
                            col.property.SetValue(item, null);
#else
                            col.property.SetValue(item, null, new object[] { });
#endif
                        }
                        cIndex++;
                    }
                    ret.Add(item);
                }

            }
            if (isPrimitiveType)
            {
                return retObjs.Cast<T>().ToList();
            }
            return ret;
        }
    }
}
