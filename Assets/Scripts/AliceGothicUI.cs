using UnityEngine;
public partial class ChibiGame
{
    readonly System.Collections.Generic.Dictionary<string,Texture2D> uiArt=new System.Collections.Generic.Dictionary<string,Texture2D>();
    Texture2D UITexture(string name){if(!uiArt.TryGetValue(name,out var t)){t=Resources.Load<Texture2D>("Art/"+name);uiArt[name]=t;}return t;}
    void UIArt(string name,Rect r){var t=UITexture(name);if(t)GUI.DrawTexture(r,t,ScaleMode.StretchToFill,true);}
    void GothicPanel(Rect r,bool light)
    {
        var old=GUI.color;GUI.color=GUI.enabled?Color.white:new Color(.55f,.55f,.55f,1);
        if(r.height<110){UIArt("ui_button",new Rect(r.x-12,r.y-8,r.width+24,r.height+16));}
        else {
            var tex=UITexture("ui_window");if(tex){
                float ex=Mathf.Min(90,r.width*.16f),ey=Mathf.Min(78,r.height*.24f);
                float[] xs={r.x-18,r.x+ex,r.xMax-ex,r.xMax+18},ys={r.y-30,r.y+ey,r.yMax-ey,r.yMax+30};
                float[] us={0,.16f,.84f,1},vs={0,.25f,.75f,1};
                for(int y=0;y<3;y++)for(int x=0;x<3;x++)GUI.DrawTextureWithTexCoords(Rect.MinMaxRect(xs[x],ys[y],xs[x+1],ys[y+1]),tex,new Rect(us[x],1-vs[y+1],us[x+1]-us[x],vs[y+1]-vs[y]));
            }
        }
        GUI.color=old;
    }
    void ScreenTint(Color color){var old=GUI.matrix;GUI.matrix=Matrix4x4.identity;Fill(new Rect(0,0,Screen.width,Screen.height),color);GUI.matrix=old;}
    bool IconButton(string icon,string label,Rect r){
        UIArt("ui_ring",r);var texture=UITexture(icon);
        if(texture){
            // Crop transparent margins before fitting the visible icon into the ring.
            Rect pixels=icon=="ui_inventory"?new Rect(30,16,155,123):icon=="ui_pause"?new Rect(100,27,87,127):new Rect(0,0,texture.width,texture.height);
            float fit=Mathf.Min(r.width*.44f/pixels.width,r.height*.44f/pixels.height);
            Rect image=new Rect(r.center.x-pixels.width*fit/2,r.center.y-pixels.height*fit/2,pixels.width*fit,pixels.height*fit);
            GUI.DrawTextureWithTexCoords(image,texture,new Rect(pixels.x/texture.width,pixels.y/texture.height,pixels.width/texture.width,pixels.height/texture.height));
        }
        return GUI.Button(r,GUIContent.none,GUIStyle.none);
    }
    bool HasPriorityNotice=>tutorial||castleNoticeTime>0||catNotice>0||dungeonRoarTime>0||dungeonQuoteTime>0||sinking>.3f;
    void DrawMobileHUD(){
        var old=GUI.matrix;Rect safe=InterfaceSafeArea;float scale=Mathf.Max(.25f,safe.height/720f);
        float width=safe.width/scale;
        GUI.matrix=Matrix4x4.TRS(new Vector3(safe.x,Screen.height-safe.yMax,0),Quaternion.identity,Vector3.one*scale);
        UIArt("ui_plaque",new Rect(20,8,420,145));
        Text(dungeonActive?"ПОДЗЕМЕЛЬЕ":"ШИЗОЛЕСЬЕ",92,62,280,26,20,C("#e8c7ba"),true,TextAnchor.MiddleCenter);
        Text("ДОМОЙ "+Mathf.FloorToInt(px/length*100)+"%  •  ЭЛИКСИРЫ "+collected,90,92,280,25,16,C("#d3b4b2"),false,TextAnchor.MiddleCenter);
        float right=width-440;UIArt("ui_plaque",new Rect(right,8,420,145));
        int hearts=Mathf.Max(3,lives);float step=Mathf.Min(48,190f/hearts);
        for(int i=0;i<hearts;i++)Pic(i<lives?"life_full":"life_empty",new Rect(right+126+i*step,53,step,40));
        Text("ЗДОРОВЬЕ  "+hp+" / 100",right+85,96,260,24,19,C("#ffe1d3"),false,TextAnchor.MiddleCenter);
        if(toastTime>0&&!HasPriorityNotice){float w=Mathf.Min(740,width-560);UIArt("ui_button",new Rect((width-w)/2,592,w,110));Text(toast,(width-w)/2+65,625,w-130,55,21,C("#ffe1d3"),false,TextAnchor.MiddleCenter);}
        if(!mobileControls){
            if(IconButton("ui_inventory","",new Rect(width/2-96,12,86,86)))ToggleInventory();
            if(IconButton("ui_pause","",new Rect(width/2+10,12,86,86))){mode=Mode.Pause;Save();}
        }
        GUI.matrix=old;
    }
    void DrawScreenNotices()
    {
        if(mode!=Mode.Play||inventoryOpen||doctorDialogue||badgeNotice>0)return;
        if(dungeonQuoteTime>0||dungeonRoarTime>0){DrawDungeonOverlay();return;}
        if(tutorial)return;
        if(castleNoticeTime>0){DrawCastleNotice();return;}
        if(catNotice>0){Wood(new Rect(395,150,490,96));Text("ДОСТИЖЕНИЕ",420,163,440,24,16,C("#cc9c70"),false,TextAnchor.MiddleCenter);Text("получен котенок РА",410,193,460,35,28,C("#ffe0ad"),false,TextAnchor.MiddleCenter);return;}
        if(sinking>.3f){Wood(new Rect(470,540,340,56),false);Text("ТРЯСИНА!",480,550,320,36,27,C("#e096a5"),true,TextAnchor.MiddleCenter);}
        DrawDungeonOverlay();
    }
}
