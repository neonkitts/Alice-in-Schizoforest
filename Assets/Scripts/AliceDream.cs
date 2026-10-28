using System;
using System.Collections.Generic;
using UnityEngine;

public partial class ChibiGame
{
    public class DreamEnemy { public float x,y,origin,phase,dying,vy,jumpCooldown; public int hp=1,lastHit=-1,facing=1; public bool demon,alerted,grounded=true; }
    public List<DreamEnemy> bugs=new List<DreamEnemy>();
    public List<Rect> mires=new List<Rect>();
    float storyClock,demonClock,sinking; int attackSerial;
    AudioClip demonVoice;
    Texture2D[] demonArt=new Texture2D[4],bugArt=new Texture2D[4];
    void LoadDreamArt(){for(int i=0;i<4;i++){demonArt[i]=Resources.Load<Texture2D>("Art/demon"+i);bugArt[i]=Resources.Load<Texture2D>("Art/bug"+i);}foreach(var n in new[]{"wine","beer","paper"})art[n]=Resources.Load<Texture2D>("Art/"+n);art["paper"]=Resources.Load<Texture2D>("Art/ui_attack");}
    string SaveKey=>autoRun?"alice.smoke":"alice.save";
    void StoreSave(string json){if(PlayerPrefs.HasKey(SaveKey)){string previous=ReadSave();try{var old=JsonUtility.FromJson<SaveData>(previous);if(old!=null&&old.lives>0&&old.length>=100)PlayerPrefs.SetString(SaveKey+".backup",previous);}catch{}}PlayerPrefs.SetString(SaveKey,json);PlayerPrefs.Save();}
    string ReadSave()=>PlayerPrefs.GetString(SaveKey,"");
    bool HasSave()=>PlayerPrefs.HasKey(SaveKey);
    void DeleteSave(){PlayerPrefs.DeleteKey(SaveKey);PlayerPrefs.DeleteKey(SaveKey+".backup");PlayerPrefs.Save();}
    void GenerateDream()
    {
        bugs.Clear();mires.Clear();pterodactyls.Clear();motes.Clear();nectarBottles.Clear();vines.Clear();demonClock=0;sinking=0;
        var rng=new System.Random(seed^7391);
        for(int i=0;i<blocks.Count;i++)
        {
            var b=blocks[i];if(b.type==0||b.type>=7)continue;
            if(i%9==0)mires.Add(b.r.y>0?new Rect(b.r.x,b.r.y-.17f,b.r.width,.17f):new Rect(b.r.x,b.r.yMax-.015f,b.r.width,.18f));
            if(i%7==0)motes.Add(new Vector2(b.r.center.x,b.r.y>0?.4f:b.r.yMax+.4f));
        }
        foreach(var ground in blocks)if(ground.type==0)
        {
            for(float x=Mathf.Max(ground.r.x+3,25);x<ground.r.xMax-3&&x<length-20;x+=32+rng.Next(16))
            {
                var area=new Rect(x-1,0,3,1);bool clear=true;
                foreach(var b in blocks)if(b.type!=0&&area.Overlaps(b.r)){clear=false;break;}
                if(clear){bool demon=x>360&&bugs.Count%10==0;bugs.Add(new DreamEnemy{x=x,origin=x,phase=(float)rng.NextDouble()*6,demon=demon,hp=demon?4:1});if(rng.Next(3)==0)nectarBottles.Add(new Vector2(x+1,.3f));}
            }
        }
        if(castleStart>0)
        {
            mires.RemoveAll(r=>r.xMax>castleStart-5&&r.xMin<castleEnd+5);
            bugs.RemoveAll(e=>e.x>castleStart-8&&e.x<castleEnd+8);
            motes.RemoveAll(v=>v.x>castleStart-5&&v.x<castleEnd+5);
            nectarBottles.RemoveAll(v=>v.x>castleStart-5&&v.x<castleEnd+5);
        }
        if(dungeonEntrance>0)
        {
            mires.RemoveAll(r=>r.xMax>dungeonEntrance-6&&r.xMin<DungeonEnd+4);
            bugs.RemoveAll(e=>e.x>dungeonEntrance-4&&e.x<DungeonEnd+4);
            motes.RemoveAll(v=>v.x>dungeonEntrance-4&&v.x<DungeonEnd+4);
            nectarBottles.RemoveAll(v=>Mathf.Abs(v.x-dungeonEntrance)<7);
        }
    }
    public static int WineHealing(int count)=>count>0&&count%3==0?10:0;
    public void SpawnDemon(){bugs.Add(new DreamEnemy{x=px+7,origin=px+7,y=GroundAt(px+7),hp=4,demon=true});}
    void UpdateDream(float dt)
    {
        if(dungeonActive)return;
        // Encounters live on the route; waiting does not spawn enemies.
        var player=PlayerRect();bool inMire=false;
        foreach(var mire in mires)if(player.Overlaps(mire)){inMire=true;break;}
        if(inMire){sinking+=dt;vx*=.88f;if(sinking>.35f)Damage(10);if(sinking>2.5f){Damage(100,true);sinking=0;Notify("Трясина затянула. Не стой на месте!");}}
        else sinking=Mathf.Max(0,sinking-dt*3);
        foreach(var e in bugs)
        {
            if(e.hp<=0){e.dying+=dt;continue;}e.phase+=dt*(e.grounded?7:3);e.jumpCooldown=Mathf.Max(0,e.jumpCooldown-dt);
            if(e.demon){if(!e.alerted&&Mathf.Abs(px-e.x)<7&&Mathf.Abs(py-e.y)<3){e.alerted=true;Notify("ДЕМОН ТРЕВОЖНОСТИ");Play(demonVoice);}if(!e.alerted)continue;float previousX=e.x;MoveGroundActor(ref e.x,ref e.y,ref e.vy,ref e.grounded,1.1f,1.9f,1.35f,px,dt,blocks,ref e.jumpCooldown);e.facing=ActorFacing(e.x-previousX,e.facing);}
            else {if(e.origin==0)e.origin=e.x;float oldX=e.x;e.x=e.origin+Mathf.Sin(e.phase*.22f)*.65f;if(Mathf.Abs(e.x-oldX)>.0001f)e.facing=e.x>oldX?1:-1;e.y=GroundAt(e.x);}
            if(player.Overlaps(EnemyRect(e)))Damage(e.demon?25:10);
        }
        HitDreamEnemies();
        for(int i=0;i<motes.Count;i++)if(!taken.Contains(i)&&Vector2.Distance(new Vector2(px,py+.6f),motes[i])<.85f){taken.Add(i);collected++;hp=Mathf.Min(100,hp+WineHealing(collected));Play(chime);Notify(collected%3==0?"Пошла третья бутылка вина. Полет отличный":"Винный эликсир • "+collected%3+" / 3");}
        for(int i=0;i<nectarBottles.Count;i++)if(!taken.Contains(10000+i)&&Vector2.Distance(new Vector2(px,py+.6f),nectarBottles[i])<.85f){taken.Add(10000+i);hp=Mathf.Min(100,hp+5);Play(chime);Notify("Пивной эликсир • +5 здоровья");}
    }
    Rect EnemyRect(DreamEnemy e)=>new Rect(e.x-(e.demon?.55f:.35f),e.y,e.demon?1.1f:.7f,e.demon?1.9f:.45f);
    void MoveGroundActor(ref float x,ref float y,ref float vertical,ref bool onGround,float width,float height,float speed,float targetX,float dt,System.Collections.Generic.IEnumerable<Block> solids,ref float jumpCooldown)
    {
        float direction=targetX>x+.05f?1:targetX<x-.05f?-1:0;
        float support=float.MinValue;
        foreach(var b in solids)if(x+width*.32f>b.r.xMin&&x-width*.32f<b.r.xMax&&b.r.yMax<=y+.16f)support=Mathf.Max(support,b.r.yMax);
        if(support>float.MinValue&&vertical<=0&&y<=support+.08f){y=support;vertical=0;onGround=true;}
        else if(onGround&&(support==float.MinValue||y>support+.11f))onGround=false;
        bool blocked=false;
        if(direction!=0)
        {
            Rect probe=new Rect(x+direction*(width*.55f)-width*.35f,y+.08f,width*.7f,height*.78f);
            foreach(var b in solids)if(probe.Overlaps(b.r)&&b.r.yMax>y+.12f){blocked=true;break;}
        }
        if(blocked&&onGround&&jumpCooldown<=0){vertical=7.7f;onGround=false;jumpCooldown=.55f;}
        float nextX=x+direction*speed*dt;
        Rect horizontal=new Rect(nextX-width*.5f,y+.08f,width,height-.14f);bool hitWall=false;
        foreach(var b in solids)if(horizontal.Overlaps(b.r)){hitWall=true;break;}
        if(!hitWall)x=nextX;else if(onGround&&jumpCooldown<=0){vertical=7.7f;onGround=false;jumpCooldown=.55f;}
        float previousY=y;vertical-=Gravity*dt;float nextY=y+vertical*dt;
        if(vertical<=0)
        {
            float landing=float.MinValue;
            foreach(var b in solids)if(x+width*.42f>b.r.xMin&&x-width*.42f<b.r.xMax&&previousY>=b.r.yMax-.08f&&nextY<=b.r.yMax)landing=Mathf.Max(landing,b.r.yMax);
            if(landing>float.MinValue){y=landing;vertical=0;onGround=true;return;}
        }
        else
        {
            float top=nextY+height;
            foreach(var b in solids)if(x+width*.42f>b.r.xMin&&x-width*.42f<b.r.xMax&&previousY+height<=b.r.yMin+.05f&&top>=b.r.yMin){nextY=b.r.yMin-height-.01f;vertical=0;break;}
        }
        y=nextY;
    }
    void HitDreamEnemies()
    {
        if(attack<=0)return;var hit=new Rect(facing>0?px:px-1.8f,py,1.8f,1.9f);
        foreach(var e in bugs)if(e.hp>0&&e.lastHit!=attackSerial&&hit.Overlaps(EnemyRect(e))){e.lastHit=attackSerial;e.hp-=currentAttackDamage;var pushed=EnemyRect(e);pushed.x+=facing*.25f;if(!OverlapSolid(pushed))e.x+=facing*.25f;if(e.hp<=0)PlayDeath();else Play(hitSound);}
    }
    public static Vector2 PaperAttackOffset(float progress,int direction){
        float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(progress));
        return new Vector2((direction<0?-1:1)*Mathf.Lerp(.45f,1.2f,t),Mathf.Lerp(2.3f,.75f,t));
    }
    void DrawDreamWorld()
    {
        foreach(var r in mires)if(r.xMax>cameraX-2&&r.x<cameraX+32)
        {
            var a=W(r.x,r.y,r.width,r.height);int pieces=Mathf.Max(1,Mathf.CeilToInt(a.width/52));
            for(int i=0;i<pieces;i++)Pic("flora6",new Rect(a.x+i*a.width/pieces-4,a.y-35,a.width/pieces+8,a.height+39),.9f);
        }
        foreach(var e in bugs)
        {
            if(e.x<cameraX-3||e.x>cameraX+32||(e.hp<=0&&e.dying>.5f))continue;
            var tex=(e.demon?demonArt:bugArt)[e.demon&&!e.grounded?2:Mathf.Abs((int)e.phase)%4];
            var r=e.demon?W(e.x-1.1f,e.y,2.2f,2.6f):W(e.x-.55f,e.y-.03f,1.1f,.7f);
            DrawSolidActor(tex,r,e.demon?e.facing:-e.facing);
            if(e.demon&&e.hp>0){Wood(new Rect(r.x-55,r.y-40,r.width+110,34),false);Text("Демон тревожности",r.x-50,r.y-36,r.width+100,30,16,C("#edacbc"),false,TextAnchor.MiddleCenter);Fill(new Rect(r.x,r.y-6,r.width*e.hp/4,4),C("#b74e6b"));}
        }
        for(int i=0;i<motes.Count;i++)if(!taken.Contains(i)&&Mathf.Abs(motes[i].x-px)<35)Pic("wine",W(motes[i].x-.18f,motes[i].y-.16f,.36f,.7f));
        for(int i=0;i<nectarBottles.Count;i++)if(!taken.Contains(10000+i)&&Mathf.Abs(nectarBottles[i].x-px)<35)Pic("beer",W(nectarBottles[i].x-.18f,nectarBottles[i].y-.16f,.36f,.65f));
        if(attack>0){Vector2 offset=PaperAttackOffset(1-attack/.28f,facing);Pic("paper",W(px+offset.x-.6f,py+offset.y-.55f,1.2f,1.1f));}
    }
    Matrix4x4 MobileInterfaceMatrix
    {
        get
        {
            // Fit UI inside the visible canvas, independently of the cropped world.
            Rect safe=InterfaceSafeArea;
            float margin=Mathf.Max(12,Screen.height*.035f);
            float scale=Mathf.Min((safe.width-margin*2)/1280f,(safe.height-margin*2)/720f);
            return Matrix4x4.TRS(new Vector3(safe.x+(safe.width-1280*scale)/2,Screen.height-safe.yMax+(safe.height-720*scale)/2,0),Quaternion.identity,Vector3.one*scale);
        }
    }
    Rect TouchRect(int id)
    {
        if(id==0)return new Rect(55,515,120,100);
        if(id==1)return new Rect(195,515,120,100);
        if(id==2)return new Rect(825,515,120,100);
        if(id==3)return new Rect(970,515,120,100);
        if(id==4)return new Rect(1115,515,120,100);
        return new Rect(970,410,265,82);
    }
    int browserButtons;
    public void BrowserControls(string mask){int.TryParse(mask,out browserButtons);}
    public void BrowserAction(string action){if(action=="inventory"&&(mode==Mode.Play||inventoryOpen))ToggleInventory();if(action=="pause"){if(mode==Mode.Play)mode=Mode.Pause;else if(mode==Mode.Pause&&!inventoryOpen)mode=Mode.Play;}}
    bool TouchHeld(int id){
#if UNITY_WEBGL && !UNITY_EDITOR
        if(!mobileControls || !CanTouchPlay)return false;
        if(id==2)return browserStick.y<-.42f;
        if(id==3)return browserStick.y>.48f;
        return (browserButtons&(1<<id))!=0;
#else
        foreach(var touch in Input.touches){var screenPoint=new Vector3(touch.position.x,Screen.height-touch.position.y,0);Vector2 p=mobileControls?(Vector2)MobileInterfaceMatrix.inverse.MultiplyPoint3x4(screenPoint):new Vector2(screenPoint.x*1280/Screen.width,screenPoint.y*720/Screen.height);if(touch.phase!=TouchPhase.Ended&&touch.phase!=TouchPhase.Canceled&&TouchRect(id).Contains(p))return true;}return false;
#endif
    }
    bool touchJumpLast,touchAttackLast,touchInteractLast,touchInteractPressed;
    void LateUpdate()
    {
        SyncTouchState();
        bool j=TouchHeld(3),a=TouchHeld(4),e=TouchHeld(5);if(CanTouchPlay){if(j)jumpBuffer=.15f;if(a&&!touchAttackLast)Attack();if(e&&!touchInteractLast)touchInteractPressed=true;}touchJumpLast=j;touchAttackLast=a;touchInteractLast=e;
        if(music&&!music.isPlaying&&Input.anyKeyDown)music.Play();
    }
    void DrawTouch()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return; // Browser controls are anchored to the visual viewport, outside the game image.
#endif
        if(!mobileControls)return;
        string[] names={"<\nВЛЕВО",">\nВПРАВО","ПРИГНУТЬСЯ","ПРЫЖОК","АТАКА"};
        for(int i=0;i<names.Length;i++)
        {
            var r=TouchRect(i);bool held=TouchHeld(i);Wood(r,!held);if(held)Fill(new Rect(r.x+5,r.y+5,r.width-10,r.height-10),new Color(.55f,.16f,.29f,.48f));
            Text(names[i],r.x+5,r.y+(i<2?20:31),r.width-10,i<2?72:46,i<2?20:16,C("#ffe2e5"),false,TextAnchor.MiddleCenter);
        }
        if(catSpawned&&!catRescued&&Mathf.Abs(px-catX)<3.2f)
        {
            var take=TouchRect(5);Wood(take,!TouchHeld(5));Text("ВЗЯТЬ КОТЁНКА РА",take.x+8,take.y+20,take.width-16,48,18,C("#ffe0ad"),false,TextAnchor.MiddleCenter);
        }
        if(Button("ИНВ.",1035,145,100))ToggleInventory();
        if(Button("II",1150,145,85))mode=Mode.Pause;
    }
}

