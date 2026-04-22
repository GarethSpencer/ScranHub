using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Models.Results;

public class UserGroupResult
{
    public required Guid GroupId { get; set; }
    public required string GroupName { get; set; }
    public required int Users { get; set; }
}
