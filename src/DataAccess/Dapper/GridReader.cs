using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;

namespace DataAccess
{
    public partial class GridReader : IDisposable
    {
        private IDataReader reader;
        private IDbCommand command;
        private Identity identity;
        private bool addToCache;
        private IDbConnection connection;

        internal GridReader(IDbConnection connection, IDbCommand command, IDataReader reader, Identity identity, IParameterCallbacks callbacks, bool addToCache)
        {
            this.connection = connection;
            this.command = command;
            this.reader = reader;
            this.identity = identity;
            this.callbacks = callbacks;
            this.addToCache = addToCache;
        }


        /// <summary>
        /// Read the next grid of results, returned as a dynamic object
        /// </summary>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public IEnumerable<dynamic> Read(bool buffered = true)
        {
            return ReadImpl<dynamic>(typeof(SqlMapper.DapperRow), buffered);
        }

        /// <summary>
        /// Read the next grid of results
        /// </summary>
        public IEnumerable<T> Read<T>(bool buffered = true)
        {
            return ReadImpl<T>(typeof(T), buffered);
        }

        /// <summary>
        /// Read the next grid of results
        /// </summary>
        public IEnumerable<object> Read(Type type, bool buffered = true)
        {
            if (type == null) throw new ArgumentNullException("type");
            return ReadImpl<object>(type, buffered);
        }

        private IEnumerable<T> ReadImpl<T>(Type type, bool buffered)
        {
            if (reader == null) throw new ObjectDisposedException(GetType().FullName, "The reader has been disposed; this can happen after all data has been consumed");
            if (consumed) throw new InvalidOperationException("Query results must be consumed in the correct order, and each result can only be consumed once");
            var typedIdentity = identity.ForGrid(type, gridIndex);
            SqlMapper.CacheInfo cache = SqlMapper.GetCacheInfo(typedIdentity, null, addToCache);
            var deserializer = cache.Deserializer;

            int hash = SqlMapper.GetColumnHash(reader);
            if (deserializer.Func == null || deserializer.Hash != hash)
            {
                deserializer = new SqlMapper.DeserializerState(hash, SqlMapper.GetDeserializer(type, reader, 0, -1, false));
                cache.Deserializer = deserializer;
            }
            consumed = true;
            var result = ReadDeferred<T>(gridIndex, deserializer.Func, typedIdentity);
            return buffered ? result.ToList() : result;
        }


        private IEnumerable<TReturn> MultiReadInternal<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(Delegate func, string splitOn)
        {
            var identity = this.identity.ForGrid(typeof(TReturn), new Type[] { 
                    typeof(TFirst), 
                    typeof(TSecond),
                    typeof(TThird),
                    typeof(TFourth),
                    typeof(TFifth),
                    typeof(TSixth),
                    typeof(TSeventh)
                }, gridIndex);
            try
            {
                foreach (var r in SqlMapper.MultiMapImpl<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(null, default(CommandDefinition), func, splitOn, reader, identity, false))
                {
                    yield return r;
                }
            }
            finally
            {
                NextResult();
            }
        }

        private IEnumerable<TReturn> MultiReadInternal<TReturn>(Type[] types, Func<object[], TReturn> map, string splitOn)
        {
            var identity = this.identity.ForGrid(typeof(TReturn), types, gridIndex);
            try
            {
                foreach (var r in SqlMapper.MultiMapImpl<TReturn>(null, default(CommandDefinition), types, map, splitOn, reader, identity, false))
                {
                    yield return r;
                }
            }
            finally
            {
                NextResult();
            }
        }
        /// <summary>
        /// Read multiple objects from a single record set on the grid
        /// </summary>
        public IEnumerable<TReturn> Read<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> func, string splitOn = "id", bool buffered = true)
        {
            var result = MultiReadInternal<TFirst, TSecond, SqlMapper.DontMap, SqlMapper.DontMap, SqlMapper.DontMap, SqlMapper.DontMap, SqlMapper.DontMap, TReturn>(func, splitOn);
            return buffered ? result.ToList() : result;
        }

        /// <summary>
        /// Read multiple objects from a single record set on the grid
        /// </summary>
        public IEnumerable<TReturn> Read<TFirst, TSecond, TThird, TReturn>(Func<TFirst, TSecond, TThird, TReturn> func, string splitOn = "id", bool buffered = true)
        {
            var result = MultiReadInternal<TFirst, TSecond, TThird, SqlMapper.DontMap, SqlMapper.DontMap, SqlMapper.DontMap, SqlMapper.DontMap, TReturn>(func, splitOn);
            return buffered ? result.ToList() : result;
        }

        /// <summary>
        /// Read multiple objects from a single record set on the grid
        /// </summary>
        public IEnumerable<TReturn> Read<TFirst, TSecond, TThird, TFourth, TReturn>(Func<TFirst, TSecond, TThird, TFourth, TReturn> func, string splitOn = "id", bool buffered = true)
        {
            var result = MultiReadInternal<TFirst, TSecond, TThird, TFourth, SqlMapper.DontMap, SqlMapper.DontMap, SqlMapper.DontMap, TReturn>(func, splitOn);
            return buffered ? result.ToList() : result;
        }



        /// <summary>
        /// Read multiple objects from a single record set on the grid
        /// </summary>
        public IEnumerable<TReturn> Read<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> func, string splitOn = "id", bool buffered = true)
        {
            var result = MultiReadInternal<TFirst, TSecond, TThird, TFourth, TFifth, SqlMapper.DontMap, SqlMapper.DontMap, TReturn>(func, splitOn);
            return buffered ? result.ToList() : result;
        }
        /// <summary>
        /// Read multiple objects from a single record set on the grid
        /// </summary>
        public IEnumerable<TReturn> Read<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> func, string splitOn = "id", bool buffered = true)
        {
            var result = MultiReadInternal<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, SqlMapper.DontMap, TReturn>(func, splitOn);
            return buffered ? result.ToList() : result;
        }
        /// <summary>
        /// Read multiple objects from a single record set on the grid
        /// </summary>
        public IEnumerable<TReturn> Read<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> func, string splitOn = "id", bool buffered = true)
        {
            var result = MultiReadInternal<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(func, splitOn);
            return buffered ? result.ToList() : result;
        }

        /// <summary>
        /// Read multiple objects from a single record set on the grid
        /// </summary>
        public IEnumerable<TReturn> Read<TReturn>(Type[] types, Func<object[], TReturn> map, string splitOn = "id", bool buffered = true)
        {
            var result = MultiReadInternal<TReturn>(types, map, splitOn);
            return buffered ? result.ToList() : result;
        }


        private IEnumerable<T> ReadDeferred<T>(int index, Func<IDataReader, object> deserializer, Identity typedIdentity)
        {
            try
            {
                while (index == gridIndex && reader.Read())
                {
                    yield return (T)deserializer(reader);
                }
            }
                finally // finally so that First etc progresses things even when multiple rows
            {
                if (index == gridIndex)
                {
                    NextResult();
                }
            }
        }
        private int gridIndex, readCount;
        private bool consumed;
        private IParameterCallbacks callbacks;

        /// <summary>
        /// Has the underlying reader been consumed?
        /// </summary>
        public bool IsConsumed
        {
            get
            {
                return consumed;
            }
        }
        private void NextResult()
        {
            if (reader.NextResult())
            {
                readCount++;
                gridIndex++;
                consumed = false;
            }
            else
            {
                // happy path; close the reader cleanly - no
                // need for "Cancel" etc
                reader.Dispose();
                reader = null;
                if (callbacks != null) callbacks.OnCompleted();
                Dispose();
            }
        }
        /// <summary>
        /// Dispose the grid, closing and disposing both the underlying reader and command.
        /// </summary>
        public void Dispose()
        {
            if (reader != null)
            {
                if (!reader.IsClosed && command != null) command.Cancel();
                reader.Dispose();
                reader = null;
            }
            if (command != null)
            {
                command.Dispose();
                command = null;
            }

            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }
}
