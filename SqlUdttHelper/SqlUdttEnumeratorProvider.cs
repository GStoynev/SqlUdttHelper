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
            if (singleItem is System.Collections.ICollection || singleItem is System.Collections.IEnumerable)
                throw new NotSupportedException("this hsould have resoplved into the other constructor");

            _udtNameToUseForMapping = udtNameToUseForMapping;
            if (!(singleItem is System.Collections.ICollection))
            {
                _internalList = new List<T>();
                _internalList.Add(singleItem);
            }
        }

        //public SqlUdttEnumeratorProvider(IEnumerable<T> multipleItems, Type a, string udtNameToUseForMapping)
        public SqlUdttEnumeratorProvider(IEnumerable<T> multipleItems, string udtNameToUseForMapping)
        {
            _udtNameToUseForMapping = udtNameToUseForMapping;
            _internalList = new List<T>(multipleItems);
            //_internalList.AddRange(multipleItems);
        }

        public IEnumerator<Microsoft.SqlServer.Server.SqlDataRecord> GetEnumerator()
        {
            Microsoft.SqlServer.Server.SqlDataRecord sdr = GetSqlDataRecordDef();

            foreach (T od in _internalList)
            {
                foreach (var member in GetPropertiesDecoratedWithDbUdtColumnAttribute())
                {
                    var attr = GetAttribute(member);
                    if (attr != null)
                    {
                        object propVal = null;
                        if (member is System.Reflection.FieldInfo)
                            propVal = ((System.Reflection.FieldInfo)member).GetValue(od);
                        else if (member is System.Reflection.PropertyInfo)
                            propVal = ((System.Reflection.PropertyInfo)member).GetValue(od);
                        else
                            throw new NotSupportedException(string.Format("Value of {0} is not supported at SqlUdttEnumeratorProvider.section:5G3089GHRN", member.GetType().Name));
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
                                    var buffer = propVal.ToString().ToCharArray();
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
                                    sdr.SetSqlString(attr.OrdinalPosition, propVal.ToString());
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
                                    sdr.SetString(attr.OrdinalPosition, propVal.ToString());
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
                            // TODO: must implement the Length attribute property
                            meta = new Microsoft.SqlServer.Server.SqlMetaData(attr.Name, attr.SqlType, 4000);
                            break;
                        case System.Data.SqlDbType.Decimal:
                            if (attr.Precision > 0)
                            {
                                meta = new Microsoft.SqlServer.Server.SqlMetaData(attr.Name, attr.SqlType, attr.Precision, attr.Scale);
                            }
                            else
                            {
                                meta = new Microsoft.SqlServer.Server.SqlMetaData(attr.Name, attr.SqlType);
                            }

                            break;
                        default:
                            meta = new Microsoft.SqlServer.Server.SqlMetaData(attr.Name, attr.SqlType);
                            break;
                    }
                    metaL.Add(attr.OrdinalPosition, meta);
                }
                else
                    throw new NotSupportedException(String.Format("Unexpected condition for MemberInfo {0} while looking for UDTT {1} (SqlUdttEnumeratorProvider.section G397RW9)", property.Name, _udtNameToUseForMapping));
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
            var attrA = property.GetCustomAttributes(typeof(DbUdttColumnAttribute), true).Where(ca => ((DbUdttColumnAttribute)ca).UDTTName.Equals(_udtNameToUseForMapping, StringComparison.OrdinalIgnoreCase));
            if (attrA != null && attrA.Count() > 1)
                throw new NotSupportedException("Unexpected condition at section GetSqlDataRecordDef:H39FEOWFI439"); // means that I found more than one attribute for mapperName
            var attr = attrA.FirstOrDefault() as DbUdttColumnAttribute;
            return attr;
        }

        private DbUdttColumnAttribute GetAttribute(System.Reflection.MemberInfo member)
        {
            var attrA = Attribute.GetCustomAttributes(member, typeof(DbUdttColumnAttribute), true).Where(ca => ((DbUdttColumnAttribute)ca).UDTTName.Equals(_udtNameToUseForMapping, StringComparison.OrdinalIgnoreCase));
            if (attrA != null && attrA.Count() > 1)
                throw new NotSupportedException("Unexpected condition at section GetSqlDataRecordDef:H39FEOWFI439-2"); // means that I found more than one attribute for mapperName
            var attr = attrA.FirstOrDefault() as DbUdttColumnAttribute;
            return attr;
        }

        private IEnumerable<System.Reflection.MemberInfo> GetPropertiesDecoratedWithDbUdtColumnAttribute()
        {
            //var ret = typeof(T).GetProperties()
            //                   .Where(pi => pi.GetCustomAttributes(typeof(DbUdttColumnAttribute), false)
            //                                  .Where(ca => ((DbUdttColumnAttribute)ca).UDTTName == _udtNameToUseForMapping).Count() > 0);
            List<System.Reflection.MemberInfo> ret = new List<System.Reflection.MemberInfo>();
            var members = typeof(T).GetMembers();
            foreach (var mi in members)
            {
                foreach (Attribute a in Attribute.GetCustomAttributes(mi, typeof(DbUdttColumnAttribute), true))
                {
                    var dbudttA = a as DbUdttColumnAttribute;
                    if (dbudttA != null && dbudttA.UDTTName.Equals(_udtNameToUseForMapping, StringComparison.OrdinalIgnoreCase))
                    {
                        ret.Add(mi);
                    }
                }
            }
            return ret;
        }
    }
}
