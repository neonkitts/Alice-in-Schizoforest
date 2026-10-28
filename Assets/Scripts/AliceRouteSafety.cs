using System.Collections.Generic;
using UnityEngine;
public partial class ChibiGame {
    // A full jump lasts 2*8.2/20 seconds (4.18 units at run speed).
    // Keep room to land and switch stance between ground blocks and low ceilings.
    public const float ForestTransitionClearance=3.6f;
    public static int EnsureForestClearance(List<Block> terrain){
        var lower=terrain.FindAll(b=>b.type>0&&b.type<5&&Mathf.Abs(b.r.y)<.001f);
        return terrain.RemoveAll(ceiling=>{
            if(ceiling.type<=0||ceiling.type>=5||ceiling.r.y<=0)return false;
            foreach(var floor in lower){
                float gap=Mathf.Max(ceiling.r.xMin-floor.r.xMax,floor.r.xMin-ceiling.r.xMax);
                if(gap<ForestTransitionClearance)return true;
            }
            return false;
        });
    }
}
