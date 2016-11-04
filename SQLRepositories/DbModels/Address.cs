using System.ComponentModel.DataAnnotations;

namespace SQLRepositories.DbModels
{
    public class Address
    {
        [Key]
        public string LegacyAddress { get; set; }

        [Required]
        public string ColoredAddress { get; set; }
    }
}
