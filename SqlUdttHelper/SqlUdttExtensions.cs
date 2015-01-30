using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlUdttHelper
{
    public static class SqlUdttExtensions
    {
        public static IEnumerable<Microsoft.SqlServer.Server.SqlDataRecord> AsSqlDataRecord<T>(this T entity, string mapperName) where T : class
        {
            if ((entity is System.Collections.IEnumerable || entity is System.Collections.ICollection) && entity.GetType().IsGenericType)
            {
                throw new NotSupportedException("Most likely you did not mean to call this extension method on a colection. Use the overload that takes type if you are calling on a collection");
            }
            else
                return new SqlUdttEnumeratorProvider<T>(entity, mapperName);
        }

        public static IEnumerable<Microsoft.SqlServer.Server.SqlDataRecord> AsSqlDataRecord<T>(this ICollection<T> entityList, string mapperName) where T : class
        {
            return new SqlUdttEnumeratorProvider<T>(entityList, mapperName);
        }

        public static IEnumerable<Microsoft.SqlServer.Server.SqlDataRecord> AsSqlDataRecord<T>(this IEnumerable<T> entityList, string mapperName) where T : class
        {
            return new SqlUdttEnumeratorProvider<T>(entityList, mapperName);
        }

        public static IEnumerable<Microsoft.SqlServer.Server.SqlDataRecord> AsSqlDataRecord<T, P>(this IEnumerable<T> entityList, string mapperName) where T : class where P : class
        {
            List<Microsoft.SqlServer.Server.SqlDataRecord> result = Enumerable.Empty<Microsoft.SqlServer.Server.SqlDataRecord>().ToList();
            foreach (var entity in entityList)
            {
                IEnumerable<P> castT = entity as IEnumerable<P>;

                foreach (var p in castT)
                {
                    result.Concat(new SqlUdttEnumeratorProvider<P>(p, mapperName));
                }
            }
            
            return result;
        }

        public static IEnumerable<Microsoft.SqlServer.Server.SqlDataRecord> AsSqlDataRecord<T, P, Q>(this IEnumerable<T> entityList, string mapperName)
            where T : class
            where P : class
            where Q : class
        {
            List<Microsoft.SqlServer.Server.SqlDataRecord> result = Enumerable.Empty<Microsoft.SqlServer.Server.SqlDataRecord>().ToList();
            foreach (var entity in entityList)
            {
                IEnumerable<P> castT = entity as IEnumerable<P>;

                foreach (var p in castT)
                {
                    IEnumerable<Q> castQ = entity as IEnumerable<Q>;
                    foreach (var q in castQ)
                    {
                        result.Concat(new SqlUdttEnumeratorProvider<Q>(q, mapperName));
                    }
                }
            }

            return result;
        }

        public static object GetUdttFieldValueAt<T>(this T entity, int ordinalPositionSought, string mapperName)
            where T : class
        {
            object result = null;

            if (entity != null)
            {
                Func<object, bool> onlySoughtUdttName = delegate (object ca) 
                {
                    return ((DbUdttColumnAttribute)ca).UDTTName.Equals(mapperName, StringComparison.OrdinalIgnoreCase);
                };

                 
                //foreach (
                //    var property in typeof(T).GetProperties()
                //                             .Where(pi => pi.GetCustomAttributes(typeof(DbUdttColumnAttribute), true)
                //                                            .Where(ca => onlySoughtUdttName(ca)).Count() > 0))
                var members = typeof(T).GetMembers();
                foreach(var mi in members)
                {
                    foreach(Attribute a in Attribute.GetCustomAttributes(mi, typeof(DbUdttColumnAttribute), true))
                    {
                        var attrA = a as DbUdttColumnAttribute;
                        if (attrA != null && attrA.UDTTName.Equals(mapperName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (attrA.OrdinalPosition == ordinalPositionSought)
                            {
                                if (mi is System.Reflection.FieldInfo)
                                {
                                    result = ((System.Reflection.FieldInfo)mi).GetValue(entity);
                                    break; // I'm done: found it
                                }
                                else if (mi is System.Reflection.PropertyInfo)
                                {
                                    result = ((System.Reflection.PropertyInfo)mi).GetValue(entity);
                                    break; // I'm done: found it
                                }
                                else
                                {
                                    throw new NotSupportedException(string.Format("Value of {0} is not supported at SqlUdttExtensions.section:GF3F9WEN", mi.GetType().Name));
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
