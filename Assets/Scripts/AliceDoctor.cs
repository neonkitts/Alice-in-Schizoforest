using UnityEngine;

public partial class ChibiGame
{
    public bool doctorSpawned,doctorFinished,doctorDialogue;
    public int doctorChoice,badges,pendingPower;
    public float doctorX;
    int currentAttackDamage=1;bool doctorVisit;
    float badgeNotice;
    float GroundAt(float x){float y=-12;foreach(var b in blocks)if(x>=b.r.xMin&&x<=b.r.xMax&&b.r.yMax<.9f)y=Mathf.Max(y,b.r.yMax);return y;}
    void PlaceRoadEvents(){if(length<600)return;doctorX=compactRoute?4+RoadLegDistance:150+Mathf.Abs(seed%19);catX=compactRoute?doctorX+RoadLegDistance:275+Mathf.Abs(seed%23);doctorSpawned=catSpawned=true;foreach(float x in new[]{doctorX,catX}){blocks.RemoveAll(b=>b.type>0&&b.type<5&&b.r.xMax>x-5&&b.r.xMin<x+5);bugs.RemoveAll(e=>Mathf.Abs(e.x-x)<8);mires.RemoveAll(r=>r.xMax>x-5&&r.xMin<x+5);}}

    void ResetDoctor(){doctorSpawned=doctorFinished=doctorDialogue=doctorVisit=false;doctorChoice=badges=pendingPower=0;badgeNotice=0;doctorX=0;currentAttackDamage=1;}
    void LoadDoctorArt(){for(int i=0;i<8;i++)art["doctor"+i]=Resources.Load<Texture2D>("Art/doctor"+i);art["doctor_badge"]=Resources.Load<Texture2D>("Art/doctor_badge");}
    public void UpdateDoctor()
    {
        if(tutorial||dungeonActive)return;
        if(Mathf.Abs(px-doctorX)>3.5f)doctorVisit=false;
        if(doctorSpawned&&Mathf.Abs(px-doctorX)<2&&!doctorVisit){doctorVisit=true;doctorDialogue=true;vx=vy=0;attack=0;}
    }
    public void ChooseDoctorReply(int choice)
    {
        if(!doctorDialogue)return;
        if(doctorFinished){doctorDialogue=false;return;}
        doctorChoice=choice;doctorFinished=true;doctorDialogue=false;
        if(choice==2){badges++;badgeNotice=4.5f;Notify("Получен артефакт «Аутизм» • инвентарь [I]");Play(chime);}
        Save();
    }
    public bool UseDoctorBadge()
    {
        if(badges<1)return false;
        badges--;pendingPower+=40;Play(chime);Save();return true;
    }
    void DrawDoctor()
    {
        if(!doctorSpawned||doctorX<cameraX-3||doctorX>cameraX+33)return;
        string pose="doctor"+((int)(Time.unscaledTime*5)%8);var tex=art[pose];float h=2.8f,w=tex?h*tex.width/tex.height:1.7f;Pic(pose,W(doctorX-w/2,GroundAt(doctorX),w,h));
    }
    void DrawBadgeReward(){
        Wood(new Rect(300,175,680,350));
        DrawTrophy("doctor_badge",new Vector2(640,300),210,154);
        Text("ПОЛУЧЕН АРТЕФАКТ «АУТИЗМ»",330,391,620,45,28,C("#ffe0ad"),true,TextAnchor.MiddleCenter);
        Text("В инвентаре [I] • следующий удар +40",330,454,620,40,23,C("#efd3d4"),false,TextAnchor.MiddleCenter);
    }
    void DrawDoctorDialogue()
    {
        Wood(new Rect(240,140,800,450));
        Text("ЧУМНОЙ ДОКТОР",330,200,620,38,27,C("#efcfaa"),true);
        Text("Над нами будут ставить опыты и превратят в собак.....",330,255,620,85,27,C("#f5dad0"));
        if(Button("Да. Было",340,365,600))ChooseDoctorReply(1);
        if(Button("Кто принес кал с кровью в гримерку???",340,445,600))ChooseDoctorReply(2);
    }
}
