using UnityEngine;
using System.Globalization;

public partial class ChibiGame
{
    // Web canvas already covers the visible viewport; browser chrome/safe-area pixels
    // must not be applied a second time in Unity's render-buffer coordinates.
    Rect InterfaceSafeArea {
        get {
#if UNITY_WEBGL && !UNITY_EDITOR
            return new Rect(0,0,Screen.width,Screen.height);
#else
            return Screen.safeArea;
#endif
        }
    }
    Vector2 browserStick;
    float crouchBlend, crouchPhase;
    bool MobileGameplay => mobileControls && (mode==Mode.Play || mode==Mode.Pause || mode==Mode.Dead);
    bool CanTouchPlay => mode==Mode.Play && !HasLargeWindow && !DungeonLocksMovement;
#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")] static extern void AliceTouchState(int playing,int cat);
#endif
    int lastTouchState=-1;
    public void BrowserViewport(string value){
        var parts=value.Split(',');
        if(parts.Length!=2||!int.TryParse(parts[0],out int w)||!int.TryParse(parts[1],out int h)||w<1||h<1)return;
        Screen.SetResolution(w,h,FullScreenMode.Windowed);
        Debug.Log("ALICE_VIEWPORT requested="+w+"x"+h);
    }
    public void BrowserStart(){mobileControls=true;if(mode==Mode.Splash)mode=Mode.Menu;}
    Matrix4x4 MobileWorldMatrix
    {
        get {
            float scale=Mathf.Max(Screen.width/1280f,Screen.height/720f);
            float y=(mode==Mode.Play||mode==Mode.Pause||mode==Mode.Dead)?Screen.height*.76f-550*scale:(Screen.height-720*scale)/2;
            return Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,y,0),Quaternion.identity,Vector3.one*scale);
        }
    }
    void SyncTouchState()
    {
        int state=(CanTouchPlay?1:0)|(catSpawned&&!catRescued&&Mathf.Abs(px-catX)<3.2f?2:0);
        if(state==lastTouchState)return;
        lastTouchState=state;
        if(!CanTouchPlay){browserStick=Vector2.zero;browserButtons=0;touchInteractPressed=false;jumpBuffer=0;}
#if UNITY_WEBGL && !UNITY_EDITOR
        AliceTouchState(state&1,state&2);
#endif
    }

    public void BrowserJoystick(string value)
    {
        var parts=value.Split(',');
        if(parts.Length==2 && float.TryParse(parts[0],NumberStyles.Float,CultureInfo.InvariantCulture,out float x)
            && float.TryParse(parts[1],NumberStyles.Float,CultureInfo.InvariantCulture,out float y))
            browserStick=Vector2.ClampMagnitude(new Vector2(x,y),1);
        else browserStick=Vector2.zero;
    }
    public static float StickMovement(float x) => Mathf.Abs(x)<.20f ? 0 : Mathf.Sign(x)*Mathf.InverseLerp(.20f,.85f,Mathf.Abs(x));
    void AnimateCrouch(float dt)
    {
        crouchBlend=Mathf.MoveTowards(crouchBlend,crouch?1:0,dt*9);
        crouchPhase+=dt*(Mathf.Abs(vx)>.2f?10:3);
    }
    void DrawAlice(float alpha=1)
    {
        string pose=flash>0?"hurt":attack>0?"attack":!grounded?"jump":Mathf.Abs(vx)>.3f?((int)(Time.unscaledTime*9)%2==0?"walk1":"walk2"):"idle";
        Rect p=W(px-1.18f,py-.10f,2.36f,2.36f);
        if(grounded)Ellipse(new Rect(p.x+13,p.y+p.height-6,p.width-26,5),new Color(.02f,.01f,.07f,.48f));
        var matrix=GUI.matrix;
        if(facing<0)GUIUtility.ScaleAroundPivot(new Vector2(-1,1),p.center);
        // Feet remain anchored: the entry, breathing and shuffle are animated independently.
        if(crouchBlend>.01f && grounded && flash<=0)
        {
            int frame=crouchBlend<.34f?0:crouchBlend<.75f?1:Mathf.Abs(vx)>.2f?4+(int)(crouchPhase)%4:2+(int)(crouchPhase*.65f)%2;
            Pic("alice_crouch"+frame,p,alpha);
        }
        else Pic("fairy_"+pose,p,alpha);
        GUI.matrix=matrix;
    }
}
