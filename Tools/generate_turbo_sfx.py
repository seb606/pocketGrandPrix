import numpy as np
import wave
import os

sample_rate = 44100

def write_wav(filename, data, peak_level=0.55):
    max_val = np.max(np.abs(data))
    if max_val > 0:
        data = data / max_val * peak_level
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
# Son discret, feutré, sans aigu strident : souffle chaud & grondement aérodynamique
duration = 2.0
num_samples = int(sample_rate * duration)
t = np.linspace(0, duration, num_samples, endpoint=False)

np.random.seed(42)
white = np.random.uniform(-1, 1, num_samples)

# Souffle chaud filtré (250 Hz - 950 Hz), doux et feutré
air_warm = bandpass(white, 220.0, 950.0) * 1.6
air_sub = lowpass(white, 280.0) * 1.4

# Turbine grave et discrète (440 Hz au lieu de 1720 Hz)
spool_whistle = np.sin(2 * np.pi * 440.0 * t) * 0.08 + np.sin(2 * np.pi * 660.0 * t) * 0.03
airflow = 0.92 + 0.08 * np.sin(2 * np.pi * 8.0 * t)

# Poussée sourde basse fréquence (65 Hz)
sub_thrust = np.sin(2 * np.pi * 65.0 * t) * 0.25

turbo_loop = (air_warm * 0.55 + air_sub * 0.35 + spool_whistle * airflow + sub_thrust * 0.20)

fade_len = int(sample_rate * 0.08)
ramp = np.linspace(0, 1, fade_len)
turbo_loop[:fade_len] = turbo_loop[:fade_len] * ramp + turbo_loop[-fade_len:] * (1 - ramp)
turbo_loop = turbo_loop[:num_samples - fade_len]
write_wav(os.path.join(out_dir, "turbo_loop.wav"), turbo_loop, peak_level=0.50)

# 2. Turbo Ignite (déclenchement doux et feutré)
ignite_dur = 0.28
n_ig = int(sample_rate * ignite_dur)
t_ig = np.linspace(0, ignite_dur, n_ig, endpoint=False)
env_punch = np.exp(-t_ig * 16.0)

# Coup sourd bas médium (110 Hz -> 55 Hz)
f_punch = 110.0 * np.exp(-t_ig * 18.0) + 55.0
punch_sub = np.sin(np.cumsum(2 * np.pi * f_punch / sample_rate)) * env_punch * 0.65

white_ig = np.random.uniform(-1, 1, n_ig)
air_ig = bandpass(white_ig, 350.0, 1400.0) * np.exp(-t_ig * 14.0) * 0.5
turbo_ignite = punch_sub * 0.50 + air_ig * 0.50
write_wav(os.path.join(out_dir, "turbo_ignite.wav"), turbo_ignite, peak_level=0.45)

# 3. Blow-off Valve (décharge douce et feutrée "pshh")
bo_dur = 0.30
n_bo = int(sample_rate * bo_dur)
t_bo = np.linspace(0, bo_dur, n_bo, endpoint=False)
white_bo = np.random.uniform(-1, 1, n_bo)

# Échappement feutré filtré à 1600 Hz max
air_bo = bandpass(white_bo, 450.0, 1600.0)
env_bo = np.exp(-t_bo * 10.0)
turbo_blowoff = air_bo * env_bo * 0.8
write_wav(os.path.join(out_dir, "turbo_blowoff.wav"), turbo_blowoff, peak_level=0.40)

print("Subtle, warm, low-pitched turbo SFX generated successfully!")
