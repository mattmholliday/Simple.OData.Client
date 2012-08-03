﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.OData.Schema;

namespace Simple.Data.OData.Schema
{
    public class DatabaseSchema
    {
        private static readonly ConcurrentDictionary<string, DatabaseSchema> Instances = new ConcurrentDictionary<string, DatabaseSchema>();

        private readonly RequestBuilder _requestBuilder;
        private readonly ISchemaProvider _schemaProvider;
        private readonly Lazy<TableCollection> _lazyTables;

        private DatabaseSchema(ISchemaProvider schemaProvider, RequestBuilder requestBuilder)
        {
            _lazyTables = new Lazy<TableCollection>(CreateTableCollection);
            _schemaProvider = schemaProvider;
            _requestBuilder = requestBuilder;
        }

        public RequestBuilder RequestBuilder
        {
            get { return _requestBuilder; }
        }

        public ISchemaProvider SchemaProvider
        {
            get { return _schemaProvider; }
        }

        public bool IsAvailable
        {
            get { return _schemaProvider != null; }
        }

        public IEnumerable<Table> Tables
        {
            get { return _lazyTables.Value.AsEnumerable(); }
        }

        public Table FindTable(string tableName)
        {
            return _lazyTables.Value.Find(tableName);
        }

        private TableCollection CreateTableCollection()
        {
            return new TableCollection(_schemaProvider.GetTables()
                .Select(table => new ODataTable(table.ActualName, _requestBuilder, this)));
        }

        public static DatabaseSchema Get(RequestBuilder providerHelper)
        {
            return Instances.GetOrAdd(providerHelper.UrlBase,
                                      sp => new DatabaseSchema(new SchemaProvider(providerHelper), providerHelper));
        }

        public static void ClearCache()
        {
            Instances.Clear();
        }
    }
}