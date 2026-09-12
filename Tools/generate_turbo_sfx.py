import numpy as np
import wave
import os

sample_rate = 44100

def write_wav(filename, data):
    max_val = np.max(np.abs(data))
    if max_val > 0:
        data = data / max_val * 0.85
    int_data = (data * 32767).astype(np.int16)
    
    with wave.open(filename, 'wb') as wav:
        wav.setnchannels(1)
        wav.setsampwidth(2)
        wav.setframerate(sample_rate)
        wav.writeframes(int_data.tobytes())
    print(f"Generated: {filename} ({len(data)/sample_rate:.2f}s)")

out_dir = r"Assets/PocketGP/Resources/Audio"
os.makedirs(out_dir, exist_ok=True)

def lowpass(x, cutoff_hz):
    alpha = min(1.0, 2.0 * np.pi * cutoff_hz / sample_rate)
    y = np.zeros_like(x)
    val = 0.0
    for i in range(len(x)):
        val += alpha * (x[i] - val)
        y[i] = val
    return y

def bandpass(x, low_hz, high_hz):
    lp = lowpass(x, high_hz)
    hp = lp - lowpass(lp, low_hz)
    return hp

# 1. Turbo Jet Loop (Seamless 2.0s loop)
duration = 2.0
num_samples = int(sample_rate * duration)
t = np.linspace(0, duration, num_samples, endpoint=False)

f0, f1, f2 = 980.0, 1470.0, 2450.0
spool = (
    0.35 * np.sin(2 * np.pi * f0 * t) +
    0.20 * np.sin(2 * np.pi * f1 * t + 0.3) +
    0.15 * np.sin(2 * np.pi * f2 * t + 0.7)
)
flutter = 0.85 + 0.15 * np.sin(2 * np.pi * 24.0 * t)
spool *= flutter

np.random.seed(42)
white = np.random.uniform(-1, 1, num_samples)
roar = lowpass(white, 280.0) * 2.2
nitrous = bandpass(white, 1600.0, 6000.0) * 1.3
sub = 0.45 * np.sin(2 * np.pi * 65.0 * t) + 0.25 * np.sin(2 * np.pi * 130.0 * t)

turbo_loop = spool * 0.42 + nitrous * 0.38 + roar * 0.32 + sub * 0.25

fade_len = int(sample_rate * 0.05)
ramp = np.linspace(0, 1, fade_len)
turbo_loop[:fade_len] = turbo_loop[:fade_len] * ramp + turbo_loop[-fade_len:] * (1 - ramp)
turbo_loop = turbo_loop[:num_samples - fade_len]
write_wav(os.path.join(out_dir, "turbo_loop.wav"), turbo_loop)

# 2. Turbo Ignite (0.35s punchy burst)
ignite_dur = 0.35
n_ig = int(sample_rate * ignite_dur)
t_ig = np.linspace(0, ignite_dur, n_ig, endpoint=False)
env_ig = np.exp(-t_ig * 9.0)
freq_ig = 520.0 * np.exp(-t_ig * 15.0) + 110.0
phase_ig = np.cumsum(2 * np.pi * freq_ig / sample_rate)
body_ig = np.sin(phase_ig) * env_ig

white_ig = np.random.uniform(-1, 1, n_ig)
rush_ig = bandpass(white_ig, 700.0, 4200.0) * np.exp(-t_ig * 12.0)
turbo_ignite = body_ig * 0.7 + rush_ig * 0.6
write_wav(os.path.join(out_dir, "turbo_ignite.wav"), turbo_ignite)

# 3. Blow-off Valve (0.4s flutter discharge)
bo_dur = 0.40
n_bo = int(sample_rate * bo_dur)
t_bo = np.linspace(0, bo_dur, n_bo, endpoint=False)
white_bo = np.random.uniform(-1, 1, n_bo)
air_bo = bandpass(white_bo, 1800.0, 5800.0)
flutter_bo = 0.45 + 0.55 * np.sin(2 * np.pi * 26.0 * t_bo)
env_bo = np.exp(-t_bo * 7.0)
chirp_bo = np.sin(2 * np.pi * 1750.0 * t_bo) * np.exp(-t_bo * 14.0) * 0.3
turbo_blowoff = air_bo * flutter_bo * env_bo + chirp_bo
write_wav(os.path.join(out_dir, "turbo_blowoff.wav"), turbo_blowoff)

print("All turbo SFX successfully generated!")
