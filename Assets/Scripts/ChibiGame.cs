using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

// Deterministic, self-contained 2D platformer. All gameplay uses world units, independent of resolution.
public partial class ChibiGame : MonoBehaviour
{
    public const float RoadLegDistance = 60f * 5.1f;
    bool compactRoute;
    public const float FullLength = 1920, Speed = 5.1f, Gravity = 20, JumpSpeed = 8.2f;
    public enum Mode { Menu, Story, Options, Play, Pause, Dead, Loading, Ending, Splash }
    [Serializable] public class SaveData { public int version,seed, lives, hp, collected, lifePotions,doctorChoice,badges,pendingPower; public float checkpointY,resumeX,resumeY; public bool bossDefeated,rewardTaken; public bool doctorSpawned,doctorFinished; public float doctorX; public bool catSpawned,catRescued,castleCleared,dungeonCleared; public float catX,checkpoint, elapsed, length; public List<int> taken = new List<int>(); public List<int> cut = new List<int>(); }
    public struct Block { public Rect r; public int type; public Block(float x,float y,float w,float h,int t) { r=new Rect(x,y,w,h);type=t; } }
    public class Vine { public Rect r; public bool cut, hanging; public Vector2 anchor; public float phase; }
    public class Pterodactyl { public float x, baseY, phase, fade; public bool defeated, hit; }
    public List<Block> blocks = new List<Block>();
    public List<Vine> vines = new List<Vine>();
    public List<Pterodactyl> pterodactyls = new List<Pterodactyl>();
    public List<Vector2> nectarBottles = new List<Vector2>();
    public List<Vector2> motes = new List<Vector2>();
    public HashSet<int> taken = new HashSet<int>();
    readonly Dictionary<string,Texture2D> art = new Dictionary<string,Texture2D>();
    readonly string[] chapters = { "Порог сна", "Багряная чаща", "Топи тревоги", "Лес без имени", "Пробуждение" };
    public Mode mode;
    public int seed, lives=3, hp=100, collected;
    public float length=FullLength, px=4, py, vx, vy, checkpoint=4, elapsed;
    float cameraX, still, owlTime=-1, owlX, invuln, attack, attackCooldown, coyote, jumpBuffer, bubble, toastTime, intro, loading, flash, musicClock;
    float accumulator, lastPadVertical; int facing=1; bool grounded, crouch, owlHit, muted, shortRoute, mobileControls;
    string toast=""; Font body,title; Texture2D circle,sky; AudioSource music,dangerMusic,sfx; AudioClip jumpSound,hitSound,deathSound,chime,swish,burp;
    string savePath; bool capture; float captureAt; string capturePath; bool autoRun; int autoStage; float autoClock;
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern int AliceIsMobileDevice();
#endif
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot() { if(FindAnyObjectByType<ChibiGame>()==null) new GameObject("AliceGame").AddComponent<ChibiGame>(); }
    void Awake()
    {
        Application.targetFrameRate=60; QualitySettings.vSyncCount=1;gameObject.AddComponent<YandexBridge>();
#if UNITY_WEBGL && !UNITY_EDITOR
        mobileControls=AliceIsMobileDevice()!=0;
#else
        mobileControls=Application.isMobilePlatform;
#endif
        if(Camera.main==null) { var c=new GameObject("Camera").AddComponent<Camera>();c.tag="MainCamera";c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=Color.black; }
        foreach(var n in new[]{"fairy_idle","fairy_walk1","fairy_walk2","fairy_jump","fairy_crouch","fairy_attack","fairy_hurt","fairy_sleep","forest","flora","earth","peach","mint","home","vine","owl","lantern","splash"}) art[n]=Resources.Load<Texture2D>("Art/"+n);
        for(int i=0;i<8;i++)art["alice_crouch"+i]=Resources.Load<Texture2D>("Art/alice_crouch"+i);
        body=Resources.Load<Font>("Fonts/Pixel");title=body;
        MakePixelEnemies();LoadDreamArt();LoadCastleArt();LoadDungeonArt();LoadDoctorArt();foreach(var n in new[]{"cat0","cat1","cat2","cat_jump","checkpoint","life_full","life_empty"})art[n]=Resources.Load<Texture2D>("Art/"+n);
        circle=new Texture2D(64,64,TextureFormat.RGBA32,false);var cc=new Color[4096];for(int y=0;y<64;y++)for(int x=0;x<64;x++)cc[y*64+x]=new Color(1,1,1,Mathf.Clamp01((31-Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f)))*.8f));circle.SetPixels(cc);circle.Apply();
        sky=new Texture2D(1,256);for(int y=0;y<256;y++)sky.SetPixel(0,y,Color.Lerp(C("#c4a1c5"),C("#292441"),y/255f));sky.Apply();
        music=gameObject.AddComponent<AudioSource>();dangerMusic=gameObject.AddComponent<AudioSource>();sfx=gameObject.AddComponent<AudioSource>();music.loop=dangerMusic.loop=true;music.volume=.32f;dangerMusic.volume=0;sfx.volume=.32f;
        music.clip=Resources.Load<AudioClip>("Audio/MainTheme");dangerMusic.clip=Resources.Load<AudioClip>("Audio/DemonChase");if(music.clip)music.Play();demonVoice=Resources.Load<AudioClip>("Audio/AnxietyDemon");jumpSound=Resources.Load<AudioClip>("Audio/BranchCrunch");deathSound=Resources.Load<AudioClip>("Audio/MonsterDefeat");hitSound=Tone(180,65,.24f);chime=Tone(740,1100,.22f);swish=Tone(300,110,.12f);burp=Tone(130,70,.15f);
        // Keep saves beside the standalone on the project drive: C: may be full.
        savePath=Path.Combine(Application.persistentDataPath,"Alice.save.json");
        var args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length;i++) { if(args[i]=="--capture"&&i+1<args.Length){capture=true;capturePath=args[++i];captureAt=3;} if(args[i]=="--autotest")autoRun=true; if(args[i]=="--mobile-test"){mobileControls=true;Screen.SetResolution(1640,720,FullScreenMode.Windowed);} }
        if(capture||autoRun)savePath=Path.Combine(Path.GetTempPath(),"Chibi-runtime-check.save.json");if(autoRun)Application.runInBackground=true;
        Debug.Log("ALICE_VERSION "+Application.version+" input=separate-keyboard-gamepad");
        LoadPreferences();mode=Mode.Splash; Generate(1987,FullLength); cameraX=0;
        if(capture||autoRun){NewGame(1987,FullLength);intro=0;Debug.Log("CHIBI_V2 obstacles="+blocks.FindAll(b=>b.type!=0).Count+" vines="+vines.Count+" musicSeconds="+(music.clip?music.clip.length:0)+" chaseSeconds="+(dangerMusic.clip?dangerMusic.clip.length:0));}
    }
    public void Generate(int mapSeed,float mapLength)
    {
        seed=mapSeed;compactRoute=mapLength>=1700&&mapLength<=2000;length=compactRoute?FullLength:mapLength;shortRoute=mapLength<1700;blocks.Clear();vines.Clear();pterodactyls.Clear();nectarBottles.Clear();motes.Clear();taken.Clear();
        var r=new System.Random(seed);float groundStart=-15;
        float cursor=14;int obstacleIndex=0;
        while(cursor<length-36)
        {
            float x=cursor+(float)r.NextDouble()*.8f;
            bool safe=Mathf.Abs(x-Mathf.Round(x/320)*320)<9;
            if(safe){cursor+=7+(float)r.NextDouble()*5;obstacleIndex++;continue;}
            int type=obstacleIndex<3?obstacleIndex%2:r.Next(9);
            if(type==0) { float h=.7f+(float)r.NextDouble()*.25f;blocks.Add(new Block(x,0,1.4f,h,1)); }
            if(type==1) { blocks.Add(new Block(x,1.05f,2.8f,1.2f,2)); }
            if(type==2) { blocks.Add(new Block(x,0,1.5f,.55f,1));blocks.Add(new Block(x+3.5f,1.05f,1.7f,1.2f,2)); }
            if(type==3) { blocks.Add(new Block(x,0,1.1f,.55f,2));blocks.Add(new Block(x+2.7f,0,1.1f,.9f,1)); }
            if(type==4) { blocks.Add(new Block(x,1.05f,2.6f,1.2f,2));blocks.Add(new Block(x+4.7f,0,1.2f,.7f,1)); }
            if(type==5) { blocks.Add(new Block(x,0,1.3f,.4f,1));blocks.Add(new Block(x+1.3f,0,1.3f,.8f,2));blocks.Add(new Block(x+4.6f,0,1.1f,.55f,1)); }
            if(type==6) { blocks.Add(new Block(x,0,1.4f,.65f,3));blocks.Add(new Block(x+3.6f,0,1.1f,.85f,4)); }
            if(type==7) { blocks.Add(new Block(x,1.05f,1.2f,1.2f,4));blocks.Add(new Block(x+2,1.05f,1.2f,1.2f,4)); }
            if(type==8) { blocks.Add(new Block(x,0,.8f,.45f,4));blocks.Add(new Block(x+1.8f,0,.8f,.7f,4));blocks.Add(new Block(x+4.2f,0,1.2f,.5f,3)); }
            motes.Add(new Vector2(x+.7f,type==1||type==4?.55f:2.1f));
            if(type>=3)motes.Add(new Vector2(x+3.5f,2.2f));
            cursor+=6.15f+(float)r.NextDouble()*6.35f;
            if(r.NextDouble()<.075)cursor+=5+(float)r.NextDouble()*7;
            obstacleIndex++;
        }
        blocks.Add(new Block(groundStart,-4,length+30-groundStart,4,0));
        GenerateCastle(r);if(compactRoute){length=castleEnd+RoadLegDistance*2+166;blocks.RemoveAll(b=>b.type>0&&b.type<5&&b.r.xMin>length-36);}GenerateDungeon(r);EnsureForestClearance(blocks);
        // Attach vines to existing isolated obstacles. Never add emergency blocks:
        // a lower block underneath an overhang could otherwise make an impassable tunnel.
        var hosts=blocks.FindAll(b=>b.type!=0 && (b.r.y>0 || b.r.width>1));
        hosts.Sort((a,b)=>a.r.x.CompareTo(b.r.x));
        for(int i=0;i<9;i++)
        {
            var candidates=i%2==0?hosts.FindAll(b=>b.r.y>0):hosts;
            var host=candidates[Mathf.Clamp((int)((i+.5f)*candidates.Count/9),0,candidates.Count-1)].r;
            vines.Add(new Vine{r=host.y>0?new Rect(host.center.x-.22f,.12f,.44f,.44f):new Rect(host.x-.04f,host.yMax,host.width+.08f,.52f),hanging=host.y>0,anchor=new Vector2(host.center.x,host.y),phase=i*1.7f});
        }
        // The green pterodactyls patrol open gaps. They dive only when the fairy reaches their territory.
        for(float x=64+(float)r.NextDouble()*18;x<length-40;x+=52+(float)r.NextDouble()*31)
        {
            pterodactyls.Add(new Pterodactyl{x=x,baseY=3.2f+(float)r.NextDouble()*1.3f,phase=(float)r.NextDouble()*6.28f});
            if(r.NextDouble()>.42) pterodactyls.Add(new Pterodactyl{x=x+2.4f+(float)r.NextDouble()*2.2f,baseY=4.3f+(float)r.NextDouble()*1.1f,phase=(float)r.NextDouble()*6.28f});
        }
        for(float x=55+(float)r.NextDouble()*20;x<length-30;x+=145+(float)r.NextDouble()*55)
            nectarBottles.Add(new Vector2(x,.28f));
        blocks.Sort((a,b)=>a.r.x.CompareTo(b.r.x));
        GenerateDream();PlaceRoadEvents();
    }
    public void NewGame(int s,float len) {Generate(s,len);ResetExpedition();ResetDoctor();PlaceRoadEvents();cameraY=0;px=checkpoint=safeX=4;py=safeY=checkpointY=saveClock=0;vx=vy=0;lives=3;hp=100;elapsed=0;collected=0;cameraX=0;still=0;owlTime=-1;invuln=2;intro=0;toastTime=0;mode=Mode.Play;Save();}
    int NextDreamSeed(){int next;do next=Guid.NewGuid().GetHashCode()^Environment.TickCount^unchecked(seed*486187739);while(next==seed||next==1987);return next;}
    void Fresh() {tutorial=false;NewGame(NextDreamSeed(),shortRoute?1100:FullLength);mode=Mode.Story;intro=0;storyClock=0;}
    void Save()
    {
        if(tutorial)return;
        CaptureSafePosition();
        try { var v=new SaveData{version=2,resumeX=safeX,resumeY=safeY,checkpointY=checkpointY,bossDefeated=dungeonBossDefeated,rewardTaken=dungeonRewardTaken,doctorSpawned=doctorSpawned,doctorFinished=doctorFinished,doctorChoice=doctorChoice,doctorX=doctorX,badges=badges,pendingPower=pendingPower,catSpawned=catSpawned,catRescued=catRescued,castleCleared=castleCleared,dungeonCleared=dungeonCleared,catX=catX,lifePotions=lifePotions,seed=seed,lives=lives,hp=hp,collected=collected,checkpoint=checkpoint,elapsed=elapsed,length=length,taken=new List<int>(taken)};for(int i=0;i<vines.Count;i++)if(vines[i].cut)v.cut.Add(i);StoreSave(JsonUtility.ToJson(v,true)); } catch(Exception e){Debug.LogWarning("Save unavailable: "+e.Message);}
    }
    void Continue()
    {
        try {var v=ReadValidatedSave();if(v.lives<1||v.length<100)throw new Exception("Invalid save");ResetDoctor();ResetExpedition();Generate(v.seed,v.length);doctorSpawned=true;doctorFinished=v.doctorFinished;doctorChoice=v.doctorChoice;doctorX=v.doctorX>0?v.doctorX:doctorX;badges=v.badges;pendingPower=v.pendingPower;doctorDialogue=false;cameraY=0;catSpawned=true;catRescued=v.catRescued;castleCleared=v.castleCleared;dungeonCleared=v.dungeonCleared;catX=v.catX>0?v.catX:catX;lifePotions=Mathf.Max(0,v.lifePotions);inventoryOpen=false;catFollowX=v.checkpoint-1;lives=v.lives;hp=v.hp;collected=v.collected;elapsed=v.elapsed;checkpoint=px=v.checkpoint;py=vy=vx=0;cameraX=px-9;taken=new HashSet<int>(v.taken??new List<int>());foreach(int i in v.cut??new List<int>())if(i>=0&&i<vines.Count)vines[i].cut=true;mode=Mode.Play;invuln=2;still=0;owlTime=-1;intro=0;checkpointY=v.checkpointY;
            float resume=v.version>=2?v.resumeX:v.checkpoint;
            if(v.version<2){if(v.catRescued)resume=Mathf.Max(resume,catX);foreach(int item in taken){if(item>=0&&item<motes.Count)resume=Mathf.Max(resume,motes[item].x-2);else if(item>=10000&&item-10000<nectarBottles.Count)resume=Mathf.Max(resume,nectarBottles[item-10000].x-2);}}
            resume=Mathf.Clamp(resume,4,length-3);float resumeY=v.version>=2?v.resumeY:GroundAt(resume);RestorePosition(resume,resumeY);
            if(px>=dungeonEntrance&&px<DungeonEnd&&dungeonEntrance>0){EnterDungeon();dungeonBossDefeated=v.bossDefeated||v.dungeonCleared;dungeonRewardTaken=v.rewardTaken||v.dungeonCleared;}
            Notify("Путь восстановлен. С возвращением!");}catch(System.Exception e){Debug.LogWarning("Save load failed: "+e.Message);mode=Mode.Menu;Notify("Не удалось прочитать сохранение. Оно сохранено для восстановления.");}
    }
    void Notify(string s){toast=s;toastTime=4;}
    bool Held(KeyCode a,KeyCode b){return Input.GetKey(a)||Input.GetKey(b);}
    bool Down(KeyCode a,KeyCode b){return Input.GetKeyDown(a)||Input.GetKeyDown(b);}
    // Conventional horizontal controls for Alice. Keyboard is kept separate from
    // Unity's default Horizontal axis, which otherwise also contains A/D and cancels it out.
    public static float MovementIntent(bool leftKey,bool rightKey,float stick)
    {
        if(leftKey || rightKey) return (rightKey?1f:0f)-(leftKey?1f:0f);
        return Mathf.Abs(stick)<.2f?0f:Mathf.Clamp(stick,-1f,1f);
    }
    void Update()
    {
        float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);UpdateMusicMix(dt);
        if(mode==Mode.Splash){if(Input.anyKeyDown||Input.touchCount>0)mode=Mode.Menu;return;}
        if(doctorDialogue)return;
        if(badgeNotice>0){badgeNotice=Mathf.Max(0,badgeNotice-dt);return;}
        if(mode==Mode.Play&&dungeonQuoteTime>0){UpdateDungeon(dt);return;}
        if(Input.GetKeyDown(KeyCode.F11)){prefs.fullscreen=!Screen.fullScreen;ApplyPreferences();SavePreferences();}
        if(Input.GetKeyDown(KeyCode.M)){muted=!muted;AudioListener.volume=muted?0:1;}
        if(Input.GetKeyDown(KeyCode.I))ToggleInventory();
        if(Input.GetKeyDown(KeyCode.Escape)){if(inventoryOpen){ToggleInventory();return;}if(mode==Mode.Options){SavePreferences();mode=optionsReturn;}else if(mode==Mode.Play){mode=Mode.Pause;Save();}else if(mode==Mode.Pause)mode=Mode.Play;}
        if(mode==Mode.Loading){loading+=dt;if(loading>2.7f)mode=Mode.Ending;return;}
        if(mode==Mode.Ending && autoRun && autoStage==3){autoStage=4;StartCoroutine(FinishSmoke());}
        if(mode==Mode.Story){storyClock+=dt;return;}
        if(mode!=Mode.Play){toastTime=Mathf.Max(0,toastTime-dt);return;}
        if(doctorDialogue){vx=0;jumpBuffer=0;touchInteractPressed=false;return;}
        if(tutorial)TickTutorial(dt);
        elapsed+=dt;intro=Mathf.Max(0,intro-dt);toastTime=Mathf.Max(0,toastTime-dt);flash=Mathf.Max(0,flash-dt);invuln=Mathf.Max(0,invuln-dt);attack=Mathf.Max(0,attack-dt);attackCooldown=Mathf.Max(0,attackCooldown-dt);
        float move=MovementIntent(Held(KeyCode.A,KeyCode.LeftArrow),Held(KeyCode.D,KeyCode.RightArrow),Input.GetAxisRaw("ChibiPadHorizontal"));
        float padVertical=Input.GetAxisRaw("ChibiPadVertical");
        bool duck=Held(KeyCode.S,KeyCode.DownArrow)||padVertical<-.5f||TouchHeld(2);
        bool padJumpPressed=padVertical>.5f && lastPadVertical<=.5f;
        lastPadVertical=padVertical;
        if(Down(KeyCode.W,KeyCode.UpArrow)||Input.GetKeyDown(KeyCode.JoystickButton0)||padJumpPressed)jumpBuffer=.15f;
        if(Down(KeyCode.Space,KeyCode.J)||Input.GetKeyDown(KeyCode.JoystickButton2))Attack();
        if(mobileControls && Mathf.Abs(browserStick.x)>.20f)move=StickMovement(browserStick.x);
        else {if(TouchHeld(0))move=-1;if(TouchHeld(1))move=1;}
        if(autoRun) AutoInput(dt,ref move,ref duck);
        if(DungeonLocksMovement){move=0;duck=false;vx=0;jumpBuffer=0;}
        bool obstructed=OverlapSolid(new Rect(px-.32f,py+.74f,.64f,.84f));crouch=duck||(crouch&&obstructed);AnimateCrouch(dt);
        if(move!=0)facing=move>0?1:-1;
        accumulator+=dt;while(accumulator>=1f/120){Step(1f/120,move);accumulator-=1f/120;}
        if(Mathf.Abs(vx)<.12f&&grounded)still+=dt;else still=0;
        TrackProgress(dt);UpdateDoctor();if(doctorDialogue)return;
        UpdateDream(dt);UpdateCastle(dt);UpdateExpedition(dt,Input.GetKeyDown(KeyCode.E)||touchInteractPressed);touchInteractPressed=false;UpdateDungeon(dt);
        if(!dungeonActive){float next=Mathf.Floor(px/320)*320;if(next>checkpoint&&py>=-.01f){checkpoint=next;checkpointY=CheckpointSurface(next);hp=Mathf.Min(100,hp+20);Save();Notify("Фонарь зажжён • +20 здоровья • путь сохранён");Play(chime);}}
        bubble=5;
        cameraY=Mathf.Lerp(cameraY,dungeonActive?Mathf.Min(0,py):(castleStart>0&&px>=castleStart&&px<=castleEnd?Mathf.Max(0,CheckpointSurface(px)-.4f):0),1-Mathf.Exp(-5*dt));
        cameraX=Mathf.Lerp(cameraX,Mathf.Clamp(px-9,0,length-22),1-Mathf.Exp(-5*dt));
        if(!tutorial&&!dungeonActive&&px>=length-2&&py<1){mode=Mode.Loading;loading=0;try{DeleteSave();}catch{}Play(chime);}
        if(tutorial)px=Mathf.Min(px,60);
        if(capture){captureAt-=dt;if(captureAt<=0){capture=false;ScreenCapture.CaptureScreenshot(capturePath);}}
    }
    public Rect PlayerRect(){return new Rect(px-.32f,py,.64f,crouch?.74f:1.58f);}
    bool OverlapSolid(Rect rect){foreach(var b in ActiveBlocks)if(rect.Overlaps(b.r))return true;return false;}
    public void Step(float dt,float move)
    {
        coyote=grounded?.12f:Mathf.Max(0,coyote-dt);jumpBuffer=Mathf.Max(0,jumpBuffer-dt);
        if(jumpBuffer>0&&coyote>0&&!crouch){vy=JumpSpeed;grounded=false;coyote=0;jumpBuffer=0;Play(jumpSound);}
        vx=Mathf.MoveTowards(vx,move*(crouch?2.9f:ActiveRunSpeed),dt*(grounded?36:22));
        px+=vx*dt;var pr=PlayerRect();foreach(var b in ActiveBlocks)if(pr.Overlaps(b.r)){float rise=b.r.yMax-py;if((dungeonActive||(dungeonEntrance>0&&px>dungeonEntrance-2&&px<DungeonEnd+2))&&rise>=-.001f&&rise<=.36f){var raised=new Rect(px-.32f,b.r.yMax+.005f,.64f,pr.height);if(!OverlapSolid(raised)){py=b.r.yMax+.005f;pr=PlayerRect();continue;}}if(vx>0)px=b.r.xMin-.321f;else if(vx<0)px=b.r.xMax+.321f;vx=0;pr=PlayerRect();}
        px=Mathf.Max(.4f,px);vy-=Gravity*dt;py+=vy*dt;grounded=false;pr=PlayerRect();
        foreach(var b in ActiveBlocks)if(pr.Overlaps(b.r)){if(vy<=0){py=b.r.yMax;grounded=true;}else py=b.r.yMin-pr.height-.001f;vy=0;pr=PlayerRect();}
        if(py< -14){Damage(50,true);if(mode==Mode.Play){RestorePosition(checkpoint,checkpointY);invuln=2;still=0;Notify("Осторожно, пропасть! −50 здоровья");}}
    }
    public void Damage(int amount,bool force=false)
    {
        if(tutorial){if(invuln<=0){invuln=1.4f;flash=.25f;Notify("Учебная площадка: жизни не тратятся");}return;}
        if(mode!=Mode.Play||(!force&&invuln>0))return;hp-=amount;invuln=1.4f;flash=.35f;Play(hitSound);
        if(hp<=0){if(checkpoint>4){lives=3;hp=100;if(dungeonActive)AbortDungeon();RestorePosition(checkpoint,checkpointY);invuln=3;flash=0;still=0;Notify("Фонарь восстановил три жизни");Save();return;}lives--;if(lives<=0){hp=0;mode=Mode.Dead;try{DeleteSave();}catch{}return;}hp=100;if(dungeonActive)AbortDungeon();else{RestorePosition(checkpoint,checkpointY);}still=0;owlTime=-1;invuln=3;Notify("Туман сна… Дымка стала гуще");Save();}
    }
    public void Attack()
    {
        if(attackCooldown>0||doctorDialogue||dungeonQuoteTime>0)return;attack=.28f;attackCooldown=.42f;Play(swish);attackSerial++;currentAttackDamage=1+pendingPower;pendingPower=0;
        Notify("ФЕРЕТИН 18!!!");HitDreamEnemies();HitDungeonBoss();
    }
    void AutoInput(float dt,ref float move,ref bool duck)
    {
        // Built-in executable smoke journey, never enabled during ordinary play.
        autoClock+=dt;move=1;duck=false;
        foreach(var b in ActiveBlocks)if(b.type!=0&&b.r.xMin>px&&b.r.xMin-px<2.4f){if(b.r.yMin>py+.72f)duck=true;else if(b.r.yMax>py+.06f&&grounded)jumpBuffer=.15f;}
        foreach(var b in ActiveBlocks)if(b.type==0&&px>b.r.xMin&&px<b.r.xMax&&b.r.xMax-px<1.4f&&grounded)jumpBuffer=.15f;
        foreach(var v in vines)if(!v.cut&&Mathf.Abs(v.r.center.x-px)<1.7f)Attack();
        if(autoClock>14&&autoStage==0){Debug.Log("CHIBI_SMOKE movement="+px+" hp="+hp);Damage(100,true);autoStage++;}
        if(autoClock>15&&autoStage==1){Damage(100,true);Debug.Log("CHIBI_SMOKE fog lives="+lives);autoStage++;}
        if(autoClock>16&&autoStage==2){if(dungeonActive)LeaveDungeon(true);px=length-1;py=0;Debug.Log("CHIBI_SMOKE finish reached");autoStage++;}
    }
    System.Collections.IEnumerator FinishSmoke()
    {
        yield return new WaitForEndOfFrame();
        string folder=Path.GetDirectoryName(capturePath ?? Path.Combine(Application.dataPath,"ending.png"));
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"ending.png"));
        Debug.Log("CHIBI_SMOKE ending displayed");
        yield return new WaitForSecondsRealtime(1);
        mode=Mode.Splash;
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"splash.png"));
        yield return new WaitForSecondsRealtime(1);
        mode=Mode.Menu;cameraX=0;toastTime=0;
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"menu.png"));
        yield return new WaitForSecondsRealtime(1);
        OpenOptions(Mode.Menu);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"settings.png"));
        yield return new WaitForSecondsRealtime(1);
        StartTutorial(false);tutorialTime=72;TickTutorial(0);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"tutorial.png"));
        yield return new WaitForSecondsRealtime(1);
        tutorialTime=96;TickTutorial(0);px=10;
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"enemies.png"));
        yield return new WaitForSecondsRealtime(1);
        attackCooldown=0;Attack();
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"attack.png"));
        yield return new WaitForSecondsRealtime(1);
        tutorial=false;mode=Mode.Story;storyClock=6;
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"story.png"));
        yield return new WaitForSecondsRealtime(1);
        NewGame(1987,FullLength);elapsed=60;UpdateExpedition(0);px=catX;py=0;cameraX=px-9;UpdateExpedition(0,true);px+=3;vx=Speed;for(int i=0;i<5;i++)UpdateExpedition(.05f);
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"kitten.png"));
        yield return new WaitForSecondsRealtime(1);py=2;grounded=false;catFollowY=1.25f;catGrounded=false;
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"jump-no-shadows.png"));
        yield return new WaitForSecondsRealtime(1);ToggleInventory();
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"inventory.png"));
        if(!UseLifePotion()||lives!=4||lifePotions!=0)throw new Exception("Inventory smoke failed");
        yield return new WaitForSecondsRealtime(1);ToggleInventory();px=320;checkpoint=320;py=0;cameraX=px-9;
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"checkpoint.png"));
        yield return new WaitForSecondsRealtime(1);px=length-8;cameraX=length-19;py=0;
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"tree.png"));
        yield return new WaitForSecondsRealtime(1);NewGame(1987,FullLength);px=castleStart+7;py=0;foreach(var b in blocks)if(b.type>=5&&b.r.y==0&&px>=b.r.xMin&&px<=b.r.xMax)py=Mathf.Max(py,b.r.yMax);cameraX=px-9;
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"castle-ascent.png"));
        yield return new WaitForSecondsRealtime(1);px=castleChaseStart+.2f;py=0;foreach(var b in blocks)if(b.type>=5&&b.r.y==0&&px>=b.r.xMin&&px<=b.r.xMax)py=Mathf.Max(py,b.r.yMax);cameraX=px-9;UpdateCastle(.01f);
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"castle-chase.png"));
        yield return new WaitForSecondsRealtime(1);castleHeadStart=0;UpdateMusicMix(.1f);
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"castle-pursuit.png"));
        yield return new WaitForSecondsRealtime(1);var shownBarrier=blocks.Find(b=>b.type>=5&&b.r.y>0);px=shownBarrier.r.xMin-2.1f;py=shownBarrier.r.y;cameraX=px-9;
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"castle-obstacle.png"));
        yield return new WaitForSecondsRealtime(1);gymDemonX=px-8;castleClock=9.95f;UpdateCastle(.1f);
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"castle-clear.png"));
        yield return new WaitForSecondsRealtime(1);px=castleEnd-8;py=0;foreach(var b in blocks)if(b.type>=5&&b.r.y==0&&px>=b.r.xMin&&px<=b.r.xMax)py=Mathf.Max(py,b.r.yMax);cameraX=px-9;
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"castle-descent.png"));
        yield return new WaitForSecondsRealtime(1);NewGame(1987,FullLength);px=dungeonEntrance-6;py=0;cameraX=px-9;cameraY=0;
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"dungeon-approach.png"));
        yield return new WaitForSecondsRealtime(1);px=DungeonX(20);py=DungeonSurfaceAt(px);EnterDungeon();cameraX=px-9;cameraY=py;
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"dungeon-descent.png"));
        yield return new WaitForSecondsRealtime(1);dungeonDescentTime=0;px=DungeonX(72);py=DungeonSurfaceAt(px)+.02f;cameraX=px-9;cameraY=py;UpdateDungeon(.01f);dungeonFade=0;
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"dungeon-roar.png"));
        yield return new WaitForSecondsRealtime(1);dungeonRoarTime=0;dungeonBossHp=1;dungeonBossX=px+.7f;dungeonBossY=py;attackCooldown=0;Attack();
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"dungeon-defeat.png"));
        yield return new WaitForSecondsRealtime(1);UpdateDungeon(1.2f);px=DungeonX(100);py=DungeonSurfaceAt(px)+.02f;cameraX=px-9;cameraY=py;
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"dungeon-trophy.png"));
        yield return new WaitForSecondsRealtime(1);px=DungeonX(103);py=DungeonSurfaceAt(px)+.02f;cameraX=px-9;cameraY=py;UpdateDungeon(0);
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"dungeon-reward.png"));
        yield return new WaitForSecondsRealtime(1);dungeonQuoteTime=0;px=DungeonX(146);py=DungeonSurfaceAt(px);cameraX=px-9;cameraY=py;
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"dungeon-ascent.png"));
        yield return new WaitForSecondsRealtime(1);NewGame(1987,FullLength);elapsed=35;UpdateDoctor();px=doctorX-1;cameraX=px-9;UpdateDoctor();
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"doctor-dialogue.png"));
        yield return new WaitForSecondsRealtime(1);ChooseDoctorReply(2);
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"doctor-badge.png"));
        yield return new WaitForSecondsRealtime(1);badgeNotice=0;ToggleInventory();
        yield return new WaitForSecondsRealtime(1);UseDoctorBadge();ToggleInventory();checkpoint=320;lives=1;hp=10;Damage(100,true);cameraX=px-9;
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"checkpoint-three-hearts.png"));
        yield return new WaitForSecondsRealtime(1);mode=Mode.Menu;toastTime=0;
        Debug.Log("ALICE_SMOKE PASS inventory=4-lives kitten=rescued castle=escaped dungeon=rewarded");
        yield return new WaitForSecondsRealtime(1);
        Application.Quit();
    }
    void DrawTerrain(){
        foreach(var b in blocks)
        {
            if(b.r.xMax<cameraX-2||b.r.xMin>cameraX+34)continue;
            string a=b.type==0?"earth":b.type==1?"peach":b.type==2?"mint":b.type==5?"castle_stone":b.type==6?"castle_moss":b.type>=7?"castle_stone":"mint";
            if(b.type==0){float bottom=TerrainVisualBottom;Fill(W(b.r.xMin,bottom,b.r.width,-bottom),C("#24151e"));const float tile=8f/3f;int first=Mathf.Max(0,Mathf.FloorToInt((cameraX-2-b.r.xMin)/tile));for(int i=first;;i++){float x=b.r.xMin+i*tile;if(x>=b.r.xMax||x>=cameraX+34)break;for(float y=Mathf.Floor(bottom/4)*4;y<0;y+=4)Pic(a,W(x,y,Mathf.Min(tile,b.r.xMax-x),Mathf.Min(4,-y)));}}
            else {var obstacle=W(b.r.x,b.r.y,b.r.width,b.r.height);if(b.type>=5)DrawCastleBlock(b,a);else Pic(a,obstacle);
            }
        }
        const float plantSpacing=3.55f;
        for(float fx=Mathf.Floor(cameraX/plantSpacing)*plantSpacing;fx<cameraX+34;fx+=plantSpacing)
        {
            bool onGround=false;foreach(var g in blocks)if(g.type==0&&fx>g.r.xMin+.4f&&fx<g.r.xMax-1){onGround=true;break;}
            if(!onGround)continue;
            int plantHash=Mathf.Abs(Mathf.RoundToInt(fx*17)+seed*31);string plant=plantHash%7==0?"flora":"flora"+(2+plantHash%5);
            float scale=.88f+(plantHash%5)*.07f;Pic(plant,W(fx+(plantHash%3)*.13f,-.06f,1.05f*scale,1.0f*scale),.92f);
        }
    }
    public static int ActorFacing(float delta,int previous)=>Mathf.Abs(delta)>.00001f?(delta>0?1:-1):previous;
    void DrawTrophy(string key,Vector2 center,float diameter,float iconSize){
        Ellipse(new Rect(center.x-diameter/2,center.y-diameter/2,diameter,diameter),new Color(1f,.7f,.18f,.32f));
        if(!art.TryGetValue(key,out var tex)||!tex)return;
        float scale=iconSize/Mathf.Max(tex.width,tex.height),w=tex.width*scale,h=tex.height*scale;
        Pic(key,new Rect(center.x-w/2,center.y-h/2,w,h));
    }
    void DrawSolidActor(Texture2D tex,Rect r,int direction){
        if(!tex)return;var matrix=GUI.matrix;GUI.color=Color.white;
        if(direction<0)GUIUtility.ScaleAroundPivot(new Vector2(-1,1),r.center);
        GUI.DrawTexture(r,tex);GUI.matrix=matrix;GUI.color=Color.white;
    }
    void Play(AudioClip clip){if(sfx&&clip)sfx.PlayOneShot(clip);}
    void PlayDeath(){Play(deathSound);Debug.Log("ALICE_SOUND monster-defeat");}
    AudioClip Tone(float a,float b,float seconds)
    {
        int n=(int)(22050*seconds);var data=new float[n];float phase=0;for(int i=0;i<n;i++){float t=i/(float)n;phase+=Mathf.Lerp(a,b,t)*2*Mathf.PI/22050;data[i]=Mathf.Sin(phase)*Mathf.Sin(t*Mathf.PI)*.3f;}var clip=AudioClip.Create("Chibi tone",n,1,22050,false);clip.SetData(data,0);return clip;
    }
    static Color C(string s){ColorUtility.TryParseHtmlString(s,out var c);return c;}
    void Fill(Rect r,Color c){GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
    void Ellipse(Rect r,Color c){GUI.color=c;GUI.DrawTexture(r,circle);GUI.color=Color.white;}
    void Pic(string n,Rect r,float alpha=1){GUI.color=new Color(1,1,1,alpha);if(art.TryGetValue(n,out var t)&&t){if(n=="earth"||n=="castle_stone"||n=="castle_moss")t.filterMode=mobileControls?FilterMode.Bilinear:FilterMode.Point;GUI.DrawTexture(r,t);}GUI.color=Color.white;}
    void Text(string s,float x,float y,float w,float h,int size,Color color,bool serif=false,TextAnchor align=TextAnchor.UpperLeft)
    {
        var style=new GUIStyle{font=serif?title:body,fontSize=size,normal={textColor=color},alignment=align,wordWrap=true};
        var shadow=new GUIStyle(style);shadow.normal.textColor=new Color(0,0,0,color.a);int edge=size>=40?4:size>=18?2:1;
        for(int dy=-edge;dy<=edge;dy++)for(int dx=-edge;dx<=edge;dx++)if((dx!=0||dy!=0)&&dx*dx+dy*dy<=edge*edge+1)GUI.Label(new Rect(x+dx,y+dy,w,h),s,shadow);
        GUI.Label(new Rect(x,y,w,h),s,style);
    }
    bool Button(string s,float x,float y,float w=300,bool primary=false)
    {
        Rect r=new Rect(x,y,w,50);bool hover=r.Contains(Event.current.mousePosition);Wood(r,!hover);
        float inner=w*.70f;int fs=22;var measure=new GUIStyle{font=body,fontSize=fs};while(fs>12&&measure.CalcSize(new GUIContent(s)).x>inner){fs--;measure.fontSize=fs;}
        Text(s,x+(w-inner)/2,y,inner,50,fs,C("#efd3d4"),false,TextAnchor.MiddleCenter);return GUI.Button(r,GUIContent.none,GUIStyle.none);
    }
    Rect W(float x,float y,float w,float h){float zoom=MobileGameplay?1.38f:1;float scale=48*zoom;float anchor=MobileGameplay?432*(1-zoom):0;float pixel=Mathf.Max(Screen.width/1280f,Screen.height/720f);float left=Mathf.Round(((x-cameraX)*scale+anchor)*pixel)/pixel,right=Mathf.Round(((x+w-cameraX)*scale+anchor)*pixel)/pixel,top=Mathf.Round((550-(y+h-cameraY)*scale)*pixel)/pixel,bottom=Mathf.Round((550-(y-cameraY)*scale)*pixel)/pixel;return Rect.MinMaxRect(left,top,right,bottom);}
    void OnGUI()
    {
        GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/720f,1));
        if(mode==Mode.Splash){GUI.matrix=Matrix4x4.identity;if(art.TryGetValue("splash",out var cover)&&cover)GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),cover,ScaleMode.ScaleAndCrop);return;}
        GUI.matrix=MobileWorldMatrix;
        DrawWorld(mode==Mode.Menu||mode==Mode.Story||mode==Mode.Options||mode==Mode.Ending);
        GUI.matrix=MobileInterfaceMatrix;
        if((mode==Mode.Play||mode==Mode.Dead)&&!HasLargeWindow) DrawHUD();
        if(tutorial&&mode==Mode.Play)DrawTutorial();
        if(mode==Mode.Menu) DrawMenu();
        if(mode==Mode.Story) DrawStory();
        if(mode==Mode.Options) DrawOptions();
        if(mode==Mode.Pause&&!inventoryOpen){Wood(new Rect(280,80,720,530));Text("Тихая передышка",340,145,600,65,42,C("#f7dbcd"),true,TextAnchor.MiddleCenter);Text("Шизолесье замерло. Твоё место на тропе сохранено.",320,220,640,45,19,C("#c9c0d7"),false,TextAnchor.MiddleCenter);if(Button("Вернуться на тропу",490,290,300,true))mode=Mode.Play;if(Button("Настройки",490,355))OpenOptions(Mode.Pause);if(Button("Новая случайная карта",490,420))Fresh();if(Button("В главное меню",490,485)){Save();tutorial=false;mode=Mode.Menu;cameraX=0;toastTime=0;}}
        if(mode==Mode.Dead){ScreenTint(new Color(.1f,.08f,.16f,.85f));Text("Сон ещё не отпускает",290,180,700,75,40,C("#f7d7d7"),true,TextAnchor.MiddleCenter);Text("Все жизни потеряны. Новая попытка — новая тропа.",310,265,660,60,20,C("#c6bed3"),false,TextAnchor.MiddleCenter);if(Button("Попробовать ещё раз",475,365,330,true))Fresh();if(Button("Главное меню",475,430,330)){mode=Mode.Menu;cameraX=0;toastTime=0;}}
        if(mode==Mode.Loading){ScreenTint(new Color(1,.94f,.85f,Mathf.Clamp01(loading/2.7f)));Text("Дом уже близко…",340,285,600,90,38,C("#f5d1cc"),true,TextAnchor.MiddleCenter);Fill(new Rect(490,405,300,3),C("#51455e"));Fill(new Rect(490,405,300*Mathf.Clamp01(loading/2.7f),3),C("#f0cba9"));}
        if(mode==Mode.Ending)DrawEnding();
        DrawScreenNotices();
        if(mode==Mode.Play)DrawTouch();if(inventoryOpen)DrawInventory();if(doctorDialogue)DrawDoctorDialogue();if(badgeNotice>0)DrawBadgeReward();
    }
    void DrawForestBackground(bool decorative)
    {
        float t=Time.unscaledTime;
        float panorama=(decorative?t*2:cameraX*5)%1280;
        int page=Mathf.FloorToInt((decorative?t*2:cameraX*5)/1280);
        for(int i=0;i<2;i++)
        {
            Rect bg=new Rect(i*1280-panorama,0,1280,720);
            if((page+i)%2!=0){var matrix=GUI.matrix;GUIUtility.ScaleAroundPivot(new Vector2(-1,1),bg.center);Pic("forest",bg);GUI.matrix=matrix;}
            else Pic("forest",bg);
        }
        ScreenTint(new Color(.10f,.06f,.20f,decorative?.13f:.23f));
    }
    void DrawWorld(bool decorative)
    {
        if(!decorative&&dungeonEntrance>0&&cameraX+34>dungeonEntrance&&cameraX<DungeonEnd){DrawDungeonWorld();return;}
        float t=Time.unscaledTime;DrawForestBackground(decorative);
        if(decorative)
        {
            for(int i=0;i<11;i++)Pic("earth",new Rect(i*128,630,128,128));
            Pic("home",new Rect(635,45,625,625));
            Pic(mode==Mode.Ending?"fairy_sleep":"fairy_idle",mode==Mode.Ending?new Rect(855,526,104,104):new Rect(855,510,120,120));
            return;
        }
        DrawCastleBackdrop();
        DrawTerrain();
        DrawDungeonEntrance();
        for(float x=Mathf.Floor(cameraX/320)*320;x<cameraX+35;x+=320)if(x>0)DrawCheckpoint(x);
        
        DrawDreamWorld();DrawCastleActors();DrawCompanion();DrawDoctor();
        if(length-cameraX<40){Ellipse(W(length-3.7f,-.12f,7.4f,.25f),new Color(.015f,.01f,.015f,.8f));Pic("home",W(length-5,-.65f,10,10));}
        if(mode!=Mode.Loading)DrawAlice(invuln>0&&Mathf.Sin(t*25)<0?.45f:1);
        for(int i=0;i<(prefs.particles?22:0);i++){float x=(i*83+t*(4+i%4))%1280;float y=330+(i*37)%280+Mathf.Sin(t*.7f+i)*15;Ellipse(new Rect(x,y,3,3),new Color(1,.85f,.7f,.4f));}
        if(lives<3){float a=(3-lives)*.115f;ScreenTint(new Color(.62f,.63f,.68f,a));for(int i=0;i<6;i++)Ellipse(new Rect((i*283+t*12)%1650-350,190+Mathf.Sin(t*.25f+i)*130,650,220),new Color(.7f,.7f,.74f,a*.27f));}
        if(flash>0)ScreenTint(new Color(.85f,.29f,.43f,flash*.5f));
    }
    void DrawHUD(){DrawMobileHUD();}
    void DrawMenu()
    {
        // Menu sits directly over the forest, with no letterboxed dark overlay.
        DrawSign();
        if(Button("Новая игра",200,450,208,true))Fresh();
        GUI.enabled=HasSave();
        if(Button("Продолжить",424,450,208)) {tutorial=false;Continue();}
        GUI.enabled=true;
        if(Button("Настройки",648,450,208))OpenOptions(Mode.Menu);
        if(Button("Об игре",872,450,208))Notify("Алиса в шизолесье • пиксельный сон");
        if(toastTime>0)Text(toast,300,605,680,40,22,C("#dfbbc3"),false,TextAnchor.MiddleCenter);
        if(Button("Обучение",500,550,280))StartTutorial(false);
        Text("1.7.4",1150,670,100,28,16,C("#dfb5c1"),false,TextAnchor.MiddleRight);
    }
    void DrawStory()
    {
        ScreenTint(new Color(.025f,.015f,.025f,Mathf.Clamp01(1-storyClock/7)));
        if(storyClock<5){Text("Алиса, проснись!",180,290,920,110,65,new Color(.9f,.78f,.8f,Mathf.Clamp01((5-storyClock)/3)),true,TextAnchor.MiddleCenter);return;}
        Wood(new Rect(170,150,940,440));
        Text("Шизолесье",215,180,840,80,54,C("#efd3d4"),true,TextAnchor.MiddleCenter);
        Text("Алисе нужно внутри собственного сна добраться\nчерез шизолесье к себе домой, и только тогда\nона сможет проснуться…",220,290,840,160,30,C("#dfbcc4"),false,TextAnchor.MiddleCenter);
        if(Button("Войти в сон • обучение 2 минуты",340,500,600,true))StartTutorial(true);
    }
    void DrawOptions()
    {
        
        Wood(new Rect(130,48,1020,626));
        Color ink=C("#efd3d4");
        Text("Настройки",300,76,680,60,40,ink,true,TextAnchor.MiddleCenter);
        Text("Графика",169,150,390,36,28,ink);
        GUI.enabled=Application.platform!=RuntimePlatform.WebGLPlayer;
        if(Button(prefs.fullscreen?"Полный экран":"Оконный режим",169,201,412)){prefs.fullscreen=!prefs.fullscreen;ApplyPreferences();}
        var r=resolutions[prefs.resolution];
        if(Button("Разрешение: "+r.x+" x "+r.y,169,262,412)){prefs.resolution=(prefs.resolution+1)%resolutions.Length;ApplyPreferences();}
        if(Button(prefs.vsync?"VSync: включён":"VSync: выключен",169,323,412)){prefs.vsync=!prefs.vsync;ApplyPreferences();}
        GUI.enabled=true;
        if(Button(prefs.particles?"Частицы: включены":"Частицы: выключены",169,384,412))prefs.particles=!prefs.particles;
        Text("Музыка",179,455,140,32,22,ink);
        prefs.volume=GUI.HorizontalSlider(new Rect(326,469,234,20),prefs.volume,0,.6f);music.volume=prefs.volume;
        if(Button(muted?"Звук: выключен":"Звук: включён",169,509,412)){muted=!muted;AudioListener.volume=muted?0:1;}
        Text("Управление",633,150,450,36,28,ink);
        string[] keys=mobileControls?new[]{"Джойстик ←","Джойстик →","Джойстик ↓","Джойстик ↑","Атака","Инвент.","Взять РА","Пауза"}:new[]{"A / влево","D / вправо","W / вверх","S / вниз","Пробел / J","I","E","Esc","F11 / M"};
        string[] actions=mobileControls?new[]{"Идти влево","Идти вправо","Пригнуться","Прыжок","Удар анализами","Инвентарь","Забрать котёнка","Пауза"}:new[]{"Идти влево","Идти вправо","Прыжок","Пригнуться","Удар анализами","Инвентарь","Забрать котёнка","Пауза","Экран / звук"};
        for(int i=0;i<keys.Length;i++){Text(keys[i],633,203+i*35,185,35,22,ink);Text(actions[i],839,203+i*35,270,35,22,ink);}
        Text(mobileControls?"Сенсорное управление включено":"Обычное управление • Пробел — атака",633,533,440,50,20,ink);
        if(Button("Сохранить и назад",440,600,400,true)){SavePreferences();mode=optionsReturn;}
    }
    void DrawEnding()
    {
        ScreenTint(new Color(.98f,.91f,.80f,.35f));
        Wood(new Rect(70,130,690,490));
        Text("Алиса проснулась, сон прошел, а я благодарю за внимание.",145,210,540,100,30,C("#f3d8db"),true);
        Text("Игра была создана в подарок девушке блогеру \"Алиса в Шизолесье\" вдохновившись ее эстетикой.",145,330,540,120,25,C("#e4bbc4"));
        Text($"Путь: {(int)elapsed/60} мин • Эликсиры: {collected}",145,465,540,35,20,C("#c99da9"));
        if(Button("Новый сон",110,530,240))Fresh();if(Button("Меню",380,530,240)){mode=Mode.Menu;cameraX=0;toastTime=0;}
    }
    void OnApplicationFocus(bool focused)
    {
        if(!focused && mode==Mode.Play && !autoRun){mode=Mode.Pause;vx=0;jumpBuffer=0;lastPadVertical=0;Save();}
    }
    void OnApplicationQuit(){if(mode==Mode.Play||mode==Mode.Pause)Save();}
}


