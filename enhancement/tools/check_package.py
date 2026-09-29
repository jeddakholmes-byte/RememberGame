"""Validate the delivery, gameplay references, script invariants and source hashes."""
from pathlib import Path
from collections import deque
import hashlib,json,re,struct

ROOT=Path(__file__).resolve().parents[1];SRC=ROOT.parent/'RememberMeAct1'
checks=[];errors=[];refs=set()
def verify(name,ok):
    (checks if ok else errors).append(name)
def asset(folder,name,ext='png'):
    if not name:return
    rel=f'{folder}/{name}.{ext}';refs.add(rel)
    verify('Asset '+rel,(ROOT/'assets'/rel).is_file() or (SRC/rel).is_file())
def load(name):
    p=ROOT/'patch_data'/name
    return json.loads((p if p.exists() else SRC/'data'/name).read_text())
maps=[load('Map001.json'),load('Map002.json')]
for f in (SRC/'data').glob('*.json'):
    load(f.name)
checks.append('All project database JSON files parse')
for mid,m in enumerate(maps,1):
    verify(f'Map {mid} tile dimensions',len(m['data'])==m['width']*m['height']*6)
    asset('img/parallaxes',m['parallaxName'])
    for event in m['events']:
        if not event:continue
        verify(f'Map {mid} event {event["id"]} has matching index',m['events'][event['id']]==event)
        for pi,page in enumerate(event['pages'],1):
            tag=f'Map {mid} event {event["id"]} page {pi}'
            asset('img/characters',page['image']['characterName'])
            verify(tag+' terminates',page['list'][-1]['code']==0)
            for c in page['list']:
                code,p=c['code'],c['parameters']
                if code==101:
                    asset('img/faces',p[0]);verify(tag+' valid face index',0<=p[1]<8)
                if code==231:asset('img/pictures',p[1])
                if code in [241,245,250]:
                    for ext in ['ogg','m4a']:asset('audio/'+{241:'bgm',245:'bgs',250:'se'}[code],p[0]['name'],ext)
                if code==205:
                    for step in p[1]['list']:
                        if step['code']==41:asset('img/characters',step['parameters'][0])
                if code==401:verify(tag+' English text',not re.search('[\u4e00-\u9fff]',p[0]))
                if code==201:
                    target=maps[p[1]-1];tile=target['data'][p[3]*target['width']+p[2]]
                    verify(tag+' transfer destination passable',tile==1536)
def texts(m):return [c['parameters'][0] for c in m['events'][1]['pages'][0]['list'] if c['code']==401]
old=json.loads((SRC/'data/Map002.json').read_text())
verify('All original flashback dialogue retained in order',texts(maps[1])==texts(old))
verify('No heartbeat or shake in revised flashback',all(c['code']!=225 and not(c['code']==250 and c['parameters'][0]['name']=='RM_Heartbeat') for c in maps[1]['events'][1]['pages'][0]['list']))
verify('Flashback uses backward movement code 13',any(c['code']==205 and c['parameters'][0]==-1 and any(s['code']==13 for s in c['parameters'][1]['list']) for c in maps[1]['events'][1]['pages'][0]['list']))
for f in (ROOT/'assets/img').rglob('*.png'):
    b=f.read_bytes();w,h=struct.unpack('>II',b[16:24]);expected=(576,288) if f.parent.name=='faces' else (144,192)
    verify('PNG grid '+f.name,(w,h)==expected)

# Reachability from start to at least one adjacent interaction tile for every map 1 target.
m=maps[0];w=m['width'];h=m['height']
blocked={(e['x'],e['y']) for e in m['events'] if e and e['pages'][0]['priorityType']==1}
todo=deque([(12,12)]);seen=set(todo)
while todo:
    x,y=todo.popleft()
    for a,b in [(x-1,y),(x+1,y),(x,y-1),(x,y+1)]:
        if 0<=a<w and 0<=b<h and (a,b) not in seen and (a,b) not in blocked and m['data'][b*w+a]==1536:
            seen.add((a,b));todo.append((a,b))
for e in m['events']:
    if e and '<RMHint:' in e.get('note',''):
        x,y=e['x'],e['y'];verify('Reachable interaction '+e['name'],any(p in seen for p in [(x-1,y),(x+1,y),(x,y-1),(x,y+1)]))
system=load('System.json')
verify('All system terms are English',not re.search('[\u4e00-\u9fff]',json.dumps(system['terms'],ensure_ascii=False)))
snapshot=json.loads((ROOT/'review/source_sha256.json').read_text())
drift=[name for name,digest in snapshot.items() if not (SRC/name).exists() or hashlib.sha256((SRC/name).read_bytes()).hexdigest()!=digest]
verify('Every original file remains byte-for-byte unchanged',not drift)
report=dict(status='passed' if not errors else 'failed',checks_passed=len(checks),unique_assets_checked=len(refs),original_files_checked=len(snapshot),errors=errors,source_drift=drift,checks=checks)
(ROOT/'review/static_checks.json').write_text(json.dumps(report,ensure_ascii=False,indent=2))
print(json.dumps({k:v for k,v in report.items() if k!='checks'},ensure_ascii=False))
raise SystemExit(bool(errors))
