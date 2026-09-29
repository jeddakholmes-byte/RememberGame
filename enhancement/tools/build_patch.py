"""Build proposed data in this delivery folder. Never write to the source project."""
from pathlib import Path
import copy
import json

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT.parent / 'RememberMeAct1'
OUT = ROOT / 'patch_data'

def read(name):
    return json.loads((SOURCE / 'data' / (name + '.json')).read_text(encoding='utf-8-sig'))

def cmd(code, *params, indent=0):
    return dict(code=code, indent=indent, parameters=list(params))

def msg(face, index, speaker, *lines):
    text = ([speaker] if speaker else []) + list(lines)
    assert len(text) <= 4
    return [cmd(101, face, index, 0, 2)] + [cmd(401, line) for line in text]

def route(target, *steps):
    r = dict(list=[dict(code=c, parameters=p) for c,p in steps] + [dict(code=0, parameters=[])], repeat=False, skippable=False, wait=True)
    return [cmd(205, target, r)] + [cmd(505, copy.deepcopy(s)) for s in r['list'][:-1]]

def sound(code, name, volume):
    return cmd(code, dict(name=name, volume=volume, pitch=100, pan=0))

def one_time(page, repeat, switch=None):
    """Preserve page conditions; branch on a shared switch or self switch A."""
    original = copy.deepcopy(page['list'][:-1])
    condition = cmd(111, 0, switch, 1) if switch else cmd(111, 2, 'A', 1)
    for c in original: c['indent'] += 1
    set_seen = cmd(121, switch, switch, 0, indent=1) if switch else cmd(123, 'A', 0, indent=1)
    repeated = copy.deepcopy(repeat)
    for c in repeated: c['indent'] += 1
    page['list'] = [condition] + original + [set_seen,cmd(411)] + repeated + [cmd(412),cmd(0)]

m1, m2, system, actors = read('Map001'), read('Map002'), read('System'), read('Actors')
events = m1['events']
opening = events[1]['pages'][0]['list']
for c in opening:
    if c['code']==135: c['parameters']=[0]
    if c['code']==401:
        c['parameters'][0] = {'\\C[7]':'\\C[7]CONTROLS\\C[0]', 'TIPS:':'Arrow keys: Move', '↑↓←→: Move':'Enter / Space: Look or talk', 'ENTER / SPACE: Interact with items':'Esc: Pause / save'}.get(c['parameters'][0],c['parameters'][0])
    if c['code']==241: c['parameters'][0]['volume']=32
opening.insert(-1,cmd(135,1))

G='\\C[4]GRACE\\C[0]'; D='\\C[1]DAD\\C[0]'; M='\\C[6]MOM\\C[0]'; GM='\\C[5]GRANDMA\\C[0]'
one_time(events[2]['pages'][0],msg('RM_Face_Mom',1,M,"I'll be right here."))
events[2]['pages'][1]['list'] = [cmd(111,0,2,1),cmd(111,0,7,1,indent=1)] + [dict(c,indent=2) for c in msg('RM_Face_Mom',1,M,"Grandma's stone is near Dad.","I'll be right here.")] + [cmd(121,7,7,0,indent=2),cmd(412,indent=1),cmd(412),cmd(121,6,6,1),cmd(0)]
for eid in [5,18]:
    one_time(events[eid]['pages'][0],msg('RM_Face_Grace',7,G,'Flowers made her sneeze.'),switch=8)
for e in events:
    if not e: continue
    for pg in e['pages']:
        for c in pg['list']:
            if c['code']==401:
                c['parameters'][0] = {'It seems to be abondoned a long time ago.':'It looks like no one has been here','The name on the grave is barely legible.':'I can barely read the name.'}.get(c['parameters'][0],c['parameters'][0])
        for i,c in enumerate(pg['list']):
            if c['code']==401 and c['parameters']==['It looks like no one has been here']:
                pg['list'].insert(i+1,cmd(401,'for a long time.',indent=c['indent'])); break

events[7]['pages'][0]['image'].update(characterName='$RM_UncleJames',characterIndex=0)
events[7]['pages'][0]['list'][0]['parameters'][:2]=['RM_Face_UncleJames',7]
events[8]['name']='Aunt'
events[8]['pages'][0]['image'].update(characterName='$RM_Aunt',characterIndex=0)
events[8]['pages'][0]['list'][0]['parameters'][:2]=['RM_Face_Aunt',2]

grandpa=copy.deepcopy(events[8]);grandpa.update(id=20,name='Grandpa',x=16,y=7)
grandpa['pages'][0]['image'].update(characterName='$RM_Grandpa',characterIndex=0,direction=4)
grandpa['pages'][0]['list']=msg('RM_Face_Grandpa',2,'','Grandpa is standing very still.')+[cmd(0)]
assert len(events)==20
events.append(grandpa)
hints={2:'Talk to Mom',3:'Talk to Dad',4:"Look at Grandma\'s stone",5:'Look at the flowers',6:'Look at the old grave',7:'Talk to Uncle James',8:'Look at Aunt',9:'Look at the bench',13:"Look at Grandma\'s stone",15:'Look at the bench',16:'Look at the old grave',17:'Look at the old grave',18:'Look at the flowers',19:'Look at the old grave',20:'Look at Grandpa'}
for eid,label in hints.items(): events[eid]['note']=(events[eid].get('note','')+'\n<RMHint: '+label+'>').strip()

# Fix Grace's cemetery mark before the flashback; otherwise she returns elsewhere.
dad=events[3]['pages'][1]['list']
fade=next(i for i,c in enumerate(dad) if c['code']==221)
dad[fade:fade]=[cmd(135,0)]
for c in dad:
    if c['code']==201: c['parameters']=[0,2,12,13,6,2]

# A single cut back to the same conversation, with Mom approaching visibly.
ending=events[10]['pages'][1]['list']
for c in ending:
    if c['code']==241 and c['parameters'][0]['name']=='RM_Act1_Cemetery': c['parameters'][0]['volume']=30
    if c['code']==241 and c['parameters'][0]['name']=='RM_Box': c['parameters'][0]['volume']=32
cut_start=next(i for i,c in enumerate(ending) if c['code']==101 and i>25 and c['parameters'][0]=='RM_Face_Mom')
cut_end=next(i for i,c in enumerate(ending) if c['code']==241 and c['parameters'][0]['name']=='RM_Box')
# Keep Mom's existing line; place her beside the group by a traversable route.
momline=copy.deepcopy(ending[cut_start:cut_start+3])
approach=[cmd(203,2,0,13,12,8)]+route(2,(4,[]),(4,[]),(4,[]),(4,[]))
ending[cut_start:cut_end]=approach+momline
for i,c in enumerate(ending):
    if c['code']==401 and c['parameters']==['ACT I — END']: c['parameters']=['ACT I — END']
# Make the return rain audible before restoring the cemetery music.
music=next(i for i,c in enumerate(ending) if c['code']==241)
bgm=ending.pop(music)
first_fade=next(i for i,c in enumerate(ending) if c['code']==222)
ending[first_fade+1:first_fade+1]=[cmd(230,24),bgm]
ending.insert(0,cmd(135,0))

# Original flashback dialogue is retained verbatim and in the same order.
m2['events'][2].update(x=12,y=5)
m2['events'][3].update(x=11,y=6)
m2['data'][4*m2['width']+12]=1536
f=[cmd(223,[-16,-12,-6,48],1,True),cmd(355,'$gameMap.setDisplayPos(3.5, 3);'),sound(245,'RM_Room',16),sound(241,'RM_Flashback',22),cmd(222),cmd(230,24)]
f+=msg('RM_Face_Grandma',2,GM,'I need to go home.')
f+=msg('RM_Face_Dad',5,D,'Mom.','You are home.')
f+=route(2,(4,[]))+[sound(250,'RM_DoorHandle',30),cmd(230,20)]
f+=msg('RM_Face_Grandma',5,GM,'No.')
f+=route(3,(4,[]),(3,[]),(19,[]))+msg('RM_Face_Dad',1,D,'We talked about this.')
f+=[sound(250,'RM_DoorHandle',34),cmd(230,18)]
f+=msg('RM_Face_Grandma',2,GM,'Grace?')+route(-1,(19,[]))
f+=msg('RM_Face_Dad',1,D,'Mom, please—')
f+=[sound(250,'RM_DoorPalm',35),cmd(230,12)]
f+=msg('RM_Face_Grandma',4,GM,'Let me go home!')+[cmd(230,24)]
f+=msg('RM_Face_Dad',6,D,"I CAN'T DO THIS AGAIN.")
f+=[cmd(242,1),cmd(246,1),cmd(251),cmd(230,60)]
f+=route(-1,(13,[]))+[sound(250,'RM_Step1',18),cmd(230,36),cmd(221),cmd(121,3,3,1),cmd(121,4,4,0),cmd(201,0,1,13,7,6,2),cmd(0)]
m2['events'][1]['pages'][0]['list']=f

# Restore Grace to the same mark during the final cemetery dialogue before entering memory.
# A short fade covers the repositioning so it cannot collide with player approach direction.
dad=events[3]['pages'][1]['list']
# No extra cemetery teleport: the return puts her beside Dad, matching the script.

system['locale']='en_US';system['titleBgm']['volume']=25
system['switches'][1:9]=['OpeningComplete','VisitedGrave','FlashbackActive','FlashbackComplete','Act1Complete','MomCall','MomHintDelivered','FlowersSeen']
system['variables'][1]='OtherGravesInspected'
system['optFollowers']=False
system['terms']['basic']=['Level','Lv','HP','HP','MP','MP','TP','TP','Experience','EXP']
system['terms']['params']=['Max HP','Max MP','Attack','Defense','M.Attack','M.Defense','Agility','Luck','Hit Rate','Evasion Rate']
system['terms']['commands']=['Fight','Escape','Attack','Guard','Items','Skills','Equipment','Status','Formation','Save','Return to Title','Options','Weapons','Armor','Key Items','Equip','Optimize','Clear','New Game','Continue',None,'Return to Title','Cancel',None,'Buy','Sell']
translations={'actionFailure':'There was no effect on %1!','actorDamage':'%1 took %2 damage!','actorDrain':'%1 was drained of %2 %3!','actorGain':'%1 gained %2 %3!','actorLoss':'%1 lost %2 %3!','actorNoDamage':'%1 took no damage!','actorNoHit':'Miss! %1 took no damage!','actorRecovery':'%1 recovered %2 %3!','alwaysDash':'Always Dash','bgmVolume':'Music Volume','bgsVolume':'Ambience Volume','buffAdd':"%1\'s %2 increased!",'buffRemove':"%1\'s %2 returned to normal!",'commandRemember':'Remember Commands','counterAttack':'%1 counterattacked!','criticalToActor':'A painful blow!','criticalToEnemy':'An excellent hit!','debuffAdd':"%1\'s %2 decreased!",'defeat':'%1 was defeated.','emerge':'%1 emerged!','enemyDamage':'%1 took %2 damage!','enemyDrain':'%1 was drained of %2 %3!','enemyGain':'%1 gained %2 %3!','enemyLoss':'%1 lost %2 %3!','enemyNoDamage':'%1 took no damage!','enemyNoHit':'Miss! %1 took no damage!','enemyRecovery':'%1 recovered %2 %3!','escapeFailure':'Could not escape!','escapeStart':'%1 started to escape!','evasion':'%1 evaded the attack!','expNext':'To Next %1','expTotal':'Current %1','file':'File','levelUp':'%1 is now %2 %3!','loadMessage':'Load which file?','magicEvasion':'%1 nullified the magic!','magicReflection':'%1 reflected the magic!','meVolume':'Music Effect Volume','obtainExp':'%1 %2 received!','obtainGold':'%1\\G received!','obtainItem':'%1 received!','obtainSkill':'%1 learned!','partyName':"%1\'s Party",'possession':'Possession','preemptive':'%1 got the upper hand!','saveMessage':'Save to which file?','seVolume':'Sound Effect Volume','substitute':'%1 protected %2!','surprise':'%1 was surprised!','useItem':'%1 uses %2!','victory':'%1 was victorious!'}
assert set(system['terms']['messages']) == set(translations)
system['terms']['messages']=translations
actors[1]['faceName']='RM_Face_Grace';actors[1]['faceIndex']=3
for name,data in [('Map001',m1),('Map002',m2),('System',system),('Actors',actors)]:
    (OUT/(name+'.json')).write_text(json.dumps(data,ensure_ascii=False,separators=(',',':')),encoding='utf-8')
print('Built four data copies. Source project unchanged.')
