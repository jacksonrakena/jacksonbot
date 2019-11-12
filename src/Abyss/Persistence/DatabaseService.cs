using LiteDB;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace Abyss
{
    public class DatabaseService : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _database.Dispose();
            }
        }

        ~DatabaseService()
        {
            Dispose(false);
        }

        private readonly LiteDatabase _database;

        public DatabaseService(IHostEnvironment hostEnvironment)
        {
            _database = new LiteDatabase(Path.Combine(hostEnvironment.ContentRootPath, "AbyssDatabase.db"));
        }

        public PersistenceGuild GetOrCreateGuild(ulong guildId)
        {
            var guilds = _database.GetCollection<PersistenceGuild>();
            var guild0 = guilds.FindOne(d => d.Id == guildId);
            if (guild0 != null) return guild0;

            var newGuild = new PersistenceGuild
            {
                Id = guildId
            };

            guilds.Insert(newGuild);

            return newGuild;
        }

        public bool UpdateGuild(PersistenceGuild guild)
        {
            var data = _database.GetCollection<PersistenceGuild>();
            data.EnsureIndex(c => c.Id, true);
            return data.Upsert(guild);
        }
    }
}
