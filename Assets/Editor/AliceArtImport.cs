using System.IO;
using UnityEngine;
using UnityEditor;
public static class AliceArtImport
{
 public static void Import(){
  var doctor=ReadRaw("doctor-original.png");for(int i=0;i<4;i++){int left=i*doctor.width/4,right=(i+1)*doctor.width/4;Slice(doctor,new RectInt(left,0,right-left,doctor.height),i<3?"doctor"+i:"doctor_badge",i<3?160:96);}
  var doctorV2=Read("doctor-v2-original.png");for(int i=0;i<8;i++)SliceClean(doctorV2,new RectInt(i%4*doctorV2.width/4,(1-i/4)*doctorV2.height/2,doctorV2.width/4,doctorV2.height/2),"doctor"+i,160);
  var companion=Read("companion-original.png");int[] edges={0,500,1040,1475,companion.width};for(int i=0;i<4;i++)Slice(companion,new RectInt(edges[i],0,edges[i+1]-edges[i],companion.height),i<3?"cat"+i:"checkpoint",i<3?64:128);
  var catJump=Read("cat-jump-original.png");Slice(catJump,new RectInt(0,0,catJump.width,catJump.height),"cat_jump",64);
  var lives=Read("life-icons-original.png");int half=lives.width/2;Slice(lives,new RectInt(0,0,half,lives.height),"life_full",64);Slice(lives,new RectInt(half,0,lives.width-half,lives.height),"life_empty",64);
  var castle=ReadRaw("castle-kit-original.png");for(int i=0;i<3;i++){int x0=Mathf.RoundToInt(i*castle.width/3f),x1=Mathf.RoundToInt((i+1)*castle.width/3f);var cell=i<2?new RectInt(x0+82,68,x1-x0-164,castle.height-136):new RectInt(x0,0,x1-x0,castle.height);Slice(castle,cell,i==0?"castle_stone":i==1?"castle_moss":"castle_arch",i<2?160:256);}
  var gym=Read("gym-demon-v2-original.png");for(int i=0;i<12;i++){int col=i%4,row=2-i/4;SliceClean(gym,new RectInt(col*gym.width/4,row*gym.height/3,gym.width/4,gym.height/3),"gym_demon"+i,160);}
  var plants=ReadRaw("plants-extra-original.png");for(int i=0;i<5;i++){int x0=Mathf.RoundToInt(i*plants.width/5f),x1=Mathf.RoundToInt((i+1)*plants.width/5f);Slice(plants,new RectInt(x0,0,x1-x0,plants.height),"flora"+(i+2),96);}
  var tattoo=Read("tattoo-healing-v3-original.png");for(int i=0;i<12;i++){int col=i%4,row=2-i/4;int left=col*tattoo.width/4,right=(col+1)*tattoo.width/4;if(i>=8){int[] tattooEdges={0,378,790,1090,1536};left=tattooEdges[col]*tattoo.width/1536;right=(col==2?1152:tattooEdges[col+1])*tattoo.width/1536;}SliceClean(tattoo,new RectInt(left,row*tattoo.height/3,right-left,tattoo.height/3),"tattoo_monster"+i,160);}
  var cave=ReadRaw("dungeon-backdrop-v2.png");Resize(cave,960,540,"dungeon_backdrop");
  var dungeon=ReadRaw("dungeon-kit-original.png");string[] dungeonNames={"dungeon_wall","dungeon_entrance","dungeon_exit","dungeon_decor"};for(int i=0;i<4;i++){int x0=Mathf.RoundToInt(i*dungeon.width/4f),x1=Mathf.RoundToInt((i+1)*dungeon.width/4f);Slice(dungeon,new RectInt(x0,0,x1-x0,dungeon.height),dungeonNames[i],i==0?160:192);}
  var ground=Read("ground-original.png");var terrain=new Texture2D(192,128,TextureFormat.RGBA32,false);for(int y=0;y<128;y++)for(int x=0;x<192;x++)terrain.SetPixel(x,y,ground.GetPixel(x*ground.width/192,y*ground.height/128));terrain.Apply();File.WriteAllBytes("Assets/Resources/Art/earth.png",terrain.EncodeToPNG());
  var source=Read("enemies-original.png");int split=source.height*37/100;for(int i=4;i<8;i++)Slice(source,new RectInt((i%4)*source.width/4,i<4?split:0,source.width/4,i<4?source.height-split:split),i<4?"demon"+i:"bug"+(i-4),i<4?96:64);
  var anxiety=Read("anxiety-v2-original.png");for(int i=0;i<4;i++)Slice(anxiety,new RectInt(i*anxiety.width/4,0,anxiety.width/4,anxiety.height),"demon"+i,160);
  var tiles=Read("tiles-original.png");int cw=tiles.width/3,ch=tiles.height/2;
  Slice(tiles,new RectInt(cw+70,0,cw/2-70,ch-45),"beer",32);Slice(tiles,new RectInt(cw+cw/2+20,0,cw/2-20,ch-45),"wine",32);
  var feast=Read("feast-v2-original.png");SliceClean(feast,new RectInt(0,0,feast.width,feast.height),"dungeon_feast",128);
  var wine=Read("wine-v2-original.png");SliceClean(wine,new RectInt(0,0,wine.width,wine.height),"wine",64);
  var paper=new Texture2D(24,20,TextureFormat.RGBA32,false);var px=new Color[480];for(int y=0;y<20;y++)for(int x=0;x<24;x++)px[y*24+x]=x<2||x>21||y<2||y>17?Color.clear:(y%4==0&&x>4&&x<19?new Color(.45f,.24f,.30f):new Color(.94f,.87f,.77f));paper.SetPixels(px);paper.Apply();File.WriteAllBytes("Assets/Resources/Art/paper.png",paper.EncodeToPNG());
  AssetDatabase.Refresh();foreach(string id in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Resources/Art"})){var imp=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(id));imp.filterMode=FilterMode.Point;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.mipmapEnabled=imp.assetPath.EndsWith("/earth.png")||imp.assetPath.EndsWith("/castle_stone.png")||imp.assetPath.EndsWith("/castle_moss.png");imp.npotScale=TextureImporterNPOTScale.None;imp.alphaIsTransparency=true;imp.SaveAndReimport();}
 }
 static Texture2D Read(string name){var t=new Texture2D(2,2);t.LoadImage(File.ReadAllBytes("Assets/ArtSource/"+name));var p=t.GetPixels();for(int i=0;i<p.Length;i++){if(p[i].a<.08f||(p[i].g>.12f&&p[i].g>p[i].r*1.15f&&p[i].g>p[i].b*1.15f))p[i]=Color.clear;else {var c=p[i];c.a=1;p[i]=c;}}t.SetPixels(p);t.Apply();return t;}
 static Texture2D ReadRaw(string name){var t=new Texture2D(2,2);t.LoadImage(File.ReadAllBytes("Assets/ArtSource/"+name));var p=t.GetPixels();for(int i=0;i<p.Length;i++){if(p[i].a>.08f){var c=p[i];c.a=1;p[i]=c;}else p[i]=Color.clear;}t.SetPixels(p);t.Apply();return t;}
 static Texture2D ReadBlackBackdrop(string name){var t=ReadRaw(name);var p=t.GetPixels();for(int i=0;i<p.Length;i++)if(p[i].r<.018f&&p[i].g<.018f&&p[i].b<.018f)p[i]=Color.clear;t.SetPixels(p);t.Apply();return t;}
 static void Resize(Texture2D src,int width,int height,string name){var t=new Texture2D(width,height,TextureFormat.RGBA32,false);for(int y=0;y<height;y++)for(int x=0;x<width;x++)t.SetPixel(x,y,src.GetPixel(Mathf.Clamp(x*src.width/width,0,src.width-1),Mathf.Clamp(y*src.height/height,0,src.height-1)));t.Apply();File.WriteAllBytes("Assets/Resources/Art/"+name+".png",t.EncodeToPNG());}
 static void SliceClean(Texture2D src,RectInt cell,string name,int size){
  int w=cell.width,h=cell.height;var pixels=src.GetPixels(cell.x,cell.y,w,h);var visited=new bool[pixels.Length];var largest=new System.Collections.Generic.List<int>();
  for(int start=0;start<pixels.Length;start++){if(visited[start]||pixels[start].a<.5f)continue;var component=new System.Collections.Generic.List<int>();var queue=new System.Collections.Generic.Queue<int>();queue.Enqueue(start);visited[start]=true;
   while(queue.Count>0){int at=queue.Dequeue();component.Add(at);int x=at%w,y=at/w;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=x+dx,ny=y+dy;if(nx<0||nx>=w||ny<0||ny>=h)continue;int next=ny*w+nx;if(!visited[next]&&pixels[next].a>=.5f){visited[next]=true;queue.Enqueue(next);}}}
   if(component.Count>largest.Count)largest=component;
  }
  var clean=new Color[pixels.Length];foreach(int i in largest)clean[i]=pixels[i];var t=new Texture2D(w,h,TextureFormat.RGBA32,false);t.SetPixels(clean);t.Apply();Slice(t,new RectInt(0,0,w,h),name,size);
 }
 static void Slice(Texture2D src,RectInt cell,string name,int size){int x0=cell.xMax,y0=cell.yMax,x1=cell.xMin,y1=cell.yMin;for(int y=cell.y;y<cell.yMax;y++)for(int x=cell.x;x<cell.xMax;x++)if(src.GetPixel(x,y).a>.5f){x0=Mathf.Min(x0,x);x1=Mathf.Max(x1,x);y0=Mathf.Min(y0,y);y1=Mathf.Max(y1,y);}int w=x1-x0+1,h=y1-y0+1;if(w<1||h<1)throw new System.Exception("Empty sprite "+name);float scale=(size-4f)/Mathf.Max(w,h);int dw=Mathf.Max(1,Mathf.RoundToInt(w*scale)),dh=Mathf.Max(1,Mathf.RoundToInt(h*scale));var t=new Texture2D(dw,dh,TextureFormat.RGBA32,false);for(int y=0;y<dh;y++)for(int x=0;x<dw;x++)t.SetPixel(x,y,src.GetPixel(x0+x*w/dw,y0+y*h/dh));t.Apply();File.WriteAllBytes("Assets/Resources/Art/"+name+".png",t.EncodeToPNG());}
}
