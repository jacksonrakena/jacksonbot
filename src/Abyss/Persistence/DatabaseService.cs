using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abyss
{
    public class DatabaseService
    {
        private readonly DataService _data;
        private readonly LiteDatabase _database;

        public DatabaseService(DataService data)
        {
            _data = data;
            _database = new LiteDatabase(_data.GetDatabasePath());
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
