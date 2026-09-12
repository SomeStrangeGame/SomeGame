import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const siteRelease=process.argv[2];
const playerRelease=process.argv[3];
if(![siteRelease,playerRelease].every(x=>typeof x==='string'&&/^[0-9]{8}-[a-zA-Z0-9-]+$/.test(x)))throw Error('Provide site and reader release IDs');
const output=path.join(root,'dist','hosting',siteRelease);
if(fs.existsSync(output))throw Error('Output already exists; use a fresh release ID');
function write(rel,text){const p=path.join(output,rel);fs.mkdirSync(path.dirname(p),{recursive:true});fs.writeFileSync(p,text);}
function copy(src,rel){write(rel,fs.readFileSync(path.join(root,src)));}
const html=fs.readFileSync(path.join(root,'hosting/index.html'),'utf8').replace(/\/site\/releases\/[^/]+\//g,`/site/releases/${siteRelease}/`);
write('index.html',html);
for(const file of ['app.js','styles.css'])copy('hosting/'+file,`site/releases/${siteRelease}/${file}`);
const readerHtml=fs.readFileSync(path.join(root,'public/player/index.html'),'utf8').replaceAll('/player/reader.',`/player/releases/${playerRelease}/reader.`);
write('player/index.html',readerHtml);
for(const file of ['reader.js','reader.css'])copy('public/player/'+file,`player/releases/${playerRelease}/${file}`);
const config=JSON.parse(fs.readFileSync(path.join(root,'public/player/config.json'),'utf8'));
if(config.schema!==1||!config.player||!config.stories['chernaya-melnitsa'])throw Error('Reader not enabled');
copy('public/player/config.json','player/config.json');
console.log(output);
