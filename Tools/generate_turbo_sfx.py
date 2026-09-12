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

# 1. Turbo Jet Thruster Loop (2.0s seamless loop)
# Authentique souffle puissant de réacteur / protoxyde d'azote (Nitrous jet rush)
duration = 2.0
num_samples = int(sample_rate * duration)
t = np.linspace(0, duration, num_samples, endpoint=False)

np.random.seed(1337)
white = np.random.uniform(-1, 1, num_samples)

# Souffle d'air sous haute pression (large bande lisse, pas de sifflet strident)
air_body = bandpass(white, 400.0, 3800.0) * 1.5
air_high = bandpass(white, 2400.0, 7500.0) * 0.75
air_roar = lowpass(white, 320.0) * 1.8

# Sifflement doux et fluide de compresseur aérodynamique (1750 Hz)
spool_whistle = np.sin(2 * np.pi * 1720.0 * t) * 0.15 + np.sin(2 * np.pi * 3440.0 * t) * 0.05
# Ondulation douce du flux d'air
airflow = 0.88 + 0.12 * np.sin(2 * np.pi * 16.0 * t)

# Poussée grave moteur
sub_thrust = np.sin(2 * np.pi * 58.0 * t) * 0.35 + np.sin(2 * np.pi * 116.0 * t) * 0.20

turbo_loop = (air_body * 0.45 + air_high * 0.30 + air_roar * 0.35 + spool_whistle * airflow + sub_thrust * 0.25)

fade_len = int(sample_rate * 0.06)
ramp = np.linspace(0, 1, fade_len)
turbo_loop[:fade_len] = turbo_loop[:fade_len] * ramp + turbo_loop[-fade_len:] * (1 - ramp)
turbo_loop = turbo_loop[:num_samples - fade_len]
write_wav(os.path.join(out_dir, "turbo_loop.wav"), turbo_loop)

# 2. Turbo Ignite (0.32s coup de bélier et décharge d'allumage nitro)
ignite_dur = 0.32
n_ig = int(sample_rate * ignite_dur)
t_ig = np.linspace(0, ignite_dur, n_ig, endpoint=False)
env_punch = np.exp(-t_ig * 14.0)

# Percussion sourde d'injection (85 Hz -> 42 Hz)
f_punch = 85.0 * np.exp(-t_ig * 20.0) + 42.0
punch_sub = np.sin(np.cumsum(2 * np.pi * f_punch / sample_rate)) * env_punch * 0.9

# Jet d'air instantané
white_ig = np.random.uniform(-1, 1, n_ig)
air_ig = bandpass(white_ig, 800.0, 5200.0) * np.exp(-t_ig * 11.0) * 0.8
turbo_ignite = punch_sub * 0.55 + air_ig * 0.65
write_wav(os.path.join(out_dir, "turbo_ignite.wav"), turbo_ignite)

# 3. Blow-off Valve (0.38s détente pneumatique de soupape "pshh-tu-tu")
bo_dur = 0.38
n_bo = int(sample_rate * bo_dur)
t_bo = np.linspace(0, bo_dur, n_bo, endpoint=False)
white_bo = np.random.uniform(-1, 1, n_bo)

# Échappement d'air comprimé
air_bo = bandpass(white_bo, 2200.0, 7000.0)
flutter_bo = 0.4 + 0.6 * np.sin(2 * np.pi * 22.0 * t_bo) * np.exp(-t_bo * 8.0)
env_bo = np.exp(-t_bo * 8.5)
turbo_blowoff = air_bo * (1.0 + 0.8 * flutter_bo) * env_bo
write_wav(os.path.join(out_dir, "turbo_blowoff.wav"), turbo_blowoff)

print("Realistic racing turbo SFX generated successfully!")
