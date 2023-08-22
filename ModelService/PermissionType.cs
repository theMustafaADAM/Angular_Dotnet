using System.ComponentModel.DataAnnotations;

namespace ModelService
{
    public class PermissionType
    {
        [Key]
        public int ID { get; set; }
        public string? Type { get; set; }
    }
}