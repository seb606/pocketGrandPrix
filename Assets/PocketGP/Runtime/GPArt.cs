using UnityEngine;
using System.Collections.Generic;
namespace PocketGP {
public static class GPArt {
    static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();
    public static Color Hex(string value) { Color c; ColorUtility.TryParseHtmlString("#"+value,out c); return c; }
    public static Material Mat(string color, float gloss=.25f, float metal=0) {
        string key=color+gloss+metal; if(cache.ContainsKey(key)) return cache[key];
        var shader=Resources.Load<Shader>("PocketLit"); if(!shader) shader=Shader.Find("Standard");
        var m=new Material(shader); m.color=Hex(color); m.SetFloat("_Glossiness",gloss); m.SetFloat("_Metallic",metal); cache[key]=m; return m;
    }
    public static GameObject Shape(Transform parent,string name,PrimitiveType type,Vector3 p,Vector3 scale,Material mat) {
        var o=GameObject.CreatePrimitive(type); o.name=name; o.transform.SetParent(parent,false); o.transform.localPosition=p; o.transform.localScale=scale;
        o.GetComponent<Renderer>().sharedMaterial=mat; Object.Destroy(o.GetComponent<Collider>()); return o;
    }
    public static GameObject Box(Transform parent,string name,Vector3 p,Vector3 s,Material m) { return Shape(parent,name,PrimitiveType.Cube,p,s,m); }
    public static GameObject Sphere(Transform parent,string name,Vector3 p,Vector3 s,Material m) { return Shape(parent,name,PrimitiveType.Sphere,p,s,m); }
    public static GameObject Cylinder(Transform parent,string name,Vector3 p,Vector3 s,Material m) { return Shape(parent,name,PrimitiveType.Cylinder,p,s,m); }
    public static Transform Car(Transform parent,int color) {
        string[] colors={"FF684D","45DCD3","FCCB54","A290FF","F078B8"};
        var root=new GameObject("Carrosserie").transform; root.SetParent(parent,false);
        var paint=Mat(colors[color%5],.8f,.3f); var black=Mat("152B3A",.45f); var glass=Mat("25596B",.9f,.4f); var white=Mat("FFF5DA",.8f);
        Box(root,"Chassis",new Vector3(0,.27f,0),new Vector3(.94f,.23f,1.7f),black);
        Sphere(root,"Carrosserie arrondie",new Vector3(0,.48f,0),new Vector3(1.06f,.65f,1.85f),paint);
        Box(root,"Capot",new Vector3(0,.51f,.55f),new Vector3(.86f,.2f,.65f),paint);
        Sphere(root,"Verriere",new Vector3(0,.75f,-.13f),new Vector3(.77f,.59f,.88f),glass);
        Box(root,"Toit",new Vector3(0,.97f,-.23f),new Vector3(.61f,.075f,.40f),paint);
        Box(root,"Bande capot",new Vector3(0,.626f,.59f),new Vector3(.14f,.025f,.56f),white);
        Box(root,"Bande toit",new Vector3(0,1.015f,-.23f),new Vector3(.14f,.018f,.38f),white);
        Box(root,"Aileron",new Vector3(0,.73f,-.78f),new Vector3(1.13f,.09f,.19f),paint);
        for(int i=-1;i<=1;i+=2) {
            Box(root,"Phare",new Vector3(i*.31f,.49f,.899f),new Vector3(.23f,.14f,.055f),Mat("FFF5BA",.8f));
            Box(root,"Feu",new Vector3(i*.32f,.45f,-.84f),new Vector3(.23f,.12f,.06f),Mat("FA354B",.8f));
            for(int j=-1;j<=1;j+=2) {
                var w=Cylinder(root,"Roue",new Vector3(i*.50f,.29f,j*.57f),new Vector3(.47f,.115f,.47f),black); w.transform.localRotation=Quaternion.Euler(0,0,90);
                var hub=Cylinder(root,"Jante",new Vector3(i*.615f,.29f,j*.57f),new Vector3(.25f,.012f,.25f),Mat("CCDCE4",.9f,.8f)); hub.transform.localRotation=Quaternion.Euler(0,0,90);
            }
        }
        return root;
    }
    public static void Label(Transform parent,string value,Vector3 p,float size,Color color) {
        var o=new GameObject(value); o.transform.SetParent(parent,false); o.transform.localPosition=p; o.transform.localRotation=Quaternion.Euler(90,0,0);
        var t=o.AddComponent<TextMesh>(); t.text=value; t.fontSize=64; t.characterSize=size; t.anchor=TextAnchor.MiddleCenter; t.color=color;
    }
    public static void Decor(Transform parent,int theme,List<Vector3> path,float width) {
        var random=new System.Random(127+theme); var wood=Mat(theme==0?"DDB17A":theme==1?"647785":"7CAE72");
        Box(parent,"Table",new Vector3(0,-.7f,0),new Vector3(110,1.2f,100),wood);
        if(theme<2) for(int i=-5;i<=5;i++) Box(parent,"Joint de plateau",new Vector3(i*10,-.089f,0),new Vector3(.07f,.018f,100),Mat(theme==0?"BF925E":"5E707B"));
        for(int i=0;i<66;i++) {
            var pos=new Vector3((float)random.NextDouble()*100-50,0,(float)random.NextDouble()*88-44);
            float min=1000; foreach(var q in path) min=Mathf.Min(min,Vector3.Distance(q,pos));
            if(min<width*.5f+5.5f || pos.magnitude<4) continue;
            var root=new GameObject("Decor "+i).transform; root.SetParent(parent,false); root.localPosition=pos; root.localRotation=Quaternion.Euler(0,random.Next(360),0);
            string[] palette={"F28D79","67C7BB","ECC860","98A0DA"}; Material col=Mat(palette[i%4],.5f);
            if(theme==0) {
                if(i%3==0) { Cylinder(root,"Tasse",new Vector3(0,2,0),new Vector3(4,2,4),col); Cylinder(root,"Cafe",new Vector3(0,4.02f,0),new Vector3(3.4f,.025f,3.4f),Mat("56382D")); Sphere(root,"Anse",new Vector3(2,2,0),new Vector3(2.1f,2.6f,1),col); }
                else if(i%3==1) { Cylinder(root,"Assiette",new Vector3(0,.2f,0),new Vector3(7,.17f,7),Mat("FFF1D4",.65f)); for(int k=0;k<3;k++) Cylinder(root,"Biscuit",new Vector3(k-1,.65f+k*.2f,0),new Vector3(2.5f,.24f,2.5f),Mat("C48847")); }
                else { Box(root,"Boite cereales",new Vector3(0,3,0),new Vector3(4,6,1.9f),col); Box(root,"Etiquette",new Vector3(0,3,1),new Vector3(3,3,.03f),Mat("FFF2CC")); }
            } else if(theme==1) {
                if(i%3==0) { Box(root,"Livre",new Vector3(0,.5f,0),new Vector3(7,1,5),col); Box(root,"Pages",new Vector3(0,.51f,0),new Vector3(6.7f,.65f,4.8f),Mat("FFF4DB")); Box(root,"Couverture",new Vector3(0,1.04f,0),new Vector3(7,.15f,5),col); }
                else if(i%3==1) { var pencil=Cylinder(root,"Crayon",new Vector3(0,.6f,0),new Vector3(.8f,5,.8f),col); pencil.transform.localRotation=Quaternion.Euler(90,0,0); }
                else { Box(root,"Gomme",new Vector3(0,.7f,0),new Vector3(3,1.4f,2),col); Box(root,"Bande papier",new Vector3(0,.72f,0),new Vector3(1.3f,1.45f,2.03f),Mat("F5EEE0")); }
            } else {
                if(i%3==0) { Cylinder(root,"Pot",new Vector3(0,1.2f,0),new Vector3(3.5f,1.2f,3.5f),Mat("C97755")); Cylinder(root,"Tronc",new Vector3(0,3,0),new Vector3(.55f,2,.55f),Mat("886347")); Sphere(root,"Feuillage",new Vector3(0,5,0),new Vector3(5,5,5),Mat(i%2==0?"4D9166":"65AC70")); }
                else if(i%3==1) { Sphere(root,"Galet",new Vector3(0,.65f,0),new Vector3(3.8f,1.8f,3),Mat("A4ADB1",.4f)); }
                else { Cylinder(root,"Tige",new Vector3(0,1,0),new Vector3(.2f,1,.2f),Mat("4A8055")); for(int k=0;k<5;k++) { float a=k*Mathf.PI*2/5; Sphere(root,"Petale",new Vector3(Mathf.Cos(a)*.7f,2,Mathf.Sin(a)*.7f),new Vector3(1.2f,.4f,1.2f),col); } Sphere(root,"Coeur",new Vector3(0,2.2f,0),Vector3.one*.65f,Mat("FFE380")); }
            }
        }
    }
}
}
