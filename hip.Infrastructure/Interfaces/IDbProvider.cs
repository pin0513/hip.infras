using System.Collections.Generic;
using System.Data;

namespace hip.Infrastructure.Interfaces
{
    /// <summary>
    /// 資料庫
    /// </summary>
    public interface IDbProvider
    {
        DataTable QueryForDataTable(string sql, Dictionary<string, object> parameters = null);
        DataSet QueryForDataSet(string sql, Dictionary<string, object> parameters = null);
        
        DataTable QueryForDataTableSp(string sql, Dictionary<string, object> parameters = null);
        DataSet QueryForDataSetSp(string sql, Dictionary<string, object> parameters = null);

        int ExecuteNonQuery(string sql, Dictionary<string, object> parameters = null);

        T QueryForObject<T>(string sql, Dictionary<string, object> parameters = null) where T : new();
        
        IList<T> QueryForList<T>(string sql, Dictionary<string, object> parameters = null) where T : new();
        
        void SetTransaction();
        void TransactionCommit();
        void TransactionRollback();
    }

}
