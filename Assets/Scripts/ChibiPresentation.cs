using System;
using System.IO;
using UnityEngine;

public partial class ChibiGame
{
    [Serializable] class Preferences { public int resolution=1; public bool fullscreen=false, vsync=true, particles=true; public float volume=.32f; }
    Preferences prefs=new Preferences();
    Mode optionsReturn=Mode.Menu;
    readonly Vector2Int[] resolutions={new Vector2Int(960,540),new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)};
    Texture2D[] enemyFrames;
    bool tutorial, tutorialStartsGame;
    float tutorialTime;
    int tutorialRoom;
    public static Rect SwingHitbox(Vector2 anchor,float phase)
    {
        float angle=Mathf.Sin(phase)*.92f;
        Vector2 tip=anchor+new Vector2(Mathf.Sin(angle),-Mathf.Cos(angle))*.77f;
        return new Rect(tip.x-.22f,tip.y-.22f,.44f,.44f);
    }
    void UpdateVines(float dt)
    {
        foreach(var v in vines)if(v.hanging&&!v.cut){v.phase+=dt*2.25f;v.r=SwingHitbox(v.anchor,v.phase);}
    }
    void Wood(Rect r,bool light=true)
    {
        GothicPanel(r,light);
    }
    void DrawSign()
    {
        Wood(new Rect(180,90,920,320));
        Text("АЛИСА",220,130,840,100,76,C("#edc8cf"),true,TextAnchor.MiddleCenter);
        Text("В ШИЗОЛЕСЬЕ",220,235,840,80,54,C("#e2a3b0"),true,TextAnchor.MiddleCenter);
        Text("Найди дорогу из собственного сна",220,314,840,35,20,C("#cbb0b6"),false,TextAnchor.MiddleCenter);
    }
    void OpenOptions(Mode back){optionsReturn=back;mode=Mode.Options;}
    string PrefPath=>Path.Combine(autoRun?Path.GetTempPath():Application.persistentDataPath,"Alice.settings.json");
    void LoadPreferences()
    {
        try{if(PlayerPrefs.HasKey("alice.preferences"))prefs=JsonUtility.FromJson<Preferences>(PlayerPrefs.GetString("alice.preferences"))??new Preferences();}catch{prefs=new Preferences();}
        prefs.resolution=Mathf.Clamp(prefs.resolution,0,resolutions.Length-1);ApplyPreferences();
    }
    void ApplyPreferences()
    {
        var r=resolutions[prefs.resolution];if(Application.platform!=RuntimePlatform.WebGLPlayer)Screen.SetResolution(r.x,r.y,prefs.fullscreen?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed);
        QualitySettings.vSyncCount=prefs.vsync?1:0;Application.targetFrameRate=60;music.volume=Mathf.Clamp(prefs.volume,0,.6f);if(dangerMusic)dangerMusic.volume=0;
    }
    void SavePreferences(){try{PlayerPrefs.SetString("alice.preferences",JsonUtility.ToJson(prefs));PlayerPrefs.Save();}catch(Exception e){Debug.LogWarning(e.Message);}}
    void StartTutorial(bool startGame)
    {
        ResetExpedition();tutorial=true;tutorialStartsGame=startGame;tutorialTime=0;tutorialRoom=-1;mode=Mode.Play;
        lives=3;hp=100;elapsed=0;collected=0;length=70;intro=0;cameraX=0;toastTime=0;toast="";flash=0;accumulator=0;lastPadVertical=0;TickTutorial(0);
    }
    void FinishTutorial()
    {
        tutorial=false;
        if(tutorialStartsGame)NewGame(unchecked(Environment.TickCount*397),shortRoute?1100:FullLength);
        else{mode=Mode.Menu;Generate(1987,FullLength);cameraX=0;}
    }
    void TickTutorial(float dt)
    {
        tutorialTime+=dt;
        if(tutorialTime>=120){FinishTutorial();return;}
        int room=Mathf.Min(4,(int)(tutorialTime/24));
        if(room==tutorialRoom)return;tutorialRoom=room;
        bugs.Clear();mires.Clear();blocks.Clear();vines.Clear();pterodactyls.Clear();motes.Clear();nectarBottles.Clear();taken.Clear();
        blocks.Add(new Block(-20,-4,110,4,0));px=checkpoint=4;py=vx=vy=0;cameraX=0;still=0;owlTime=-1;invuln=2;attack=attackCooldown=0;
        for(int i=0;i<5;i++)
        {
            float x=12+i*8;
            if(room==1)blocks.Add(new Block(x,0,1.3f,.8f,1));
            if(room==2)blocks.Add(new Block(x,1.05f,2.6f,1.2f,2));
            if(room==3){blocks.Add(new Block(x,1.05f,2.6f,1.2f,2));var anchor=new Vector2(x+1,1.05f);vines.Add(new Vine{hanging=true,anchor=anchor,r=SwingHitbox(anchor,0),phase=i});}
            if(room==4)bugs.Add(new DreamEnemy{x=x,y=0,hp=i==0?4:1,demon=i==0});
            motes.Add(new Vector2(x+2,room==1?2.2f:.6f));
            if(room==3)mires.Add(new Rect(x,.88f,2.6f,.17f));
        }
    }
    void DrawTutorial()
    {
        string[] names={"1 / 5  Шаги во сне","2 / 5  Прыжок","3 / 5  Низкие проходы","4 / 5  Трясина","5 / 5  Демон тревожности"};
        string[] help={"A / влево — налево. D / вправо — направо.\nСобирай эликсиры: каждые три винных дают +10 здоровья.","W / стрелка вверх — ПРЫЖОК.\nУдерживай D, чтобы перепрыгнуть блоки.","S / стрелка вниз — ПРИСЕСТЬ. Удерживай S и D.\nПройди под верхними блоками, затем отпусти S.","Трясина цепляет и затягивает: не задерживайся!\nПерепрыгивай нижнюю и пригибайся под верхней.","ПРОБЕЛ — удар анализами. Жучку достаточно одного.\nДемону нужны четыре удара. Отступай между атаками."};
        if(mobileControls){help[0]="Джойстик влево / вправо — движение.\nСобирай эликсиры: три винных дают +10 здоровья.";help[1]="Джойстик вверх — ПРЫЖОК.\nВверх и вправо вместе — прыжок вперёд.";help[2]="Джойстик вниз — ПРИСЕСТЬ.\nВниз и вправо — пройти под блоком.";help[4]="Кнопка АТАКА справа — удар анализами.\nЖучку нужен один удар, демону — четыре.";}
        Wood(new Rect(250,150,780,190));Text(names[tutorialRoom],320,183,640,38,25,C("#efd3d4"));
        Text(help[tutorialRoom],320,230,640,70,21,C("#dfb9c4"));
        Text("ОБУЧЕНИЕ  "+Mathf.CeilToInt(120-tutorialTime)+" сек",490,347,310,35,22,C("#efd3d4"),false,TextAnchor.MiddleCenter);
        if(Button("Завершить обучение",480,610,320))FinishTutorial();
    }
    void MakePixelEnemies()
    {
        enemyFrames=new Texture2D[8];
        for(int frame=0;frame<8;frame++)
        {
            var tex=new Texture2D(48,32,TextureFormat.RGBA32,false);tex.filterMode=FilterMode.Point;tex.SetPixels(new Color[48*32]);
            Action<int,int,Color> dot=(x,y,c)=>{if(x>=0&&x<48&&y>=0&&y<32)tex.SetPixel(x,y,c);};
            Color outline=C("#293a49"),green=C("#64b976"),light=C("#b9e899"),wing=C("#479d98");
            int flap=new[]{25,28,25,20,12,8,12,20}[frame];
            for(int x=3;x<42;x++)
            {
                int distance=Math.Abs(x-23);int top=(int)Mathf.Lerp(18,flap,distance/20f);int bottom=16-distance/5;
                for(int y=Math.Min(top,bottom);y<=Math.Max(top,bottom);y++)dot(x,y,(y==top||y==bottom)?outline:(x%5==0?green:wing));
            }
            for(int y=10;y<=23;y++)for(int x=19;x<=28;x++)if((x-24)*(x-24)/30f+(y-17)*(y-17)/55f<1)dot(x,y,x<22?outline:green);
            for(int y=20;y<27;y++)for(int x=25;x<34;x++)dot(x,y,y==20||y==26||x==25?outline:light);
            for(int x=33;x<43;x++)for(int y=21;y<25-(x-33)/3;y++)dot(x,y,C("#dca766"));
            dot(31,24,outline);dot(30,24,Color.white);dot(29,27,green);dot(28,28,outline);
            for(int x=10;x<22;x++)dot(x,11+(x-10)/3,green);
            dot(22,9,outline);dot(25,9,outline);tex.Apply();enemyFrames[frame]=tex;
        }
    }
    void DrawEnemy(Rect r,float phase,float alpha=1)
    {
        GUI.color=new Color(1,1,1,alpha);GUI.DrawTexture(r,enemyFrames[(int)(phase*5)%8]);GUI.color=Color.white;
    }
    void DrawVine(Vine v)
    {
        if(!v.hanging){Pic("vine",W(v.r.x,v.r.y,v.r.width,v.r.height));return;}
        Vector2 end=v.r.center;
        for(int i=0;i<10;i++)
        {
            Vector2 p=Vector2.Lerp(v.anchor,end,i/9f);
            Fill(W(p.x-.035f,p.y,.07f,.10f),C("#6ecddd"));
            if(i%3==0)Fill(W(p.x-.10f,p.y,.20f,.07f),C("#99e5da"));
        }
        var r=W(v.r.x,v.r.y,v.r.width,v.r.height);
        Fill(new Rect(r.x+6,r.y,r.width-12,r.height),C("#e98ebf"));Fill(new Rect(r.x,r.y+6,r.width,r.height-12),C("#e98ebf"));
        Fill(new Rect(r.center.x-4,r.center.y-4,8,8),C("#fff2ad"));
    }
}
