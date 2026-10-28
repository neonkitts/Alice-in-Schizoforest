using UnityEngine;
using UnityEditor;
using System.IO;
public static class AliceUIImport {
 static bool Inside(Vector2 p,Vector2[] poly){bool c=false;for(int i=0,j=poly.Length-1;i<poly.Length;j=i++)if((poly[i].y>p.y)!=(poly[j].y>p.y)&&p.x<(poly[j].x-poly[i].x)*(p.y-poly[i].y)/(poly[j].y-poly[i].y)+poly[i].x)c=!c;return c;}
 static Vector2[] Poly(params float[] xy){var p=new Vector2[xy.Length/2];for(int i=0;i<p.Length;i++)p[i]=new Vector2(xy[i*2],xy[i*2+1]);return p;}
 static Texture2D Read(string n){var t=new Texture2D(2,2);t.LoadImage(File.ReadAllBytes("Assets/ArtSource/"+n));return t;}
 static void Export(Texture2D src,string name,RectInt rect,int width,Vector2[] silhouette=null,bool key=false){
  Color[] region=new Color[rect.width*rect.height];for(int yy=0;yy<rect.height;yy++)for(int xx=0;xx<rect.width;xx++)region[yy*rect.width+xx]=src.GetPixel(rect.x+xx,src.height-1-rect.y-yy);
  bool[] outside=new bool[region.Length];
  if(!key){
   bool[] smooth=new bool[region.Length];
   for(int yy=0;yy<rect.height;yy++)for(int xx=0;xx<rect.width;xx++){
    float lo=1,hi=0;for(int oy=-2;oy<=2;oy++)for(int ox=-2;ox<=2;ox++){var c=region[Mathf.Clamp(yy+oy,0,rect.height-1)*rect.width+Mathf.Clamp(xx+ox,0,rect.width-1)];float l=c.grayscale;lo=Mathf.Min(lo,l);hi=Mathf.Max(hi,l);}
    smooth[yy*rect.width+xx]=hi-lo<.035f;
   }
   var queue=new System.Collections.Generic.Queue<int>();
   for(int i=0;i<region.Length;i++)if((i<rect.width||i>=region.Length-rect.width||i%rect.width==0||i%rect.width==rect.width-1)&&smooth[i]){outside[i]=true;queue.Enqueue(i);}
   while(queue.Count>0){int n=queue.Dequeue(),x=n%rect.width,y=n/rect.width;foreach(int d in new[]{-1,1,-rect.width,rect.width}){int k=n+d;if(k<0||k>=region.Length||Mathf.Abs(k%rect.width-x)>1||outside[k]||!smooth[k])continue;outside[k]=true;queue.Enqueue(k);}}
   // Extend the matte to the first high-contrast contour, retaining carved detail.
   for(int pass=0;pass<2;pass++){var grown=(bool[])outside.Clone();for(int y=1;y<rect.height-1;y++)for(int x=1;x<rect.width-1;x++){int n=y*rect.width+x;if(outside[n])continue;foreach(int d in new[]{-1,1,-rect.width,rect.width})if(outside[n+d]&&Mathf.Abs(region[n].grayscale-region[n+d].grayscale)<.045f)grown[n]=true;}outside=grown;}
  }
  int height=Mathf.RoundToInt(width*(float)rect.height/rect.width);var dst=new Texture2D(width,height,TextureFormat.RGBA32,false);
  for(int y=0;y<height;y++)for(int x=0;x<width;x++){
   float u=(x+.5f)/width,v=1-(y+.5f)/height;var c=src.GetPixel(rect.x+Mathf.Min(rect.width-1,(int)(u*rect.width)),src.height-1-rect.y-Mathf.Min(rect.height-1,(int)(v*rect.height)));
   if(outside[Mathf.Min(rect.height-1,(int)(v*rect.height))*rect.width+Mathf.Min(rect.width-1,(int)(u*rect.width))])c=Color.clear;
   if(key&&c.g>c.r*1.2f&&c.g>c.b*1.2f)c=Color.clear;
   dst.SetPixel(x,y,c);
  }dst.Apply();File.WriteAllBytes("Assets/Resources/Art/"+name+".png",dst.EncodeToPNG());
 }
 public static void Import(){
  var window=Read("ui-window-source.png");
  Export(window,"ui_window",new RectInt(0,0,1536,1024),1024,Poly(0,.24f,.04f,.01f,.07f,.12f,.10f,.04f,.13f,.14f,.25f,.09f,.31f,.12f,.33f,.05f,.35f,.13f,.44f,.09f,.47f,.12f,.5f,0,.54f,.12f,.63f,.1f,.68f,.13f,.70f,.06f,.72f,.12f,.79f,.09f,.87f,.13f,.9f,.04f,.93f,.13f,.96f,.01f,1,.24f,.98f,.32f,.98f,.69f,1,.78f,.97f,.86f,.96f,.94f,.93f,.84f,.90f,.9f,.87f,.82f,.78f,.86f,.72f,.83f,.70f,.91f,.68f,.84f,.58f,.88f,.54f,.84f,.5f,1,.46f,.84f,.4f,.88f,.35f,.84f,.33f,.91f,.31f,.84f,.23f,.86f,.13f,.82f,.10f,.94f,.08f,.83f,.04f,.95f,.02f,.84f,0,.76f,.02f,.67f,.02f,.32f));
  var kit=Read("ui-kit-source.png");
  Export(kit,"ui_button",new RectInt(25,55,755,245),768,Poly(0,.64f,.04f,.32f,.17f,0,.16f,.24f,.22f,.21f,.45f,.22f,.50f,.04f,.54f,.22f,.78f,.22f,.82f,0,.96f,.34f,1,.64f,.94f,.56f,.91f,.80f,.88f,.68f,.83f,1,.77f,.77f,.54f,.79f,.50f,.95f,.46f,.78f,.23f,.77f,.17f,1,.12f,.68f,.08f,.83f,.05f,.57f));
  Export(kit,"ui_plaque",new RectInt(792,10,730,305),768,Poly(.02f,.40f,.08f,.19f,.17f,.18f,.18f,.08f,.25f,.23f,.43f,.18f,.5f,0,.57f,.18f,.75f,.22f,.84f,.10f,.88f,.23f,.96f,.28f,1,.46f,.96f,.60f,.99f,.75f,.9f,.82f,.83f,.95f,.75f,.85f,.57f,.84f,.5f,1,.44f,.84f,.25f,.84f,.16f,.96f,.07f,.83f,0,.60f,.08f,.54f));
  Export(kit,"ui_card",new RectInt(120,321,580,640),384,Poly(.5f,0,.58f,.10f,.88f,.17f,.90f,.08f,.96f,.20f,.94f,.35f,.97f,.78f,1,.86f,.91f,.95f,.6f,.94f,.5f,1,.4f,.94f,.09f,.95f,0,.86f,.03f,.78f,.06f,.35f,.04f,.2f,.1f,.08f,.12f,.17f,.42f,.1f));
  Export(kit,"ui_ring",new RectInt(805,320,700,690),384,Poly(.5f,0,.59f,.17f,.76f,.13f,.78f,.26f,.89f,.37f,1,.47f,.84f,.55f,.88f,.77f,.75f,.77f,.59f,.86f,.5f,1,.41f,.86f,.25f,.79f,.13f,.81f,.17f,.57f,0,.47f,.16f,.39f,.23f,.25f,.25f,.13f,.42f,.18f));
  var icons=Read("ui-icons-source.png");string[] names={"ui_attack","ui_inventory","ui_pause","ui_hand"};for(int i=0;i<4;i++)Export(icons,names[i],new RectInt(i%2*icons.width/2,i/2*icons.height/2,icons.width/2,icons.height/2),256,null,true);
  AssetDatabase.Refresh();
 }
}
