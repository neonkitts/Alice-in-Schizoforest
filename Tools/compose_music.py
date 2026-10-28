from pathlib import Path
import numpy as np, wave
rate=22050; duration=96; n=rate*duration
audio=np.zeros((n,2),np.float32)
def note(midi,start,seconds,volume,kind):
 t=np.arange(int(seconds*rate))/rate; f=440*2**((midi-69)/12)
 if kind=='bell': y=(np.sin(2*np.pi*f*t)+.3*np.sin(2*np.pi*f*2.003*t))*np.exp(-t*1.5)
 elif kind=='pad': y=(np.sin(2*np.pi*f*t)+.28*np.sin(2*np.pi*f*1.003*t)+.15*np.sin(2*np.pi*f*2*t))*.6
 else:y=np.sin(2*np.pi*(48*t+30*.05*(1-np.exp(-t/.05))))*np.exp(-t*9)
 env=np.minimum(t/.1,1)*np.minimum((seconds-t)/.3,1);y*=env*volume
 k=int(start*rate);count=min(len(y),n-k)
 audio[k:k+count,0]+=y[:count];audio[k:k+count,1]+=y[:count]*(.8 if midi%2 else 1)
chords=[(38,50,53,57),(34,46,50,53),(36,48,51,55),(33,45,49,52)]
for bar in range(24):
 start=bar*4; chord=chords[bar%4]
 for m in chord:note(m,start,4,.075,'pad')
 for beat in range(4):
  note(chord[1+beat%3]+12,start+beat,2,.065 if bar%8<4 else .095,'bell')
  if beat%2==0:note(36,start+beat,.6,.09,'pulse')
for delay,gain in [(0.43,.2),(.91,.10),(1.7,.06)]:
 d=int(delay*rate);audio[d:]+=audio[:-d,::-1]*gain
fade=rate;audio[:fade]*=np.linspace(0,1,fade)[:,None];audio[-fade:]*=np.linspace(1,0,fade)[:,None]
audio*=.8/max(float(np.max(abs(audio))),.001)
path=Path(__file__).resolve().parents[1]/'Assets/Resources/Audio/Schizoforest.wav'
with wave.open(str(path),'wb') as f:f.setnchannels(2);f.setsampwidth(2);f.setframerate(rate);f.writeframes((audio*32767).astype('<i2').tobytes())
print('Original dark ambient score: 96 seconds')
