using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT{
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class DisplayOnNodeAttribute : Attribute{
    public DisplayOnNodeAttribute(){}
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class CreateNodeMenuAttribute : Attribute{
    public string path;
    public CreateNodeMenuAttribute(string path){
        this.path = path;
    }
    public CreateNodeMenuAttribute(){
        path = null;
        
    }
}

}