using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
namespace PocketGP {
public sealed class GPUi : MonoBehaviour {
    GPRace game;Canvas canvas;CanvasScaler scaler;RectTransform layer;Font font;
    Text hud,count,notice;Image nitro;bool left,right;public bool Brake,Drift,Boost;
    public float TouchSteer{get{return (right?1:0)-(left?1:0);}}
    readonly Color ink=GPArt.Hex("F8EED9"),muted=GPArt.Hex("AEC8CC"),panel=GPArt.Hex("172E40"),mint=GPArt.Hex("54E3C5"),coral=GPArt.Hex("FF785A");
    public void Init(GPRace g){
        game=g;font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var o=new GameObject("Interface",typeof(RectTransform));o.transform.SetParent(transform,false);canvas=o.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        scaler=o.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        o.AddComponent<GraphicRaycaster>();
        if(!FindFirstObjectByType<EventSystem>()){var e=new GameObject("EventSystem");e.AddComponent<EventSystem>();e.AddComponent<StandaloneInputModule>();}
        Responsive();
    }
    void Responsive(){bool portrait=Screen.height>Screen.width;scaler.referenceResolution=portrait?new Vector2(760,1200):new Vector2(1280,900);scaler.matchWidthOrHeight=portrait?0:1;}
    RectTransform Rect(Transform p,string n,Vector2 anchor,Vector2 pos,Vector2 size){var o=new GameObject(n,typeof(RectTransform));o.transform.SetParent(p,false);var r=o.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=pos;r.sizeDelta=size;return r;}
    Image Block(Transform p,Vector2 anchor,Vector2 pos,Vector2 size,Color color){var r=Rect(p,"Panel",anchor,pos,size);var image=r.gameObject.AddComponent<Image>();image.color=color;return image;}
    Text Txt(Transform p,string value,Vector2 pos,Vector2 size,int fontsize,Color color,TextAnchor align=TextAnchor.MiddleCenter){var r=Rect(p,value,new Vector2(.5f,.5f),pos,size);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=fontsize;t.color=color;t.alignment=align;t.raycastTarget=false;return t;}
    Button Button(Transform p,string title,Vector2 pos,Vector2 size,UnityAction action,bool primary=false){var image=Block(p,new Vector2(.5f,.5f),pos,size,primary?mint:GPArt.Hex("29485B"));var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;var colors=b.colors;colors.highlightedColor=new Color(1,1,1,.86f);colors.pressedColor=new Color(.72f,.82f,.86f);b.colors=colors;b.onClick.AddListener(()=>{game.Audio.StartAudio();game.Audio.Beep();action();});Txt(image.transform,title,Vector2.zero,size-new Vector2(12,4),24,primary?panel:ink);return b;}
    void Reset(){ClearInput();hud=count=notice=null;nitro=null;if(layer){layer.gameObject.SetActive(false);Destroy(layer.gameObject);}layer=Rect(canvas.transform,"Ecran",new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);layer.anchorMin=Vector2.zero;layer.anchorMax=Vector2.one;layer.offsetMin=layer.offsetMax=Vector2.zero;}
    RectTransform Card(string eyebrow,string title){Reset();var veil=Block(layer,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero,new Color(.025f,.08f,.12f,.6f));veil.rectTransform.anchorMin=Vector2.zero;veil.rectTransform.anchorMax=Vector2.one;veil.rectTransform.offsetMin=veil.rectTransform.offsetMax=Vector2.zero;
        var card=Block(layer,new Vector2(.5f,.5f),Vector2.zero,new Vector2(680,820),new Color(panel.r,panel.g,panel.b,.97f)).rectTransform;
        Block(card,new Vector2(.5f,.5f),new Vector2(0,404),new Vector2(680,12),mint);
        Txt(card,eyebrow,new Vector2(0,350),new Vector2(620,30),18,mint);Txt(card,title,new Vector2(0,295),new Vector2(640,85),43,ink);return card;
    }
    public void ShowMenu(){var c=Card("PETITES VOITURES. GRANDES COURSES.","POCKET GRAND PRIX");
        Txt(c,"Le salon devient votre terrain de jeu.",new Vector2(0,226),new Vector2(620,38),22,muted);
        Txt(c,"CIRCUIT",new Vector2(0,174),new Vector2(600,30),17,mint);
        Button(c,"‹",new Vector2(-273,125),new Vector2(54,54),()=>{game.TrackIndex=(game.TrackIndex+2)%3;game.BuildWorld(game.TrackIndex);ShowMenu();});
        Txt(c,GPTrack.Names[game.TrackIndex],new Vector2(0,129),new Vector2(480,55),25,ink);
        Button(c,"›",new Vector2(273,125),new Vector2(54,54),()=>{game.TrackIndex=(game.TrackIndex+1)%3;game.BuildWorld(game.TrackIndex);ShowMenu();});
        float best=PlayerPrefs.GetFloat("best_"+game.TrackIndex+"_"+game.Difficulty,0);Txt(c,best>0?"Record : "+TimeText(best):"3 tours  ·  5 pilotes  ·  un podium",new Vector2(0,80),new Vector2(620,30),18,muted);
        string[] dif={"Facile","Moyen","Difficile"};for(int i=0;i<3;i++){int k=i;Button(c,dif[i],new Vector2((i-1)*204,18),new Vector2(194,50),()=>{game.Difficulty=k;ShowMenu();},game.Difficulty==i);}
        string[] names={"Corail","Menthe","Citron","Lilas","Rose"};Button(c,"Voiture : "+names[game.ColorIndex]+"   ›",new Vector2(0,-47),new Vector2(604,50),()=>{game.ColorIndex=(game.ColorIndex+1)%5;game.BuildWorld(game.TrackIndex);ShowMenu();});
        Button(c,"COURSE RAPIDE",new Vector2(0,-123),new Vector2(604,66),()=>game.StartRace(),true);
        Button(c,"CHAMPIONNAT · 3 CIRCUITS",new Vector2(0,-199),new Vector2(604,62),()=>game.StartRace(true));
        Button(c,"Comment jouer",new Vector2(-154,-269),new Vector2(296,52),ShowHelp);Button(c,"Audio & affichage",new Vector2(154,-269),new Vector2(296,52),()=>ShowSettings(false));
        Txt(c,"Accélération automatique · clavier ou écran tactile",new Vector2(0,-337),new Vector2(620,52),19,muted);
    }
    public void ShowHelp(){var c=Card("LE GUIDE DU PILOTE","PETIT FORMAT, GRAND FUN");
        Txt(c,"L'accélération est automatique.\n\n← / → ou Q / D (A / D) : tourner\n↓ ou S : freiner · Espace : déraper\nMaj : turbo · R : retour sur la piste\nP ou Échap : pause\n\nTéléphone : boutons tactiles en bas de l'écran.\nMaintenir DRIFT dans les virages, puis relâcher\npour obtenir un petit turbo.\n\nLes jetons rechargent le turbo et donnent 100 points.\nLes bandes vertes accélèrent toutes les voitures.\nRester sur la route : l'extérieur ralentit fortement.\n\nTerminer 3 tours en passant les points de contrôle.\nChampionnat : 10 / 7 / 5 / 3 / 1 points par course.",new Vector2(0,-25),new Vector2(610,525),23,ink);
        Button(c,"RETOUR",new Vector2(0,-345),new Vector2(604,58),ShowMenu,true);
    }
    void Slider(Transform p,string label,float y,float value,UnityAction<float> change){Txt(p,label,new Vector2(0,y+30),new Vector2(600,34),23,ink);var bg=Block(p,new Vector2(.5f,.5f),new Vector2(0,y-10),new Vector2(550,30),GPArt.Hex("35546A"));var s=bg.gameObject.AddComponent<Slider>();s.minValue=0;s.maxValue=1;
        var handle=Block(bg.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(28,42),mint);s.handleRect=handle.rectTransform;s.targetGraphic=handle;s.direction=UnityEngine.UI.Slider.Direction.LeftToRight;s.value=value;s.onValueChanged.AddListener(change);
    }
    public void ShowSettings(bool paused){var c=Card("À VOTRE RYTHME","AUDIO & AFFICHAGE");Slider(c,"Musique",155,game.Audio.MusicVolume,v=>game.Audio.MusicVolume=v);Slider(c,"Effets sonores",40,game.Audio.EffectsVolume,v=>game.Audio.EffectsVolume=v);
        Button(c,"Écouter un son test",new Vector2(0,-70),new Vector2(604,56),()=>game.Audio.Beep(true));
        Button(c,"Qualité : "+(QualitySettings.shadows==ShadowQuality.Disable?"Éco":"Élevée"),new Vector2(0,-143),new Vector2(604,56),()=>{bool eco=QualitySettings.shadows!=ShadowQuality.Disable;QualitySettings.shadows=eco?ShadowQuality.Disable:ShadowQuality.All;QualitySettings.antiAliasing=eco?0:2;ShowSettings(paused);});
        Txt(c,"En cas de ralentissement sur téléphone,\nchoisir la qualité Éco. Plein écran : bouton de la page Web.",new Vector2(0,-237),new Vector2(610,75),22,muted);
        Button(c,"RETOUR",new Vector2(0,-345),new Vector2(604,58),()=>{PlayerPrefs.Save();if(paused)ShowPause();else ShowMenu();},true);
    }
    void Hold(string label,Vector2 anchor,Vector2 position,Vector2 size,System.Action<bool> callback){var image=Block(layer,anchor,position,size,new Color(.08f,.19f,.26f,.88f));Txt(image.transform,label,Vector2.zero,size,25,ink);var h=image.gameObject.AddComponent<GPHold>();h.Changed=callback;}
    public void ClearInput(){left=right=Brake=Drift=Boost=false;}
    public void ShowHUD(){Reset();var header=Block(layer,new Vector2(0,1),new Vector2(245,-64),new Vector2(460,98),new Color(.06f,.15f,.21f,.88f));hud=Txt(header.transform,"",Vector2.zero,new Vector2(426,85),25,ink,TextAnchor.MiddleLeft);
        var pause=Button(layer,"PAUSE",Vector2.zero,new Vector2(150,58),()=>game.Pause());var pr=pause.GetComponent<RectTransform>();pr.anchorMin=pr.anchorMax=new Vector2(1,1);pr.anchoredPosition=new Vector2(-94,-46);
        var bar=Block(layer,new Vector2(0,1),new Vector2(245,-129),new Vector2(450,12),panel);nitro=Block(bar.transform,new Vector2(0,.5f),Vector2.zero,new Vector2(450,12),mint);nitro.rectTransform.pivot=new Vector2(0,.5f);
        count=Txt(layer,"",new Vector2(0,30),new Vector2(500,150),110,ink);notice=Txt(layer,"",new Vector2(0,145),new Vector2(680,55),27,mint);
        Hold("‹",new Vector2(0,0),new Vector2(76,87),new Vector2(112,115),v=>left=v);Hold("›",new Vector2(0,0),new Vector2(200,87),new Vector2(112,115),v=>right=v);
        Hold("FREIN",new Vector2(1,0),new Vector2(-308,78),new Vector2(108,96),v=>Brake=v);Hold("DRIFT",new Vector2(1,0),new Vector2(-186,78),new Vector2(118,96),v=>Drift=v);Hold("TURBO",new Vector2(1,0),new Vector2(-65,100),new Vector2(112,140),v=>Boost=v);
        // Touch controls remain available on hybrid PCs and browser device emulation.
    }
    public void ShowPause(){var c=Card("ON SOUFFLE UN PEU","PAUSE AU STAND");Button(c,"REPRENDRE",new Vector2(0,110),new Vector2(604,70),()=>game.Pause(),true);Button(c,"Audio & affichage",new Vector2(0,15),new Vector2(604,60),()=>ShowSettings(true));Button(c,"Recommencer",new Vector2(0,-75),new Vector2(604,60),()=>game.Retry());Button(c,"Menu principal",new Vector2(0,-165),new Vector2(604,60),()=>game.Menu());}
    public void ShowResults(){var c=Card(GPTrack.Names[game.TrackIndex].ToUpperInvariant(),game.ResultPlace==1?"VICTOIRE !":"COURSE TERMINÉE");
        Txt(c,game.ResultPlace+" / 5",new Vector2(0,155),new Vector2(600,110),76,mint);Txt(c,"Temps : "+TimeText(game.FinalTime)+"\nJetons : "+game.Score+" points",new Vector2(0,42),new Vector2(600,90),28,ink);
        Txt(c,game.Championship?"Championnat · manche "+(game.TrackIndex+1)+" / 3":"Record enregistré sur cet appareil.",new Vector2(0,-38),new Vector2(610,45),22,muted);
        if(game.Championship)Button(c,game.TrackIndex<2?"CIRCUIT SUIVANT":"RÉSULTAT DU CHAMPIONNAT",new Vector2(0,-136),new Vector2(604,64),()=>game.Next(),true);else Button(c,"REJOUER",new Vector2(0,-136),new Vector2(604,64),()=>game.Retry(),true);
        Button(c,"MENU PRINCIPAL",new Vector2(0,-219),new Vector2(604,60),()=>game.Menu());
    }
    public void ShowChampionship(){var c=Card("LES TROIS CIRCUITS SONT TERMINÉS","FIN DU CHAMPIONNAT");Txt(c,game.ChampionshipPoints+" / 30",new Vector2(0,115),new Vector2(620,110),70,mint);Txt(c,game.ChampionshipPoints>=25?"Trophée OR":game.ChampionshipPoints>=18?"Trophée ARGENT":"Trophée BRONZE",new Vector2(0,8),new Vector2(600,60),34,ink);Button(c,"MENU PRINCIPAL",new Vector2(0,-180),new Vector2(604,66),()=>game.Menu(),true);}
    public static string TimeText(float t){return ((int)t/60).ToString("00")+":"+((int)t%60).ToString("00")+"."+((int)(t*100)%100).ToString("00");}
    public void Refresh(){Responsive();if(hud&&game.Cars.Count>0){hud.text=game.Position()+" / 5     TOUR "+Mathf.Min(game.Cars[0].Laps+1,GPRace.TotalLaps)+" / "+GPRace.TotalLaps+"\n"+TimeText(game.RaceTime)+"     "+game.Score+" pts";nitro.rectTransform.sizeDelta=new Vector2(450*game.Cars[0].Nitro,12);count.text=game.State==GPState.Countdown?Mathf.Max(1,Mathf.CeilToInt(game.Countdown)).ToString():"";notice.text=game.Notice;}}
}
}
