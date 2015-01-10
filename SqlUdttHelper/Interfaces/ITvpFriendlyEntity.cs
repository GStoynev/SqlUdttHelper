using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlUdttHelper
{
    public interface ITvpFriendlyEntity
    {
        IEnumerable<Microsoft.SqlServer.Server.SqlDataRecord> AsSqlDataRecord<T>(string mapperName) where T : class;
        
        IEnumerable<Microsoft.SqlServer.Server.SqlDataRecord> AsSqlDataRecord<T, P>(string mapperName) where T : class where P : class;
        
        IEnumerable<Microsoft.SqlServer.Server.SqlDataRecord> AsSqlDataRecord<T, P, Q>(string mapperName)
            where T : class
            where P : class
            where Q : class;

        object GetUdttFieldValueAt<T>(int ordinalPositionSought, string mapperName);
    }
}
