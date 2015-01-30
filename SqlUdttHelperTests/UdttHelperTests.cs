using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SqlUdttHelper;
using System.Data;
using System.Data.SqlTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlUdttHelperTests
{
    [TestClass]
    public class UdttHelperTests
    {
        private readonly string UDTT_tbl_Foo = "UDTT_tbl_Foo";

        private class BarBase
        {
            [DbUdttColumn(UDTTName: "UDTT_tbl_Foo", Name: "varchar_col_foo", OrdinalPosition: 10, SqlType: SqlDbType.VarChar)]
            [DbUdttColumn(UDTTName: "UDTT_tbl_Bar", Name: "varchar_col_bar", OrdinalPosition: 1, SqlType: SqlDbType.VarChar)]
            public string BaseStringProperty { get; set; }
        }

        private class EntityFoo : BarBase
        {
            [DbUdttColumn(UDTTName: "UDTT_tbl_Bar", Name: "int_col", OrdinalPosition: 0, SqlType: SqlDbType.Int)]
            public int IntProperty { get; set; }
        }

        private class EntityBar : BarBase
        {
            [DbUdttColumn(UDTTName: "UDTT_tbl_Foo", Name: "varchar_col", OrdinalPosition: 0, SqlType: SqlDbType.VarChar)]
            public string StringProperty { get; set; }

            [DbUdttColumn(UDTTName: "UDTT_tbl_Foo", Name: "varchar_col", OrdinalPosition: 6, SqlType: SqlDbType.VarChar)]
            public string CharProperty { get; set; }

            [DbUdttColumn(UDTTName: "UDTT_tbl_Foo", Name: "int_col", OrdinalPosition: 1, SqlType: SqlDbType.Int)]
            public int IntProperty { get; set; }

            [DbUdttColumn(UDTTName: "UDTT_tbl_Foo", Name: "int_col", OrdinalPosition: 3, SqlType: SqlDbType.Int)]
            public int? NullableIntProperty { get; set; }

            [DbUdttColumn(UDTTName: "UDTT_tbl_Foo", Name: "bigint_col", OrdinalPosition: 5, SqlType: SqlDbType.BigInt)]
            public long LongProperty { get; set; }

            [DbUdttColumn(UDTTName: "UDTT_tbl_Foo", Name: "nullable_bigint_col", OrdinalPosition: 4, SqlType: SqlDbType.BigInt)]
            public long? NullableLongProperty { get; set; }

            [DbUdttColumn(UDTTName: "UDTT_tbl_Foo", Name: "datetime_col", OrdinalPosition: 2, SqlType: SqlDbType.DateTime)]
            public DateTime DateTimeProperty { get; set; }

            [DbUdttColumn(UDTTName: "UDTT_tbl_Foo", Name: "nullable_datetime_col", OrdinalPosition: 7, SqlType: SqlDbType.DateTime)]
            public DateTime? NullableDateTimeProperty { get; set; }

            [DbUdttColumn(UDTTName: "UDTT_tbl_Foo", Name: "uniqueidentifier_col", OrdinalPosition: 8, SqlType: SqlDbType.UniqueIdentifier)]
            public Guid GuidProperty { get; set; }

            [DbUdttColumn(UDTTName: "UDTT_tbl_Foo", Name: "bit_col", OrdinalPosition: 9, SqlType: SqlDbType.Bit)]
            public bool BoolProperty { get; set; }
            
            public string UnmappedProperty { get; set; }
        }

        [TestMethod]
        public void GetUdttValueFromAttributeAt_Works_With_First_Level_Fields()
        {
            int MY_TEST_VAL = -333;
            EntityBar p = new EntityBar();
            // I just know, that at time of writing this test property tagged with ordinal position of 5 for UDTT_tbl_Foo is the ID
            p.LongProperty = MY_TEST_VAL;

            var val = p.GetUdttFieldValueAt(5, UDTT_tbl_Foo);
            Assert.IsNotNull(val);
            Assert.AreEqual(int.Parse(val.ToString()), MY_TEST_VAL);
        }

        [TestMethod]
        public void Product_to_Udtt_AsSqlDataRecord()
        {
            EntityBar p = new EntityBar()
                {
                    StringProperty = "AdditionalInfo",
                    IntProperty = -123,
                    DateTimeProperty = DateTime.Now,
                    NullableIntProperty = null, // TODO: test with value, too
                    NullableLongProperty = null, // TODO: test with value, too
                    LongProperty = -333,
                    NullableDateTimeProperty = null, // TODO: test with value, too
                    GuidProperty = new Guid(),
                    BoolProperty = true
                };
            



            var udttProduct = p.AsSqlDataRecord(UDTT_tbl_Foo);
            
            Assert.IsNotNull(udttProduct);

            Assert.AreEqual(udttProduct.Count(), 1);

            AssertExtensions.DoesNotThrow(() =>
                {
                    foreach (var row in udttProduct)
                    {
                        for(int i = 0; i < row.FieldCount; i++)
                        {
                            var valueFromP = p.GetUdttFieldValueAt(i, UDTT_tbl_Foo);
                            
                            var sqlType = row.GetSqlFieldType(i);

                            if (
                                row.IsDBNull(i)
                                &&
                                valueFromP != null
                                &&
                                !string.IsNullOrEmpty(valueFromP.ToString())
                                )
                            {
                                throw new Exception(string.Format("Huston, we have a problem at 2349H. Values for i={2} were {0} and {1}", row.GetValue(i).ToString(), valueFromP.ToString(), i));
                            }
                            else if (
                                !row.IsDBNull(i)
                                &&
                                ((valueFromP == null) || (string.IsNullOrEmpty(valueFromP.ToString())))
                                )
                            {
                                throw new Exception(string.Format("Huston, we have a problem at RWE34. Values for i={2} were {0} and {1}", row.GetValue(i).ToString(), valueFromP.ToString(), i));
                            }
                            else if (
                                row.IsDBNull(i)
                                &&
                                ((valueFromP == null) || (string.IsNullOrEmpty(valueFromP.ToString())))
                                )
                            {
                                // do nothing - both are Null, which is "equality"
                            }
                            else
                            {
                                if (sqlType == typeof(long)
                                    || sqlType == typeof(Int64)
                                    || sqlType == typeof(SqlInt64))
                                {
                                    if (!long.Parse(valueFromP.ToString()).Equals(row.GetInt64(i)))
                                    {
                                        throw new Exception(string.Format("Huston, we have a problem. Values for i={2} were {0} and {1}", row.GetValue(i).ToString(), valueFromP.ToString(), i));
                                    }
                                    else
                                    {
                                        // I am all good - do nothing 
                                    }
                                }
                                else if (sqlType == typeof(int)
                                || sqlType == typeof(Int32)
                                || sqlType == typeof(SqlInt32))
                                {
                                    if (!int.Parse(valueFromP.ToString()).Equals(row.GetInt32(i)))
                                    {
                                        throw new Exception(string.Format("Huston, we have a problem at the Int32 thing. Values for i={2} were {0} and {1}", row.GetValue(i).ToString(), valueFromP.ToString(), i));
                                    }
                                    else
                                    {
                                        // I am all good - do nothing 
                                    }
                                }
                                else if (sqlType == typeof(string)
                                    || sqlType == typeof(String)
                                    || sqlType == typeof(SqlString))
                                {
                                    if (!String.Equals(valueFromP.ToString(), row.GetSqlString(i).Value, StringComparison.Ordinal))
                                    {
                                        throw new Exception(string.Format("Huston, we have a problem at the String thing. Values for i={2} were {0} and {1}", row.GetValue(i).ToString(), valueFromP.ToString(), i));
                                    }
                                    else
                                    {
                                        // I am all good - do nothing 
                                    }
                                }
                                else if (sqlType == typeof(Guid)
                                    || sqlType == typeof(SqlGuid))
                                {
                                    if (!Guid.Parse(valueFromP.ToString()).Equals(row.GetGuid(i)))
                                    {
                                        throw new Exception(string.Format("Huston, we have a problem at the Guid thing. Values for i={2} were {0} and {1}", row.GetValue(i).ToString(), valueFromP.ToString(), i));
                                    }
                                    else
                                    {
                                        // I am all good - do nothing 
                                    }
                                }
                                else if (sqlType == typeof(int)
                                || sqlType == typeof(long)
                                || sqlType == typeof(Int16)
                                || sqlType == typeof(Int32)
                                || sqlType == typeof(Int64)
                                || sqlType == typeof(byte)
                                || sqlType == typeof(Byte)
                                || sqlType == typeof(SByte)
                                || sqlType == typeof(SqlInt64)
                                || sqlType == typeof(SqlInt32)
                                || sqlType == typeof(SqlInt16))
                                {
                                    if (!Guid.Parse(valueFromP.ToString()).Equals(row.GetGuid(i)))
                                    {
                                        throw new Exception(string.Format("Huston, we have a problem at the [what's next] thing. Values for i={2} were {0} and {1}", row.GetValue(i).ToString(), valueFromP.ToString(), i));
                                    }
                                    else
                                    {
                                        // I am all good - do nothing 
                                    }
                                }
                                else
                                {
                                    throw new Exception(string.Format("not expected for sqlType of {0}", sqlType));
                                }
                            }
                        }
                    }
                });

            p = new EntityBar()
            {
                StringProperty = "AdditionalInfo",
                IntProperty = -123,
                DateTimeProperty = DateTime.Now,
                NullableIntProperty = 123,
                NullableLongProperty = 999999,
                LongProperty = -333,
                NullableDateTimeProperty = DateTime.Now,
                GuidProperty = new Guid(),
                BoolProperty = true
            };

            System.Diagnostics.Trace.WriteLine("STARTIIIIIIIIIIIIIIIIIING");

            udttProduct = p.AsSqlDataRecord(UDTT_tbl_Foo);

            Assert.IsNotNull(udttProduct);

            Assert.AreEqual(udttProduct.Count(), 1);

            AssertExtensions.DoesNotThrow(() =>
            {
                foreach (var row in udttProduct)
                {
                    for (int i = 0; i < row.FieldCount; i++)
                    {
                        var valueFromP = p.GetUdttFieldValueAt(i, UDTT_tbl_Foo);

                        var sqlType = row.GetSqlFieldType(i);

                        if (
                            row.IsDBNull(i)
                            &&
                            valueFromP != null
                            &&
                            !string.IsNullOrEmpty(valueFromP.ToString())
                            )
                        {
                            throw new Exception(string.Format("Huston, we have a problem at 2349H. Values for i={2} were {0} and {1}", row.GetValue(i).ToString(), valueFromP.ToString(), i));
                        }
                        else if (
                            !row.IsDBNull(i)
                            &&
                            ((valueFromP == null) || (string.IsNullOrEmpty(valueFromP.ToString())))
                            )
                        {
                            throw new Exception(string.Format("Huston, we have a problem at RWE34. Values for i={2} were {0} and {1}", row.GetValue(i).ToString(), valueFromP.ToString(), i));
                        }
                        else if (
                            row.IsDBNull(i)
                            &&
                            ((valueFromP == null) || (string.IsNullOrEmpty(valueFromP.ToString())))
                            )
                        {
                            // do nothing - both are Null, which is "equality"
                        }
                        else
                        {
                            if (sqlType == typeof(long)
                                || sqlType == typeof(Int64)
                                || sqlType == typeof(SqlInt64))
                            {
                                if (!long.Parse(valueFromP.ToString()).Equals(row.GetInt64(i)))
                                {
                                    throw new Exception(string.Format("Huston, we have a problem. Values for i={2} were {0} and {1}", row.GetValue(i).ToString(), valueFromP.ToString(), i));
                                }
                                else
                                {
                                    // I am all good - do nothing 
                                }
                            }
                            else if (sqlType == typeof(int)
                            || sqlType == typeof(Int32)
                            || sqlType == typeof(SqlInt32))
                            {
                                if (!int.Parse(valueFromP.ToString()).Equals(row.GetInt32(i)))
                                {
                                    throw new Exception(string.Format("Huston, we have a problem at the Int32 thing. Values for i={2} were {0} and {1}", row.GetValue(i).ToString(), valueFromP.ToString(), i));
                                }
                                else
                                {
                                    // I am all good - do nothing 
                                }
                            }
                            else if (sqlType == typeof(string)
                                || sqlType == typeof(String)
                                || sqlType == typeof(SqlString))
                            {
                                if (!String.Equals(valueFromP.ToString(), row.GetSqlString(i).Value, StringComparison.Ordinal))
                                {
                                    throw new Exception(string.Format("Huston, we have a problem at the String thing. Values for i={2} were {0} and {1}", row.GetValue(i).ToString(), valueFromP.ToString(), i));
                                }
                                else
                                {
                                    // I am all good - do nothing 
                                }
                            }
                            else if (sqlType == typeof(Guid)
                                || sqlType == typeof(SqlGuid))
                            {
                                if (!Guid.Parse(valueFromP.ToString()).Equals(row.GetGuid(i)))
                                {
                                    throw new Exception(string.Format("Huston, we have a problem at the Guid thing. Values for i={2} were {0} and {1}", row.GetValue(i).ToString(), valueFromP.ToString(), i));
                                }
                                else
                                {
                                    // I am all good - do nothing 
                                }
                            }
                            else if (sqlType == typeof(DateTime)
                            || sqlType == typeof(SqlDateTime))
                            {
                                if (!DateTime.Parse(valueFromP.ToString()).Equals(row.GetDateTime(i)))
                                {
                                    throw new Exception(string.Format("Huston, we have a problem at the DateTime thing. Values for i={2} were {0} and {1}", row.GetValue(i).ToString(), valueFromP.ToString(), i));
                                }
                                else
                                {
                                    // I am all good - do nothing 
                                }
                            }
                            else if (sqlType == typeof(int)
                            || sqlType == typeof(long)
                            || sqlType == typeof(Int16)
                            || sqlType == typeof(Int32)
                            || sqlType == typeof(Int64)
                            || sqlType == typeof(byte)
                            || sqlType == typeof(Byte)
                            || sqlType == typeof(SByte)
                            || sqlType == typeof(SqlInt64)
                            || sqlType == typeof(SqlInt32)
                            || sqlType == typeof(SqlInt16))
                            {
                                if (!Guid.Parse(valueFromP.ToString()).Equals(row.GetGuid(i)))
                                {
                                    throw new Exception(string.Format("Huston, we have a problem at the [what's next] thing. Values for i={2} were {0} and {1}", row.GetValue(i).ToString(), valueFromP.ToString(), i));
                                }
                                else
                                {
                                    // I am all good - do nothing 
                                }
                            }
                            else
                            {
                                throw new Exception(string.Format("not expected for sqlType of {0}", sqlType));
                            }
                        }
                    }
                }
            });
        }

        [TestMethod]
        public void GetSqlDataRecordDefGeneric_For_Single_Instance_Of_Type_T_Works()
        {
            // setup
            EntityBar p = new EntityBar();

            // test
            var magic = new SqlUdttEnumeratorProvider<EntityBar>(p, UDTT_tbl_Foo);
            var sqlDataRecord = magic.GetSqlDataRecordDef();

            Assert.IsNotNull(sqlDataRecord);

            Assert.IsTrue(sqlDataRecord.FieldCount > 0);
        }

        [TestMethod]
        public void GetSqlDataRecordDefGeneric_For_Collection_Of_Type_T_Works()
        {
            // setup
            List<EntityBar> pl = new List<EntityBar>();
            pl.Add(new EntityBar());
            pl.Add(new EntityBar());

            // test
            var magic = new SqlUdttEnumeratorProvider<EntityBar>(pl, UDTT_tbl_Foo);
            var sqlDataRecord = magic.GetSqlDataRecordDef();

            Assert.IsNotNull(sqlDataRecord);

            Assert.IsTrue(sqlDataRecord.FieldCount > 0);
        }

        [TestMethod]
        public void GetSqlDataRecordDefGeneric_For_Type_T_Returns_Null_If_Type_Has_NO_Properties_Decorated_With_DbUdtColumnAttribute()
        {
            // setup
            object p = new object();

            // test
            var magic = new SqlUdttEnumeratorProvider<object>(p, UDTT_tbl_Foo);
            var sqlDataRecord = magic.GetSqlDataRecordDef();

            Assert.IsNull(sqlDataRecord);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DecimalWithOnlyScale_Throws()
        {
            var arg = new DbUdttColumnAttribute("name", "name2", 0, SqlDbType.Decimal, null, 3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DecimalWithOnlyPrecision_Throws()
        {
            var arg = new DbUdttColumnAttribute("name", "name2", 0, SqlDbType.Decimal, 3, null);
        }

        [TestMethod]
        public void DecimalWith_No_PrecisionAndScale_Works()
        {
            var arg = new DbUdttColumnAttribute("name", "name2", 0, SqlDbType.Decimal);
        }

        [TestMethod]
        public void DecimalWithPrecisionAndScale_Works()
        {
            var arg = new DbUdttColumnAttribute("name", "name2", 0, SqlDbType.Decimal, 3, 1);
        }
    }
}
