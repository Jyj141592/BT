using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT.Editor{
public static class BTEditorUtility
{
    //return a.b.c -> c
    public static string NameSpaceToClassName(string path){
        var name = path.Split('.');
        return name[^1];
    }
}
}
