from pathlib import Path
from collections import deque
import json, re, yaml
root=Path(__file__).resolve().parents[1]
rooms={}
for p in (root/'Assets/Data/Arenas').glob('*.asset'):
    body=re.sub(r'^%.*\n','',p.read_text(),flags=re.M)
    body=re.sub(r'^--- !u!.*\n','',body,flags=re.M)
    room=yaml.safe_load(body)['MonoBehaviour']; rooms[room['id']]=room
sides=['north','east','south','west']
bits={'A':1,'B':2}
def required(room):return sum(bits[k] for k in room['requiredKeys'])
def next_states(state):
    name,held=state; room=rooms[name]
    if name=='final': return []  # Vitória é terminal.
    result=[]
    reward=bits.get(room['rewardKey'],0)
    if reward and not held&reward:result.append((name,held|reward))
    for side in sides:
        dest=room[side]
        if dest and required(rooms[dest])&held==required(rooms[dest]):result.append((dest,held))
    return result
start=('estudio',0); seen={start}; q=deque([start]); reverse={}
while q:
    state=q.popleft()
    for dest in next_states(state):
        reverse.setdefault(dest,set()).add(state)
        if dest not in seen:seen.add(dest);q.append(dest)
assert ('final',3) in seen
assert not any(name=='final' and held!=3 for name,held in seen)
can_win={('final',3)};q=deque(can_win)
while q:
    state=q.popleft()
    for predecessor in reverse.get(state,[]):
        if predecessor not in can_win:can_win.add(predecessor);q.append(predecessor)
assert seen<=can_win, ('Estados sem caminho para vitória',seen-can_win)
left_key_cases=0
for name,room in rooms.items():
    reward=bits.get(room['rewardKey'],0)
    if not reward: continue
    for state in seen:
        if state[0]!=name or state[1]&reward:continue
        for dest in next_states(state):
            if dest[0]==name:continue
            visited={dest};q=deque(visited)
            while q:
                here=q.popleft()
                for there in next_states(here):
                    if there[1]==state[1] and there not in visited:visited.add(there);q.append(there)
            assert state in visited, ('Chave esquecida sem retorno',state,dest)
            left_key_cases+=1
# Verifica a conectividade na resolução e margens do campo de navegação.
# Essa análise é geométrica; não simula colisões físicas ou comportamento no Unity.
nav={}
for name,room in rooms.items():
    clearance=.55 if room['bossArena'] else .4
    def pos(c):return(-7+(c[0]+.5)*.5,-4+(c[1]+.5)*.5)
    def free(p,r):
        return all(not(abs(p[0]-o['position']['x'])<o['size']['x']/2+r and abs(p[1]-o['position']['y'])<o['size']['y']/2+r) for o in room['obstacles'])
    cells={(x,y) for x in range(28) for y in range(16) if free(pos((x,y)),clearance)}
    first=next(iter(cells));visited={first};q=deque(visited)
    while q:
        c=q.popleft()
        for dx in [-1,0,1]:
            for dy in [-1,0,1]:
                n=(c[0]+dx,c[1]+dy)
                if n not in cells or n in visited:continue
                if dx and dy and ((n[0],c[1]) not in cells or (c[0],n[1]) not in cells):continue
                visited.add(n);q.append(n)
    assert cells==visited, ('Campo de navegação desconectado',name,len(cells-visited))
    # Ocupa artificialmente os cinco pontos do algoritmo antigo. Ainda deve sobrar
    # um local novo seguro, conectado e com margem para o pacote (raio 0,5).
    old_points=[(-5,-2.5),(5,2.5),(5,-2.5),(-5,2.5),(0,0)]
    candidates=[]
    for cell in cells:
        p=pos(cell)
        if abs(p[0])>6.5 or abs(p[1])>3.5 or not free(p,.5):continue
        if any((p[0]-b[0])**2+(p[1]-b[1])**2<1 for b in old_points):continue
        candidates.append(p)
    assert candidates, ('Sem reposição fora dos cinco pontos antigos',name)
    nav[name]={'connected_cells':len(cells),'alternative_supply_positions':len(candidates)}
report={'reachable_room_key_states':len(seen),'all_reachable_states_can_reach_victory':True,'leave_and_recover_key_cases':left_key_cases,'navigation':nav,'unity_editor_executed':False}

print(json.dumps(report,ensure_ascii=False,indent=2))
