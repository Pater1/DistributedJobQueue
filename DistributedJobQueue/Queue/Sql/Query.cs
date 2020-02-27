using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedJobQueue.Queue.Sql
{
    public readonly struct Query<Conn> where Conn: DbConnection
    {
        public Query(Func<Conn> connectionFactory, string queary)
        {
            ConnectionFactory = connectionFactory;
            Queary = queary;
        }

        public Func<Conn> ConnectionFactory { get; }
        public string Queary { get; }

        public void ExecuteNonQuery()
        {
            using (Conn connection = ConnectionFactory())
            {
                connection.Open();
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandTimeout = 0;
                    command.CommandText = Queary;
                    command.ExecuteNonQuery();
                }
            }
        }
        public T ExecuteScalar<T>()
        {
            return (T)ExecuteScalar();
        }
        public object ExecuteScalar()
        {
            using (Conn connection = ConnectionFactory())
            {
                connection.Open();
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandTimeout = 0;
                    command.CommandText = Queary;
                    return command.ExecuteScalar();
                }
            }
        }
        public IEnumerable<IDataRecord> EnumerateRaw()
        {
            using (Conn connection = ConnectionFactory())
            {
                connection.Open();
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandTimeout = 0;
                    command.CommandText = Queary;
                    using (DbDataReader dataReader = command.ExecuteReader())
                    {
                        return dataReader.Cast<IDataRecord>().ToList();
                    }
                }
            }
        }
        public IEnumerable<T> EnumerateAsItems<T>() where T : ISelfFactory<T, IDataRecord>, new()
        {
            T factorT = new T();
            return EnumerateRaw().Select(x => factorT.Build(x));
        }
        public IEnumerable<IDictionary<string, string>> EnumerateAsDictionary()
        {
            return EnumerateRaw().Select(x =>
            {
                Dictionary<string, string> ret = new Dictionary<string, string>();
                for (int i = 0; i < x.FieldCount; i++)
                {
                    ret.Add(x.GetName(i).ToLower(), x.GetValue(i).ToString().ToLower());
                }
                return ret;
            });
        }
    }
}
