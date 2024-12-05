using System;
using System.Collections.Generic;

namespace Hakaton.Models;

public partial class GeoDatum
{
    public int ImageId { get; set; }

    public DateTime DateTime { get; set; }

    public byte[] ImageFile { get; set; } = null!;

    public string KmlData { get; set; } = null!;
}
