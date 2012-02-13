using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Raven.Json.Linq;
using Raven.Abstractions.Data;
using System.IO;
using Raven.Client.Listeners;
using IrcBot.Bots.Model;

namespace IrcBot.Bots.Data
{
    public interface ISession
    {
        IEnumerable<TEntity> All<TEntity>() where TEntity : NoSqlEntity, new();
        IEnumerable<TEntity> FindAll<TEntity>(Expression<Func<TEntity, bool>> predicate)  where TEntity : NoSqlEntity, new();

        TEntity Random<TEntity>() where TEntity : NoSqlEntity, new();
        void Add<TEntity>(TEntity entity)  where TEntity : NoSqlEntity, new();
        void Delete<TEntity>(TEntity entity) where TEntity  : NoSqlEntity, new();
        void Update<TEntity>(TEntity entity) where TEntity  : NoSqlEntity, new();
        void CommitChanges();

        string GetDocumentUrl<TEntity>(TEntity entity) where TEntity : NoSqlEntity, new();
        RavenJObject GetMetadataFor<TEntity>(TEntity entity) where TEntity : NoSqlEntity, new();
        Attachment GetAttachment(string key);
        void AddAttachment(string key, Stream data, RavenJObject metadata);
        void UpdateAttachment(string key, Attachment attachment);
        void DeleteAttachment(string key, Guid etag);

        void RegisterListener(IDocumentConversionListener listener);
    }
}
