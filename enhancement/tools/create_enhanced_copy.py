"""Create a NEW playable project; refuse source drift and never overwrite a folder."""
from pathlib import Path
import argparse, hashlib, json, shutil, sys

ROOT=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('source',type=Path,help='Original RememberMeAct1 project directory')
parser.add_argument('destination',type=Path,help='New directory that must not exist')
args=parser.parse_args()
source=args.source.resolve(); destination=args.destination.resolve()
if not (source/'Game.rpgproject').is_file(): sys.exit('Source must contain Game.rpgproject.')
if destination.exists(): sys.exit('Destination already exists. Choose a NEW directory; nothing was changed.')
if source in destination.parents or destination in source.parents:
    sys.exit('Source and destination must be separate sibling directories.')
baseline=json.loads((ROOT/'review/source_sha256.json').read_text())
critical=['data/Map001.json','data/Map002.json','data/System.json','data/Actors.json','data/Tilesets.json','js/plugins.js']
critical += [name for name in baseline if name.startswith(('img/faces/RM_','img/characters/RM_','img/parallaxes/!RM_','img/pictures/RM_'))]
drift=[name for name in critical if not (source/name).is_file() or hashlib.sha256((source/name).read_bytes()).hexdigest()!=baseline[name]]
if drift:
    sys.exit('Source differs from the reviewed version. Use the manual guide. Changed files:\n'+'\n'.join(drift))
shutil.copytree(source,destination,ignore=shutil.ignore_patterns('node_modules','dist','build','coverage','save','.git','*.lock','package-lock.json','yarn.lock','pnpm-lock.yaml','__pycache__','.DS_Store'))
for file in (ROOT/'patch_data').glob('*.json'): shutil.copy2(file,destination/'data'/file.name)
for file in (ROOT/'assets').rglob('*'):
    if file.is_file():
        target=destination/file.relative_to(ROOT/'assets');target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(file,target)
p=destination/'js/plugins.js';text=p.read_text(encoding='utf-8-sig')
plugins=json.loads(text[text.index('['):text.rindex(']')+1])
plugins=[p for p in plugins if p['name'] not in ['RM_Act1_Experience','RM_EnglishTypography']]
for plugin in plugins:
    if plugin['name'] in ['MadeWithMv','RM_Act1_Polish']:plugin['status']=False
plugins.append(dict(name='RM_Act1_Experience',status=True,description='Act I interaction hints, quiet footsteps and story menu',parameters={}))
plugins.append(dict(name='RM_EnglishTypography',status=True,description='English typography',parameters={'FontFace':'Arial, Helvetica, sans-serif','FontSize':'26','OutlineWidth':'2'}))
p.write_text('// Generated for the NEW enhanced copy.\nvar $plugins =\n'+json.dumps(plugins,ensure_ascii=False,indent=2)+';\n',encoding='utf-8')
print('Created: '+str(destination))
print('Open Game.rpgproject in this NEW directory and choose New Game. The source was not modified.')
