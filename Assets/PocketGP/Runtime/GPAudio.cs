using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace PocketGP {
public sealed class GPAudio : MonoBehaviour {
    AudioSource music, engine, skid, turbo, fx;
    AudioClip beep, coin, shootClip, explodeClip, splatClip, lapClip, itemClip, turboBlast, blowOffClip, jumpClip, landClip, bumpClip;
    float musicVolume = .35f, effectsVolume = .65f;
    bool wasTurbo;

    readonly string[] mp3Files = {
        "track1_turbo_rush.mp3",
        "track2_neon_drift.mp3",
        "track3_cyber_grandprix.mp3"
    };
    int currentTrackIndex = -1;
    bool audioStarted;

    public float MusicVolume {
        get { return musicVolume; }
        set { musicVolume = value; if (music) music.volume = value; PlayerPrefs.SetFloat("music", value); }
    }
    public float EffectsVolume {
        get { return effectsVolume; }
        set { effectsVolume = value; if (fx) fx.volume = value; PlayerPrefs.SetFloat("effects", value); }
    }

    AudioClip Tone(string name, float freq, float seconds, bool sweep = false) {
        const int rate = 22050;
        float[] samples = new float[(int)(rate * seconds)];
        for (int i = 0; i < samples.Length; i++) {
            float t = i / (float)rate;
            float env = Mathf.Min(t * 80, 1) * Mathf.Pow(1 - t / seconds, 2);
            samples[i] = Mathf.Sin(2 * Mathf.PI * (freq * t + (sweep ? freq * t * t : 0))) * env * .5f;
        }
        var clip = AudioClip.Create(name, samples.Length, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    void Awake() {
        music = gameObject.AddComponent<AudioSource>();
        engine = gameObject.AddComponent<AudioSource>();
        skid = gameObject.AddComponent<AudioSource>();
        turbo = gameObject.AddComponent<AudioSource>();
        fx = gameObject.AddComponent<AudioSource>();

        musicVolume = PlayerPrefs.GetFloat("music", .35f);
        effectsVolume = PlayerPrefs.GetFloat("effects", .65f);
        music.volume = musicVolume;
        fx.volume = effectsVolume;

        beep = Tone("Signal", 660, .17f);
        coin = Tone("Collecte", 880, .24f, true);

        const int rate = 22050;

        // Musique de secours procédurale (avant chargement du MP3)
        int total = rate * 16;
        float[] data = new float[total];
        int[] notes = { 0, 7, 12, 7, 3, 10, 15, 10, 5, 12, 17, 12, 7, 14, 19, 14 };
        for (int i = 0; i < total; i++) {
            float t = i / (float)rate;
            int step = (int)(t * 4);
            float beat = t * 4 - step;
            float f = 130.81f * Mathf.Pow(2, notes[step % 16] / 12f);
            float lead = (Mathf.Sin(t * f * Mathf.PI * 2) + .25f * Mathf.Sin(t * f * Mathf.PI * 4)) * Mathf.Exp(-beat * 5) * .12f;
            float bass = Mathf.Sin(t * 65.405f * Mathf.PI * 2) * Mathf.Exp(-beat * 4) * .15f;
            float kick = Mathf.Sin(2 * Mathf.PI * (45 * beat / 4 + 6 * (1 - Mathf.Exp(-beat * 10)))) * Mathf.Exp(-beat * 14) * .23f;
            float hat = Mathf.Sin(t * 8171) * Mathf.Sin(t * 5213) * Mathf.Exp(-beat * 40) * .055f;
            data[i] = lead + bass + kick + hat;
        }
        music.clip = AudioClip.Create("Pocket groove", total, 1, rate, false);
        music.clip.SetData(data, 0);
        music.loop = true;

        // Moteur
        float[] hum = new float[2205];
        for (int i = 0; i < hum.Length; i++) {
            float t = i / (float)rate;
            hum[i] = (Mathf.Sin(t * 80 * Mathf.PI * 2) + Mathf.Sin(t * 160 * Mathf.PI * 2) * .35f) * .22f;
        }
        engine.clip = AudioClip.Create("Moteur", hum.Length, 1, rate, false);
        engine.clip.SetData(hum, 0);
        engine.loop = true;

        // Bruit de dérapage réaliste (chargement du fichier WAV utilisateur s'il existe)
        var resSkid = Resources.Load<AudioClip>("Audio/skid_loop");
        if (resSkid) {
            skid.clip = resSkid;
        } else {
            int skidLen = rate;
            float[] skidSamples = new float[skidLen];
            for (int i = 0; i < skidLen; i++) {
                float t = i / (float)rate;
                float noise = (Mathf.Repeat(Mathf.Sin(i * 12.9898f + t * 78.233f) * 43758.5453f, 1f) - 0.5f);
                float slipFlutter = 0.65f + 0.35f * Mathf.Sin(2 * Mathf.PI * 42f * t);
                float chirp = Mathf.Sin(2 * Mathf.PI * 2240f * t) * 0.4f + Mathf.Sin(2 * Mathf.PI * 2890f * t) * 0.25f;
                skidSamples[i] = (noise * 0.55f + chirp * 0.45f) * slipFlutter * 0.45f;
            }
            skid.clip = AudioClip.Create("Derapage", skidLen, 1, rate, false);
            skid.clip.SetData(skidSamples, 0);
        }
        skid.loop = true;
        skid.volume = 0;
        skid.Play();

        // Bruit de turbo réaliste (chargement des WAV haute qualité synthétisés)
        var resTurboLoop = Resources.Load<AudioClip>("Audio/turbo_loop");
        var resTurboIgnite = Resources.Load<AudioClip>("Audio/turbo_ignite");
        var resTurboBlowoff = Resources.Load<AudioClip>("Audio/turbo_blowoff");

        if (resTurboLoop) {
            turbo.clip = resTurboLoop;
        } else {
            int turboLen = rate;
            float[] turboSamples = new float[turboLen];
            for (int i = 0; i < turboLen; i++) {
                float t = i / (float)rate;
                float air = (Mathf.Repeat(Mathf.Sin(i * 9.17f + t * 145.2f) * 12345.6f, 1f) - 0.5f);
                float spool = Mathf.Sin(2 * Mathf.PI * 1280f * t) * 0.28f + Mathf.Sin(2 * Mathf.PI * 1920f * t) * 0.16f;
                float intake = Mathf.Sin(2 * Mathf.PI * 72f * t) * 0.22f;
                turboSamples[i] = Mathf.Clamp((air * 0.45f + spool + intake) * 0.38f, -0.6f, 0.6f);
            }
            turbo.clip = AudioClip.Create("Turbo", turboLen, 1, rate, false);
            turbo.clip.SetData(turboSamples, 0);
        }
        turbo.loop = true;
        turbo.volume = 0;
        turbo.Play();

        // Effets sonores d'armes, gameplay & fanfare
        shootClip = BuildShoot(rate);
        explodeClip = BuildExplosion(rate);
        splatClip = BuildSplat(rate);
        lapClip = BuildLap(rate);
        itemClip = BuildItemGet(rate);
        turboBlast = resTurboIgnite ? resTurboIgnite : BuildTurboBlast(rate);
        blowOffClip = resTurboBlowoff ? resTurboBlowoff : BuildBlowOff(rate);
        jumpClip = BuildJump(rate);
        landClip = BuildLand(rate);
        bumpClip = BuildBump(rate);
    }

    AudioClip BuildBlowOff(int rate) {
        int len = (int)(rate * 0.35f);
        float[] s = new float[len];
        for (int i = 0; i < len; i++) {
            float t = i / (float)rate;
            float n = (Mathf.Repeat(Mathf.Sin(i * 9.21f + t * 145.8f) * 31415.9f, 1f) - 0.5f);
            float flutter = 0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * 34f * t);
            float env = Mathf.Exp(-t * 9.5f);
            s[i] = n * flutter * env * 0.85f;
        }
        var c = AudioClip.Create("BlowOff", len, 1, rate, false);
        c.SetData(s, 0);
        return c;
    }

    AudioClip BuildShoot(int rate) {
        int len = (int)(rate * 0.2f);
        float[] s = new float[len];
        for (int i = 0; i < len; i++) {
            float t = i / (float)rate;
            float f = Mathf.Lerp(820, 220, t / 0.2f);
            s[i] = Mathf.Sin(2 * Mathf.PI * f * t) * Mathf.Pow(1 - t / 0.2f, 2) * 0.45f;
        }
        var c = AudioClip.Create("Shoot", len, 1, rate, false);
        c.SetData(s, 0);
        return c;
    }

    AudioClip BuildExplosion(int rate) {
        int len = (int)(rate * 0.4f);
        float[] s = new float[len];
        for (int i = 0; i < len; i++) {
            float t = i / (float)rate;
            float n = (Mathf.Repeat(Mathf.Sin(i * 3.1415f + t * 45.123f) * 23456.7f, 1f) - 0.5f);
            float low = Mathf.Sin(2 * Mathf.PI * 75 * t);
            s[i] = (n * 0.6f + low * 0.4f) * Mathf.Exp(-t * 9) * 0.7f;
        }
        var c = AudioClip.Create("Explode", len, 1, rate, false);
        c.SetData(s, 0);
        return c;
    }

    AudioClip BuildSplat(int rate) {
        int len = (int)(rate * 0.28f);
        float[] s = new float[len];
        for (int i = 0; i < len; i++) {
            float t = i / (float)rate;
            float f = Mathf.Lerp(450, 120, Mathf.Pow(t / 0.28f, 0.5f));
            s[i] = Mathf.Sin(2 * Mathf.PI * f * t) * (1 - t / 0.28f) * 0.5f;
        }
        var c = AudioClip.Create("Splat", len, 1, rate, false);
        c.SetData(s, 0);
        return c;
    }

    AudioClip BuildLap(int rate) {
        int len = (int)(rate * 0.45f);
        float[] s = new float[len];
        float[] chord = { 523.25f, 659.25f, 783.99f, 1046.50f };
        for (int i = 0; i < len; i++) {
            float t = i / (float)rate;
            int n = Mathf.Clamp((int)(t * 10), 0, 3);
            s[i] = Mathf.Sin(2 * Mathf.PI * chord[n] * t) * Mathf.Exp(-(t - n * 0.1f) * 7) * 0.4f;
        }
        var c = AudioClip.Create("Lap", len, 1, rate, false);
        c.SetData(s, 0);
        return c;
    }

    AudioClip BuildItemGet(int rate) {
        int len = (int)(rate * 0.22f);
        float[] s = new float[len];
        for (int i = 0; i < len; i++) {
            float t = i / (float)rate;
            float f = t < 0.1f ? 784f : 1174.66f;
            s[i] = Mathf.Sin(2 * Mathf.PI * f * t) * Mathf.Exp(-t * 6) * 0.45f;
        }
        var c = AudioClip.Create("ItemGet", len, 1, rate, false);
        c.SetData(s, 0);
        return c;
    }

    AudioClip BuildTurboBlast(int rate) {
        int len = (int)(rate * 0.28f);
        float[] s = new float[len];
        for (int i = 0; i < len; i++) {
            float t = i / (float)rate;
            float freq = Mathf.Lerp(140, 480, Mathf.Pow(t / 0.28f, 0.4f));
            float n = (Mathf.Repeat(Mathf.Sin(i * 5.31f + t * 62.4f) * 8765.4f, 1f) - 0.5f);
            float sine = Mathf.Sin(2 * Mathf.PI * freq * t);
            s[i] = Mathf.Clamp((sine * 0.42f + n * 0.32f) * Mathf.Pow(1 - t / 0.28f, 1.2f) * 0.52f, -0.6f, 0.6f);
        }
        var c = AudioClip.Create("TurboBlast", len, 1, rate, false);
        c.SetData(s, 0);
        return c;
    }

    AudioClip BuildJump(int rate) {
        int len = (int)(rate * 0.26f);
        float[] s = new float[len];
        for (int i = 0; i < len; i++) {
            float t = i / (float)rate;
            float f = Mathf.Lerp(280, 680, Mathf.Pow(t / 0.26f, 0.5f));
            s[i] = Mathf.Sin(2 * Mathf.PI * f * t) * (1 - t / 0.26f) * 0.38f;
        }
        var c = AudioClip.Create("Jump", len, 1, rate, false);
        c.SetData(s, 0);
        return c;
    }

    AudioClip BuildLand(int rate) {
        int len = (int)(rate * 0.24f);
        float[] s = new float[len];
        for (int i = 0; i < len; i++) {
            float t = i / (float)rate;
            float n = (Mathf.Repeat(Mathf.Sin(i * 8.12f + t * 54.3f) * 12345f, 1f) - 0.5f);
            float low = Mathf.Sin(2 * Mathf.PI * 62 * t);
            s[i] = (low * 0.55f + n * 0.35f) * Mathf.Exp(-t * 14f) * 0.48f;
        }
        var c = AudioClip.Create("Land", len, 1, rate, false);
        c.SetData(s, 0);
        return c;
    }

    AudioClip BuildBump(int rate) {
        int len = (int)(rate * 0.18f);
        float[] s = new float[len];
        for (int i = 0; i < len; i++) {
            float t = i / (float)rate;
            float f = Mathf.Lerp(220, 85, t / 0.18f);
            s[i] = Mathf.Sin(2 * Mathf.PI * f * t) * (1 - t / 0.18f) * 0.45f;
        }
        var c = AudioClip.Create("Bump", len, 1, rate, false);
        c.SetData(s, 0);
        return c;
    }

    public void StartAudio() {
        if (!audioStarted) {
            audioStarted = true;
            if (!music.isPlaying) music.Play();
            StartCoroutine(PlayRandomMp3());
        }
        if (!engine.isPlaying) engine.Play();
    }

    public void NextTrack() {
        StartCoroutine(PlayRandomMp3());
    }

    IEnumerator PlayRandomMp3() {
        if (mp3Files == null || mp3Files.Length == 0) yield break;
        int next = Random.Range(0, mp3Files.Length);
        if (next == currentTrackIndex && mp3Files.Length > 1) {
            next = (next + 1) % mp3Files.Length;
        }
        currentTrackIndex = next;
        string filename = mp3Files[currentTrackIndex];

        string url;
        #if !UNITY_EDITOR && UNITY_WEBGL
        url = "StreamingAssets/Music/" + filename;
        #else
        url = System.IO.Path.Combine(Application.streamingAssetsPath, "Music", filename);
        if (!url.StartsWith("file://") && !url.StartsWith("http://") && !url.StartsWith("https://")) {
            url = "file://" + url;
        }
        #endif

        using (var req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG)) {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success) {
                var clip = DownloadHandlerAudioClip.GetContent(req);
                if (clip) {
                    music.clip = clip;
                    music.loop = true;
                    music.volume = musicVolume;
                    music.Play();
                }
            }
        }
    }

    public void Engine(float speed, bool active) {
        engine.volume = active ? effectsVolume * .22f : 0;
        engine.pitch = .65f + speed * .065f;
    }

    public void SetSkid(bool active, float intensity) {
        skid.volume = active ? effectsVolume * Mathf.Clamp01(intensity) * 0.70f : 0;
        skid.pitch = 0.95f + Mathf.Clamp01(intensity) * 0.20f;
    }

    public void SetTurbo(bool active, float speed = 0) {
        if (active && !wasTurbo) {
            if (turboBlast) fx.PlayOneShot(turboBlast, effectsVolume * 0.35f);
            if (!turbo.isPlaying) turbo.Play();
        } else if (!active && wasTurbo) {
            if (blowOffClip) fx.PlayOneShot(blowOffClip, effectsVolume * 0.30f);
        }
        wasTurbo = active;
        turbo.volume = active ? effectsVolume * 0.28f : 0;
        float targetPitch = 0.95f + Mathf.Clamp01(speed / 25f) * 0.30f;
        turbo.pitch = Mathf.Lerp(turbo.pitch, targetPitch, Time.deltaTime * 6f);
    }

    public void Beep(bool pickup = false) { fx.PlayOneShot(pickup ? coin : beep); }
    public void PlayShoot() { fx.PlayOneShot(shootClip); }
    public void PlayExplosion() { fx.PlayOneShot(explodeClip); }
    public void PlaySplat() { fx.PlayOneShot(splatClip); }
    public void PlayLap() { fx.PlayOneShot(lapClip); }
    public void PlayItemGet() { fx.PlayOneShot(itemClip); }
    public void PlayJump() { if (jumpClip) fx.PlayOneShot(jumpClip, effectsVolume * 0.75f); }
    public void PlayLand() { if (landClip) fx.PlayOneShot(landClip, effectsVolume * 0.85f); }
    public void PlayBump() { if (bumpClip) fx.PlayOneShot(bumpClip, effectsVolume * 0.80f); }

    void OnDestroy() {
        if (music && music.clip) Destroy(music.clip);
        if (engine && engine.clip) Destroy(engine.clip);
        if (skid && skid.clip) Destroy(skid.clip);
        if (turbo && turbo.clip) Destroy(turbo.clip);
        if (beep) Destroy(beep);
        if (coin) Destroy(coin);
    }
}
}
