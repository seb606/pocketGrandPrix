using UnityEngine;
namespace PocketGP {
public sealed class GPAudio : MonoBehaviour {
    AudioSource music,engine,fx;AudioClip beep,coin;float musicVolume=.35f,effectsVolume=.65f;
    public float MusicVolume{get{return musicVolume;}set{musicVolume=value;if(music)music.volume=value;PlayerPrefs.SetFloat("music",value);}}
    public float EffectsVolume{get{return effectsVolume;}set{effectsVolume=value;if(fx)fx.volume=value;PlayerPrefs.SetFloat("effects",value);}}
    AudioClip Tone(string name,float freq,float seconds,bool sweep=false) {
        const int rate=22050;float[] samples=new float[(int)(rate*seconds)];
        for(int i=0;i<samples.Length;i++){float t=i/(float)rate;float env=Mathf.Min(t*80,1)*Mathf.Pow(1-t/seconds,2);samples[i]=Mathf.Sin(2*Mathf.PI*(freq*t+(sweep?freq*t*t:0)))*env*.5f;}
        var clip=AudioClip.Create(name,samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
    }
    void Awake(){
        music=gameObject.AddComponent<AudioSource>();engine=gameObject.AddComponent<AudioSource>();fx=gameObject.AddComponent<AudioSource>();
        musicVolume=PlayerPrefs.GetFloat("music",.35f);effectsVolume=PlayerPrefs.GetFloat("effects",.65f);music.volume=musicVolume;fx.volume=effectsVolume;
        beep=Tone("Signal",660,.17f);coin=Tone("Collecte",880,.24f,true);
        const int rate=22050;int total=rate*16;float[] data=new float[total];int[] notes={0,7,12,7,3,10,15,10,5,12,17,12,7,14,19,14};
        for(int i=0;i<total;i++) {
            float t=i/(float)rate;int step=(int)(t*4);float beat=t*4-step;float f=130.81f*Mathf.Pow(2,notes[step%16]/12f);
            float lead=(Mathf.Sin(t*f*Mathf.PI*2)+.25f*Mathf.Sin(t*f*Mathf.PI*4))*Mathf.Exp(-beat*5)*.12f;
            float bass=Mathf.Sin(t*65.405f*Mathf.PI*2)*Mathf.Exp(-beat*4)*.15f;
            float kick=Mathf.Sin(2*Mathf.PI*(45*beat/4+6*(1-Mathf.Exp(-beat*10))))*Mathf.Exp(-beat*14)*.23f;
            float hat=Mathf.Sin(t*8171)*Mathf.Sin(t*5213)*Mathf.Exp(-beat*40)*.055f;data[i]=lead+bass+kick+hat;
        }
        music.clip=AudioClip.Create("Pocket groove",total,1,rate,false);music.clip.SetData(data,0);music.loop=true;
        float[] hum=new float[2205];for(int i=0;i<hum.Length;i++){float t=i/(float)rate;hum[i]=(Mathf.Sin(t*80*Mathf.PI*2)+Mathf.Sin(t*160*Mathf.PI*2)*.35f)*.22f;}
        engine.clip=AudioClip.Create("Moteur",hum.Length,1,rate,false);engine.clip.SetData(hum,0);engine.loop=true;
    }
    public void StartAudio(){if(!music.isPlaying)music.Play();if(!engine.isPlaying)engine.Play();}
    public void Engine(float speed,bool active){engine.volume=active?effectsVolume*.22f:0;engine.pitch=.65f+speed*.065f;}
    public void Beep(bool pickup=false){fx.PlayOneShot(pickup?coin:beep);}
    void OnDestroy(){if(music&&music.clip)Destroy(music.clip);if(engine&&engine.clip)Destroy(engine.clip);if(beep)Destroy(beep);if(coin)Destroy(coin);}
}
}
