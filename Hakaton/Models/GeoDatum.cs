using System;
using System.Collections.Generic;

namespace Hakaton.Models;

public partial class GeoDatum
{
    public int ImageId { get; set; }

    public DateTime DateTime { get; set; }

    public string KmlData { get; set; } = null!;

    public string ImagePath { get; set; } = null!;
}
