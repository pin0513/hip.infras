using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using log4net;
using hip.Infrastructure.Interfaces;

namespace hip.Package.DefaultDatabaseAccess
{
    public class DatabaseAccess : IDbProvider, IDisposable
    {
        public int CommandTimeout = 30;
        private readonly ILog _logger = LogManager.GetLogger(typeof(DatabaseAccess));

        // Fields
        private string _connectionString;
        private string _fullSqlStatement;
        private Dictionary<string, object> _parameters;
        private SqlCommand _sqlCommand;
        private SqlConnection _sqlConnection;
        private SqlTransaction _sqlTransaction;
        // Constructors
        public DatabaseAccess()
        {
        }

        // Constructors
        public DatabaseAccess(string sqlConnectionString)
        {
            _connectionString = sqlConnectionString;
        }

        public string FullSqlStatement { get { return _fullSqlStatement; } }
        public void CloseConnection(SqlConnection conn)
        {
            //判斷連接的狀態。如果是關閉狀態，則打開
            if (conn != null)
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
                conn.Dispose();
            }
        }

        public void Dispose()
        {
            if (_sqlConnection != null)
                _sqlConnection.Dispose();
        }

        public int ExecuteNonQuery(string sql, Dictionary<string, object> parameters = null)
        {
            try
            {
                // 建立Command及Parameter
                Prepare(sql, parameters, _sqlConnection);

                return _sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logger.Error(sql, ex);
            }

            return -1;
        }

        // Methods
        public SqlConnection OpenConnection()
        {
            if (!string.IsNullOrEmpty(_connectionString))
            {
                _sqlConnection = new SqlConnection(_connectionString);
            }
            return _sqlConnection;
        }

        public SqlConnection OpenConnection(string connectionString)
        {
            _connectionString = connectionString;
            return OpenConnection();
        }

        public void OpenConnection(SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
        }

        private DataSet QueryForDataSet(string sql, Dictionary<string, object> parameters, CommandType commandType)
        {
            DataSet ds = new DataSet();
            _sqlCommand = new SqlCommand(sql)
            {
                CommandType = commandType,
                Connection = OpenConnection()
            };

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    _sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
            }


            try
            {
                SetFullSqlStatement(_sqlCommand);

                SqlDataAdapter da = new SqlDataAdapter(_sqlCommand);
                //加入主索引鍵
                da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                da.Fill(ds);
            }
            finally
            {
                _sqlCommand.Parameters.Clear();
                CloseConnection(_sqlCommand.Connection);
            }
            return ds;
        }

        public DataSet QueryForDataSet(string sql, Dictionary<string, object> parameters = null)
        {
            return QueryForDataSet(sql, parameters, CommandType.Text);
        }

        public DataSet QueryForDataSetSp(string sql, Dictionary<string, object> parameters = null)
        {
            return QueryForDataSet(sql, parameters, CommandType.StoredProcedure);
        }


        private DataTable QueryForDataTable(string sql, Dictionary<string, object> parameters, CommandType commandType)
        {
            var dt = new DataTable();

            _sqlCommand = new SqlCommand(sql)
            {
                CommandType = commandType,
                Connection = OpenConnection()
            };

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    _sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
            }

            try
            {
                SetFullSqlStatement(_sqlCommand);

                SqlDataAdapter da = new SqlDataAdapter(_sqlCommand);
                //加入主索引鍵
                da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                da.Fill(dt);
            }
            finally
            {
                _sqlCommand.Parameters.Clear();
                CloseConnection(_sqlConnection);
            }
            return dt;
        }

        public DataTable QueryForDataTable(string sql, Dictionary<string, object> parameters = null)
        {
            return QueryForDataTable(sql, parameters, CommandType.Text);
        }

        public DataTable QueryForDataTableSp(string sql, Dictionary<string, object> parameters = null)
        {
            return QueryForDataTable(sql, parameters, CommandType.StoredProcedure);
        }

        // 搭配ValueInjector直接將DataTable轉成List<T>物件回傳
        public IList<T> QueryForList<T>(string sql, Dictionary<string, object> parameters = null) where T : new()
        {
            IList<T> list = new List<T>();

            try
            {
                // 建立Command及Parameter
                Prepare(sql, parameters);

                var dataTable = new DataTable();
                dataTable.Load(_sqlCommand.ExecuteReader());
                
                list = dataTable.ToList<T>();
            }
            catch (Exception ex)
            {
                _logger.Error(sql, ex);
            }

            return list;
        }

        // 搭配ValueInjector直接將DataTable轉成T物件回傳
        public T QueryForObject<T>(string sql, Dictionary<string, object> parameters = null) where T : new()
        {
            var list = QueryForList<T>(sql, parameters);
            var t = list.Count > 0 ? list.First() : default(T);
            return t;
        }

        // 建立Command及Parameter
        private void Prepare(String sql, Dictionary<string, object> parameters = null, SqlConnection conn = null)
        {
            if (string.IsNullOrEmpty(sql)) throw new ArgumentNullException();

            if (_sqlConnection == null && conn == null)
            {
                if (!string.IsNullOrEmpty(_connectionString))
                {
                    _sqlConnection = new SqlConnection(_connectionString);
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }

            if (_sqlConnection != null && _sqlConnection.ConnectionString == string.Empty)
            {
                _sqlConnection = OpenConnection();
            }

            _parameters = parameters != null ? new Dictionary<string, object>(parameters) : new Dictionary<string, object>();
            _sqlCommand = new SqlCommand(sql, conn ?? _sqlConnection);
            if (CommandTimeout < 30)
                CommandTimeout = 30;
            _sqlCommand.CommandTimeout = CommandTimeout;
            if (_sqlConnection != null && _sqlConnection.State != ConnectionState.Open)
            {
                _sqlConnection.Open();
            }

            foreach (var parameter in _parameters)
            {
                _sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }
        }

        // 產生交易
        public void SetTransaction()
        {
            if (_sqlConnection == null || _sqlConnection.State == ConnectionState.Closed)
            {
                OpenConnection();
            }

            if (_sqlConnection != null) _sqlTransaction = _sqlConnection.BeginTransaction();
        }

        // 完成交易
        public void TransactionCommit()
        {
            if (_sqlTransaction != null && _sqlTransaction.Connection != null)
                _sqlTransaction.Commit();
        }

        // 取消交易
        public void TransactionRollback()
        {
            if (_sqlTransaction != null && _sqlTransaction.Connection != null)
                _sqlTransaction.Rollback();
        }

        // SQL Query字串，執行SqlcommandExecute後會將帶入參數後的完整SQL儲存，DEBUG時會用到
        private void SetFullSqlStatement(SqlCommand sqlCommand)
        {
            string strSql = string.Empty;
            try
            {
                strSql = sqlCommand.CommandText;
                if (sqlCommand.Parameters.Count > 0)
                {
                    foreach (SqlParameter aMySqlParameter in sqlCommand.Parameters)
                    {
                        if (strSql.IndexOf(aMySqlParameter.ParameterName + ",", StringComparison.Ordinal) != -1)
                        {
                            strSql = strSql.Replace(aMySqlParameter.ParameterName + ",", "'" + aMySqlParameter.Value + "',");
                        }

                        if (strSql.IndexOf(aMySqlParameter.ParameterName + ",", StringComparison.Ordinal) == -1)
                        {
                            strSql = strSql.Replace(aMySqlParameter.ParameterName, "'" + aMySqlParameter.Value + "'");
                        }
                    }
                }
            }
            catch
            {
                strSql = string.Empty;
            }
            finally
            {
                _fullSqlStatement = strSql;
            }
        } // end of setFullSqlStatement       

    }
}
