using SQLite;

namespace AnaderiaDemo.Models
{
    public class BaseTable
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }
    }
}
