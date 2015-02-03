﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlUdttHelper
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class DbUdttColumnAttribute : Attribute
    {
        public string UDTTName { get; private set; }
        public string Name { get; private set; }
        public int OrdinalPosition { get; private set; }
        public System.Data.SqlDbType SqlType { get; private set; }
        // TODO: implement
        public int? Length 
        { 
            get
            {
                throw new NotImplementedException("Not implemented at section F383947GF");
            }
            set
            {
                throw new NotImplementedException("Not implemented at section 0F38408F");; 
            }
        }
        public byte Precision { get; set; }
        public byte Scale { get; set; }

        private DbUdttColumnAttribute() { }

        /// <summary>
        /// Defines how the decorated field should map to UDTT specified by the <paramref name="Name"/> parameter
        /// </summary>
        /// <param name="UDTTName"></param>
        /// <param name="Name"></param>
        /// <param name="OrdinalPosition"></param>
        /// <param name="SqlType"></param>
        public DbUdttColumnAttribute(string UDTTName, string Name, int OrdinalPosition, System.Data.SqlDbType SqlType)
        {
            this.UDTTName = UDTTName;
            this.Name = Name;
            this.OrdinalPosition = OrdinalPosition;
            this.SqlType = SqlType;
        }

        /// <summary>
        /// Defines how the decorated field should map to UDTT specified by the <paramref name="Name"/> parameter
        /// </summary>
        /// <param name="UDTTName"></param>
        /// <param name="Name"></param>
        /// <param name="OrdinalPosition"></param>
        /// <param name="SqlType"></param>
        /// <param name="precision">Optional, but always use with <see cref="scale"/>; 1 through 38</param>
        /// <param name="scale">Optional, but always use with <see cref="precision"/>; always smaller than <see cref="precision"/></param>
        public DbUdttColumnAttribute(string UDTTName, string Name, int OrdinalPosition, System.Data.SqlDbType SqlType, byte Precision, byte Scale)
        {
            if (Precision < 1 || Precision > 38)
            {
                throw new ArgumentException("precision must be between 1 and 38");
            }

            if (!(0 <= Scale && Scale <= Precision))
            {
                throw new ArgumentException("0 <= Smust <= Precision");
            }
            this.UDTTName = UDTTName;
            this.Name = Name;
            this.OrdinalPosition = OrdinalPosition;
            this.SqlType = SqlType;
            // TODO: implement 
            /// <param name="length">Optional</param>
            //this.Length = length; 
            this.Precision = Precision;
            this.Scale = Scale;
        }

        // this is so that multiple attributes can be used on same target
        private object _typeId = new object();
        public override object TypeId
        {
            get
            {
                return this._typeId;
            }
        }
    }
}
