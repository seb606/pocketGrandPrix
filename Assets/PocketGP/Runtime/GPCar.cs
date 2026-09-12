using UnityEngine;

namespace PocketGP {
public sealed class GPCar : MonoBehaviour {
    public int Id, Gate = 1, Passed, Laps;
    public float Heading, Speed, Nitro = 1, FinishTime = -1, DriftCharge;
    public Vector3 Velocity;
    public bool Human { get { return Id == 0; } }
    public float RankProgress { get { return Passed * 1000 - Vector3.Distance(transform.position, Game.Track.Gates[Gate]); } }
    public GPRace Game;
    public GPItemType CurrentItem = GPItemType.None;
    public bool HasShield;

    Transform body;
    TrailRenderer leftTrail, rightTrail;
    GameObject flameL, flameR, shieldObj;
    float rescueTimer, boostTime, smoothedSteer, spinTimer, aiItemTimer;
    Vector3 previous;

    public void Setup(GPRace game, int id) {
        Game = game;
        Id = id;
        body = GPArt.Car(transform, id == 0 ? game.ColorIndex : id);
        var track = game.Track;
        Vector3 f = track.Tangent(0), r = Vector3.Cross(Vector3.up, f);
        transform.position = track.Points[0] - f * (2.5f + (id / 2) * 2.5f) + r * (id % 2 == 0 ? -1.4f : 1.4f);
        Heading = Quaternion.LookRotation(f).eulerAngles.y;
        transform.rotation = Quaternion.Euler(0, Heading, 0);
        previous = transform.position;

        leftTrail = Trail(-.4f);
        rightTrail = Trail(.4f);

        // Flammes d'échappement turbo
        flameL = GPArt.Flame(transform, new Vector3(-.32f, .45f, -.88f));
        flameR = GPArt.Flame(transform, new Vector3(.32f, .45f, -.88f));

        // Bouclier
        shieldObj = GPArt.Shield(transform);
        shieldObj.SetActive(false);

        aiItemTimer = Random.Range(2f, 5f);
    }

    TrailRenderer Trail(float x) {
        var o = new GameObject("Trace pneu");
        o.transform.SetParent(transform, false);
        o.transform.localPosition = new Vector3(x, .052f, -.6f);
        var tr = o.AddComponent<TrailRenderer>();
        tr.time = 1.5f;
        tr.minVertexDistance = .16f;
        tr.startWidth = .13f;
        tr.endWidth = .08f;
        tr.sharedMaterial = GPArt.Mat("253B43");
        tr.emitting = false;
        tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return tr;
    }

    public void Step(float dt) {
        if (FinishTime >= 0) {
            if (Human) {
                Game.Audio.SetSkid(false, 0);
                Game.Audio.SetTurbo(false);
            }
            return;
        }

        // Effet de rotation en tête-à-queue
        if (spinTimer > 0) {
            spinTimer -= dt;
            Heading += 720 * dt;
            transform.rotation = Quaternion.Euler(0, Heading, 0);
            Speed = Mathf.MoveTowards(Speed, 0, dt * 18);
            Velocity = Vector3.MoveTowards(Velocity, Vector3.zero, dt * 14);
            transform.position += Velocity * dt;
            if (Human) {
                Game.Audio.SetSkid(true, 0.8f);
                Game.Audio.SetTurbo(false);
            }
            return;
        }

        float throttle = 1, steer = 0;
        bool drift = false, boost = false, useItem = false;
        float roadDistance;
        int nearest = Game.Track.Nearest(transform.position, out roadDistance);

        if (Human) {
            steer = (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) ? 1 : 0) -
                    (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q) ? 1 : 0) +
                    Game.UI.TouchSteer;
            steer = Mathf.Clamp(steer, -1, 1);

            bool brake = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || Game.UI.Brake;
            throttle = brake ? -0.7f : 1;

            drift = Input.GetKey(KeyCode.Space) || Game.UI.Drift;
            boost = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Game.UI.Boost) && Nitro > 0;

            useItem = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.LeftControl) || Game.UI.ItemTrigger;
            Game.UI.ItemTrigger = false;
        } else {
            int target = Gate * 8;
            Vector3 point = Game.Track.Points[target] + Vector3.Cross(Vector3.up, Game.Track.Tangent(target)) * ((Id - 2.5f) * .45f);
            Vector3 dir = point - transform.position;
            float angle = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
            steer = Mathf.Clamp(angle / 30, -1, 1);
            throttle = Mathf.Abs(angle) > 65 ? .35f : 1;
            boost = Mathf.Abs(angle) < 10 && Nitro > .45f && roadDistance < 2;

            if (CurrentItem != GPItemType.None) {
                aiItemTimer -= dt;
                if (aiItemTimer <= 0) {
                    useItem = true;
                    aiItemTimer = Random.Range(3f, 7f);
                }
            }
        }

        if (useItem && CurrentItem != GPItemType.None) {
            TriggerItem();
        }

        float max = (Human ? 18.5f : 16.3f + Game.Difficulty * 1.5f + Id * .15f);
        if (roadDistance > Game.Track.Width * .5f) max *= .46f;

        bool isBoosting = boost || boostTime > 0;
        if (boost) {
            Nitro = Mathf.Max(0, Nitro - dt * .28f);
            max *= 1.42f;
        } else {
            Nitro = Mathf.Min(1, Nitro + dt * .035f);
        }

        boostTime = Mathf.Max(0, boostTime - dt);
        if (boostTime > 0) max *= 1.25f;

        // Activation visuelle des flammes turbo
        if (flameL && flameR) {
            bool showFlame = isBoosting && Speed > 7f;
            flameL.SetActive(showFlame);
            flameR.SetActive(showFlame);
            if (showFlame) {
                float s = 1f + Mathf.PingPong(Time.time * 25f, 0.4f);
                flameL.transform.localScale = Vector3.one * s;
                flameR.transform.localScale = Vector3.one * s;
            }
        }

        float targetSpeed = throttle < 0 ? 0 : max * throttle;
        Speed = Mathf.MoveTowards(Speed, targetSpeed, dt * (throttle < 0 ? 24 : isBoosting ? 20 : 8.5f));
        smoothedSteer = Mathf.Lerp(smoothedSteer, steer, dt * 9);
        Heading += smoothedSteer * (drift ? 124 : 105) * Mathf.Clamp01(Speed / 5) * dt;
        transform.rotation = Quaternion.Euler(0, Heading, 0);

        Vector3 desired = transform.forward * Speed;
        Velocity = Vector3.Lerp(Velocity, desired, 1 - Mathf.Exp(-(drift ? 3.3f : 9) * dt));

        bool sliding = drift && Speed > 6f && Mathf.Abs(steer) > .15f;
        if (sliding) DriftCharge += dt;
        if (!drift && DriftCharge > 0) {
            if (DriftCharge > .65f) {
                boostTime = .9f;
                Nitro = Mathf.Clamp01(Nitro + .12f);
                if (Human) {
                    Game.Notify("DÉRAPAGE TURBO !");
                    Game.Audio.PlayShoot();
                }
            }
            DriftCharge = 0;
        }

        leftTrail.emitting = rightTrail.emitting = sliding;
        if (sliding) {
            Color trColor = DriftCharge > .65f ? GPArt.Hex("45DCD3") : GPArt.Hex("FCCB54");
            leftTrail.sharedMaterial = GPArt.Mat(DriftCharge > .65f ? "45DCD3" : "FCCB54", 0.9f);
            rightTrail.sharedMaterial = leftTrail.sharedMaterial;
        }

        // Gestion audio pour le joueur humain
        if (Human) {
            Game.Audio.SetSkid(sliding, Mathf.Clamp01(Mathf.Abs(steer) * (Speed / 12f)));
            Game.Audio.SetTurbo(isBoosting);
        }

        previous = transform.position;
        transform.position += Velocity * dt;
        body.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 22) * Speed * .025f, 0, -smoothedSteer * Speed * .32f);

        // --- VALIDATION ROBUSTE DES TOURS ET CHECKPOINTS ---
        int nextGate = (Gate + 1) % Game.Track.Gates.Count;
        Vector3 gatePos = Game.Track.Gates[Gate];
        Vector3 gateTangent = Game.Track.Tangent(Gate * 8);

        float prevDot = Vector3.Dot(previous - gatePos, gateTangent);
        float currDot = Vector3.Dot(transform.position - gatePos, gateTangent);
        float latDist = Vector3.Distance(transform.position, gatePos);

        bool crossedGate = (prevDot <= 0.1f && currDot >= -0.1f && latDist < Game.Track.Width * 1.5f + 3f) ||
                          (latDist < Game.Track.Width * 0.7f + 1.2f);

        // Tolérance : si le joueur a glissé et se retrouve déjà au checkpoint suivant
        Vector3 nextPos = Game.Track.Gates[nextGate];
        if (!crossedGate && Vector3.Distance(transform.position, nextPos) < Game.Track.Width * 0.7f + 1.2f) {
            crossedGate = true;
        }

        if (crossedGate) {
            Passed++;
            if (Gate == 0) {
                Laps++;
                if (Human) {
                    if (Laps >= GPRace.TotalLaps) {
                        Game.Notify("ARRIVÉE !");
                    } else {
                        Game.Audio.PlayLap();
                        Game.Notify(Laps == GPRace.TotalLaps - 1 ? "DERNIER TOUR !" : "TOUR " + (Laps + 1) + " / " + GPRace.TotalLaps);
                    }
                }
                if (Laps >= GPRace.TotalLaps) {
                    FinishTime = Game.RaceTime;
                    Game.Finished(this);
                }
            }
            Gate = nextGate;
        }

        rescueTimer = roadDistance > Game.Track.Width + 3 ? rescueTimer + dt : 0;
        if (rescueTimer > 2.5f || Mathf.Abs(transform.position.x) > 54 || Mathf.Abs(transform.position.z) > 47) Rescue();
        if (Human && Input.GetKey(KeyCode.R)) Rescue();
    }

    public void GiveItem(GPItemType item) {
        CurrentItem = item;
        if (Human) {
            Game.Audio.PlayItemGet();
            string[] names = { "", "MISSILE (Touche E)", "FLAQUE D'HUILE (Touche E)", "SUPER TURBO (Touche E)", "BOUCLIER (Touche E)" };
            Game.Notify(names[(int)item]);
        }
    }

    void TriggerItem() {
        switch (CurrentItem) {
            case GPItemType.Missile:
                if (GPWeapons.Instance) GPWeapons.Instance.SpawnMissile(this);
                break;
            case GPItemType.OilSlick:
                if (GPWeapons.Instance) GPWeapons.Instance.SpawnOil(this);
                break;
            case GPItemType.SuperBoost:
                Pad();
                Nitro = 1f;
                if (Human) Game.Notify("SUPER TURBO !");
                break;
            case GPItemType.Shield:
                HasShield = true;
                if (shieldObj) shieldObj.SetActive(true);
                if (Human) Game.Notify("BOUCLIER ACTIF !");
                break;
        }
        CurrentItem = GPItemType.None;
    }

    public void Hit(Vector3 force) {
        if (HasShield) {
            HasShield = false;
            if (shieldObj) shieldObj.SetActive(false);
            if (Human) Game.Notify("BOUCLIER ABSORBÉ !");
            return;
        }
        spinTimer = 0.9f;
        Speed *= 0.35f;
        Velocity = force;
        if (Human) Game.Notify("TOUCHÉ !");
    }

    public void Spin() {
        if (HasShield) {
            HasShield = false;
            if (shieldObj) shieldObj.SetActive(false);
            if (Human) Game.Notify("BOUCLIER ABSORBÉ !");
            return;
        }
        spinTimer = 0.85f;
        Speed *= 0.3f;
        if (Human) Game.Notify("TÊTE-À-QUEUE !");
    }

    public void Rescue() {
        int last = (Gate + Game.Track.Gates.Count - 1) % Game.Track.Gates.Count;
        transform.position = Game.Track.Gates[last];
        int i = last * 8;
        Heading = Quaternion.LookRotation(Game.Track.Tangent(i)).eulerAngles.y;
        transform.rotation = Quaternion.Euler(0, Heading, 0);
        Velocity = Vector3.zero;
        Speed = 0;
        rescueTimer = 0;
        spinTimer = 0;
        leftTrail.Clear();
        rightTrail.Clear();
        if (flameL) flameL.SetActive(false);
        if (flameR) flameR.SetActive(false);
    }

    public void Pad() {
        boostTime = 1.8f;
        if (Human) {
            Game.Notify("TURBO !");
            Game.Audio.PlayShoot();
        }
    }
}
}
