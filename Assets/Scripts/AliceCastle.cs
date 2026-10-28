using System;
using UnityEngine;

public partial class ChibiGame
{
    public const float GymDemonSpeed=4.62f;
    public float castleStart=-1,castleEnd=-1,castleChaseStart=-1,castleScheduledTime;
    public bool castleChase,castleCleared;
    float castleClock,castleNoticeTime,gymDemonX,gymDemonY,gymDemonVy,gymDemonJumpCooldown; public float castleHeadStart;
    bool gymDemonGrounded;int gymFacing=1;
    bool castleTriggered;
    string castleNoticeText="";
    AudioClip gymDemonVoice;
    float gymRunPhase,gymLanding;bool gymMoving;
    readonly Texture2D[] gymDemonArt=new Texture2D[12];

    void LoadCastleArt()
    {
        foreach(var n in new[]{"castle_stone","castle_moss","castle_arch","flora2","flora3","flora4","flora5","flora6"})art[n]=Resources.Load<Texture2D>("Art/"+n);
        for(int i=0;i<12;i++)gymDemonArt[i]=Resources.Load<Texture2D>("Art/gym_demon"+i);
        gymDemonVoice=Resources.Load<AudioClip>("Audio/GymDemon");
    }

    public void GenerateCastle(System.Random rng)
    {
        castleStart=castleEnd=castleChaseStart=-1;castleScheduledTime=0;castleChase=castleCleared=castleTriggered=false;castleClock=castleNoticeTime=castleHeadStart=0;
        if(length<600)return;
        castleScheduledTime=90f+(float)rng.NextDouble()*210f;
        castleStart=Mathf.Min(length-260,castleScheduledTime*4.35f);
        castleStart=Mathf.Max(300,castleStart);
        if(compactRoute){castleStart=4+RoadLegDistance*3;castleScheduledTime=(castleStart-4)/Speed;}
        float plannedEnd=castleStart+205;
        blocks.RemoveAll(b=>b.type!=0&&b.r.xMax>castleStart-6&&b.r.xMin<plannedEnd+6);

        float x=castleStart,height=0;
        const int ascentCount=11;const float stairWidth=2.45f;const float overlap=.06f;
        for(int i=0;i<ascentCount;i++)
        {
            height+=.38f;
            blocks.Add(new Block(x,0,stairWidth+overlap,height,5+i%2));
            x+=stairWidth;
        }
        castleChaseStart=x+7;

        int flatRun=0;
        for(int i=0;i<39;i++)
        {
            int direction=rng.Next(3)-1;
            if(flatRun>2)direction=rng.Next(2)==0?-1:1;
            float next=Mathf.Clamp(height+direction*.28f,3.65f,5.45f);
            flatRun=Mathf.Abs(next-height)<.01f?flatRun+1:0;height=next;
            float width=3.05f+(float)rng.NextDouble()*.75f;
            blocks.Add(new Block(x,0,width+overlap,height,5+(i+rng.Next(2))%2));
            if(i%8==4)
            {
                float barrierWidth=.72f;
                blocks.Add(new Block(x+width*.57f,height,barrierWidth,.52f,6));
            }
            x+=width;
        }

        int descentCount=Mathf.CeilToInt(height/.38f);
        float descentStep=height/descentCount;
        for(int i=0;i<descentCount;i++)
        {
            height=Mathf.Max(0,height-descentStep);
            blocks.Add(new Block(x,0,stairWidth+overlap,height,5+i%2));
            x+=stairWidth;
        }
        castleEnd=x;
    }

    float CastleSurfaceAt(float x)
    {
        float surface=0;
        foreach(var b in blocks)if(b.type>=5&&b.r.y<=.001f&&x>=b.r.xMin&&x<=b.r.xMax)surface=Mathf.Max(surface,b.r.yMax);
        return surface;
    }

    public void UpdateCastle(float dt)
    {
        castleNoticeTime=Mathf.Max(0,castleNoticeTime-dt);
        if(castleStart<0||tutorial||dungeonActive||mode!=Mode.Play)return;
        if(!castleTriggered&&!castleCleared&&px>=castleChaseStart&&px<castleEnd-12&&py>2)
        {
            castleTriggered=castleChase=true;castleClock=0;castleHeadStart=1.65f;gymFacing=1;gymRunPhase=gymLanding=0;gymMoving=false;gymDemonX=px-3.8f;gymDemonY=CastleSurfaceAt(gymDemonX);gymDemonVy=0;gymDemonGrounded=true;gymDemonJumpCooldown=0;
            castleNoticeText="ДЕМОН КАЧАЛКИ ПРЕСЛЕДУЕТ ТЕБЯ…";castleNoticeTime=2.4f;Play(gymDemonVoice);Debug.Log("ALICE_VOICE gym-demon");
        }
        if(!castleChase)return;
        if(castleHeadStart>0){castleHeadStart=Mathf.Max(0,castleHeadStart-dt);return;}
        castleClock+=dt;
        bool wasGrounded=gymDemonGrounded;float previousX=gymDemonX;gymDemonJumpCooldown=Mathf.Max(0,gymDemonJumpCooldown-dt);MoveGroundActor(ref gymDemonX,ref gymDemonY,ref gymDemonVy,ref gymDemonGrounded,1.24f,2.05f,GymDemonSpeed,px,dt,blocks,ref gymDemonJumpCooldown);gymFacing=ActorFacing(gymDemonX-previousX,gymFacing);gymMoving=Mathf.Abs(gymDemonX-previousX)>.0001f;gymRunPhase+=Mathf.Abs(gymDemonX-previousX)*2.6f;gymLanding=(!wasGrounded&&gymDemonGrounded)?.14f:Mathf.Max(0,gymLanding-dt);
        Rect demonRect=new Rect(gymDemonX-.62f,gymDemonY,1.24f,2.05f);
        if(demonRect.Overlaps(PlayerRect()))
        {
            castleChase=false;castleTriggered=false;castleClock=castleHeadStart=0;
            Damage(100,true);
            if(mode==Mode.Play)Notify("Демон качалки настиг тебя. Погоню придётся пройти снова.");
            return;
        }
        if(castleClock>=10)
        {
            castleChase=false;castleCleared=true;castleHeadStart=0;
            castleNoticeText="ВЫ МОЖЕТЕ СЛЕДОВАТЬ ДАЛЬШЕ…";castleNoticeTime=4.5f;Play(chime);Save();
        }
    }

    void UpdateMusicMix(float dt)
    {
        bool pursuit=mode==Mode.Play&&((castleChase&&castleHeadStart<=0)||(dungeonActive&&dungeonBossAwake&&!dungeonBossDefeated)||bugs.Exists(e=>e.demon&&e.alerted&&e.hp>0&&Mathf.Abs(e.x-px)<10));
        if(pursuit&&dangerMusic&&!dangerMusic.isPlaying){dangerMusic.time=0;dangerMusic.Play();Debug.Log("ALICE_MUSIC chase-start");}
        float baseVolume=Mathf.Clamp(prefs.volume,0,.6f);
        if(music)music.volume=Mathf.MoveTowards(music.volume,pursuit?0:baseVolume,dt*.45f);
        if(dangerMusic)
        {
            dangerMusic.volume=Mathf.MoveTowards(dangerMusic.volume,pursuit?Mathf.Min(.6f,baseVolume*1.18f):0,dt*.55f);
            if(!pursuit&&dangerMusic.isPlaying&&dangerMusic.volume<=.001f){dangerMusic.Stop();Debug.Log("ALICE_MUSIC gothic-return");}
        }
    }

    void DrawCastleBackdrop()
    {
        if(castleStart<0||castleEnd<cameraX-8||castleStart>cameraX+38)return;
        float first=castleStart-3;
        for(int i=0;i<8;i++)
        {
            float x=first+i*21.5f;
            if(x>castleEnd+4)break;
            Pic("castle_arch",W(x,-.02f,8.8f,9.2f));
        }
    }

    void DrawCastleBlock(Block b,string texture)
    {
        const float tile=1.45f; const float tileWidth=2.6f;const float seam=.055f;
        for(float y=b.r.y;y<b.r.yMax-.01f;y+=tile)for(float x=Mathf.Max(b.r.x,b.r.x+Mathf.Floor((cameraX-3-b.r.x)/tileWidth)*tileWidth);x<Mathf.Min(b.r.xMax-.01f,cameraX+34);x+=tileWidth)
            Pic(texture,W(x-seam*.5f,y-seam*.5f,Mathf.Min(tileWidth,b.r.xMax-x)+seam,Mathf.Min(tile,b.r.yMax-y)+seam));
    }
    void DrawCastleActors()
    {
        if(!castleChase||gymDemonX<cameraX-4||gymDemonX>cameraX+34)return;
        Rect r=W(gymDemonX-1.4f,gymDemonY-.05f,2.8f,3.05f);
        int frame=!gymDemonGrounded?10:gymLanding>0?11:castleHeadStart>0||!gymMoving?8+(int)(Time.unscaledTime*3)%2:(int)gymRunPhase%8;
        var tex=gymDemonArt[frame];float h=frame==10?2.35f:frame==11?2.15f:3.05f;if(tex){float w=h*tex.width/tex.height;r=W(gymDemonX-w/2,gymDemonY,w,h);}
        DrawSolidActor(tex,r,gymFacing);
    }

    void DrawCastleNotice()
    {
        if(castleNoticeTime<=0)return;
        Wood(new Rect(305,132,670,92));
        Text("ДОСТИЖЕНИЕ",330,140,620,24,16,C("#be8d74"),false,TextAnchor.MiddleCenter);
        Text(castleNoticeText,325,168,630,38,25,C("#ffe0cf"),false,TextAnchor.MiddleCenter);
    }
}

