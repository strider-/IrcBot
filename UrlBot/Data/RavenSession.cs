using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;
using Raven.Json.Linq;
using Raven.Abstractions.Data;
using System.IO;
using Raven.Client.Listeners;
using IrcBot.Bots.Model;

namespace IrcBot.Bots.Data
{
    public class RavenRepository : ISession, IDisposable
    {
        private readonly IDocumentSession _session;
        private readonly DocumentStore _store;
        private readonly string _db;

        public RavenRepository(string server, string database = null)
        {
            _db = database;
            _store = new DocumentStore { Url = server };
            _store.Initialize();

            if(!string.IsNullOrWhiteSpace(_db))
            {
                _db = _db.TrimEnd('/').ToLower();
                _store.DatabaseCommands.EnsureDatabaseExists(_db);
                _session = _store.OpenSession(_db);
            }
            else
            {
                _session = _store.OpenSession();
            }           
        }

        public TEntity Random<TEntity>() where TEntity : NoSqlEntity, new()
        {
            int max = _session.Query<TEntity>().Count();
            Random rnd = new Random();
            return _session.Query<TEntity>().Skip(rnd.Next(0, max + 1)).Take(1).First();
        }

        public IEnumerable<TEntity> All<TEntity>() where TEntity  : NoSqlEntity, new()
        {
            return _session.Query<TEntity>().ToList();
        }

        public IEnumerable<TEntity> FindAll<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : NoSqlEntity, new()
        {
            return _session.Query<TEntity>().Where(predicate).ToList();
        }

        public void Add<TEntity>(TEntity entity) where TEntity : NoSqlEntity, new()
        {
            _session.Store(entity);
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : NoSqlEntity, new()
        {
            _session.Delete(entity);
        }

        public void Update<TEntity>(TEntity entity) where TEntity : NoSqlEntity, new()
        {
            _session.Store(entity);
        }

        public void CommitChanges()
        {
            _session.SaveChanges();
        }

        public string GetDocumentUrl<TEntity>(TEntity entity) where TEntity : NoSqlEntity, new()
        {
            // default database, the built in method suits us.
            if(string.IsNullOrWhiteSpace(_db))
            {
                return _session.Advanced.GetDocumentUrl(entity);
            }

            // welp, using a specific database, we'll have to build our own url.
            string id = _session.Advanced.GetDocumentId(entity);
            return string.Format("{0}/databases/{1}/docs/{2}", _store.Url, _db, id);
        }

        public RavenJObject GetMetadataFor<TEntity>(TEntity entity) where TEntity : NoSqlEntity, new()
        {
            return _session.Advanced.GetMetadataFor(entity);
        }

        public Attachment GetAttachment(string key)
        {
            return _store.DatabaseCommands.GetAttachment(key);
        }

        public void AddAttachment(string key, Stream data, RavenJObject metadata)
        {
            _store.DatabaseCommands.PutAttachment(key, null, data, metadata);
        }

        public void UpdateAttachment(string key, Attachment attachment)
        {
            _store.DatabaseCommands.PutAttachment(key, attachment.Etag, attachment.Data(), attachment.Metadata);
        }

        public void DeleteAttachment(string key, Guid etag)
        {
            _store.DatabaseCommands.DeleteAttachment(key, etag);
        }

        public void RegisterListener(IDocumentConversionListener listener)
        {
            _store.RegisterListener(listener);
        }

        public void Dispose()
        {
            _session.Dispose();
            _store.Dispose();            
        }
    }
}
