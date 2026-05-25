using System;
using System.Collections.Generic;

namespace PHP.QARAdjustmentTool.API.Test;

public partial class QaradjustmentToolPermissionFolder
{
    public int QaradjustmentToolPermissionFoldersId { get; set; }

    public string FolderPath { get; set; } = null!;

    public int RoleId { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual QaradjustmentToolRole Role { get; set; } = null!;
}
