using UnityEngine;
using System;
using System.Reflection;
public static class AlicePersistenceChecks {
 public static void Run(){
  var go=new GameObject("PersistenceChecks");var g=go.AddComponent<ChibiGame>();
  var f=BindingFlags.Instance|BindingFlags.NonPublic;
  Action<string,object> set=(n,v)=>typeof(ChibiGame).GetField(n,f).SetValue(g,v);
  Action<string> call=n=>typeof(ChibiGame).GetMethod(n,f).Invoke(g,null);
  Action<bool,string> check=(ok,n)=>{if(!ok)throw new Exception("PERSISTENCE_CHECK_FAILED: "+n);};
  set("autoRun",true);g.NewGame(919,ChibiGame.FullLength);
  g.px=1800;g.py=0;g.blocks.RemoveAll(b=>b.type>0&&b.r.xMin<1802&&b.r.xMax>1798);set("grounded",true);
  g.catRescued=true;g.lifePotions=1;call("Save");g.px=4;g.catRescued=false;call("Continue");
  check(Mathf.Abs(g.px-1800)<.01f&&g.catRescued&&g.lifePotions==1,"position and cat restore together at mid-map");
  g.px=1850;g.py=8;set("grounded",false);call("Save");call("Continue");
  check(Mathf.Abs(g.px-1800)<.01f,"airborne save returns to latest supported position");
  call("Save");PlayerPrefs.SetString("alice.smoke","{broken");call("Continue");
  check(g.mode==ChibiGame.Mode.Play&&Mathf.Abs(g.px-1800)<.01f,"corrupt primary restores valid backup");
  g.NewGame(919,ChibiGame.FullLength);g.px=g.doctorX;g.UpdateDoctor();check(g.doctorDialogue,"doctor opens dialogue on approach");
  g.ChooseDoctorReply(2);int badges=g.badges;g.px=g.doctorX+5;g.UpdateDoctor();g.px=g.doctorX;g.UpdateDoctor();
  check(g.doctorDialogue,"doctor can be revisited");g.ChooseDoctorReply(2);check(g.badges==badges,"repeat conversation does not duplicate reward");
  call("Continue");g.px=g.doctorX;g.UpdateDoctor();check(g.doctorDialogue,"doctor remains interactive after loading");
  PlayerPrefs.DeleteKey("alice.smoke");PlayerPrefs.DeleteKey("alice.smoke.backup");PlayerPrefs.Save();
  UnityEngine.Object.DestroyImmediate(go);Debug.Log("ALICE_PERSISTENCE_CHECKS_PASS checks=7");
 }
}
