using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlUdttHelper
{
    public class SqlUdttEnumeratorProvider<T> : IEnumerable<Microsoft.SqlServer.Server.SqlDataRecord> where T : class
    {
        private List<T> _internalList;
        private string _udtNameToUseForMapping;

        private SqlUdttEnumeratorProvider() { /*don't let them use parameterless ctor*/ }

        public SqlUdttEnumeratorProvider(T singleItem, string udtNameToUseForMapping)
        {
            _udtNameToUseForMapping = udtNameToUseForMapping;
            _internalList = new List<T>();
            _internalList.Add(singleItem);
        }

        public SqlUdttEnumeratorProvider(IEnumerable<T> multipleItems, string udtNameToUseForMapping)
        {
            _udtNameToUseForMapping = udtNameToUseForMapping;
            _internalList = new List<T>();
            _internalList.AddRange(multipleItems);
        }

        public IEnumerator<Microsoft.SqlServer.Server.SqlDataRecord> GetEnumerator()
        {
            Microsoft.SqlServer.Server.SqlDataRecord sdr = GetSqlDataRecordDef();
            
            foreach (T od in _internalList)
            {
                foreach (var property in GetPropertiesDecoratedWithDbUdtColumnAttribute())
                {
                    var attr = GetAttribute(property);
                    if (attr != null)
                    {
                        object propVal = property.GetValue(od);
                        if (propVal == null)
                            sdr.SetDBNull(attr.OrdinalPosition);
                        else
                        {
                            switch (attr.SqlType)
                            {
                                case System.Data.SqlDbType.BigInt:
                                    sdr.SetSqlInt64(attr.OrdinalPosition, long.Parse(propVal.ToString()));
                                    break;
                                case System.Data.SqlDbType.Binary:
                                case System.Data.SqlDbType.Date:
                                case System.Data.SqlDbType.DateTime2:
                                case System.Data.SqlDbType.DateTimeOffset:
                                case System.Data.SqlDbType.Float:
                                case System.Data.SqlDbType.Image:
                                case System.Data.SqlDbType.Money:
                                case System.Data.SqlDbType.NChar:
                                case System.Data.SqlDbType.NText:
                                case System.Data.SqlDbType.Real:
                                case System.Data.SqlDbType.SmallDateTime:
                                case System.Data.SqlDbType.SmallMoney:
                                case System.Data.SqlDbType.Structured:
                                case System.Data.SqlDbType.Text:
                                case System.Data.SqlDbType.Time:
                                case System.Data.SqlDbType.Timestamp:
                                case System.Data.SqlDbType.Udt:
                                case System.Data.SqlDbType.VarBinary:
                                case System.Data.SqlDbType.Variant:
                                case System.Data.SqlDbType.Xml:
                                    throw new NotImplementedException(string.Format("Feature not implemented for type {0} at SqlUdttHelper:section GF4239HF", attr.SqlType));
                                    break;
                                case System.Data.SqlDbType.Bit:
                                    sdr.SetSqlBoolean(attr.OrdinalPosition, bool.Parse(propVal.ToString()));
                                    break;
                                case System.Data.SqlDbType.Char:
                                    //var findOffset = 0;
                                    var buffer =propVal.ToString().ToCharArray();
                                    //var bufferOffset = 0;
                                    //var length = buffer.Length;
                                    System.Data.SqlTypes.SqlChars val = new System.Data.SqlTypes.SqlChars(buffer);
                                    //sdr.SetChars(attr.OrdinalPosition, findOffset, buffer, bufferOffset, length);
                                    sdr.SetSqlChars(attr.OrdinalPosition, val);
                                    break;
                                case System.Data.SqlDbType.DateTime:
                                    sdr.SetSqlDateTime(attr.OrdinalPosition, DateTime.Parse(propVal.ToString()));
                                    break;
                                case System.Data.SqlDbType.Decimal:
                                    sdr.SetSqlDecimal(attr.OrdinalPosition, decimal.Parse(propVal.ToString()));
                                    break;
                                case System.Data.SqlDbType.Int:
                                    sdr.SetSqlInt32(attr.OrdinalPosition, int.Parse(propVal.ToString()));
                                    break;
                                case System.Data.SqlDbType.NVarChar:
                                    sdr.SetSqlString(attr.OrdinalPosition,propVal.ToString());
                                    break;
                                case System.Data.SqlDbType.SmallInt:
                                    sdr.SetSqlInt16(attr.OrdinalPosition, short.Parse(propVal.ToString()));
                                    break;
                                case System.Data.SqlDbType.TinyInt:
                                    sdr.SetByte(attr.OrdinalPosition, byte.Parse(propVal.ToString()));
                                    break;
                                case System.Data.SqlDbType.UniqueIdentifier:
                                    sdr.SetGuid(attr.OrdinalPosition, Guid.Parse(propVal.ToString()));
                                    break;
                                case System.Data.SqlDbType.VarChar:
                                    sdr.SetString(attr.OrdinalPosition,propVal.ToString());
                                    break;
                                default:
                                    throw new NotImplementedException(string.Format("Feature not implemented for type {0} at SqlUdttHelper:section G325239HF", attr.SqlType));
                                    break;
                            }
                        }
                    }
                }

                yield return sdr;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public Microsoft.SqlServer.Server.SqlDataRecord GetSqlDataRecordDef() 
        {
            SortedDictionary<int, SqlMetaData> metaL = new SortedDictionary<int, SqlMetaData>();
            foreach (var property in GetPropertiesDecoratedWithDbUdtColumnAttribute())
            {
                var attr = GetAttribute(property);
                if (attr != null)
                {
                    Microsoft.SqlServer.Server.SqlMetaData meta = null;
                    switch (attr.SqlType)
                    {
                        case System.Data.SqlDbType.Binary:
                        case System.Data.SqlDbType.Char:
                        case System.Data.SqlDbType.NChar:
                        case System.Data.SqlDbType.Image:
                        case System.Data.SqlDbType.NText:
                        case System.Data.SqlDbType.NVarChar:
                        case System.Data.SqlDbType.Text:
                        case System.Data.SqlDbType.VarBinary:
                        case System.Data.SqlDbType.VarChar:
                            meta = new Microsoft.SqlServer.Server.SqlMetaData(attr.Name, attr.SqlType, 1000);
                            break;
                        default:
                            meta = new Microsoft.SqlServer.Server.SqlMetaData(attr.Name, attr.SqlType);
                            break;
                    }
                    metaL.Add(attr.OrdinalPosition, meta);
                }
            }

            // now, some magic:
            // attribute's value "OrdinalPosition" is mapped to actual mapped UDTT
            // but C# class may not to have all UDTT columns mapped
            // I want to "fluf" the missing ones here
            Microsoft.SqlServer.Server.SqlDataRecord result = null;
            if (metaL.Count() > 0)
            {
                var maxMappedOrdinal = metaL.Keys.Last();
                SqlMetaData[] resultArray = new SqlMetaData[maxMappedOrdinal + 1];
                for (int i = 0; i <= maxMappedOrdinal; i++)
                {
                    SqlMetaData something = null;
                    if (!metaL.TryGetValue(i, out something))
                        something = new SqlMetaData("Placeholder" + i.ToString(), System.Data.SqlDbType.Variant);
                    resultArray[i] = something;
                }
                result = new SqlDataRecord(resultArray);
            }
            return result;
        }

        private DbUdttColumnAttribute GetAttribute(System.Reflection.PropertyInfo property)
        {
            var attrA = property.GetCustomAttributes(typeof(DbUdttColumnAttribute), false).Where(ca => ((DbUdttColumnAttribute)ca).UDTTName == _udtNameToUseForMapping);
            if (attrA != null && attrA.Count() > 1)
                throw new NotSupportedException("Unexpected condition at section GetSqlDataRecordDef:H39FEOWFI439"); // means that I found more than one attribute for mapperName
            var attr = attrA.FirstOrDefault() as DbUdttColumnAttribute;
            return attr;
        }

        private IEnumerable<System.Reflection.PropertyInfo> GetPropertiesDecoratedWithDbUdtColumnAttribute()
        {
            return typeof(T).GetProperties()
                                                         .Where(pi => pi.GetCustomAttributes(typeof(DbUdttColumnAttribute), false)
                                                                        .Where(ca => ((DbUdttColumnAttribute)ca).UDTTName == _udtNameToUseForMapping).Count() > 0);
        }
    }
}
