using UnityEngine;
using System.Collections.Generic;

namespace PocketGP {
public enum GPItemType { None, Missile, OilSlick, SuperBoost, Shield }

public sealed class GPWeapons : MonoBehaviour {
    public static GPWeapons Instance;
    public sealed class ActiveMissile {
        public GameObject Obj;
        public Vector3 Position, Velocity;
        public GPCar Owner;
        public float Lifetime;
    }
    public sealed class ActiveOil {
        public GameObject Obj;
        public Vector3 Position;
        public float Lifetime;
    }

    public readonly List<ActiveMissile> Missiles = new List<ActiveMissile>();
    public readonly List<ActiveOil> Oils = new List<ActiveOil>();
    GPRace game;

    public void Init(GPRace g) {
        Instance = this;
        game = g;
    }

    public void Clear() {
        foreach (var m in Missiles) if (m.Obj) Destroy(m.Obj);
        foreach (var o in Oils) if (o.Obj) Destroy(o.Obj);
        Missiles.Clear();
        Oils.Clear();
    }

    public void SpawnMissile(GPCar owner) {
        var obj = GPArt.Missile(transform);
        Vector3 start = owner.transform.position + owner.transform.forward * 2.2f + Vector3.up * 0.45f;
        obj.transform.position = start;
        obj.transform.rotation = owner.transform.rotation;
        Missiles.Add(new ActiveMissile {
            Obj = obj,
            Position = start,
            Velocity = owner.transform.forward * Mathf.Max(owner.Speed + 14f, 26f),
            Owner = owner,
            Lifetime = 4.5f
        });
        if (game.Audio) game.Audio.PlayShoot();
    }

    public void SpawnOil(GPCar owner) {
        var obj = GPArt.OilPuddle(transform);
        Vector3 start = owner.transform.position - owner.transform.forward * 1.6f + Vector3.up * 0.035f;
        obj.transform.position = start;
        obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        Oils.Add(new ActiveOil {
            Obj = obj,
            Position = start,
            Lifetime = 18f
        });
        if (game.Audio) game.Audio.PlaySplat();
    }

    public void Step(float dt) {
        for (int i = Missiles.Count - 1; i >= 0; i--) {
            var m = Missiles[i];
            m.Lifetime -= dt;
            m.Position += m.Velocity * dt;
            if (m.Obj) {
                m.Obj.transform.position = m.Position;
                m.Obj.transform.rotation = Quaternion.LookRotation(m.Velocity);
            }

            bool hit = false;
            foreach (var car in game.Cars) {
                if (car == m.Owner && m.Lifetime > 4.2f) continue;
                if (car.FinishTime >= 0) continue;
                if (Vector3.Distance(car.transform.position, m.Position) < 1.7f) {
                    car.Hit(m.Velocity * 0.4f);
                    hit = true;
                    break;
                }
            }

            if (hit || m.Lifetime <= 0 || Mathf.Abs(m.Position.x) > 60 || Mathf.Abs(m.Position.z) > 55) {
                if (hit && game.Audio) game.Audio.PlayExplosion();
                if (m.Obj) Destroy(m.Obj);
                Missiles.RemoveAt(i);
            }
        }

        for (int i = Oils.Count - 1; i >= 0; i--) {
            var o = Oils[i];
            o.Lifetime -= dt;
            bool hit = false;
            foreach (var car in game.Cars) {
                if (car.FinishTime >= 0) continue;
                if (Vector3.Distance(car.transform.position, o.Position) < 1.6f) {
                    car.Spin();
                    hit = true;
                    break;
                }
            }
            if (hit || o.Lifetime <= 0) {
                if (hit && game.Audio) game.Audio.PlaySplat();
                if (o.Obj) Destroy(o.Obj);
                Oils.RemoveAt(i);
            }
        }
    }
}
}
