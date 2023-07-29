using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnaderiaDemo.Helpers
{
    internal static class Constants
    {

        public const string DatabaseFilename = "notes.db3";
        public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

        public const string NotesTable = "notes";
        public const string NoteLinesTable = "note_lines";

        public const string CreateNotesTableStatement = $"CREATE TABLE IF NOT EXISTS {NotesTable} (id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR(255), is_cr BOOLEAN, is_co BOOLEAN, total_amount DOUBLE, created_at DATETIME, updated_at DATETIME);";
        public const string CreateNoteLinesTableStatement = $"CREATE TABLE IF NOT EXISTS {NoteLinesTable} (id INTEGER PRIMARY KEY AUTOINCREMENT,  note_id INT, piece VARCHAR(255), quantity INT, description VARCHAR(255), price DOUBLE, amount DOUBLE, FOREIGN KEY(note_id) REFERENCES notes(id));";
        
        public const SQLite.SQLiteOpenFlags Flags =
        // open the database in read/write mode
        SQLite.SQLiteOpenFlags.ReadWrite |
        // create the database if it doesn't exist
        SQLite.SQLiteOpenFlags.Create |
        // enable multi-threaded database access
        SQLite.SQLiteOpenFlags.SharedCache;
    }
}
