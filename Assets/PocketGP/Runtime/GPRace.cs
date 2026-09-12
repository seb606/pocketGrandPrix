using UnityEngine;
using System.Collections.Generic;

namespace PocketGP {
public enum GPState { Menu, Countdown, Racing, Paused, Results }

public sealed class GPRace : MonoBehaviour {
    public static GPRace Instance;
    public const int TotalLaps = 3;
    public GPState State = GPState.Menu;
    public GPTrack Track;
    public GPUi UI;
    public GPAudio Audio;
    public GPWeapons Weapons;
    public List<GPCar> Cars = new List<GPCar>();
    public int TrackIndex, Difficulty = 1, ColorIndex, Score, ChampionshipPoints, ResultPlace;
    public bool Championship, MobileMode;
    public float RaceTime, Countdown, FinalTime;
    public Camera Cam;
    Transform world;
    float noticeTime;
    string notice = "";
    int lastBeep = -1;

    public string Notice { get { return noticeTime > 0 ? notice : ""; } }
    public readonly List<int> FinishOrder = new List<int>();

    sealed class Pickup {
        public Transform Visual;
        public Vector3 Position;
        public float Respawn;
        public bool Turbo;
        public bool ItemBox;
    }
    readonly List<Pickup> pickups = new List<Pickup>();

    sealed class Hazard {
        public Transform Visual;
        public GPObstacle Obstacle;
        public Vector3 Position;
        public bool IsRamp;
        public float Radius;
        public float Respawn;
    }
    readonly List<Hazard> hazards = new List<Hazard>();
    float bumpSoundCooldown;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap() {
        if (!FindFirstObjectByType<GPRace>()) new GameObject("Pocket Grand Prix").AddComponent<GPRace>();
    }

    void Awake() {
        Instance = this;
        Application.targetFrameRate = 60;
        Time.fixedDeltaTime = 1f / 60f;
        QualitySettings.vSyncCount = 1;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Difficulty = PlayerPrefs.GetInt("difficulty", 1);
        ColorIndex = PlayerPrefs.GetInt("car", 0);
        MobileMode = PlayerPrefs.GetInt("display_mode", 0) == 1;

        var c = new GameObject("Camera");
        Cam = c.AddComponent<Camera>();
        c.AddComponent<AudioListener>();
        Cam.orthographic = true;
        Cam.orthographicSize = 12;
        Cam.nearClipPlane = .1f;
        Cam.farClipPlane = 180;
        Cam.backgroundColor = GPArt.Hex("A8C6D0");
        Cam.clearFlags = CameraClearFlags.SolidColor;

        var light = new GameObject("Soleil").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        light.color = GPArt.Hex("FFF4E6");
        light.transform.rotation = Quaternion.Euler(48, -32, 0);
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.88f;
        light.shadowBias = 0.02f;
        light.shadowNormalBias = 0.1f;
        light.shadowNearPlane = 0.1f;

        RenderSettings.ambientLight = GPArt.Hex("667585");
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        QualitySettings.shadowDistance = 45;
        QualitySettings.shadowCascades = 4;
        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
        QualitySettings.shadowProjection = ShadowProjection.CloseFit;
        QualitySettings.antiAliasing = 4;

        Audio = gameObject.AddComponent<GPAudio>();
        Weapons = gameObject.AddComponent<GPWeapons>();
        Weapons.Init(this);

        UI = gameObject.AddComponent<GPUi>();
        UI.Init(this);

        BuildWorld(0);
        UI.ShowMenu();
    }

    public void BuildWorld(int index) {
        if (world) {
            world.gameObject.SetActive(false);
            Destroy(world.gameObject);
        }
        Cars.Clear();
        pickups.Clear();
        hazards.Clear();
        FinishOrder.Clear();
        if (Weapons) Weapons.Clear();

        TrackIndex = index;
        world = new GameObject("Circuit - " + GPTrack.Names[index]).transform;
        Track = world.gameObject.AddComponent<GPTrack>();
        Track.Build(index);

        for (int i = 0; i < 5; i++) {
            var car = new GameObject(i == 0 ? "Joueur" : "Pilote " + i);
            car.transform.SetParent(world, false);
            var driver = car.AddComponent<GPCar>();
            driver.Setup(this, i);
            Cars.Add(driver);
        }

        // Répartition des turbos, boîtes d'objets et jetons d'or
        for (int i = 10; i < Track.Points.Count; i += 12) {
            bool turbo = i % 24 == 10;
            bool isBox = !turbo && (i % 24 == 22);
            Vector3 p = Track.Points[i];
            var root = new GameObject(turbo ? "Zone turbo" : isBox ? "Boite Arme" : "Jeton").transform;
            root.SetParent(world, false);
            root.position = p;

            if (turbo) {
                root.rotation = Quaternion.LookRotation(Track.Tangent(i));
                for (int k = 0; k < 3; k++) {
                    GPArt.Box(root, "Fleche", new Vector3(0, .07f, k * .65f - .65f), new Vector3(3, .055f, .26f), GPArt.Mat("4EF0D1", .6f));
                }
            } else if (isBox) {
                GPArt.ItemBox(root);
                root.position = p + Vector3.up * 0.9f;
            } else {
                var coin = GPArt.Cylinder(root, "Jeton or", new Vector3(0, .95f, 0), new Vector3(.8f, .10f, .8f), GPArt.Mat("FFD25B", .9f, .6f));
                coin.transform.localRotation = Quaternion.Euler(90, 0, 0);
            }
            pickups.Add(new Pickup { Visual = root, Position = p, Turbo = turbo, ItemBox = isBox });
        }

        // Éléments interactifs du circuit : Tremplins et Obstacles (cônes & barils)
        int totalPts = Track.Points.Count;
        int[] rampIndices = { (int)(totalPts * 0.32f), (int)(totalPts * 0.74f) };
        foreach (int rIdx in rampIndices) {
            Vector3 pos = Track.Points[rIdx];
            var ramp = GPArt.JumpRamp(world);
            ramp.transform.position = pos;
            ramp.transform.rotation = Quaternion.LookRotation(Track.Tangent(rIdx));
            hazards.Add(new Hazard { Visual = ramp.transform, Position = pos, IsRamp = true, Radius = 2.6f });
        }

        int[] obsIndices = { (int)(totalPts * 0.16f), (int)(totalPts * 0.44f), (int)(totalPts * 0.58f), (int)(totalPts * 0.88f) };
        for (int k = 0; k < obsIndices.Length; k++) {
            int pt = obsIndices[k];
            Vector3 pos = Track.Points[pt];
            Vector3 right = Vector3.Cross(Vector3.up, Track.Tangent(pt));
            bool isBarrel = k % 2 == 1;
            float sideOffset = (k % 2 == 0 ? 1.5f : -1.5f);
            Vector3 obsPos = pos + right * sideOffset;
            var obs = isBarrel ? GPArt.Barrel(world) : GPArt.TrafficCone(world);
            obs.transform.position = obsPos;
            obs.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            var obstacleComp = obs.GetComponent<GPObstacle>();
            hazards.Add(new Hazard { Visual = obs.transform, Obstacle = obstacleComp, Position = obsPos, IsRamp = false, Radius = isBarrel ? 1.35f : 1.05f });
        }

        Cam.transform.position = new Vector3(0, 65, -35);
        Cam.transform.rotation = Quaternion.Euler(65, 0, 0);
    }

    public void StartRace(bool championship = false) {
        Championship = championship;
        if (championship) {
            TrackIndex = 0;
            ChampionshipPoints = 0;
        }
        Launch();
    }

    void Launch() {
        Audio.StartAudio();
        Audio.NextTrack();
        PlayerPrefs.SetInt("difficulty", Difficulty);
        PlayerPrefs.SetInt("car", ColorIndex);
        PlayerPrefs.Save();
        BuildWorld(TrackIndex);
        RaceTime = 0;
        Score = 0;
        Countdown = 3.6f;
        lastBeep = -1;
        State = GPState.Countdown;
        UI.ShowHUD();
    }

    public void Next() {
        ChampionshipPoints += new[] { 10, 7, 5, 3, 1 }[Mathf.Clamp(ResultPlace - 1, 0, 4)];
        if (TrackIndex < 2) {
            TrackIndex++;
            Launch();
        } else {
            Championship = false;
            UI.ShowChampionship();
        }
    }

    public void Retry() { Launch(); }
    public void Menu() {
        State = GPState.Menu;
        UI.ClearInput();
        UI.ShowMenu();
    }

    public void Pause() {
        if (State == GPState.Racing || State == GPState.Countdown) {
            wasCountdown = State == GPState.Countdown;
            State = GPState.Paused;
            UI.ShowPause();
        } else if (State == GPState.Paused) {
            State = wasCountdown ? GPState.Countdown : GPState.Racing;
            UI.ShowHUD();
        }
    }
    bool wasCountdown;

    public void Notify(string value) {
        notice = value;
        noticeTime = 2.0f;
    }

    public void Finished(GPCar car) {
        if (!FinishOrder.Contains(car.Id)) FinishOrder.Add(car.Id);
        if (!car.Human) return;

        ResultPlace = FinishOrder.Count;
        FinalTime = RaceTime;
        State = GPState.Results;
        UI.ClearInput();

        string key = "best_" + TrackIndex + "_" + Difficulty;
        float old = PlayerPrefs.GetFloat(key, 99999);
        if (FinalTime < old) PlayerPrefs.SetFloat(key, FinalTime);

        int medal = ResultPlace == 1 ? 3 : ResultPlace <= 3 ? 2 : 1;
        string mk = "medal_" + TrackIndex;
        PlayerPrefs.SetInt(mk, Mathf.Max(medal, PlayerPrefs.GetInt(mk, 0)));
        PlayerPrefs.Save();

        Audio.PlayLap();
        UI.ShowResults();
    }

    public int Position() {
        if (Cars.Count == 0) return 1;
        if (Cars[0].FinishTime >= 0) return ResultPlace;
        int p = 1;
        for (int i = 1; i < Cars.Count; i++)
            if (Cars[i].FinishTime >= 0 || Cars[i].RankProgress > Cars[0].RankProgress) p++;
        return p;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) Pause();
        noticeTime -= Time.deltaTime;

        if (State == GPState.Countdown) {
            Countdown -= Time.deltaTime;
            int n = Mathf.CeilToInt(Countdown);
            if (n != lastBeep) {
                Audio.Beep();
                lastBeep = n;
            }
            if (Countdown <= 0) {
                State = GPState.Racing;
                Notify("C'EST PARTI !");
            }
        }

        Audio.Engine(Cars.Count > 0 ? Cars[0].Speed : 0, State == GPState.Racing);
        UI.Refresh();
    }

    void FixedUpdate() {
        if (State != GPState.Racing) return;
        float dt = Time.fixedDeltaTime;
        RaceTime += dt;

        foreach (var car in Cars) {
            car.Step(dt);
            if (State != GPState.Racing) break;
        }
        if (State != GPState.Racing) return;

        if (Weapons) Weapons.Step(dt);

        for (int i = 0; i < Cars.Count; i++) {
            for (int j = i + 1; j < Cars.Count; j++) {
                if (Cars[i].FinishTime >= 0 || Cars[j].FinishTime >= 0) continue;
                Vector3 delta = Cars[i].transform.position - Cars[j].transform.position;
                float d = delta.magnitude;
                if (d < 1.35f) {
                    Vector3 n = d > .001f ? delta / d : Vector3.right;
                    Vector3 push = n * (1.35f - d) * .65f;
                    Cars[i].transform.position += push;
                    Cars[j].transform.position -= push;
                    Cars[i].Velocity += n * 2.5f;
                    Cars[j].Velocity -= n * 2.5f;
                    if (Cars[i].Human || Cars[j].Human) {
                        if (bumpSoundCooldown <= 0) {
                            Audio.PlayBump();
                            bumpSoundCooldown = 0.22f;
                        }
                        Cars[i].SpawnSparks(Cars[i].transform.position);
                    }
                }
            }
        }
        if (bumpSoundCooldown > 0) bumpSoundCooldown -= dt;

        // Interaction avec les tremplins et obstacles physiques
        foreach (var h in hazards) {
            foreach (var car in Cars) {
                if (car.FinishTime >= 0) continue;
                float d = Vector3.Distance(car.transform.position, h.Visual.position);
                if (d < h.Radius) {
                    if (h.IsRamp) {
                        // Tremplin : propulsion en l'air si abordé dans le sens de la pente
                        if (Vector3.Dot(car.Velocity, h.Visual.forward) > 2f) {
                            car.Jump(14.5f);
                        }
                    } else if (h.Obstacle) {
                        // Collision physique avec le cône ou le baril
                        h.Obstacle.OnCarHit(car);
                    }
                    break;
                }
            }
        }

        foreach (var item in pickups) {
            item.Respawn = Mathf.Max(0, item.Respawn - dt);
            item.Visual.gameObject.SetActive(item.Respawn <= 0);
            if (!item.Turbo && !item.ItemBox) item.Visual.rotation = Quaternion.Euler(0, Time.time * 95, 0);
            if (item.Respawn > 0) continue;

            foreach (var car in Cars) {
                if (car.FinishTime >= 0) continue;
                float hitDist = item.Turbo ? 2.2f : item.ItemBox ? 1.6f : 1.3f;
                if (Vector3.Distance(car.transform.position, item.Position) < hitDist) {
                    if (item.Turbo) {
                        car.Pad();
                        item.Respawn = 1.2f;
                    } else if (item.ItemBox) {
                        car.GiveItem((GPItemType)Random.Range(1, 5));
                        item.Respawn = 4.5f;
                    } else {
                        car.Nitro = Mathf.Clamp01(car.Nitro + .22f);
                        if (car.Human) {
                            Score += 100;
                            Audio.Beep(true);
                            Notify("+100  •  TURBO RECHARGÉ");
                        }
                        item.Respawn = 5f;
                    }
                    break;
                }
            }
        }
    }

    void LateUpdate() {
        if (!Cam || Cars.Count == 0) return;
        bool menu = State == GPState.Menu;
        Vector3 target = menu ? new Vector3(0, 42, -22) : Cars[0].transform.position + Cars[0].Velocity * .22f + new Vector3(0, 21, -12);
        Cam.transform.position = Vector3.Lerp(Cam.transform.position, target, 1 - Mathf.Exp(-Time.unscaledDeltaTime * 6));
        Cam.transform.rotation = Quaternion.Euler(menu ? 60 : 58, 0, 0);
        float boostSurge = (!menu && Cars.Count > 0 && Cars[0].Speed > 18f ? 1.4f : 0f);
        float size = menu ? 24 : Mathf.Max(10.5f, 6.8f / Mathf.Max(.45f, Cam.aspect)) + Cars[0].Speed * .06f + boostSurge;
        Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, size, Time.unscaledDeltaTime * 4.5f);
    }

    void OnApplicationFocus(bool focus) {
        if (!focus && (State == GPState.Racing || State == GPState.Countdown)) Pause();
    }
}
}
