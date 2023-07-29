using AnaderiaDemo.Models;
using SQLite;

namespace AnaderiaDemo.Helpers
{
    public static class Database
    {
        private static SQLiteAsyncConnection _db;

        static Database()
        {
            if (_db is not null)
                return;

            _db = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

            CreateTables();
        }

        //private static async Task Init()
        //{
        //    if (_db is not null)
        //        return;

        //    _db = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

        //    await CreateTables();
        //}

        private static async Task CreateTables()
        {
            var createTableStatements = new List<string>()
            {
                Constants.CreateNotesTableStatement,
                Constants.CreateNoteLinesTableStatement
            };

            foreach (var statement in createTableStatements)
                await _db.ExecuteAsync(statement);
                
        }

        public static async Task<List<T>> GetItems<T>() where T : new()
        {
            return await _db.Table<T>()
                .ToListAsync();
        }

        public static async Task<T> GetItemById<T>(int id) where T : BaseTable, new()
        {
            return await _db.Table<T>()
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public static async Task<int> InsertItem(object item)
        {
            return await _db.InsertAsync(item);
        }

        public static async Task<int> DeleteItem(object item)
        {
            return await _db.DeleteAsync(item);
        }

        public static async Task<int> UpdateItem(object item)
        {
            return await _db.UpdateAsync(item);
        }

    }
}
