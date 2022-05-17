using System.ComponentModel.DataAnnotations;

namespace Web_Application_Test_1.Models
{
    public class Category
    {
        [Key] // Makes Id the primary key.
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = "";
        public int DisplayOrder { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;
    }
}
