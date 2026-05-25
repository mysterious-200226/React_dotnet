using System;
using System.Collections.Generic;

namespace PHP.QARAdjustmentTool.API.Test;

public partial class QaradjustmentToolProviderGroup
{
    public int GroupId { get; set; }

    public string GroupName { get; set; } = null!;

    public string? Tin { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
