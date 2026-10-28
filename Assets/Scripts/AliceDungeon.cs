using System;
using System.Collections.Generic;
using UnityEngine;

public partial class ChibiGame
{
    public float dungeonEntrance=-1;
    public bool dungeonActive,dungeonCleared,dungeonBossDefeated,dungeonRewardTaken;
    public int dungeonBossHp;
    public float speedBoostTime;
    public readonly List<Block> dungeonBlocks=new List<Block>();
    float trailReturnX,dungeonBossX=55,dungeonBossY,dungeonBossVy,dungeonBossJumpCooldown,dungeonBossPhase,dungeonRoarTime,dungeonQuoteTime,dungeonFade,dungeonDescentTime,dungeonBossHitTime,dungeonBossDeathTime;
    int dungeonFacing=1;
    int dungeonBossLastHit=-1,dungeonBossMaxHp=6;
    bool dungeonBossAwake,dungeonBossGrounded;
    AudioClip tattooRoar;
    readonly Texture2D[] tattooMonsterArt=new Texture2D[12];

    System.Collections.Generic.IEnumerable<Block> ActiveBlocks=>blocks;
    float cameraY;
    float DungeonEnd=>dungeonEntrance+166;
    float DungeonX(float local)=>dungeonEntrance+local;
    float ActiveRunSpeed=>dungeonActive&&(px<DungeonX(48)||px>=DungeonX(118))?2.7f:speedBoostTime>0?Speed*1.55f:Speed;
    public bool DungeonLocksMovement=>dungeonQuoteTime>0;

    void LoadDungeonArt()
    {
        foreach(var n in new[]{"dungeon_wall","dungeon_decor","dungeon_backdrop","castle_stone","dungeon_feast"})art[n]=Resources.Load<Texture2D>("Art/"+n);
        for(int i=0;i<12;i++)tattooMonsterArt[i]=Resources.Load<Texture2D>("Art/tattoo_monster"+i);
        tattooRoar=Resources.Load<AudioClip>("Audio/TattooRoar");
    }

    public void GenerateDungeon(System.Random rng)
    {
        dungeonBlocks.Clear();dungeonEntrance=-1;dungeonActive=dungeonCleared=dungeonBossDefeated=dungeonRewardTaken=dungeonBossAwake=false;
        dungeonRoarTime=dungeonQuoteTime=dungeonFade=dungeonDescentTime=dungeonBossHitTime=dungeonBossDeathTime=speedBoostTime=0;dungeonBossX=55;dungeonBossY=dungeonBossVy=dungeonBossPhase=dungeonBossJumpCooldown=0;dungeonBossGrounded=true;dungeonBossLastHit=-1;
        if(length<1700)return;
        float min=1100,max=Mathf.Max(min+1,length-720);
        for(int attempt=0;attempt<40;attempt++)
        {
            float candidate=min+(float)rng.NextDouble()*(max-min);
            if(castleStart>0&&candidate>castleStart-260&&candidate<castleEnd+260)continue;
            dungeonEntrance=candidate;break;
        }
        if(dungeonEntrance<0)dungeonEntrance=Mathf.Min(length-720,castleEnd+300);
        if(compactRoute)dungeonEntrance=castleEnd+RoadLegDistance;
        // A single world-space route: cut the original floor, then fill it with descending rock.
        blocks.RemoveAll(b=>b.type!=0&&b.r.xMax>dungeonEntrance-16&&b.r.xMin<DungeonEnd+3);
        var floors=blocks.FindAll(b=>b.type==0&&b.r.xMax>dungeonEntrance&&b.r.xMin<DungeonEnd);
        foreach(var floor in floors){
            blocks.Remove(floor);
            if(floor.r.xMin<dungeonEntrance)blocks.Add(new Block(floor.r.xMin,floor.r.y,dungeonEntrance-floor.r.xMin,floor.r.height,0));
            if(floor.r.xMax>DungeonEnd)blocks.Add(new Block(DungeonEnd,floor.r.y,floor.r.xMax-DungeonEnd,floor.r.height,0));
        }
        var terrainRng=new System.Random(seed^0x51A7D00D);
        float middle=-8;
        const float pitch=.5f;
        for(int i=0;i<332;i++){
            float local=i*pitch,top;
            if(local<48)top=-8*Mathf.SmoothStep(0,1,local/48f);
            else if(local>=118)top=-8*(1-Mathf.SmoothStep(0,1,(local-118)/47.5f));
            else {if(i%6==0)middle=Mathf.Clamp(middle+(terrainRng.Next(3)-1)*.08f,-8.25f,-7.8f);top=local>=115?-8:middle;}
            dungeonBlocks.Add(new Block(DungeonX(local),-12,pitch+.002f,12+top,7+i%2));
        }
        foreach(float local in new[]{56f,66f,90f,109f}){
            float top=DungeonSurfaceAt(DungeonX(local));
            dungeonBlocks.Add(new Block(DungeonX(local),top,.9f,.44f,8));
        }
        blocks.AddRange(dungeonBlocks);
        dungeonBlocks.Sort((a,b)=>a.r.x.CompareTo(b.r.x));
        dungeonBossMaxHp=5+Math.Abs(seed%2);dungeonBossHp=dungeonBossMaxHp;
    }

    float DungeonSurfaceAt(float x)
    {
        float surface=-12;
        foreach(var b in dungeonBlocks)if(x>=b.r.xMin&&x<=b.r.xMax)surface=Mathf.Max(surface,b.r.yMax);
        return surface;
    }

    void EnterDungeon()
    {
        if(dungeonActive||dungeonEntrance<0)return;
        trailReturnX=DungeonEnd;dungeonActive=true;
        if(dungeonCleared){dungeonBossDefeated=dungeonRewardTaken=true;return;}
        dungeonBossMaxHp=5+Math.Abs(seed%2);dungeonBossHp=dungeonBossMaxHp;
        dungeonFacing=-1;dungeonBossX=DungeonX(85);dungeonBossY=DungeonSurfaceAt(dungeonBossX);
        dungeonBossVy=0;dungeonBossGrounded=true;dungeonBossJumpCooldown=0;
        dungeonBossPhase=dungeonBossHitTime=dungeonBossDeathTime=0;dungeonBossLastHit=-1;
        dungeonBossDefeated=dungeonRewardTaken=dungeonBossAwake=false;
        dungeonFade=dungeonDescentTime=0;
    }

    void LeaveDungeon(bool cleared)
    {
        dungeonActive=false;dungeonDescentTime=dungeonFade=0;
        if(cleared){dungeonCleared=true;checkpoint=Mathf.Max(checkpoint,DungeonEnd+1);checkpointY=0;safeX=px;safeY=py;Notify("Алиса выбралась из подземелья и вернулась на тропу.");Save();}
    }

    void AbortDungeon(){if(dungeonActive){LeaveDungeon(false);RestorePosition(checkpoint,checkpointY);}}

    public void UpdateDungeon(float dt)
    {
        if(dungeonQuoteTime<=0)speedBoostTime=Mathf.Max(0,speedBoostTime-dt);dungeonQuoteTime=Mathf.Max(0,dungeonQuoteTime-dt);dungeonFade=Mathf.Max(0,dungeonFade-dt*1.1f);
        if(!dungeonActive)
        {
            if(!tutorial&&dungeonEntrance>0&&px>=dungeonEntrance&&px<DungeonEnd&&py<1.1f)EnterDungeon();
            return;
        }
        if(px<dungeonEntrance-.5f||px>DungeonEnd){LeaveDungeon(dungeonRewardTaken);return;}
        if(dungeonQuoteTime>0)return;
        if(!dungeonBossAwake&&px>=DungeonX(72))
        {
            dungeonBossAwake=true;dungeonRoarTime=3.65f;dungeonBossY=DungeonSurfaceAt(dungeonBossX);dungeonBossGrounded=true;
            if(sfx&&tattooRoar)sfx.PlayOneShot(tattooRoar,1.25f);Notify("ДЕМОН ЗАЖИВЛЕНИЯ ТАТУ");Debug.Log("ALICE_VOICE tattoo-roar length="+(tattooRoar?tattooRoar.length:0));
        }
        if(dungeonRoarTime>0){dungeonRoarTime=Mathf.Max(0,dungeonRoarTime-dt);return;}
        dungeonBossHitTime=Mathf.Max(0,dungeonBossHitTime-dt);dungeonBossDeathTime=Mathf.Max(0,dungeonBossDeathTime-dt);
        if(dungeonBossDefeated&&dungeonBossDeathTime>0)MoveGroundActor(ref dungeonBossX,ref dungeonBossY,ref dungeonBossVy,ref dungeonBossGrounded,1.45f,.65f,0,dungeonBossX,dt,dungeonBlocks,ref dungeonBossJumpCooldown);
        if(dungeonBossAwake&&!dungeonBossDefeated)
        {
            dungeonBossPhase+=dt*(dungeonBossGrounded?9:4);dungeonBossJumpCooldown=Mathf.Max(0,dungeonBossJumpCooldown-dt);
            float previousX=dungeonBossX;MoveGroundActor(ref dungeonBossX,ref dungeonBossY,ref dungeonBossVy,ref dungeonBossGrounded,1.45f,2.25f,2.2f,px,dt,dungeonBlocks,ref dungeonBossJumpCooldown);dungeonFacing=ActorFacing(dungeonBossX-previousX,dungeonFacing);
            if(new Rect(dungeonBossX-.725f,dungeonBossY,1.45f,2.25f).Overlaps(PlayerRect())){Damage(22);vx+=(px<dungeonBossX?-1:1)*2.4f;}
        }
        float rewardX=DungeonX(103);
        if(dungeonBossDefeated&&dungeonBossDeathTime<=0&&!dungeonRewardTaken&&Mathf.Abs(px-rewardX)<.85f)
        {
            dungeonRewardTaken=true;speedBoostTime=5f+Mathf.Abs(seed%6);dungeonQuoteTime=7.5f;Play(chime);
            Notify("Вкусное блюдо • ускорение на "+Mathf.CeilToInt(speedBoostTime)+" секунд");
        }
        if(dungeonBossDefeated&&dungeonRewardTaken&&px>=DungeonEnd)LeaveDungeon(true);
    }

    void HitDungeonBoss()
    {
        if(!dungeonActive||!dungeonBossAwake||dungeonRoarTime>0||dungeonBossDefeated||attack<=0||dungeonBossLastHit==attackSerial)return;
        var hit=new Rect(facing>0?px:px-1.8f,py,1.8f,1.9f);var monster=new Rect(dungeonBossX-.725f,dungeonBossY,1.45f,2.25f);
        if(!hit.Overlaps(monster))return;
        dungeonBossLastHit=attackSerial;dungeonBossHp-=currentAttackDamage;dungeonBossX+=facing*.35f;dungeonBossHitTime=.19f;if(dungeonBossHp<=0)PlayDeath();else Play(hitSound);
        if(dungeonBossHp<=0){dungeonBossDefeated=true;dungeonBossDeathTime=1.1f;Notify("Демон заживления тату побеждено. Пахнет ужином…");}
    }

    void DrawDungeonEntrance()
    {
        // Entrance geometry is rendered with the shared solid terrain.
    }

    void DrawDungeonWorld()
    {
        DrawForestBackground(false);
        float depth=Mathf.SmoothStep(0,1,Mathf.Clamp01(-py/5f));
        if(depth>0)Pic("dungeon_backdrop",new Rect(0,0,1280,720),depth);
        // No repeated translucent vegetation overlays in the cave.
        DrawTerrain();
        // The cave camera starts before the player enters: keep nearby forest enemies
        // and the shared paper attack visible across both terrain renderers.
        DrawDreamWorld();
        if(dungeonBossAwake&&(!dungeonBossDefeated||dungeonBossDeathTime>0))
        {
            int frame=dungeonBossDefeated?11:dungeonRoarTime>0?8:!dungeonBossGrounded?10:dungeonBossHitTime>0||Mathf.Abs(px-dungeonBossX)<2.1f?9:Mathf.Abs((int)dungeonBossPhase)%8;Rect r=W(dungeonBossX-1.35f,dungeonBossY-.02f,2.7f,3.0f);
            var tex=tattooMonsterArt[frame];if(tex){float h=frame==11?.85f:frame==9?2.35f:frame==10?2.3f:3f;float w=h*tex.width/tex.height;r=W(dungeonBossX-w*.5f,dungeonBossY,h*tex.width/tex.height,h);}DrawSolidActor(tex,r,dungeonFacing);
            if(!dungeonBossDefeated&&dungeonRoarTime<=0&&dungeonQuoteTime<=0){Wood(new Rect(r.x-65,r.y-42,r.width+130,36),false);Text("Демон заживления тату",r.x-60,r.y-38,r.width+120,30,16,C("#f0b2bd"),false,TextAnchor.MiddleCenter);Fill(new Rect(r.x,r.y-7,r.width*Mathf.Max(0,dungeonBossHp)/(float)dungeonBossMaxHp,5),C("#b54b66"));}
        }
        if(dungeonBossDefeated&&dungeonBossDeathTime<=0&&!dungeonRewardTaken)
        {
            float rewardY=DungeonSurfaceAt(DungeonX(103)),pulse=.82f+Mathf.Sin(Time.unscaledTime*4)*.12f;
            Ellipse(W(DungeonX(102.2f),rewardY-.18f,1.6f,.34f),new Color(1f,.58f,.18f,.20f+pulse*.12f));
            Ellipse(W(DungeonX(102.48f),rewardY+.02f,1.04f,1.04f),new Color(1f,.75f,.32f,.10f+pulse*.08f));
            Pic("dungeon_feast",W(DungeonX(102.42f),rewardY+.08f,.98f,.84f));
        }
        DrawCompanion();
        DrawAlice();
    }
    void DrawDungeonOverlay()
    {
        if(dungeonDescentTime>0){Wood(new Rect(380,548,520,64),false);Text("СПУСК ПОД КОРНИ…",405,560,470,38,24,C("#f0c7c6"),true,TextAnchor.MiddleCenter);}
        if(dungeonRoarTime>0&&dungeonQuoteTime<=0){Wood(new Rect(235,150,810,150));Text("ДЕМОН ЗАЖИВЛЕНИЯ ТАТУ",270,167,740,42,28,C("#e9b1bd"),true,TextAnchor.MiddleCenter);Text("ААААРРРРР!",270,215,740,62,48,C("#fff0e6"),true,TextAnchor.MiddleCenter);}
        if(speedBoostTime>0&&dungeonQuoteTime<=0&&toastTime<=0){Wood(new Rect(435,610,410,60),false);Text("СЫТНЫЙ РЫВОК",500,624,280,30,19,C("#f6d08e"),false,TextAnchor.MiddleCenter);}
        if(dungeonQuoteTime>0){
            Wood(new Rect(115,138,1050,410));
            DrawTrophy("dungeon_feast",new Vector2(640,235),180,140);
            Text("ТРОФЕЙ ПОЛУЧЕН • УСКОРЕНИЕ",240,310,800,40,25,C("#ffe1b5"),true,TextAnchor.MiddleCenter);
            Text("Самая лучшая похвала еды это когда благоверный\nпосле ужина вылизывает твою… тарелку",155,370,970,135,28,C("#ffe1b5"),true,TextAnchor.MiddleCenter);
        }
        if(dungeonFade>0)ScreenTint(new Color(0,0,0,dungeonFade));
    }
}
