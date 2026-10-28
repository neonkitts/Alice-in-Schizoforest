from pathlib import Path
import math, random, struct, wave

RATE = 44100
DURATION = .72
rng = random.Random(1976)
phase = 0.0
low_noise = 0.0
samples = []

for i in range(int(RATE * DURATION)):
    t = i / RATE
    env = min(1.0, t / .035) * max(0.0, 1.0 - t / DURATION) ** .72
    pitch = 178 - 123 * min(1.0, t / .55)
    phase += math.tau * pitch / RATE
    noise = rng.uniform(-1, 1)
    low_noise = low_noise * .93 + noise * .07
    voice = math.sin(phase) + .34 * math.sin(phase * 1.97) + .18 * math.sin(phase * .51)
    rasp = (noise - low_noise) * .46
    samples.append(math.tanh((voice * .42 + rasp) * env * 1.18) * .82)

out = Path(__file__).resolve().parents[1] / "Assets/Resources/Audio/MonsterDefeat.wav"
out.parent.mkdir(parents=True, exist_ok=True)
with wave.open(str(out), "wb") as f:
    f.setnchannels(1)
    f.setsampwidth(2)
    f.setframerate(RATE)
    f.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, sample)) * 32767)) for sample in samples))
print(out)
