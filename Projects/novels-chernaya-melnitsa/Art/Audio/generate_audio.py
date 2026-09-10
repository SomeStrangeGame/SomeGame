from pathlib import Path
import math, random, struct, wave

ROOT=Path(__file__).resolve().parents[2] / 'Assets' / 'Audio'
ROOT.mkdir(parents=True,exist_ok=True)
SR=44100

def wav(name,duration,fn):
    n=int(duration*SR)
    with wave.open(str(ROOT/name),'wb') as w:
        w.setparams((2,2,SR,n,'NONE','not compressed'))
        for i in range(n):
            t=i/SR
            l,r=fn(t,duration)
            w.writeframesraw(struct.pack('<hh',int(max(-.98,min(.98,l))*32767),int(max(-.98,min(.98,r))*32767)))

def loop_bed(seed,base,detail):
    rng=random.Random(seed)
    phases=[rng.random()*6.28 for _ in range(5)]
    freqs=[base,base*1.19,base*1.51,base*2.03,detail]
    def f(t,d):
        # Quantized cycle counts make the first and last sample phase-continuous.
        qs=[round(q*d)/d for q in freqs]
        drift=sum(math.sin(2*math.pi*q*t+p)*(0.024/(j+1)) for j,(q,p) in enumerate(zip(qs,phases)))
        breath=.008*math.sin(2*math.pi*(round(.07*d)/d)*t)
        return (drift+breath*.8,drift*.92-breath*.7)
    return f

wav('mill-pressure-loop.wav',72,loop_bed(31,47,173))
wav('forest-wind-loop.wav',58,loop_bed(47,63,421))
wav('dawn-release-loop.wav',48,loop_bed(83,71,263))

def event(kind):
    def f(t,d):
        env=math.sin(math.pi*min(1,t/d))**2
        if kind=='thump': s=(math.sin(2*math.pi*(58-18*t)*t)+.35*math.sin(2*math.pi*117*t))*math.exp(-4*t)
        elif kind=='paper': s=sum(math.sin(2*math.pi*(700+j*173)*t+j)*math.exp(-18*max(0,t-j*.035)) for j in range(5))*.16
        elif kind=='stone': s=sum(math.sin(2*math.pi*f0*t)*math.exp(-(2+j)*t) for j,f0 in enumerate((39,61,89)))*.25
        elif kind=='whisper': s=(random.random()*2-1)*(.22+math.sin(t*17)*.08)*math.exp(-1.8*t)
        else: s=sum(math.sin(2*math.pi*(190+j*71)*t+j)*math.exp(-13*max(0,t-j*.09)) for j in range(6))*.12
        return s*env*.45,s*env*.41
    return f

wav('sail-thump.wav',1.6,event('thump'))
wav('wood-catch.wav',1.3,event('catch'))
wav('paper-unfold.wav',1.1,event('paper'))
wav('flour-whisper.wav',2.8,event('whisper'))
wav('stone-stop.wav',3.0,event('stone'))
print('audio created',ROOT)
