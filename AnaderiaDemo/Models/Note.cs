using AnaderiaDemo.Helpers;
using SQLite;

namespace AnaderiaDemo.Models
{
    [Table(Constants.NotesTable)]
    public class Note : BaseTable
    {
        [Column("name")]
        public string Name { get; set; }

        [Column("is_cr")]
        public bool IsCr { get; set; }

        [Column("is_co")]
        public bool IsCo { get; set; }

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdateAt { get; set; }

    }
}
