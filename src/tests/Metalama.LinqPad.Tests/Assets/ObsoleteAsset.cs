using System;

namespace Metalama.LinqPad.Tests.Assets;

internal sealed class ObsoleteAsset
{
    public int NonObsolete { get; set; }
    
    [Obsolete]
    public int Obsolete { get; set; }
}