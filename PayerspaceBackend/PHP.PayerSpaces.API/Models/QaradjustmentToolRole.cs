using System;
using System.Collections.Generic;

namespace PHP.QARAdjustmentTool.API.Test;

public partial class QaradjustmentToolRole
{
    public int RoleId { get; set; }

    public string Name { get; set; } = null!;

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual ICollection<QaradjustmentToolPermissionFolder> QaradjustmentToolPermissionFolders { get; set; } = new List<QaradjustmentToolPermissionFolder>();

    public virtual ICollection<QaradjustmentToolUserRole> QaradjustmentToolUserRoles { get; set; } = new List<QaradjustmentToolUserRole>();
}
