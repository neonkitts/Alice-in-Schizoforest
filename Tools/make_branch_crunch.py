"""Original dry twig-crunch jump effect, deterministic transient synthesis."""
import math,random,struct,wave
from pathlib import Path
r=random.Random(191)
rate=44100
sound=[0.0]*int(rate*.29)
for start,amp,decay in [(0,1,.010),(.016,.65,.006),(.038,.7,.012),(.076,.42,.008),(.13,.22,.010),(.18,.13,.009)]:
 previous=0
 for j in range(int(rate*.055)):
  t=j/rate;i=int(start*rate)+j
  noise=r.uniform(-1,1);high=noise-previous*.65;previous=noise
  # Short irregular broadband cracks with a muted woody body; no rising tone.
  value=(high*.78+math.sin(2*math.pi*940*t)*.16+math.sin(2*math.pi*370*t)*.1)*math.exp(-t/decay)
  if i<len(sound):sound[i]+=amp*value
peak=max(map(abs,sound))
data=b''.join(struct.pack('<h',int(v/peak*24000)) for v in sound)
target=Path(__file__).resolve().parents[1]/'Assets/Resources/Audio/BranchCrunch.wav'
with wave.open(str(target),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes(data)
print(target)
