using System.Runtime.InteropServices;
using UnityEngine;
public class YandexBridge:MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void AliceReady();
    [DllImport("__Internal")] static extern void AliceGameplay(int playing);
#endif
    ChibiGame game;bool playing;
    void Start(){game=GetComponent<ChibiGame>();
#if UNITY_WEBGL && !UNITY_EDITOR
        AliceReady();
#endif
    }
    void Update(){bool next=game.mode==ChibiGame.Mode.Play;if(next==playing)return;playing=next;
#if UNITY_WEBGL && !UNITY_EDITOR
        AliceGameplay(playing?1:0);
#endif
    }
    public void PlatformPause(){if(game.mode==ChibiGame.Mode.Play)game.mode=ChibiGame.Mode.Pause;AudioListener.pause=true;}
    public void PlatformResume(){AudioListener.pause=false;}
    void OnApplicationFocus(bool focus){AudioListener.pause=!focus;}
}
