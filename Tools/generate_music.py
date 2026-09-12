import os
import math
import numpy as np
import lameenc

RATE = 44100
OUT_DIR = os.path.join(os.path.dirname(__file__), "..", "Assets", "StreamingAssets", "Music")
os.makedirs(OUT_DIR, exist_ok=True)

def encode_mp3(audio_stereo, filepath, bitrate=192):
    """Encode float32 stereo numpy array [-1, 1] into MP3."""
    audio_int16 = np.clip(audio_stereo * 32767.0, -32768, 32767).astype(np.int16)
    interleaved = np.empty((audio_int16.shape[0] * 2,), dtype=np.int16)
    interleaved[0::2] = audio_int16[:, 0]
    interleaved[1::2] = audio_int16[:, 1]
    
    encoder = lameenc.Encoder()
    encoder.set_bit_rate(bitrate)
    encoder.set_in_sample_rate(RATE)
    encoder.set_channels(2)
    encoder.set_quality(2)
    
    mp3_data = encoder.encode(interleaved.tobytes())
    mp3_data += encoder.flush()
    
    with open(filepath, "wb") as f:
        f.write(mp3_data)
    print(f"Generated {filepath} ({len(mp3_data)/1024:.1f} KB)")

def midi_to_hz(m):
    return 440.0 * (2.0 ** ((m - 69) / 12.0))

def generate_track_1():
    """Track 1: Turbo Rush - 128 BPM Synthwave / Outrun style (approx 45 seconds loop)"""
    bpm = 128.0
    beat_sec = 60.0 / bpm
    bar_sec = beat_sec * 4.0
    num_bars = 24
    duration = num_bars * bar_sec
    total_samples = int(duration * RATE)
    
    left = np.zeros(total_samples, dtype=np.float32)
    right = np.zeros(total_samples, dtype=np.float32)
    
    # 1. Kick Drum (Punchy 4-on-the-floor)
    kick_samples = int(0.25 * RATE)
    tk = np.linspace(0, 0.25, kick_samples, endpoint=False)
    kick_f = 160.0 * np.exp(-tk * 28.0) + 45.0
    kick_env = np.exp(-tk * 14.0)
    kick_click = np.random.uniform(-0.15, 0.15, kick_samples) * np.exp(-tk * 60.0)
    kick_wave = (np.sin(2 * np.pi * kick_f * tk) + kick_click) * kick_env * 0.75
    
    num_beats = int(num_bars * 4)
    for b in range(num_beats):
        idx = int(b * beat_sec * RATE)
        end = min(idx + kick_samples, total_samples)
        left[idx:end] += kick_wave[:end-idx]
        right[idx:end] += kick_wave[:end-idx]
        
    # 2. Snare / Clap
    snare_samples = int(0.22 * RATE)
    ts = np.linspace(0, 0.22, snare_samples, endpoint=False)
    snare_noise = np.random.uniform(-0.4, 0.4, snare_samples) * np.exp(-ts * 18.0)
    snare_tone = np.sin(2 * np.pi * (220.0 * np.exp(-ts * 20.0) + 120.0) * ts) * np.exp(-ts * 22.0) * 0.4
    snare_wave = (snare_noise + snare_tone) * 0.65
    for b in range(num_beats):
        if b % 2 == 1:
            idx = int(b * beat_sec * RATE)
            end = min(idx + snare_samples, total_samples)
            left[idx:end] += snare_wave[:end-idx] * 0.95
            right[idx:end] += snare_wave[:end-idx] * 1.05

    # 3. Running Hi-Hats
    hat_samples = int(0.06 * RATE)
    th = np.linspace(0, 0.06, hat_samples, endpoint=False)
    hat_wave = np.random.uniform(-0.25, 0.25, hat_samples) * np.exp(-th * 85.0)
    num_16ths = int(num_bars * 16)
    for s in range(num_16ths):
        idx = int(s * (beat_sec / 4.0) * RATE)
        end = min(idx + hat_samples, total_samples)
        vol = 0.4 if (s % 4 == 2) else 0.2
        pan = 0.5 + 0.3 * math.sin(s * 0.8)
        left[idx:end] += hat_wave[:end-idx] * vol * (1.0 - pan)
        right[idx:end] += hat_wave[:end-idx] * vol * pan

    # 4. Driving Bassline
    progression = [38, 38, 41, 41, 36, 36, 43, 43]
    for b in range(num_bars):
        root = progression[(b // 2) % len(progression)]
        for s in range(16):
            note = root if (s % 4 == 0 or s % 4 == 2) else root + 12
            freq = midi_to_hz(note)
            t_start = (b * 4 + s / 4.0) * beat_sec
            idx = int(t_start * RATE)
            dur = beat_sec / 4.0
            nsamples = int(dur * RATE)
            end = min(idx + nsamples, total_samples)
            tb = np.linspace(0, dur, end - idx, endpoint=False)
            saw = (2.0 * (tb * freq - np.floor(tb * freq + 0.5))) * 0.35
            sub = np.sin(2 * np.pi * (freq * 0.5) * tb) * 0.4
            env = np.exp(-tb * 16.0)
            bass = (saw + sub) * env * 0.55
            left[idx:end] += bass
            right[idx:end] += bass

    # 5. Lead Synth Melodies & Arpeggios
    for b in range(num_bars):
        root_chord = progression[(b // 2) % len(progression)] + 24
        for s in range(16):
            if (b >= 4 and b < 20):
                arp_note = root_chord + [0, 3, 7, 10, 12, 15, 12, 7][s % 8]
                freq = midi_to_hz(arp_note)
                t_start = (b * 4 + s / 4.0) * beat_sec
                idx = int(t_start * RATE)
                dur = (beat_sec / 4.0) * 1.5
                nsamples = int(dur * RATE)
                end = min(idx + nsamples, total_samples)
                tl = np.linspace(0, dur, end - idx, endpoint=False)
                lead = (np.sin(2 * np.pi * freq * tl) + 0.4 * np.sin(2 * np.pi * freq * 2 * tl)) * np.exp(-tl * 10.0) * 0.28
                pan = 0.5 + 0.4 * math.sin(b + s * 0.5)
                left[idx:end] += lead * (1.0 - pan)
                right[idx:end] += lead * pan

    max_val = max(np.max(np.abs(left)), np.max(np.abs(right)), 0.001)
    gain = 0.88 / max_val
    audio_stereo = np.column_stack([left * gain, right * gain])
    encode_mp3(audio_stereo, os.path.join(OUT_DIR, "track1_turbo_rush.mp3"))

def generate_track_2():
    """Track 2: Neon Drift - 132 BPM Electro-Arcade (approx 44 seconds loop)"""
    bpm = 132.0
    beat_sec = 60.0 / bpm
    bar_sec = beat_sec * 4.0
    num_bars = 24
    duration = num_bars * bar_sec
    total_samples = int(duration * RATE)
    
    left = np.zeros(total_samples, dtype=np.float32)
    right = np.zeros(total_samples, dtype=np.float32)
    
    # 1. Tight Punchy Electro Kick
    kick_samples = int(0.22 * RATE)
    tk = np.linspace(0, 0.22, kick_samples, endpoint=False)
    kick_f = 190.0 * np.exp(-tk * 32.0) + 48.0
    kick_wave = np.sin(2 * np.pi * kick_f * tk) * np.exp(-tk * 16.0) * 0.8
    num_beats = int(num_bars * 4)
    for b in range(num_beats):
        idx = int(b * beat_sec * RATE)
        end = min(idx + kick_samples, total_samples)
        left[idx:end] += kick_wave[:end-idx]
        right[idx:end] += kick_wave[:end-idx]

    # 2. Clap / Snare
    snare_samples = int(0.25 * RATE)
    ts = np.linspace(0, 0.25, snare_samples, endpoint=False)
    snare_wave = np.random.uniform(-0.5, 0.5, snare_samples) * np.exp(-ts * 14.0) * 0.55
    for b in range(num_beats):
        if b % 2 == 1:
            idx = int(b * beat_sec * RATE)
            end = min(idx + snare_samples, total_samples)
            left[idx:end] += snare_wave[:end-idx] * 0.9
            right[idx:end] += snare_wave[:end-idx] * 1.1

    # 3. Offbeat Open Hi-Hat
    ohat_samples = int(0.18 * RATE)
    th = np.linspace(0, 0.18, ohat_samples, endpoint=False)
    ohat_wave = np.random.uniform(-0.35, 0.35, ohat_samples) * np.exp(-th * 24.0) * 0.4
    for b in range(num_beats):
        idx = int((b + 0.5) * beat_sec * RATE)
        end = min(idx + ohat_samples, total_samples)
        left[idx:end] += ohat_wave[:end-idx] * 0.8
        right[idx:end] += ohat_wave[:end-idx] * 0.8

    # 4. Pumping Sidechain Bassline
    progression = [45, 41, 43, 40]
    for b in range(num_bars):
        root = progression[(b // 2) % len(progression)]
        for s in range(8):
            freq = midi_to_hz(root + (12 if s % 2 == 1 else 0))
            t_start = (b * 4 + s * 0.5) * beat_sec
            idx = int(t_start * RATE)
            dur = beat_sec * 0.5
            nsamples = int(dur * RATE)
            end = min(idx + nsamples, total_samples)
            tb = np.linspace(0, dur, end - idx, endpoint=False)
            duck = (tb / dur) ** 0.5
            wave = (np.sin(2 * np.pi * freq * tb) + 0.5 * np.sin(2 * np.pi * freq * 2 * tb)) * duck * 0.5
            left[idx:end] += wave
            right[idx:end] += wave

    # 5. Bright Crystal Plucks
    for b in range(num_bars):
        root = progression[(b // 2) % len(progression)] + 24
        for s in range(16):
            if (b >= 2 and b < 22):
                note = root + [0, 7, 12, 15, 19, 15, 12, 7][s % 8]
                freq = midi_to_hz(note)
                t_start = (b * 4 + s * 0.25) * beat_sec
                idx = int(t_start * RATE)
                dur = beat_sec * 0.35
                nsamples = int(dur * RATE)
                end = min(idx + nsamples, total_samples)
                tl = np.linspace(0, dur, end - idx, endpoint=False)
                pluck = (np.sin(2 * np.pi * freq * tl) * 0.6 + np.sin(2 * np.pi * freq * 3 * tl) * 0.2) * np.exp(-tl * 15.0) * 0.3
                pan = 0.5 + 0.45 * math.sin(s * 0.9)
                left[idx:end] += pluck * (1.0 - pan)
                right[idx:end] += pluck * pan

    max_val = max(np.max(np.abs(left)), np.max(np.abs(right)), 0.001)
    gain = 0.88 / max_val
    audio_stereo = np.column_stack([left * gain, right * gain])
    encode_mp3(audio_stereo, os.path.join(OUT_DIR, "track2_neon_drift.mp3"))

def generate_track_3():
    """Track 3: Cyber Grand Prix - 138 BPM Eurobeat / High-energy Racing (approx 42 seconds loop)"""
    bpm = 138.0
    beat_sec = 60.0 / bpm
    bar_sec = beat_sec * 4.0
    num_bars = 24
    duration = num_bars * bar_sec
    total_samples = int(duration * RATE)
    
    left = np.zeros(total_samples, dtype=np.float32)
    right = np.zeros(total_samples, dtype=np.float32)
    
    # 1. Driving Racing Kick
    kick_samples = int(0.20 * RATE)
    tk = np.linspace(0, 0.20, kick_samples, endpoint=False)
    kick_f = 210.0 * np.exp(-tk * 36.0) + 50.0
    kick_wave = np.sin(2 * np.pi * kick_f * tk) * np.exp(-tk * 18.0) * 0.85
    num_beats = int(num_bars * 4)
    for b in range(num_beats):
        idx = int(b * beat_sec * RATE)
        end = min(idx + kick_samples, total_samples)
        left[idx:end] += kick_wave[:end-idx]
        right[idx:end] += kick_wave[:end-idx]

    # 2. Aggressive Snare
    snare_samples = int(0.20 * RATE)
    ts = np.linspace(0, 0.20, snare_samples, endpoint=False)
    snare_wave = (np.random.uniform(-0.55, 0.55, snare_samples) + np.sin(2 * np.pi * 200 * ts) * 0.3) * np.exp(-ts * 18.0) * 0.6
    for b in range(num_beats):
        if b % 2 == 1:
            idx = int(b * beat_sec * RATE)
            end = min(idx + snare_samples, total_samples)
            left[idx:end] += snare_wave[:end-idx]
            right[idx:end] += snare_wave[:end-idx]

    # 3. High Energy Hi-Hats
    hat_samples = int(0.05 * RATE)
    th = np.linspace(0, 0.05, hat_samples, endpoint=False)
    hat_wave = np.random.uniform(-0.3, 0.3, hat_samples) * np.exp(-th * 90.0) * 0.3
    for s in range(int(num_bars * 16)):
        idx = int(s * (beat_sec / 4.0) * RATE)
        end = min(idx + hat_samples, total_samples)
        left[idx:end] += hat_wave[:end-idx] * 0.6
        right[idx:end] += hat_wave[:end-idx] * 0.6

    # 4. Eurobeat Octave Saw Bass
    progression = [36, 32, 34, 31]
    for b in range(num_bars):
        root = progression[(b // 2) % len(progression)]
        for s in range(16):
            octave = root if (s % 2 == 0) else root + 12
            freq = midi_to_hz(octave)
            t_start = (b * 4 + s * 0.25) * beat_sec
            idx = int(t_start * RATE)
            dur = beat_sec * 0.25
            nsamples = int(dur * RATE)
            end = min(idx + nsamples, total_samples)
            tb = np.linspace(0, dur, end - idx, endpoint=False)
            saw = (2.0 * (tb * freq - np.floor(tb * freq + 0.5))) * 0.45 * np.exp(-tb * 12.0)
            left[idx:end] += saw
            right[idx:end] += saw

    # 5. Eurobeat Brass / Supersaw Lead
    lead_notes = [60, 63, 67, 70, 72, 75, 74, 72]
    for b in range(num_bars):
        root = progression[(b // 2) % len(progression)] + 24
        for s in range(8):
            if (b >= 2 and b < 22):
                note = root + lead_notes[s % len(lead_notes)] - 60
                freq = midi_to_hz(note)
                t_start = (b * 4 + s * 0.5) * beat_sec
                idx = int(t_start * RATE)
                dur = beat_sec * 0.45
                nsamples = int(dur * RATE)
                end = min(idx + nsamples, total_samples)
                tl = np.linspace(0, dur, end - idx, endpoint=False)
                s1 = np.sin(2 * np.pi * freq * tl)
                s2 = np.sin(2 * np.pi * (freq * 1.008) * tl)
                s3 = np.sin(2 * np.pi * (freq * 0.992) * tl)
                lead = (s1 + s2 + s3) * 0.33 * np.exp(-tl * 5.0) * 0.35
                left[idx:end] += lead * 0.95
                right[idx:end] += lead * 1.05

    max_val = max(np.max(np.abs(left)), np.max(np.abs(right)), 0.001)
    gain = 0.88 / max_val
    audio_stereo = np.column_stack([left * gain, right * gain])
    encode_mp3(audio_stereo, os.path.join(OUT_DIR, "track3_cyber_grandprix.mp3"))

if __name__ == "__main__":
    print("Synthesizing dynamic racing MP3 tracks...")
    generate_track_1()
    generate_track_2()
    generate_track_3()
    print("All MP3 tracks generated successfully!")
