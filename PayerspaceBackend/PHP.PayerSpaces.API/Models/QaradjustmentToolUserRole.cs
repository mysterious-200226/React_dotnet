using System;
using System.Collections.Generic;

namespace PHP.QARAdjustmentTool.API.Test;

public partial class QaradjustmentToolUserRole
{
    public int QaradjustmentToolUserRolesId { get; set; }

    public int QaradjustmentToolAvailityUsersUserId { get; set; }

    public int RoleId { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual QaradjustmentToolAvailityUser QaradjustmentToolAvailityUsersUser { get; set; } = null!;

    public virtual QaradjustmentToolRole Role { get; set; } = null!;
}
