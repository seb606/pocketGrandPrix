using UnityEngine;
namespace PocketGP {
public sealed class GPCar : MonoBehaviour {
    public int Id, Gate=1, Passed, Laps;
    public float Heading, Speed, Nitro=1, FinishTime=-1, DriftCharge;
    public Vector3 Velocity;
    public bool Human {get{return Id==0;}}
    public float RankProgress {get {return Passed*1000-Vector3.Distance(transform.position,Game.Track.Gates[Gate]);}}
    public GPRace Game;
    Transform body;
    TrailRenderer leftTrail,rightTrail;
    float rescueTimer,boostTime,smoothedSteer;
    Vector3 previous;
    public void Setup(GPRace game,int id) {
        Game=game;Id=id;body=GPArt.Car(transform,id==0?game.ColorIndex:id);
        var track=game.Track;Vector3 f=track.Tangent(0),r=Vector3.Cross(Vector3.up,f);
        transform.position=track.Points[0]-f*(2.5f+(id/2)*2.5f)+r*(id%2==0?-1.4f:1.4f);
        Heading=Quaternion.LookRotation(f).eulerAngles.y;transform.rotation=Quaternion.Euler(0,Heading,0);previous=transform.position;
        leftTrail=Trail(-.4f);rightTrail=Trail(.4f);
    }
    TrailRenderer Trail(float x) {
        var o=new GameObject("Trace pneu");o.transform.SetParent(transform,false);o.transform.localPosition=new Vector3(x,.052f,-.6f);
        var tr=o.AddComponent<TrailRenderer>();tr.time=1.5f;tr.minVertexDistance=.16f;tr.startWidth=.13f;tr.endWidth=.08f;tr.sharedMaterial=GPArt.Mat("253B43");tr.emitting=false;tr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return tr;
    }
    public void Step(float dt) {
        if(FinishTime>=0)return;
        float throttle=1,steer=0;bool drift=false,boost=false;
        float roadDistance;int nearest=Game.Track.Nearest(transform.position,out roadDistance);
        if(Human) {
            steer=(Input.GetKey(KeyCode.RightArrow)||Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.LeftArrow)||Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.Q)?1:0)+Game.UI.TouchSteer;
            steer=Mathf.Clamp(steer,-1,1);
            bool brake=Input.GetKey(KeyCode.DownArrow)||Input.GetKey(KeyCode.S)||Game.UI.Brake;
            throttle=brake?-0.7f:1;
            drift=Input.GetKey(KeyCode.Space)||Game.UI.Drift;boost=(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)||Game.UI.Boost)&&Nitro>0;
        } else {
            int target=Gate*8; // Aim at the next unvalidated checkpoint, including after a collision.
            Vector3 point=Game.Track.Points[target]+Vector3.Cross(Vector3.up,Game.Track.Tangent(target))*((Id-2.5f)*.45f);
            Vector3 dir=point-transform.position;float angle=Vector3.SignedAngle(transform.forward,dir,Vector3.up);
            steer=Mathf.Clamp(angle/30,-1,1);throttle=Mathf.Abs(angle)>65?.35f:1;
            boost=Mathf.Abs(angle)<10&&Nitro>.45f&&roadDistance<2;
        }
        float max=(Human?18.5f:16.3f+Game.Difficulty*1.5f+Id*.15f);
        if(roadDistance>Game.Track.Width*.5f)max*=.46f;
        if(boost){Nitro=Mathf.Max(0,Nitro-dt*.28f);max*=1.4f;}else Nitro=Mathf.Min(1,Nitro+dt*.035f);
        boostTime=Mathf.Max(0,boostTime-dt);if(boostTime>0)max*=1.22f;
        float targetSpeed=throttle<0?0:max*throttle;
        Speed=Mathf.MoveTowards(Speed,targetSpeed,dt*(throttle<0?24:boost?17:8));
        smoothedSteer=Mathf.Lerp(smoothedSteer,steer,dt*9);
        Heading+=smoothedSteer*(drift?124:105)*Mathf.Clamp01(Speed/5)*dt;
        transform.rotation=Quaternion.Euler(0,Heading,0);
        Vector3 desired=transform.forward*Speed;
        Velocity=Vector3.Lerp(Velocity,desired,1-Mathf.Exp(-(drift?3.3f:9)*dt));
        bool sliding=drift&&Speed>8&&Mathf.Abs(steer)>.2f;
        if(sliding)DriftCharge+=dt;
        if(!drift&&DriftCharge>0){if(DriftCharge>.7f){boostTime=.8f;Nitro=Mathf.Clamp01(Nitro+.08f);if(Human)Game.Notify("DÉRAPAGE TURBO !");}DriftCharge=0;}
        leftTrail.emitting=rightTrail.emitting=sliding;
        previous=transform.position;transform.position+=Velocity*dt;
        body.localRotation=Quaternion.Euler(Mathf.Sin(Time.time*22)*Speed*.025f,0,-smoothedSteer*Speed*.32f);
        Vector3 gate=Game.Track.Gates[Gate];Vector3 segment=transform.position-previous;
        float u=segment.sqrMagnitude>.0001f?Mathf.Clamp01(Vector3.Dot(gate-previous,segment)/segment.sqrMagnitude):0;
        // Sequential checkpoints: driving backwards or cutting the infield cannot complete a lap.
        if(Vector3.Distance(previous+segment*u,gate)<Game.Track.Width*.5f+1.2f) {
            Passed++;if(Gate==0){Laps++;if(Laps>=GPRace.TotalLaps){FinishTime=Game.RaceTime;Game.Finished(this);}}
            Gate=(Gate+1)%Game.Track.Gates.Count;
        }
        rescueTimer=roadDistance>Game.Track.Width+3?rescueTimer+dt:0;
        if(rescueTimer>2.5f||Mathf.Abs(transform.position.x)>54||Mathf.Abs(transform.position.z)>47)Rescue();
        if(Human&&Input.GetKey(KeyCode.R))Rescue();
    }
    public void Rescue() {
        // Return to the last validated checkpoint, never to a point ahead of it.
        int last=(Gate+Game.Track.Gates.Count-1)%Game.Track.Gates.Count;
        transform.position=Game.Track.Gates[last];int i=last*8;
        Heading=Quaternion.LookRotation(Game.Track.Tangent(i)).eulerAngles.y;
        transform.rotation=Quaternion.Euler(0,Heading,0);Velocity=Vector3.zero;Speed=0;rescueTimer=0;leftTrail.Clear();rightTrail.Clear();
    }
    public void Pad(){boostTime=1.7f;if(Human)Game.Notify("TURBO !");}
}
}
