using System.ComponentModel.DataAnnotations;

namespace HelloWorld.Web.Models
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
