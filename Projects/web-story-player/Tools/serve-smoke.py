"""Local-only development host for the built player and one existing story."""
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import urlsplit, unquote
import json
import re

project = Path(__file__).resolve().parents[1]
reader = project.parents[1] / "Website/public/player"
story = project.parent / "novels-chernaya-melnitsa/Build/LocalContent"
smoke_version = "smoke-boundary-20260912"
prefix = f"/content/stories/chernaya-melnitsa/{smoke_version}/webgl/"
launch = dict(storyId="chernaya-melnitsa", storyVersion=smoke_version,
              manifestUrl=prefix + "release.json", profile="media-free-v1", locale="ru")

class Handler(SimpleHTTPRequestHandler):
    def translate_path(self, path):
        clean = unquote(urlsplit(path).path)
        if clean.startswith('/player/builds/local/'):
            relative = clean[len('/player/builds/local/'):]
            root = project / 'Build/WebGL'
        elif clean.startswith('/player/'):
            relative = clean[len('/player/'):]
            root = reader
        elif clean.startswith(prefix):
            relative = clean[len(prefix):]
            root = story if relative.startswith("Files/") else story / "Remote/WebGL"
        else:
            relative = clean.lstrip("/")
            root = project / "Build/WebGL"
        resolved = (root / relative).resolve()
        if not resolved.is_relative_to(root.resolve()):
            return str(project / "Build/missing")
        return str(resolved)

    def do_GET(self):
        path = urlsplit(self.path).path
        if path == '/player/config.json':
            template = (project / 'Build/WebGL/index.html').read_text()
            assets = {}
            for key in ('loaderUrl', 'dataUrl', 'frameworkUrl', 'codeUrl'):
                match = re.search(r'\b' + key + r'\s*[:=]\s*buildUrl\s*\+\s*"([^"]+)"', template)
                if not match:
                    self.send_error(500, 'Missing build asset reference')
                    return
                assets[key] = '/player/builds/local/Build' + match.group(1)
            assets['streamingAssetsUrl'] = '/player/builds/local/StreamingAssets'
            return self.respond(json.dumps(dict(schema=1, player=assets, stories={
                'chernaya-melnitsa': dict(version=smoke_version, title='Чёрная мельница', manifestUrl=prefix+'release.json')
            })).encode(), 'application/json')
        if path == '/player/index.html':
            html = (reader / 'index.html').read_text()
            # Fault controls exist only on this localhost fixture, never in the website.
            html = html.replace('</body>', '''<aside style="position:fixed;bottom:0;left:0;z-index:10;background:#222;padding:4px;max-width:250px;font-size:11px">
            <button id="block-writes">Block save writes</button><button id="allow-writes">Allow save writes</button>
            <details><summary>Local smoke events</summary><pre id="smoke-events" style="max-height:130px;overflow:auto;white-space:pre-wrap"></pre></details></aside>
            <script>
            let blocked=false;
            const record=text=>document.getElementById('smoke-events').textContent+='\\n'+text;
            document.getElementById('block-writes').onclick=()=>{blocked=true;record('writes blocked');};
            document.getElementById('allow-writes').onclick=()=>{blocked=false;record('writes allowed');};
            const original=IDBDatabase.prototype.transaction;
            IDBDatabase.prototype.transaction=function(...args){
              if(blocked && this.name==='somegame-story-saves' && args[1]==='readwrite')
                throw new DOMException('Local smoke simulated storage failure','QuotaExceededError');
              return original.apply(this,args);
            };
            window.addEventListener('somegame:web-player-event',e=>record(JSON.stringify(e.detail)));
            </script></body>''')
            return self.respond(html.encode(), 'text/html; charset=utf-8')
        if urlsplit(self.path).path != "/":
            return super().do_GET()
        html = (project / "Build/WebGL/index.html").read_text()
        html = html.replace("</head>", "<style>#unity-container{width:360px!important}#unity-canvas{width:360px!important;height:640px!important}#events{max-height:260px;max-width:280px;overflow:auto;white-space:pre-wrap;font-size:10px}</style></head>")
        html = html.replace("</head>", "<script>for(const kind of ['error','warn']){const original=console[kind];console[kind]=(...args)=>{original.apply(console,args);const out=document.getElementById('events');if(out)out.textContent+='\\n'+kind+': '+args.map(String).join(' ').slice(0,2500);};}</script></head>")
        html = html.replace("<body>", '<body><div style="position:fixed;z-index:99;background:white"><button id="duplicate">Duplicate launch</button><button id="missing">Missing release</button><button id="retry">Retry valid story</button><button id="invalid">Reject external URL</button><pre id="events">Events:</pre></div><script>window.addEventListener("somegame:web-player-event", e => document.getElementById("events").textContent += "\\n" + JSON.stringify(e.detail));</script>')
        html = html.replace("}).then((unityInstance) => {",
                            "}).then((unityInstance) => { unityInstance.SendMessage('WebStoryPlayer', 'Launch', "
                            + json.dumps(json.dumps(launch)) + ");"
                            + "const config=" + json.dumps(launch) + ";"
                            + "const launch=c=>unityInstance.SendMessage('WebStoryPlayer','Launch',JSON.stringify(c));"
                            + "document.getElementById('duplicate').onclick=()=>launch(config);"
                            + "document.getElementById('retry').onclick=()=>launch(config);"
                            + "document.getElementById('missing').onclick=()=>launch({...config,storyVersion:'missing',manifestUrl:config.manifestUrl.replace('/'+config.storyVersion+'/','/missing/')});"
                            + "document.getElementById('invalid').onclick=()=>launch({...config,manifestUrl:'https://example.com/release.json'});")
        # Local fault injection: the real transaction commits, but Unity receives
        # its acknowledgement later. No production bridge/storage code is changed.
        html = html.replace("<pre id=\"events\">", '<button id="delayed-stop">Switch during next save</button><button id="fail-save">Block save writes</button><button id="allow-save">Allow save writes</button><pre id="events">')
        html = html.replace('<pre id="events">', '<button id="next-episode">Next episode</button><button id="return-site">Return to site</button><pre id="events">')
        html = html.replace('position:fixed;z-index:99;background:white', 'position:fixed;z-index:99;background:white;max-width:420px')
        html = html.replace("const launch=c=>", """
            let delayNextWrite=false;
            document.getElementById('next-episode').onclick=()=>unityInstance.SendMessage('WebStoryPlayer','NextEpisode','');
            document.getElementById('return-site').onclick=()=>unityInstance.SendMessage('WebStoryPlayer','ReturnToSite','');
            let blockWrites=false;
            const record=message=>document.getElementById('events').textContent+='\\n'+message;
            document.getElementById('fail-save').onclick=()=>{blockWrites=true;record('smoke: writes blocked');};
            document.getElementById('allow-save').onclick=()=>{blockWrites=false;record('smoke: writes allowed');};
            document.getElementById('delayed-stop').onclick=()=>{delayNextWrite=true;record('smoke: armed delayed save acknowledgement');};
            const transaction=IDBDatabase.prototype.transaction;
            IDBDatabase.prototype.transaction=function(...args){
                if(blockWrites && this.name==='somegame-story-saves' && args[1]==='readwrite')
                    throw new DOMException('Local smoke simulated storage failure','QuotaExceededError');
                const tx=transaction.apply(this,args);
                if(delayNextWrite && this.name==='somegame-story-saves' && args[1]==='readwrite'){
                    delayNextWrite=false;
                    Object.defineProperty(tx,'oncomplete',{set(callback){
                        tx.addEventListener('complete',event=>{
                            record('smoke: acknowledgement held; requesting replacement');
                            launch({...config,storyVersion:'missing',manifestUrl:config.manifestUrl.replace('/'+config.storyVersion+'/','/missing/')});
                            launch(config);
                            setTimeout(()=>{record('smoke: releasing acknowledgement');callback.call(tx,event);},1500);
                        });
                    }});
                }
                return tx;
            };
            const launch=c=>""")
        body = html.encode()
        self.respond(body, 'text/html; charset=utf-8')

    def respond(self, body, content_type):
        self.send_response(200)
        self.send_header("Content-Type", content_type)
        self.send_header('Cache-Control', 'no-store')
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

ThreadingHTTPServer(("127.0.0.1", 8767), Handler).serve_forever()
