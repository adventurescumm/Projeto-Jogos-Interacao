"""Geometria/recursos do projeto Unity. Requer Python 3 + PyYAML; não simula Unity.
Execute: python Tests/validate_difficulty_layouts.py
"""
from pathlib import Path
from collections import deque, Counter
from math import hypot, ceil
import json, re, yaml
from functools import lru_cache
ROOT=Path(__file__).resolve().parents[1]
STEP=.25
PLAYER=.29  # Raio 0,25 + margem geométrica.
CHECKS=0

def check(ok,message):
    global CHECKS
    CHECKS+=1
    if not ok:raise AssertionError(message)

def docs(p):
    return {int(re.match(r'--- !u!\d+ &(-?\d+)',b)[1]):yaml.safe_load(re.sub(r'^--- !u!.*\n','',b,count=1)) for b in re.split(r'(?=--- !u!)',p.read_text())[1:]}

def point(d):return (d['x'],d['y'])
def distance(a,b):return hypot(a[0]-b[0],a[1]-b[1])
def circle_hits_segment(a,b,c,radius):
    d=(b[0]-a[0],b[1]-a[1]);length=d[0]**2+d[1]**2
    t=max(0,min(1,((c[0]-a[0])*d[0]+(c[1]-a[1])*d[1])/length)) if length else 0
    return distance((a[0]+t*d[0],a[1]+t*d[1]),c)<radius

def box_hits_segment(a,b,obstacle,radius):
    center=point(obstacle['position']);size=point(obstacle['size']);lo,hi=0.,1.
    for axis in [0,1]:
        delta=b[axis]-a[axis];minimum=center[axis]-size[axis]/2-radius;maximum=center[axis]+size[axis]/2+radius
        if abs(delta)<1e-9:
            if a[axis]<minimum or a[axis]>maximum:return False
        else:
            u=(minimum-a[axis])/delta;v=(maximum-a[axis])/delta
            lo=max(lo,min(u,v));hi=min(hi,max(u,v))
            if lo>hi:return False
    return True

prefabs={};prefab_names={}
for path in (ROOT/'Assets/Prefabs').glob('*.prefab'):
    guid=re.search(r'^guid: (\w+)',Path(str(path)+'.meta').read_text(),re.M)[1]
    prefabs[guid]=docs(path)
    prefab_names[guid]=path.stem
player=prefabs[re.search(r'^guid: (\w+)',(ROOT/'Assets/Prefabs/Player.prefab.meta').read_text(),re.M)[1]]
interval=next(v['MonoBehaviour']['shotInterval'] for v in player.values() if 'shotInterval' in v.get('MonoBehaviour',{}))
speed=next(v['MonoBehaviour']['speed'] for v in player.values() if 'speed' in v.get('MonoBehaviour',{}))
report=[];scenarios=0
for path in sorted((ROOT/'Assets/Data/Arenas').glob('*.asset')):
    for level in range(3):
        text=path.read_text();check(not re.search(r'[*&]id\d+\b',text),'Alias YAML: '+path.stem)
        room=docs(path)[11400000]['MonoBehaviour'];name=room['id'];enemies=[];ammo=[]
        time_limit=ceil(room['timeLimit']*(1.3 if level==0 else .9 if level==2 else 1))
        for spawn in room['enemies']:
            if spawn.get('minimumDifficulty',0)>level:continue
            prefab=prefabs[spawn['prefab']['guid']];config=prefab[spawn['prefab']['fileID']]['MonoBehaviour']
            transform=next(v['Transform'] for v in prefab.values() if 'Transform' in v)
            box=next(v['BoxCollider2D'] for v in prefab.values() if 'BoxCollider2D' in v)
            size=point(box['m_Size']);scale=point(transform['m_LocalScale'])
            radius=hypot(size[0]*scale[0],size[1]*scale[1])/2
            check(config['m_Enabled']==1 and box['m_Enabled']==1 and not box['m_IsTrigger'],name+': inimigo inválido')
            check(radius<=(.55 if room['bossArena'] else .4),name+': navegação menor que collider')
            shooter=next((v['MonoBehaviour'] for v in prefab.values() if 'shotsPerBurst' in v.get('MonoBehaviour',{})),None)
            family='boss' if config['isBoss'] else 'hitman' if config['stationary'] else 'soldier' if shooter and shooter['m_Enabled'] else 'robot'
            enemies.append({'position':point(spawn['position']),'radius':radius,'hp':config['maxHealth'],'boss':config['isBoss'],
                            'family':family,'prefab':prefab_names[spawn['prefab']['guid']]})
        check(len(enemies)>5,name+': cinco ou menos inimigos')
        check(sum(e['boss'] for e in enemies)==(1 if room['bossArena'] else 0),name+': contagem de chefes')
        check({'soldier','robot','hitman'} <= {e['family'] for e in enemies},name+': falta uma das três famílias de inimigos')
        for item in room['items']:
            config=prefabs[item['prefab']['guid']][item['prefab']['fileID']]['MonoBehaviour']
            if config['type']==0:ammo.append((point(item['position']),config['ammoAmount']))
        hp=sum(e['hp'] for e in enemies);supply=sum(amount for p,amount in ammo)
        check(supply>=ceil(hp*1.5),name+': sem margem de 50% de tiros extras')
        def outside_obstacles(p,r):
            if abs(p[0])>=7-r or abs(p[1])>=4-r:return False
            return all(not(abs(p[0]-o['position']['x'])<o['size']['x']/2+r and abs(p[1]-o['position']['y'])<o['size']['y']/2+r) for o in room['obstacles'])
        for index,e in enumerate(enemies):
            check(outside_obstacles(e['position'],e['radius']+.05),name+': inimigo nasce dentro da cobertura/parede')
            for other in enemies[:index]:
                check(distance(e['position'],other['position'])>e['radius']+other['radius']+.1,name+': inimigos sobrepostos')
        # Congela TODOS os inimigos como obstáculos conservadores, inclusive os móveis.
        def traversable(p):
            return outside_obstacles(p,PLAYER) and all(distance(p,e['position'])>PLAYER+e['radius'] for e in enemies)
        for p,amount in ammo:
            check(traversable(p),name+': pacote de munição sobreposto a inimigo/cobertura ou parede')
        def position(cell):return(-7+(cell[0]+.5)*STEP,-4+(cell[1]+.5)*STEP)
        cells={(x,y) for x in range(56) for y in range(32) if traversable(position((x,y)))}
        def cell_near(p):return min(cells,key=lambda c:distance(p,position(c)))
        edges={}
        for c in cells:
            neighbors=[]
            for dx,dy in [(1,0),(-1,0),(0,1),(0,-1)]:
                n=(c[0]+dx,c[1]+dy)
                if n not in cells:continue
                if any(circle_hits_segment(position(c),position(n),e['position'],e['radius']+PLAYER) for e in enemies):continue
                if any(box_hits_segment(position(c),position(n),o,PLAYER) for o in room['obstacles']):continue
                neighbors.append(n)
            edges[c]=neighbors
        @lru_cache(maxsize=None)
        def paths(start):
            reached={start:0};queue=deque([start])
            while queue:
                c=queue.popleft()
                for n in edges[c]:
                    if n in reached:continue
                    reached[n]=reached[c]+1;queue.append(n)
            return reached
        entries=[p for side,p in [('north',(0,3)),('east',(6,0)),('south',(0,-3)),('west',(-6,0))] if room[side]]
        if name=='estudio':entries.append((0,-2.5))
        firing=[]
        for index,e in enumerate(enemies):
            options=set()
            for c in cells:
                p=position(c)
                if not 1.25<=distance(p,e['position'])<=3:continue
                if any(box_hits_segment(p,e['position'],o,.05) for o in room['obstacles']):continue
                if any(circle_hits_segment(p,e['position'],other['position'],other['radius']+.05) for j,other in enumerate(enemies) if j!=index):continue
                options.add(c)
            check(bool(options),name+': inimigo sem posição de tiro')
            firing.append(options)
        worst=0
        for entry in entries:
            scenarios+=1
            check(traversable(entry),name+': entrada bloqueada')
            check(min(distance(entry,e['position']) for e in enemies)>=1.25,name+': inimigo colado à entrada')
            current=cell_near(entry);initial=paths(current)
            check(all(cell_near(p) in initial for p,amount in ammo),name+': munição inacessível com zero tiros')
            check(all(options&initial.keys() for options in firing),name+': alvo inacessível da entrada')
            # Rota construtiva: começa com zero munição; recolhe pacotes e visita
            # posições de tiro. Não inclui reação humana, esquiva nem IA/física real.
            remaining=[cell_near(p) for p,amount in ammo];length=distance(entry,position(current))
            while remaining:
                distances=paths(current);target=min(remaining,key=lambda c:distances.get(c,10**9))
                check(target in distances,name+': rota de coleta interrompida')
                length+=distances[target]*STEP;current=target;remaining.remove(target)
            targets=list(range(len(enemies)))
            while targets:
                distances=paths(current)
                choices=[(distances[c],index,c) for index in targets for c in firing[index] if c in distances]
                check(bool(choices),name+': sem sequência de eliminação')
                steps,index,current=min(choices);length+=steps*STEP;targets.remove(index)
            estimate=length/speed+ceil(hp*1.5)*interval+len(enemies)*3/12
            check(estimate<time_limit,name+': nem a rota simplificada cabe no prazo')
            worst=max(worst,estimate)
        report.append({'room':name,'difficulty':['easy','normal','hard'][level],'enemies':len(enemies),
                       'enemy_families':dict(sorted(Counter(e['family'] for e in enemies).items())),
                       'enemy_prefabs':dict(sorted(Counter(e['prefab'] for e in enemies).items())),
                       'enemy_hp':hp,'floor_ammo':supply,'entry_scenarios':len(entries),'model_route_seconds':round(worst,2),'time_limit':time_limit})
result={'checks':CHECKS,'enemy_totals_by_difficulty':{mode:sum(r['enemies'] for r in report if r['difficulty']==mode) for mode in ['easy','normal','hard']},'entry_scenarios':scenarios,'scope':'Geometria conservadora e recursos; sem execução Unity, esquiva ou dano recebido. Tempos não são benchmark de partidas.','rooms':report}
print(json.dumps(result,ensure_ascii=False,indent=2))
