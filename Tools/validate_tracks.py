import numpy as np, math, json
from pathlib import Path
import os
os.chdir(Path(__file__).resolve().parents[2])
controls=[
[(-26,-23),(6,-25),(30,-18),(34,4),(21,24),(-8,25),(-32,14),(-36,-5)],
[(-29,-25),(1,-27),(32,-23),(33,-4),(12,0),(26,21),(1,27),(-29,21),(-35,0)],
[(-28,-26),(1,-26),(30,-21),(35,-1),(21,9),(31,27),(6,29),(-6,11),(-31,24),(-37,0)]]
def track(c):
 c=np.array(c,float);p=[]
 for i in range(len(c)):
  a,b,e,d=c[(i-1)%len(c)],c[i],c[(i+1)%len(c)],c[(i+2)%len(c)]
  for k in range(16):
   t=k/16;p.append(.5*(2*b+(-a+e)*t+(2*a-5*b+4*e-d)*t*t+(-a+3*b-3*e+d)*t*t*t))
 p=np.array(p);t=np.roll(p,-1,axis=0)-np.roll(p,1,axis=0);t/=np.linalg.norm(t,axis=1)[:,None];return p,t
results=[]
for index,c in enumerate(controls):
 p,t=track(c);gates=p[::8]
 for difficulty in range(3):
  for ident in range(1,5):
   f=t[0];r=np.array([f[1],-f[0]]);pos=p[0]-f*(2.5+(ident//2)*2.5)+r*(-1.4 if ident%2==0 else 1.4)
   heading=math.atan2(f[0],f[1]);speed=steering=0.;velocity=np.zeros(2);gate=1;laps=0;rescue=0;dt=.02
   for step in range(20000):
    dist=np.linalg.norm(p-pos,axis=1);near=int(np.argmin(dist));look=min(7,max(3,3+int(speed*.17+.5)));target=gate*8
    right=np.array([t[target,1],-t[target,0]]);direction=p[target]+right*((ident-2.5)*.45)-pos
    angle=(math.atan2(direction[0],direction[1])-heading+math.pi)%(2*math.pi)-math.pi
    steer=max(-1,min(1,math.degrees(angle)/30));throttle=.35 if abs(math.degrees(angle))>65 else 1
    maximum=16.3+difficulty*1.5+ident*.15
    if dist[near]>3.8:maximum*=.46
    speed+=np.clip(maximum*throttle-speed,-8*dt,8*dt);steering+=(steer-steering)*dt*9
    heading+=steering*math.radians(105)*min(1,speed/5)*dt
    desired=np.array([math.sin(heading),math.cos(heading)])*speed;velocity+=(desired-velocity)*(1-math.exp(-9*dt));old=pos.copy();pos+=velocity*dt
    seg=pos-old;u=np.clip(np.dot(gates[gate]-old,seg)/max(np.dot(seg,seg),.00001),0,1)
    if np.linalg.norm(old+seg*u-gates[gate])<5:
     if gate==0:laps+=1
     gate=(gate+1)%len(gates)
     if laps==3:break
   assert laps==3,(index,difficulty,ident,gate,pos.tolist())
   results.append({'track':index+1,'difficulty':difficulty,'ai':ident,'seconds':round(step*dt,2)})
print(json.dumps({'finished_runs':len(results),'range_seconds':[min(x['seconds'] for x in results),max(x['seconds'] for x in results)]}))
open('PocketGrandPrix/Tools/ai_validation.json','w').write(json.dumps(results,indent=2))
if __name__=='__main__':
 import matplotlib;matplotlib.use('Agg')
 import matplotlib.pyplot as plt
 fig,axes=plt.subplots(1,3,figsize=(14,5),facecolor='#102638')
 for i,ax in enumerate(axes):
  p,t=track(controls[i]);p=np.vstack([p,p[0]])
  ax.set_facecolor('#102638');ax.plot(p[:,0],p[:,1],color='#F8EED9',lw=24,solid_capstyle='round');ax.plot(p[:,0],p[:,1],color='#526a79',lw=20,solid_capstyle='round');ax.scatter(p[0,0],p[0,1],color='#54e3c5',s=100,zorder=5)
  ax.set_title(['Petit-déjeuner express','Bureau en folie','Jardin des champions'][i],color='#F8EED9',pad=18);ax.set_aspect('equal');ax.set_xlim(-48,48);ax.set_ylim(-40,40);ax.axis('off')
 fig.suptitle('POCKET GRAND PRIX  /  PLANS DES CIRCUITS',color='#54e3c5',fontsize=19);fig.text(.5,.06,'Tracés réels du projet · Point vert : départ · Schéma, pas une capture Unity',ha='center',color='#B5CCD0')
 fig.savefig('PocketGrandPrix/Circuits.png',dpi=150,bbox_inches='tight')
