using System.ComponentModel.DataAnnotations;

namespace BMDb.Models
{
    public class GledaoSam
    {
        [Key]
        public int Id { get; set; }
        public int OsobaId { get; set; }
        public int EntertainmentId { get; set; }

        public GledaoSam() { }
    }
}
