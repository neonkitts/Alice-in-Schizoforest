from pathlib import Path
import math, random, struct, wave

RATE = 44100
DURATION = 4.15
rng = random.Random(1806)
count = int(RATE * DURATION)
dry = [0.0] * count
phase = 0.0
noise_low = 0.0
noise_high = 0.0

for i in range(count):
    t = i / RATE
    attack = min(1.0, t / .16)
    release = min(1.0, (DURATION - t) / .72)
    env = max(0.0, attack * release)
    growl_env = (.74 + .26 * math.sin(math.pi * min(1, t / DURATION))) * env
    pitch = 94 - 42 * (t / DURATION) + 7 * math.sin(t * 5.1) + 2.5 * math.sin(t * 17)
    phase += 2 * math.pi * pitch / RATE

    raw_noise = rng.uniform(-1, 1)
    noise_low = noise_low * .965 + raw_noise * .035
    noise_high = noise_high * .56 + (raw_noise - noise_low) * .44
    throat = (
        math.sin(phase)
        + .48 * math.sin(phase * 2.01 + .4)
        + .24 * math.sin(phase * 3.03)
        + .16 * math.sin(phase * .503)
    )
    formants = .30 * math.sin(phase * 5.25) + .18 * math.sin(phase * 8.1)
    rasp = noise_high * (.45 + .55 * abs(math.sin(t * 29)))
    chest_pulse = .78 + .22 * math.sin(t * 8.7) ** 2
    dry[i] = (throat * .32 + formants * .13 + rasp * .44 + noise_low * .22) * growl_env * chest_pulse

# A short cave response makes the roar feel like a large creature below ground.
samples = [0.0] * count
echoes = [(0, 1.0), (int(.085 * RATE), .32), (int(.19 * RATE), .22), (int(.34 * RATE), .14), (int(.57 * RATE), .09)]
for i in range(count):
    value = 0.0
    for delay, gain in echoes:
        if i >= delay:
            value += dry[i - delay] * gain
    samples[i] = math.tanh(value * 1.35) * .92

out = Path(__file__).resolve().parents[1] / "Assets/Resources/Audio/TattooRoar.wav"
out.parent.mkdir(parents=True, exist_ok=True)
with wave.open(str(out), "wb") as f:
    f.setnchannels(1)
    f.setsampwidth(2)
    f.setframerate(RATE)
    f.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, v)) * 32767)) for v in samples))
print(out)
