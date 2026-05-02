using System.ComponentModel.DataAnnotations;

namespace BMDb.Models
{
    public class EntertainmentZanr
    {
        [Key]
        public int IDVeze { get; set; }
        public int EntertainmentId { get; set; }
        public int ZanrId { get; set; }

        public EntertainmentZanr() { }
    }
}
