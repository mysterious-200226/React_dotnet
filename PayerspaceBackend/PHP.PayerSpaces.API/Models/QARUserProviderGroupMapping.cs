using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PHP.QARAdjustmentTool.API.Models
{
    [Table("QARUserProviderGroupMapping")]
    public class QARUserProviderGroupMapping
    {
        [Key]
        public int MappingId { get; set; }

        public int UHSUserId { get; set; }
        public int GroupId { get; set; }
        public DateTime AssignedDate { get; set; }
        public string AssignedBy { get; set; }
    }
}
