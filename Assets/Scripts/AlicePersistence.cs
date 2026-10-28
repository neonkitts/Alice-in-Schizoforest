using UnityEngine;
public partial class ChibiGame
{
    float checkpointY,safeX=4,safeY,saveClock;
    bool HasLargeWindow=>inventoryOpen||doctorDialogue||badgeNotice>0||dungeonQuoteTime>0||mode==Mode.Pause||mode==Mode.Options;
    void TrackProgress(float dt)
    {
        if(tutorial||mode!=Mode.Play)return;
        CaptureSafePosition();
        saveClock+=dt;
        if(saveClock>=5){saveClock=0;Save();}
    }
    void RestorePosition(float x,float y)
    {
        px=x;py=y;vx=vy=0;cameraX=Mathf.Max(0,x-9);cameraY=Mathf.Min(0,y);
        grounded=true;catFollowX=x-1.4f;catFollowY=CompanionSurfaceAt(catFollowX,y);catVertical=0;catGrounded=true;
        safeX=x;safeY=y;
    }
    void CaptureSafePosition()
    {
        if(mode==Mode.Play&&grounded&&!castleChase&&!DungeonLocksMovement&&
           !OverlapSolid(new Rect(px-.32f,py+.05f,.64f,1.52f))) {safeX=px;safeY=py;}
    }
    float CheckpointSurface(float x)
    {
        float top=0;foreach(var b in blocks)if(x>=b.r.xMin&&x<=b.r.xMax&&b.r.yMin<=0)top=Mathf.Max(top,b.r.yMax);
        return top;
    }
    SaveData ReadValidatedSave()
    {
        foreach(string key in new[]{SaveKey,SaveKey+".backup"}){
            try {var v=JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(key,""));
                if(v!=null&&v.lives>0&&v.length>=100&&v.length<=100000&&
                   !float.IsNaN(v.resumeX)&&!float.IsInfinity(v.resumeX)&&
                   !float.IsNaN(v.resumeY)&&!float.IsInfinity(v.resumeY))return v;
            }catch(System.Exception){ }
        }
        throw new System.Exception("No valid saved progress");
    }
    float TerrainVisualBottom=>Mathf.Min(-4,cameraY-14);
}
