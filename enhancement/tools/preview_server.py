"""Read-only local overlay preview. Source files are never modified."""
from pathlib import Path
from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler
from urllib.parse import unquote, urlsplit
import json
import argparse

ROOT=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser()
parser.add_argument('--source',type=Path,default=ROOT.parent/'RememberMeAct1')
parser.add_argument('--port',type=int,default=8766)
args=parser.parse_args()
SOURCE=args.source.resolve()

class Handler(SimpleHTTPRequestHandler):
    def translate_path(self,path):
        relative=unquote(urlsplit(path).path).lstrip('/') or 'index.html'
        parts=Path(relative).parts
        if '..' in parts: return str(ROOT/'not-found')
        original=False
        if parts[0]=='original':
            original=True;relative='/'.join(parts[1:]) or 'index.html'
        if not original:
            if relative.startswith('data/'):
                proposed=ROOT/'patch_data'/Path(relative).name
                if proposed.is_file(): return str(proposed)
            proposed=ROOT/'assets'/relative
            if proposed.is_file(): return str(proposed)
        return str(SOURCE/relative)
    def do_GET(self):
        if urlsplit(self.path).path=='/js/plugins.js':
            text=(SOURCE/'js/plugins.js').read_text()
            plugins=json.loads(text[text.index('['):text.rindex(']')+1])
            for p in plugins:
                if p['name'] in ['MadeWithMv','RM_Act1_Polish']:p['status']=False
            plugins.append(dict(name='RM_Act1_Experience',status=True,description='Act I interaction and menu polish',parameters={}))
            data=('var $plugins = '+json.dumps(plugins)+';').encode()
            self.send_response(200);self.send_header('Content-Type','application/javascript');self.send_header('Content-Length',str(len(data)));self.end_headers();self.wfile.write(data)
        else: super().do_GET()
    def log_message(self,format,*args):
        if len(args)>1 and str(args[1]) not in ['200','304']:super().log_message(format,*args)

print('Overlay preview: http://127.0.0.1:%d/ (source is read-only)'%args.port,flush=True)
ThreadingHTTPServer(('127.0.0.1',args.port),Handler).serve_forever()
