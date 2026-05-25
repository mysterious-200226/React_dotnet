using System;
using System.Collections.Generic;

namespace PHP.QARAdjustmentTool.API.Test;

public partial class QaradjustmentToolAvailityUser
{
    public int QaradjustmentToolAvailityUsersUserId { get; set; }

    public string? AvailityUserId { get; set; }

    public string? UserEmail { get; set; }

    public string? UserFirstName { get; set; }

    public string? UserLastName { get; set; }

    public string? OrganizationTaxId { get; set; }

    public string? OrganizationNpi { get; set; }

    public string? Type { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual ICollection<QaradjustmentToolUserRole> QaradjustmentToolUserRoles { get; set; } = new List<QaradjustmentToolUserRole>();
}
