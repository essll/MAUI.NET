using AnaderiaDemo.Helpers;
using SQLite;

namespace AnaderiaDemo.Models
{
    [Table(Constants.NoteLinesTable)]
    public class NoteLine : BaseTable
    { 
        [Column("note_id")]
        public int NoteId { get; set; }

        [Column("piece")]
        public string Piece { get; set; }

        [Column("quantity")]
        public decimal Quantity { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }



    }   
}
