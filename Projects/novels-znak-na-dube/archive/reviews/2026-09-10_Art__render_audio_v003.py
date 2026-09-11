#!/usr/bin/env python3
"""Deterministic original ambience/SFX renderer for Nochelestye."""
from array import array
import math, random, wave
from pathlib import Path

SR = 44100
OUT = Path(__file__).parents[1] / "Assets" / "Audio"
STEMS = Path(__file__).parent / "AudioStems"
OUT.mkdir(parents=True, exist_ok=True)
STEMS.mkdir(parents=True, exist_ok=True)

def write_stereo(path, left, right):
    peak = max(max(abs(v) for v in left), max(abs(v) for v in right), 1e-9)
    gain = min(0.92 / peak, 1.0)
    pcm = array('h')
    for a, b in zip(left, right):
        pcm.append(int(max(-1, min(1, a * gain)) * 32767))
        pcm.append(int(max(-1, min(1, b * gain)) * 32767))
    with wave.open(str(path), 'wb') as f:
        f.setnchannels(2); f.setsampwidth(2); f.setframerate(SR); f.writeframes(pcm.tobytes())

def ambience():
    dur = 48.0; n = int(SR * dur); rng = random.Random(71983)
    left = array('f', [0.0]) * n; right = array('f', [0.0]) * n
    phases = [rng.random() * math.tau for _ in range(5)]
    freqs = [43.0, 57.0, 71.0, 86.0, 113.0]
    for i in range(n):
        t = i / SR
        breathe = 0.68 + 0.22 * math.sin(math.tau*t/dur) + 0.10 * math.sin(math.tau*3*t/dur)
        bed = sum(math.sin(math.tau*f*t + p) * a for f,p,a in zip(freqs,phases,[.10,.07,.045,.03,.02]))
        root = .025 * math.sin(math.tau*(129 + 2*math.sin(math.tau*t/16))*t) * (0.5 + 0.5*math.sin(math.tau*4*t/dur))
        air = (rng.random()*2-1) * .007 * (0.7 + 0.3*math.sin(math.tau*2*t/dur))
        left[i] = (bed*breathe + root + air) * .62
        right[i] = (bed*breathe - root*.7 + air*.8) * .59
    # sparse stick-slip wood catches, all periodic inside loop window
    for at, pan in [(5.7,-.4),(13.2,.5),(22.9,-.2),(34.4,.65),(42.1,-.55)]:
        start=int(at*SR); length=int(.62*SR)
        for j in range(length):
            x=j/SR; env=(1-math.exp(-x*90))*math.exp(-x*7)
            v=(math.sin(math.tau*176*x)+.45*math.sin(math.tau*263*x))*env*.055
            left[start+j]+=v*(1-pan)*.5; right[start+j]+=v*(1+pan)*.5
    write_stereo(STEMS/'pressure-bed.wav', left, right)
    write_stereo(OUT/'nochelestye-bed.wav', left, right)

def sfx(name, dur, maker):
    n=int(SR*dur); rng=random.Random(name)
    l=array('f'); r=array('f')
    for i in range(n):
        t=i/SR; v=maker(t,dur,rng)
        l.append(v*(.96 + .04*math.sin(t*9))); r.append(v*(.92 + .08*math.sin(t*7+1)))
    write_stereo(OUT/name, l, r)

ambience()
sfx('mark-pulse.wav', 2.8, lambda t,d,r: (.16*math.sin(math.tau*(48+10*t)*t)+.07*math.sin(math.tau*91*t))*math.sin(math.pi*t/d)**2)
sfx('root-creak.wav', 3.6, lambda t,d,r: ((r.random()*2-1)*.055 + .12*math.sin(math.tau*(82-18*t/d)*t))*math.sin(math.pi*t/d)**1.4)
sfx('bronze-answer.wav', 4.2, lambda t,d,r: (.18*math.sin(math.tau*173*t)+.09*math.sin(math.tau*267*t)+.045*math.sin(math.tau*431*t))*math.exp(-t*1.25)*(1-math.exp(-t*80)))
sfx('cliffhanger-sting.wav', 6.0, lambda t,d,r: (.13*math.sin(math.tau*(39+18*t/d)*t)+.08*math.sin(math.tau*117*t))*math.sin(math.pi*t/d)**1.3)
print('rendered', *(p.name for p in sorted(OUT.glob('*.wav'))))
