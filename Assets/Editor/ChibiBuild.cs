using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
public static class ChibiBuild {
 public static void Build(){BuildTargetGame(BuildTarget.StandaloneWindows64,"Builds/Windows/Alice.exe");}
 public static void Web(){BuildTargetGame(BuildTarget.WebGL,"Builds/WebGL");foreach(string name in new[]{"ui_button","ui_plaque","ui_ring","ui_attack","ui_inventory","ui_pause","ui_hand"})File.Copy("Assets/Resources/Art/"+name+".png","Builds/WebGL/"+name+".png",true);File.Copy("Assets/Resources/Fonts/Pixel.ttf","Builds/WebGL/Pixel.ttf",true);File.Copy("Assets/Resources/Art/gothic_panel.png","Builds/WebGL/gothic-panel.png",true);File.Copy("Assets/Resources/Art/paper.png","Builds/WebGL/paper.png",true);File.Copy("Assets/Resources/Art/cat0.png","Builds/WebGL/cat.png",true);}
 static void BuildTargetGame(BuildTarget target,string output){
  ChibiArtImport.Import(); AliceUIImport.Import(); AliceArtImport.Import();
  PlayerSettings.companyName="Zlatoslava";PlayerSettings.productName="Alice in Schizoforest";PlayerSettings.bundleVersion="1.7.4";var appIcon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Art/life_full.png");if(appIcon)PlayerSettings.SetIcons(NamedBuildTarget.Standalone,new[]{appIcon},IconKind.Application);
  PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=false;
  PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{UnityEngine.Rendering.GraphicsDeviceType.Direct3D11});
  PlayerSettings.WebGL.template="PROJECT:Alice";PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;PlayerSettings.WebGL.decompressionFallback=true;
  var pixel=AssetImporter.GetAtPath("Assets/Resources/Fonts/Pixel.ttf") as TrueTypeFontImporter;if(pixel!=null){pixel.fontRenderingMode=FontRenderingMode.HintedRaster;pixel.SaveAndReimport();}
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var camera=new GameObject("Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.gameObject.AddComponent<AudioListener>();camera.clearFlags=CameraClearFlags.SolidColor;
  EditorSceneManager.SaveScene(scene,"Assets/Scenes/Island.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Island.unity",true)};
  Validate();AliceMobileChecks.Run();AlicePersistenceChecks.Run();Directory.CreateDirectory(Path.GetDirectoryName(output));
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Island.unity"},locationPathName=output,target=target,options=BuildOptions.None});
  Debug.Log("ALICE_BUILD "+report.summary.result+" bytes="+report.summary.totalSize);if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed");if(target==BuildTarget.StandaloneWindows64)File.Copy("Assets/ArtSource/AliceHeart.ico",Path.Combine(Path.GetDirectoryName(output),"AliceHeart.ico"),true);
 }
 public static void Validate(){
  int checks=0;Action<bool,string> test=(ok,name)=>{checks++;if(!ok)throw new Exception("ALICE_TEST_FAILED "+name);};
  test(ChibiGame.StickMovement(.1f)==0 && ChibiGame.StickMovement(-.1f)==0,"stick deadzone prevents drift");
  test(ChibiGame.StickMovement(.7f)>.7f && ChibiGame.StickMovement(-.7f)<-.7f,"diagonal stick preserves bidirectional movement");
  test(ChibiGame.StickMovement(1)==1 && ChibiGame.StickMovement(-1)==-1,"full stick reaches run speed");
  test(ChibiGame.MovementIntent(true,false,0)==-1,"left moves left");test(ChibiGame.MovementIntent(false,true,0)==1,"right moves right");test(ChibiGame.MovementIntent(true,true,0)==0,"opposite keys");
  test(ChibiGame.ActorFacing(-.1f,1)==-1&&ChibiGame.ActorFacing(.1f,-1)==1&&ChibiGame.ActorFacing(0,-1)==-1,"actor facing follows movement and persists when stopped");
  test(ChibiGame.WineHealing(1)==0&&ChibiGame.WineHealing(2)==0&&ChibiGame.WineHealing(3)==10&&ChibiGame.WineHealing(6)==10,"wine groups");
  test(Mathf.Approximately(ChibiGame.RoadLegDistance/ChibiGame.Speed,60),"one minute of running between landmarks");
  for(int i=0;i<=20;i++){
   float t=i/20f;var right=ChibiGame.PaperAttackOffset(t,1);var left=ChibiGame.PaperAttackOffset(t,-1);
   test(Mathf.Abs(right.x+left.x)<.001f&&Mathf.Abs(right.y-left.y)<.001f,"paper downward strike is mirrored identically");
   if(i>0)test(right.y<=ChibiGame.PaperAttackOffset((i-1)/20f,1).y,"paper travels downward throughout strike");
  }
  var trap=new System.Collections.Generic.List<ChibiGame.Block>{new ChibiGame.Block(0,0,1.5f,.55f,1),new ChibiGame.Block(1.4f,1.05f,1.7f,1.2f,2),new ChibiGame.Block(12,1.05f,2,1.2f,2)};
  test(ChibiGame.EnsureForestClearance(trap)==1&&trap.Count==2,"reported floor/ceiling trap removed while safe ceiling remains");
  test(ChibiGame.EnsureForestClearance(trap)==0,"route repair is idempotent");
  var go=new GameObject("Validation");var g=go.AddComponent<ChibiGame>();float minCastle=float.MaxValue,maxCastle=float.MinValue;var mapSignatures=new System.Collections.Generic.HashSet<int>();var dungeonSignatures=new System.Collections.Generic.HashSet<int>();
  for(int seed=0;seed<100;seed++){g.Generate(seed,ChibiGame.FullLength);test(ChibiGame.EnsureForestClearance(new System.Collections.Generic.List<ChibiGame.Block>(g.blocks))==0,"no low ceiling traps near ground obstacles");test(g.blocks.Count>g.length*.14f,"obstacle count");test(g.mires.Count>g.length*.008f,"mire count");test(g.motes.Count>g.length*.012f&&g.motes.Count<200,"moderate wine count");test(g.bugs.Count>g.length*.004f,"beetle count");int signature=17;foreach(var b in g.blocks)if(b.type>0&&b.type<5){test(b.r.y>0?b.r.y>=1.02f:b.r.height<1,"clearances");unchecked{signature=signature*31+Mathf.RoundToInt(b.r.x*10)+b.type*997;}}mapSignatures.Add(signature);
   minCastle=Mathf.Min(minCastle,g.castleStart);maxCastle=Mathf.Max(maxCastle,g.castleStart);test(Mathf.Abs((g.castleStart-g.catX)/ChibiGame.Speed-60)<.01f,"one minute from kitten to castle");test(Mathf.Abs((g.catX-g.doctorX)/ChibiGame.Speed-60)<.01f,"one minute from doctor to kitten");test(g.castleEnd>g.castleStart+140&&g.castleEnd<g.castleStart+210,"castle route length");test(g.blocks.FindAll(b=>b.type>=5&&b.type<7).Count>45,"detailed castle block count");test(g.blocks.Exists(b=>b.type>=5&&b.type<7&&b.r.y==0&&b.r.height>4.1f),"castle climbs above forest path");test(g.blocks.FindAll(b=>b.type>=5&&b.type<7&&b.r.y>0).Count>=4,"castle has visible jump barriers");test(!g.blocks.Exists(b=>b.type>=5&&b.type<7&&b.r.y>0&&b.r.height>.6f),"castle has no hidden overhead colliders");test(!g.blocks.Exists(b=>b.type>0&&b.type<5&&b.r.xMax>g.castleStart-2&&b.r.xMin<g.castleEnd+2),"castle clears ordinary obstacles");var castleFloor=g.blocks.FindAll(b=>b.type>=5&&b.type<7&&b.r.y==0&&b.r.xMin>=g.castleStart-.1f&&b.r.xMax<=g.castleEnd+.2f);castleFloor.Sort((a,b)=>a.r.x.CompareTo(b.r.x));for(int j=1;j<castleFloor.Count;j++){test(castleFloor[j].r.xMin<=castleFloor[j-1].r.xMax+.001f,"castle slabs have no collision gaps");test(castleFloor[j].r.yMax-castleFloor[j-1].r.yMax<=.39f,"castle upward step is jumpable");}test(Mathf.Abs((g.dungeonEntrance-g.castleEnd)/ChibiGame.Speed-60)<.01f&&Mathf.Abs((g.length-g.dungeonEntrance-166)/ChibiGame.Speed-60)<.01f,"one minute from castle to cave and cave exit to home");test(!(g.dungeonEntrance>g.castleStart-260&&g.dungeonEntrance<g.castleEnd+260),"dungeon and castle do not overlap");test(g.dungeonBlocks.Count>=45,"dungeon has a detailed continuous landscape");var dungeonFloor=g.dungeonBlocks.FindAll(b=>b.r.y==-12);dungeonFloor.Sort((a,b)=>a.r.x.CompareTo(b.r.x));int dungeonSignature=17;for(int j=1;j<dungeonFloor.Count;j++){test(dungeonFloor[j].r.xMin<=dungeonFloor[j-1].r.xMax+.01f,"dungeon landscape has no floor gaps");unchecked{dungeonSignature=dungeonSignature*31+Mathf.RoundToInt(dungeonFloor[j].r.height*100);}}dungeonSignatures.Add(dungeonSignature);}
  test(Mathf.Abs(maxCastle-minCastle)<.01f,"landmark pacing stays fixed while terrain varies");
  test(mapSignatures.Count==100,"every tested seed creates a distinct obstacle layout");
  test(dungeonSignatures.Count>80,"dungeon landscape changes between runs");
  test(ChibiGame.GymDemonSpeed<ChibiGame.Speed,"gym demon remains slower than running Alice");
  g.Generate(7,100);test(g.blocks.FindAll(b=>b.type==0).Count==1,"one continuous floor");test(g.blocks.Exists(b=>b.type==0&&b.r.xMin<0&&b.r.xMax>g.length),"floor spans entire map");g.mode=ChibiGame.Mode.Play;g.px=4;for(int i=0;i<120;i++)g.Step(1f/120,1);test(g.px>8,"move right physically");for(int i=0;i<120;i++)g.Step(1f/120,-1);test(g.px<6,"move left physically");
  var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
  g.Generate(21,ChibiGame.FullLength);g.mode=ChibiGame.Mode.Play;g.px=g.castleStart-1;g.py=g.vx=g.vy=0;typeof(ChibiGame).GetField("grounded",flags).SetValue(g,true);for(int step=0;step<12000&&g.px<g.castleEnd-2;step++){bool jump=false;foreach(var b in g.blocks)if(b.type>=5&&b.type<7&&b.r.xMin>g.px+.05f&&b.r.xMin-g.px<1.15f&&b.r.yMax>g.py+.06f&&b.r.yMin<g.py+1.1f){jump=true;break;}if(jump&&(bool)typeof(ChibiGame).GetField("grounded",flags).GetValue(g))typeof(ChibiGame).GetField("jumpBuffer",flags).SetValue(g,.15f);g.Step(1f/120,1);}test(g.px>=g.castleEnd-2,"simulated runner can cross entire castle without invisible barrier");
  g.Generate(44,ChibiGame.FullLength);g.mode=ChibiGame.Mode.Play;g.lives=3;g.hp=100;g.px=g.castleChaseStart+.2f;g.py=0;foreach(var b in g.blocks)if(b.type>=5&&b.type<7&&b.r.y==0&&g.px>=b.r.xMin&&g.px<=b.r.xMax)g.py=Mathf.Max(g.py,b.r.yMax);g.UpdateCastle(.01f);test(g.castleChase&&g.castleHeadStart>1.5f&&g.castleHeadStart<1.8f,"gym demon gives a short visible head start");float waitingDemon=(float)typeof(ChibiGame).GetField("gymDemonX",flags).GetValue(g);g.UpdateCastle(1.2f);test(Mathf.Approximately(waitingDemon,(float)typeof(ChibiGame).GetField("gymDemonX",flags).GetValue(g)),"gym demon stays still only during its short head start");for(int i=0;i<105;i++){g.px+=ChibiGame.Speed*.101f;g.UpdateCastle(.101f);}test(g.castleCleared&&!g.castleChase,"ten active pursuit seconds clear chase after head start");
  g.Generate(45,ChibiGame.FullLength);g.mode=ChibiGame.Mode.Play;g.lives=3;g.hp=100;g.px=g.castleChaseStart+.2f;g.py=0;foreach(var b in g.blocks)if(b.type>=5&&b.type<7&&b.r.y==0&&g.px>=b.r.xMin&&g.px<=b.r.xMax)g.py=Mathf.Max(g.py,b.r.yMax);g.UpdateCastle(.01f);g.castleHeadStart=0;
  typeof(ChibiGame).GetField("gymDemonX",flags).SetValue(g,g.px);typeof(ChibiGame).GetField("gymDemonY",flags).SetValue(g,g.py);g.UpdateCastle(.01f);test(g.lives==2&&!g.castleCleared,"gym demon catch costs one life and resets chase");
  g.Generate(7,100);g.mode=ChibiGame.Mode.Play;g.px=4;g.py=0;g.blocks.Add(new ChibiGame.Block(g.px+1.35f,g.py,1,.9f,1));g.bugs.Clear();g.bugs.Add(new ChibiGame.DreamEnemy{x=g.px+.7f,y=0,hp=4,demon=true});g.Attack();test(g.bugs[0].hp==3,"one attack one demon hit");
  typeof(ChibiGame).GetMethod("HitDreamEnemies",flags).Invoke(g,null);test(g.bugs[0].hp==3,"attack cannot repeat damage each frame");
  for(int i=0;i<3;i++){typeof(ChibiGame).GetField("attackCooldown",flags).SetValue(g,0f);g.bugs[0].x=g.px+.7f;g.Attack();}test(g.bugs[0].hp==0,"four strikes kill demon");
  typeof(ChibiGame).GetField("autoRun",flags).SetValue(g,true);g.Generate(1987,ChibiGame.FullLength);g.px=4;g.py=0;g.lives=3;g.elapsed=59.9f;g.UpdateExpedition(0);test(g.catSpawned&&g.catX>250,"kitten preplaced ahead on route");
  g.elapsed=60;g.UpdateExpedition(0);test(g.catSpawned&&g.catX>250,"waiting does not move kitten beside player");test(!g.blocks.Exists(b=>b.type!=0&&new Rect(g.catX-1.4f,0,2.8f,1.8f).Overlaps(b.r)),"kitten safe clearing");
  g.UpdateExpedition(0,true);test(!g.catRescued,"cannot rescue from a distance");g.px=g.catX;g.UpdateExpedition(0,true);test(g.catRescued&&g.lifePotions==1,"rescue grants potion");g.UpdateExpedition(0,true);test(g.lifePotions==1,"rescue cannot duplicate reward");
  typeof(ChibiGame).GetMethod("Continue",flags).Invoke(g,null);test(g.catRescued&&g.lifePotions==1,"inventory survives save reload");test(g.UseLifePotion()&&g.lives==4&&g.lifePotions==0,"potion grants fourth life");test(!g.UseLifePotion()&&g.lives==4,"empty inventory cannot grant lives");
  typeof(ChibiGame).GetMethod("Continue",flags).Invoke(g,null);test(g.lives==4&&g.lifePotions==0,"consumed potion stays consumed on reload");test(Resources.Load<AudioClip>("Audio/BranchCrunch")!=null,"twig crunch imported");test(Resources.Load<AudioClip>("Audio/MainTheme")!=null&&Resources.Load<AudioClip>("Audio/DemonChase")!=null,"gothic exploration and chase music imported");var roar=Resources.Load<AudioClip>("Audio/TattooRoar");test(roar!=null&&roar.length>4,"tattoo monster has a long layered audible roar");test(Resources.Load<AudioClip>("Audio/GymDemon")!=null,"gym demon has its own correctly named voice line");
  test(ChibiGame.CompanionRunSpeed(1)>ChibiGame.Speed,"kitten catches player immediately");test(ChibiGame.CompanionFacing(2,-1)==1&&ChibiGame.CompanionFacing(-2,1)==-1,"kitten faces running direction instead of backing up");g.catSpawned=g.catRescued=true;g.px=6;g.py=0;g.vx=ChibiGame.Speed;typeof(ChibiGame).GetField("catFollowX",flags).SetValue(g,4.55f);typeof(ChibiGame).GetField("catFollowY",flags).SetValue(g,0f);typeof(ChibiGame).GetField("catGrounded",flags).SetValue(g,true);g.UpdateExpedition(.1f);test(Mathf.Abs((float)typeof(ChibiGame).GetField("catFollowY",flags).GetValue(g))<.001f,"kitten paws stay on ground while running");g.px=321.45f;g.py=4;g.vx=0;typeof(ChibiGame).GetField("catFollowX",flags).SetValue(g,0f);typeof(ChibiGame).GetField("catFollowY",flags).SetValue(g,4f);typeof(ChibiGame).GetField("catGrounded",flags).SetValue(g,true);g.UpdateExpedition(.01f);test(Mathf.Abs((float)typeof(ChibiGame).GetField("catFollowY",flags).GetValue(g))<.01f,"kitten snaps to real ground instead of levitating after catch-up");test(Resources.Load<Texture2D>("Art/cat_jump")!=null,"kitten jump frame imported");test(Resources.Load<Texture2D>("Art/life_full")!=null&&Resources.Load<Texture2D>("Art/life_empty")!=null,"gothic life emblems imported");test(Resources.Load<Texture2D>("Art/castle_stone")!=null&&Resources.Load<Texture2D>("Art/castle_arch")!=null,"castle art imported");test(Resources.Load<Texture2D>("Art/gym_demon0")!=null,"gym demon animation imported");test(Resources.Load<Texture2D>("Art/flora6")!=null,"extra plants imported");
  var low=g.blocks.Find(b=>b.type!=0&&b.r.y==0);g.catSpawned=g.catRescued=true;g.px=low.r.xMax+3;typeof(ChibiGame).GetField("catFollowX",flags).SetValue(g,low.r.xMin-.7f);typeof(ChibiGame).GetField("catFollowY",flags).SetValue(g,0f);typeof(ChibiGame).GetField("catGrounded",flags).SetValue(g,true);g.UpdateExpedition(.1f);test((float)typeof(ChibiGame).GetField("catFollowY",flags).GetValue(g)>0,"kitten jumps over lower blocks");
  foreach(float width in new[]{1.1f,1.24f,1.45f})foreach(int direction in new[]{-1,1}){
   var solids=new System.Collections.Generic.List<ChibiGame.Block>{new ChibiGame.Block(0,-4,50,4,0)};
   object[] moveArgs={20f,0f,0f,true,width,2.25f,2f,20f+direction*10,1f/120,solids,0f};
   for(int frame=0;frame<120;frame++)typeof(ChibiGame).GetMethod("MoveGroundActor",flags).Invoke(g,moveArgs);
   test(((float)moveArgs[0]-20)*direction>1.8f&&Mathf.Abs((float)moveArgs[1])<.01f,"each demon body moves both ways while grounded");
  }
  for(int trial=0;trial<5;trial++){
   g.Generate(77+trial,ChibiGame.FullLength);g.mode=ChibiGame.Mode.Play;g.lives=3;g.hp=100;g.px=g.dungeonEntrance-12;g.py=g.vx=g.vy=0;
   typeof(ChibiGame).GetField("grounded",flags).SetValue(g,true);
   for(int n=0;n<3600&&g.px<g.dungeonEntrance+48;n++){g.Step(1f/120,1);g.UpdateDungeon(1f/120);}
   test(g.px>=g.dungeonEntrance+48&&g.py< -7,"walk from forest down entire entrance without jump");g.px=g.dungeonEntrance;g.py=g.vx=g.vy=0;
   float entryX=g.px,entryY=g.py;typeof(ChibiGame).GetMethod("EnterDungeon",flags).Invoke(g,null);
   test(g.px==entryX&&g.py==entryY&&!g.DungeonLocksMovement,"entry preserves world coordinates and control");
   test(!g.blocks.Exists(b=>b.type==0&&b.r.xMin<entryX+40&&b.r.xMax>entryX+40),"forest floor is physically cut above descent");
   float lowest=0;
   for(int step=0;step<24000&&g.px<entryX+166;step++){
    bool jump=false;foreach(var b in g.blocks)if(b.r.xMin>g.px+.05f&&b.r.xMin-g.px<1.15f&&b.r.yMax>g.py+.06f&&b.r.yMin<g.py+1.1f){jump=true;break;}
    if(jump&&(bool)typeof(ChibiGame).GetField("grounded",flags).GetValue(g))typeof(ChibiGame).GetField("jumpBuffer",flags).SetValue(g,.15f);
    float prev=g.px;g.Step(1f/120,1);test(Mathf.Abs(g.px-prev)<.1f,"route never teleports");lowest=Mathf.Min(lowest,g.py);
   }
   test(g.px>=entryX+166&&lowest< -7,"runner descends below ground and climbs back on five seeds");
  }
  g.Generate(77,ChibiGame.FullLength);g.mode=ChibiGame.Mode.Play;g.px=g.dungeonEntrance;g.py=0;
  typeof(ChibiGame).GetMethod("EnterDungeon",flags).Invoke(g,null);
  g.px=g.dungeonEntrance+72;g.py=(float)typeof(ChibiGame).GetMethod("DungeonSurfaceAt",flags).Invoke(g,new object[]{g.px});g.UpdateDungeon(.01f);
  test((bool)typeof(ChibiGame).GetField("dungeonBossAwake",flags).GetValue(g),"tattoo monster wakes underground");
  typeof(ChibiGame).GetField("dungeonRoarTime",flags).SetValue(g,0f);
  int strikes=g.dungeonBossHp;for(int i=0;i<strikes;i++){typeof(ChibiGame).GetField("attackCooldown",flags).SetValue(g,0f);typeof(ChibiGame).GetField("dungeonBossX",flags).SetValue(g,g.px+.7f);typeof(ChibiGame).GetField("dungeonBossY",flags).SetValue(g,g.py);g.Attack();}
  test(g.dungeonBossDefeated,"boss dies after separate hits");g.UpdateDungeon(1.2f);g.px=g.dungeonEntrance+103;g.UpdateDungeon(0);
  test(g.dungeonRewardTaken&&g.DungeonLocksMovement&&g.speedBoostTime>=5,"trophy shows and pauses game with full boost");
  float boost=g.speedBoostTime;g.UpdateDungeon(1);test(g.speedBoostTime==boost,"boost is not wasted during trophy panel");g.UpdateDungeon(8);g.UpdateDungeon(.5f);test(g.speedBoostTime<boost,"boost counts down after panel");
  g.px=g.dungeonEntrance+167;float exitX=g.px;g.UpdateDungeon(0);test(!g.dungeonActive&&g.px==exitX,"exit preserves position");
  g.checkpoint=320;g.lives=1;g.hp=10;g.Damage(100,true);test(g.lives==3&&g.hp==100&&g.px==320&&g.mode==ChibiGame.Mode.Play,"checkpoint restores all three hearts even on last life");
  typeof(ChibiGame).GetMethod("Continue",flags).Invoke(g,null);test(g.lives==3,"restored lives persist");
  g.NewGame(77,ChibiGame.FullLength);g.elapsed=34.9f;g.UpdateDoctor();test(g.doctorSpawned&&g.doctorX>140&&!g.doctorDialogue,"doctor waits further along route");g.elapsed=35;g.UpdateDoctor();test(g.doctorSpawned,"doctor appears in first minute");g.px=g.doctorX;g.UpdateDoctor();test(g.doctorDialogue,"approach starts dialogue");
  float waitingDoctor=g.doctorX,waitingCat=g.catX;g.px=4;g.doctorDialogue=false;g.elapsed+=600;g.UpdateDoctor();g.UpdateExpedition(0);test(g.doctorX==waitingDoctor&&g.catX==waitingCat&&!g.doctorDialogue,"idle time never relocates road events");g.px=g.doctorX;g.UpdateDoctor();
  g.ChooseDoctorReply(1);test(g.doctorFinished&&!g.doctorDialogue&&g.badges==0,"first reply closes without reward");
  g.NewGame(78,ChibiGame.FullLength);g.elapsed=35;g.UpdateDoctor();g.px=g.doctorX;g.UpdateDoctor();g.ChooseDoctorReply(2);g.ChooseDoctorReply(2);test(g.badges==1,"second reply grants exactly one badge");
  typeof(ChibiGame).GetMethod("Continue",flags).Invoke(g,null);test(g.badges==1&&g.doctorFinished,"badge and dialogue choice persist");
  test(g.UseDoctorBadge()&&g.badges==0&&g.pendingPower==40&&!g.UseDoctorBadge(),"badge consumed once for forty power");
  g.blocks.Add(new ChibiGame.Block(g.px+1.35f,g.py,1,.9f,1));g.bugs.Clear();g.bugs.Add(new ChibiGame.DreamEnemy{x=g.px+.7f,y=g.py,hp=60,demon=true});typeof(ChibiGame).GetField("attackCooldown",flags).SetValue(g,0f);g.Attack();test(g.bugs[0].hp==19&&g.pendingPower==0,"next hit has base one plus forty");test(Mathf.Abs(g.bugs[0].x-(g.px+.7f))<.001f,"hit knockback cannot push living demon into solid terrain");
  test(Resources.Load<Texture2D>("Art/dungeon_feast")&&Resources.Load<Texture2D>("Art/doctor_badge")&&Resources.Load<Texture2D>("Art/doctor2"),"trophy badge and animated NPC resources present");
  for(int i=0;i<12;i++){test(Resources.Load<Texture2D>("Art/gym_demon"+i)!=null,"gym twelve animation frames");test(Resources.Load<Texture2D>("Art/tattoo_monster"+i)!=null,"tattoo twelve animation frames");}
  for(int i=0;i<8;i++)test(Resources.Load<Texture2D>("Art/doctor"+i)!=null,"doctor eight idle frames");
  UnityEngine.Object.DestroyImmediate(go);Directory.CreateDirectory("QA");File.WriteAllText("QA/validation.txt","PASS: "+checks+" checks; 100 distinct maps, variable continuous dungeon landscape, slow integrated descent, physical jumping demons, short visible gym pursuit head start, grounded kitten, audible dungeon roar, combat and save reload.");Debug.Log("ALICE_TESTS_PASS "+checks);
 }
}


