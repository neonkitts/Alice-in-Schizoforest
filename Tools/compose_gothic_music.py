from pathlib import Path
import wave
import numpy as np

RATE=22050
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Assets/Resources/Audio'
OUT.mkdir(parents=True,exist_ok=True)
rng=np.random.default_rng(1867)

def midi(m): return 440.0*2**((m-69)/12)

def envelope(t,attack,release,length):
    return np.minimum(1,t/max(attack,1e-4))*np.minimum(1,np.maximum(0,length-t)/max(release,1e-4))

def add_tone(buf,m,start,length,amp,voice='organ',pan=0):
    k=int(start*RATE); count=min(int(length*RATE),len(buf)-k)
    if count<=0:return
    t=np.arange(count,dtype=np.float32)/RATE; f=midi(m)
    if voice=='organ':
        y=(np.sin(2*np.pi*f*t)+.42*np.sin(2*np.pi*2*f*t)+.20*np.sin(2*np.pi*3*f*t)+.10*np.sin(2*np.pi*4*f*t))
        y*=.56*(.93+.07*np.sin(2*np.pi*4.1*t))
        env=envelope(t,.12,.35,length)
    elif voice=='choir':
        y=(np.sin(2*np.pi*f*t+.4*np.sin(2*np.pi*.23*t))+.35*np.sin(2*np.pi*f*1.004*t)+.18*np.sin(2*np.pi*f*.997*t))*.48
        env=envelope(t,.7,1.1,length)
    elif voice=='pluck':
        y=(np.sin(2*np.pi*f*t)+.45*np.sin(2*np.pi*2.01*f*t)+.2*np.sin(2*np.pi*3.02*f*t))*np.exp(-t*2.8)
        env=np.minimum(1,t/.008)*np.minimum(1,np.maximum(0,length-t)/.08)
    elif voice=='bell':
        y=(np.sin(2*np.pi*f*t)+.55*np.sin(2*np.pi*2.01*f*t)+.23*np.sin(2*np.pi*3.93*f*t))*np.exp(-t*.75)
        env=np.minimum(1,t/.01)*np.minimum(1,np.maximum(0,length-t)/.15)
    else:
        y=np.sin(2*np.pi*f*t);env=envelope(t,.01,.08,length)
    y*=env*amp
    left=np.sqrt((1-pan)*.5);right=np.sqrt((1+pan)*.5)
    buf[k:k+count,0]+=y*left;buf[k:k+count,1]+=y*right

def add_hit(buf,start,amp,pitch=52):
    k=int(start*RATE); count=min(int(.34*RATE),len(buf)-k)
    if count<=0:return
    t=np.arange(count,dtype=np.float32)/RATE
    noise=rng.normal(0,1,count).astype(np.float32)
    body=np.sin(2*np.pi*(pitch+(pitch*.45)*np.exp(-t*18))*t)
    y=(body*.75+noise*.16)*np.exp(-t*13)*amp
    buf[k:k+count,0]+=y;buf[k:k+count,1]+=y*.94

def reverb(buf,taps):
    dry=buf.copy()
    for seconds,gain,swap in taps:
        d=int(seconds*RATE)
        if d<len(buf):buf[d:]+=dry[:-d,::-1 if swap else 1]*gain

def save(name,buf,fade=True):
    if fade:
        f=min(RATE,len(buf)//12);buf[:f]*=np.linspace(0,1,f)[:,None];buf[-f:]*=np.linspace(1,0,f)[:,None]
    peak=max(float(np.max(np.abs(buf))),.001);buf*=.88/peak
    with wave.open(str(OUT/name),'wb') as w:
        w.setnchannels(2);w.setsampwidth(2);w.setframerate(RATE);w.writeframes((buf*32767).astype('<i2').tobytes())

# Gothic exploration: pipe organ, distant choir, iron bells and harpsichord in D minor.
duration=96; main=np.zeros((duration*RATE,2),np.float32);bar=4.0
chords=[(38,45,50,53,57),(34,41,46,50,53),(36,43,48,51,55),(33,40,45,49,52)]
arp=[0,2,1,3,2,4,3,1]
for b in range(24):
    start=b*bar; chord=chords[b%4]
    add_tone(main,chord[0],start,4,.12,'organ',-.12)
    for m in chord[1:]:add_tone(main,m,start,4,.055,'organ',.08)
    add_tone(main,chord[2]+12,start+.15,3.7,.036,'choir',.16 if b%2 else -.16)
    for i in range(8):
        m=chord[arp[i]%len(chord)]+12+(12 if i in (3,7) else 0)
        add_tone(main,m,start+i*.5,1.1,.047,'pluck',-.42 if i%2==0 else .42)
    if b%2==0:add_tone(main,chord[0]+24,start+.08,3.1,.065,'bell',-.25 if b%4==0 else .25)
    if b%4==3:add_tone(main,61,start+3.35,.62,.022,'bell',.32)
reverb(main,[(.37,.16,True),(.79,.10,False),(1.41,.065,True),(2.21,.035,False)])
save('Schizoforest.wav',main)

# Pursuit: faster bowed-string/organ ostinato, heartbeat and dissonant bell accents.
duration=16; chase=np.zeros((duration*RATE,2),np.float32);beat=60/144
ost=[38,45,39,45,38,46,41,45]
for i in range(int(duration/beat)):
    st=i*beat;m=ost[i%len(ost)]
    add_tone(chase,m,st,beat*.95,.115,'pluck',-.28 if i%2==0 else .28)
    add_tone(chase,m+12,st,beat*.92,.052,'organ',.18)
    if i%4 in (0,2):add_hit(chase,st,.19,48 if i%4==0 else 62)
    if i%8==6:add_tone(chase,63,st,.55,.06,'bell',.35)
for st in np.arange(0,duration,2*beat):
    add_tone(chase,38,st,2*beat,.08,'organ',-.1);add_tone(chase,39,st,2*beat,.045,'organ',.1)
reverb(chase,[(.19,.12,True),(.41,.085,False),(.83,.04,True)])
save('DemonChase.wav',chase,fade=True)
print('Created original gothic exploration and anxiety chase tracks')
