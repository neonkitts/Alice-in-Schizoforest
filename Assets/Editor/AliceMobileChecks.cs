using UnityEngine;
using System;
using System.Reflection;
using System.IO;
public static class AliceMobileChecks
{
    public static void Run(){
        var go=new GameObject("MobileChecks");var g=go.AddComponent<ChibiGame>();
        var flags=BindingFlags.Instance|BindingFlags.NonPublic;
        Action<bool,string> check=(ok,n)=>{if(!ok)throw new Exception("MOBILE_CHECK_FAILED: "+n);};
        var w=typeof(ChibiGame).GetMethod("W",flags);var camera=typeof(ChibiGame).GetField("cameraX",flags);
        var ui=typeof(ChibiGame).GetProperty("MobileInterfaceMatrix",flags);
        Matrix4x4 baseline=(Matrix4x4)ui.GetValue(g);
        foreach(float progress in new[]{4f,500f,1800f,3000f})foreach(var mode in new[]{ChibiGame.Mode.Play,ChibiGame.Mode.Pause,ChibiGame.Mode.Options}){
            g.px=progress;g.py=progress>1700?-8:0;g.mode=mode;
            check((Matrix4x4)ui.GetValue(g)==baseline,"screen UI scale stays fixed across route, depth and modal states");
        }
        check(Resources.Load<Texture2D>("Art/gothic_panel")!=null,"ornate panel texture imported");
        typeof(ChibiGame).GetField("mobileControls",flags).SetValue(g,true);g.mode=ChibiGame.Mode.Play;
        for(int i=0;i<1000;i++){
            camera.SetValue(g,i*.017f);
            var a=(Rect)w.Invoke(g,new object[]{0f,-4f,8f/3f,4f});
            var b=(Rect)w.Invoke(g,new object[]{8f/3f,-4f,8f/3f,4f});
            check(Mathf.Abs(a.xMax-b.xMin)<.001f,"shared terrain edges while camera moves");
        }
        g.blocks.Clear();g.blocks.Add(new ChibiGame.Block(-10,-4,200,4,0));g.px=4;g.py=g.vy=g.vx=0;
        var buffer=typeof(ChibiGame).GetField("jumpBuffer",flags);int jumps=0;
        // A continuously held upward gesture queues a jump every frame, including airborne frames.
        for(int i=0;i<600;i++){buffer.SetValue(g,.15f);float old=g.vy;g.Step(1f/120,.78f);if(old<=0&&g.vy>0)jumps++;}
        check(jumps>=5&&g.px>20,"held diagonal repeats jumps and advances without release");
        var hashes=new System.Collections.Generic.HashSet<string>();
        for(int i=0;i<8;i++){var path="Assets/Resources/Art/alice_crouch"+i+".png";check(Resources.Load<Texture2D>("Art/alice_crouch"+i)!=null,"imported crouch frame");hashes.Add(Convert.ToBase64String(File.ReadAllBytes(path)));}
        check(hashes.Count==8,"eight distinct redrawn crouch frames");
        UnityEngine.Object.DestroyImmediate(go);Debug.Log("ALICE_MOBILE_CHECKS_PASS terrain=1000 repeat-jumps="+jumps+" sprites=8");
    }
}
