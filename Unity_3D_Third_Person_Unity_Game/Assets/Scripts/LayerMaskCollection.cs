using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayerMaskCollection
{
    public static int includeAllButPoles = ~(1 << 8);
    public static int onlyPlayer = 1 << 9;

}
