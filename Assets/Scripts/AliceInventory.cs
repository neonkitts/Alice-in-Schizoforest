using UnityEngine;

public partial class ChibiGame
{
    public bool catSpawned,catRescued;
    public int lifePotions;
    public float catX;
    float catFollowX,catFollowY,catVertical,catNotice;
    int catFacing=1;
    bool catGrounded=true,catMoving;
    bool inventoryOpen;
    public static float CompanionRunSpeed(float distance)=>Mathf.Abs(distance)>3?7.8f:6.2f;
    public static int CompanionFacing(float playerVelocity,float targetDelta)=>Mathf.Abs(playerVelocity)>.15f?(playerVelocity>0?1:-1):(Mathf.Abs(targetDelta)>.05f?(targetDelta>0?1:-1):0);
    string AchievementKey=>autoRun?"alice.smoke.ra":"alice.achievement.ra";
    float CompanionSurfaceAt(float x,float maxY)
    {
        float surface=-12;
        foreach(var b in ActiveBlocks)if(x>b.r.xMin-.14f&&x<b.r.xMax+.14f&&b.r.yMax<=maxY+.3f)surface=Mathf.Max(surface,b.r.yMax);
        return surface;
    }
    void ResetExpedition(){catSpawned=catRescued=inventoryOpen=false;lifePotions=0;catX=catFollowX=catFollowY=catVertical=0;catNotice=0;catFacing=1;catGrounded=true;catMoving=false;}
    public void UpdateExpedition(float dt,bool interact=false)
    {
        if(tutorial)return;
        catNotice=Mathf.Max(0,catNotice-dt);
        if(catSpawned&&!catRescued&&Mathf.Abs(px-catX)<2.4f&&interact)
        {
            catRescued=true;lifePotions++;catFollowX=catX-facing*1.45f;catFollowY=0;catVertical=0;catGrounded=true;catNotice=8;
            PlayerPrefs.SetInt(AchievementKey,1);PlayerPrefs.Save();
            Notify("Получен котенок РА • зелье жизни в инвентаре [I]");Play(chime);Save();
        }
        if(catRescued)
        {
            float target=px-facing*1.45f,delta=target-catFollowX;
            if(Mathf.Abs(delta)>8){catFollowX=target;catFollowY=CompanionSurfaceAt(target,Mathf.Max(0,py));catVertical=0;catGrounded=true;delta=0;}
            catMoving=Mathf.Abs(vx)>.15f||Mathf.Abs(delta)>.16f;
            int runFacing=CompanionFacing(vx,delta);if(runFacing!=0)catFacing=runFacing;
            float direction=Mathf.Sign(delta),look=catFollowX+direction*.72f;
            bool stepAhead=false;
            foreach(var b in ActiveBlocks)if(b.type!=0&&b.r.y<=catFollowY+.35f&&b.r.yMax>catFollowY+.08f&&look>b.r.xMin-.15f&&look<b.r.xMax+.15f){stepAhead=true;break;}
            float currentSupport=CompanionSurfaceAt(catFollowX,catFollowY);
            if(catGrounded&&catFollowY>currentSupport+.035f){catGrounded=false;catVertical=0;}
            if(catGrounded&&catMoving&&(stepAhead||py>catFollowY+1.15f)){catVertical=7.2f;catGrounded=false;Play(jumpSound);}
            catFollowX=Mathf.MoveTowards(catFollowX,target,dt*CompanionRunSpeed(delta));
            if(!catGrounded){catVertical-=18f*dt;catFollowY+=catVertical*dt;}
            float support=CompanionSurfaceAt(catFollowX,catFollowY);
            if(catGrounded&&catFollowY>support+.035f){catGrounded=false;catVertical=Mathf.Min(0,catVertical);}
            if(catVertical<=0&&catFollowY<=support){catFollowY=support;catVertical=0;catGrounded=true;}
        }
    }
    public bool UseLifePotion()
    {
        if(lifePotions<1)return false;
        lifePotions--;lives++;Play(chime);Notify("Зелье пробуждения • +1 жизнь");Save();return true;
    }
    void ToggleInventory()
    {
        if(tutorial)return;
        if(inventoryOpen){inventoryOpen=false;mode=Mode.Play;return;}
        if(mode==Mode.Play){inventoryOpen=true;mode=Mode.Pause;vx=0;jumpBuffer=0;Save();}
    }
    void DrawCompanion()
    {
        if(tutorial||!catSpawned)return;
        float x=catRescued?catFollowX:catX;
        if(x<cameraX-2||x>cameraX+30)return;
        float y=catRescued?catFollowY:GroundAt(catX);
        string pose=catRescued&&!catGrounded?"cat_jump":catMoving?"cat"+(1+(int)(Time.time*9)%2):"cat0";
        if(catGrounded)Ellipse(W(x-.48f,y-.035f,.96f,.12f),new Color(.015f,.008f,.018f,.72f));Rect r=W(x-.62f,y,1.24f,1.06f);var old=GUI.matrix;
        if((catRescued?catFacing:1)<0)GUIUtility.ScaleAroundPivot(new Vector2(-1,1),r.center);
        Pic(pose,r);GUI.matrix=old;
        if(!catRescued&&Mathf.Abs(px-catX)<2.4f)
        {
            var label=W(x-2.4f,y+1.6f,4.8f,.8f);
            Wood(label,false);
            Text(mobileControls?"ВЗЯТЬ РА — кнопка справа":"E — забрать котёнка",label.x+label.width*.18f,label.y+label.height*.26f,label.width*.64f,label.height*.48f,12,C("#f7dec1"),false,TextAnchor.MiddleCenter);
        }
    }

    void DrawInventory()
    {
        Wood(new Rect(190,90,900,545));
        Text("Инвентарь",290,128,700,55,38,C("#f2d5d1"),true,TextAnchor.MiddleCenter);
        Text("Сон замер. Выбери предмет.",290,184,700,32,20,C("#d0b8ba"),false,TextAnchor.MiddleCenter);
        for(int i=0;i<2;i++)UIArt("ui_card",new Rect(245+i*410,220,380,300));
        Text("Зелье пробуждения",300,292,270,32,22,C("#f5dac3"),false,TextAnchor.MiddleCenter);
        Pic("wine",new Rect(417,330,36,64));
        Text("Дополнительная жизнь: +1\nВ сумке: "+lifePotions,300,398,270,48,19,C("#d8bdbe"),false,TextAnchor.MiddleCenter);
        GUI.enabled=lifePotions>0;if(Button("Выпить",335,452,200))UseLifePotion();GUI.enabled=true;
        Text(catRescued?"Котёнок РА":"Тихое мяуканье…",710,292,270,32,22,C("#efcfaa"),false,TextAnchor.MiddleCenter);
        Pic("cat0",new Rect(809,330,72,64),catRescued?1:.25f);
        Text(catRescued?"Теперь ты идёшь не одна.\nРА следует за Алисой.":"Найди потерявшегося котёнка\nна тропе Шизолесья.",710,398,270,52,19,C("#d8bdbe"),false,TextAnchor.MiddleCenter);
        if(PlayerPrefs.GetInt(AchievementKey,0)==1)Text("Достижение получено",710,448,270,22,17,C("#e6bc83"),false,TextAnchor.MiddleCenter);
        if(badges>0){Pic("doctor_badge",new Rect(275,540,54,54));if(Button("Значок: удар +40",345,550,270))UseDoctorBadge();}
        else if(pendingPower>0)Text("Следующий удар +40",285,552,330,44,20,C("#ffe0ad"),false,TextAnchor.MiddleCenter);
        if(Button("Закрыть [I / Esc]",710,550,270))ToggleInventory();
    }
    void DrawCheckpoint(float x)
    {
        bool lit=x<=checkpoint;float ground=CheckpointSurface(x);
        Ellipse(W(x-.75f,ground-.07f,1.5f,.17f),new Color(.02f,.01f,.025f,.8f));
        Pic("checkpoint",W(x-.75f,ground-.06f,1.5f,2.1f),lit?1:.7f);
        if(lit){Ellipse(W(x-.3f,ground+1.05f,.65f,.75f),new Color(.66f,.18f,.25f,.17f));}
    }
}
