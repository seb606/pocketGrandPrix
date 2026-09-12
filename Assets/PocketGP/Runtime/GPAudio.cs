using UnityEngine;

namespace PocketGP {
public sealed class GPAudio : MonoBehaviour {
    AudioSource music, engine, skid, turbo, fx;
    AudioClip beep, coin, shootClip, explodeClip, splatClip, lapClip, itemClip;
    float musicVolume = .35f, effectsVolume = .65f;

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

        // Musique
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

        // Bruit de dérapage (crissement de pneu)
        int skidLen = rate / 2;
        float[] skidSamples = new float[skidLen];
        for (int i = 0; i < skidLen; i++) {
            float t = i / (float)rate;
            float n = (Mathf.Repeat(Mathf.Sin(i * 12.9898f + t * 78.233f) * 43758.5453f, 1f) - 0.5f);
            float mod = Mathf.Sin(2 * Mathf.PI * 850 * t) + 0.5f * Mathf.Sin(2 * Mathf.PI * 1420 * t);
            skidSamples[i] = (n * 0.4f + mod * 0.35f) * 0.35f;
        }
        skid.clip = AudioClip.Create("Derapage", skidLen, 1, rate, false);
        skid.clip.SetData(skidSamples, 0);
        skid.loop = true;
        skid.volume = 0;
        skid.Play();

        // Bruit de turbo (whoosh / réacteur)
        int turboLen = rate / 2;
        float[] turboSamples = new float[turboLen];
        for (int i = 0; i < turboLen; i++) {
            float t = i / (float)rate;
            float n = (Mathf.Repeat(Mathf.Sin(i * 9.123f + t * 133.456f) * 31415.9f, 1f) - 0.5f);
            float tone = Mathf.Sin(2 * Mathf.PI * 280 * t) + 0.3f * Mathf.Sin(2 * Mathf.PI * 560 * t);
            turboSamples[i] = (n * 0.55f + tone * 0.3f) * 0.4f;
        }
        turbo.clip = AudioClip.Create("Turbo", turboLen, 1, rate, false);
        turbo.clip.SetData(turboSamples, 0);
        turbo.loop = true;
        turbo.volume = 0;
        turbo.Play();

        // Effets sonores d'armes & fanfare
        shootClip = BuildShoot(rate);
        explodeClip = BuildExplosion(rate);
        splatClip = BuildSplat(rate);
        lapClip = BuildLap(rate);
        itemClip = BuildItemGet(rate);
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

    public void StartAudio() {
        if (!music.isPlaying) music.Play();
        if (!engine.isPlaying) engine.Play();
    }

    public void Engine(float speed, bool active) {
        engine.volume = active ? effectsVolume * .22f : 0;
        engine.pitch = .65f + speed * .065f;
    }

    public void SetSkid(bool active, float intensity) {
        skid.volume = active ? effectsVolume * Mathf.Clamp01(intensity) * 0.5f : 0;
        skid.pitch = 0.9f + intensity * 0.35f;
    }

    public void SetTurbo(bool active) {
        turbo.volume = active ? effectsVolume * 0.45f : 0;
        turbo.pitch = active ? 1.25f : 1.0f;
    }

    public void Beep(bool pickup = false) { fx.PlayOneShot(pickup ? coin : beep); }
    public void PlayShoot() { fx.PlayOneShot(shootClip); }
    public void PlayExplosion() { fx.PlayOneShot(explodeClip); }
    public void PlaySplat() { fx.PlayOneShot(splatClip); }
    public void PlayLap() { fx.PlayOneShot(lapClip); }
    public void PlayItemGet() { fx.PlayOneShot(itemClip); }

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
