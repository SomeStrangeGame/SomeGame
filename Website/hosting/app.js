const contentRoot='/content';
const manifestUrl=`${contentRoot}/kostroma-dev.json`;
const accents=['#9aa995','#86b7c6','#79bed1','#d0a76a','#b49ac9'];
const stage=document.querySelector('#stories');
const nav=document.querySelector('#navigation');
const dots=document.querySelector('.dots');
const counter=document.querySelector('.counter');
const site=document.querySelector('#site');
const accessGate=document.querySelector('#access-gate');
const accessForm=document.querySelector('#access-form');
const accessPassword=document.querySelector('#access-password');
const accessError=document.querySelector('#access-error');
let stories=[];
let active=0;
let pointerStart=null;
let previewModal=null;
let previewCleanup=null;
let webVersions={};

async function loadWebPlayer(){
  try{
    const response=await fetch('/player/config.json',{cache:'no-store'});
    if(!response.ok)return;
    const config=await response.json();
    if(config.schema!==1||!config.player||!config.stories)return;
    for(const [id,story] of Object.entries(config.stories)){
      if(!/^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(id)||!story||typeof story.version!=='string')continue;
      if(!/^[a-zA-Z0-9._-]+$/.test(story.version)||['.','..'].includes(story.version))continue;
      if(story.manifestUrl!==`/content/stories/${id}/${story.version}/webgl/release.json`)continue;
      webVersions[id]=story.version;
    }
  }catch{/* Optional reader availability must not block the catalog. */}
}

function readerLink(story){
  const version=webVersions[story.id];
  if(!version)return null;
  const link=element('a','preview-trigger web-reader-trigger','Читать в браузере →');
  link.href=`/player/index.html?story=${encodeURIComponent(story.id)}&version=${encodeURIComponent(version)}`;
  return link;
}

if(sessionStorage.getItem('kostroma-preview-access')==='granted')accessGate.hidden=true;
accessForm.addEventListener('submit',event=>{
  event.preventDefault();
  if(accessPassword.value.trim().toLocaleLowerCase('ru-RU')!=='огурец'){
    accessPassword.setAttribute('aria-invalid','true');accessError.hidden=false;accessPassword.focus();return;
  }
  sessionStorage.setItem('kostroma-preview-access','granted');accessGate.hidden=true;accessPassword.value='';
});
accessPassword.addEventListener('input',()=>{accessPassword.setAttribute('aria-invalid','false');accessError.hidden=true});

const element=(tag,className,text)=>{const node=document.createElement(tag);if(className)node.className=className;if(text)node.textContent=text;return node};

function renderActive(){
  [...stage.children].forEach((slide,index)=>{
    const selected=index===active;
    slide.classList.toggle('is-active',selected);
    slide.setAttribute('aria-hidden',String(!selected));
  });
  [...dots.children].forEach((dot,index)=>{dot.classList.toggle('active',index===active);if(index===active)dot.setAttribute('aria-current','true');else dot.removeAttribute('aria-current')});
  counter.textContent=`${active+1} / ${stories.length}`;
}

function closePreview(){if(previewCleanup){previewCleanup();previewCleanup=null}if(previewModal){previewModal.remove();previewModal=null}}

async function measureVisibleImage(image){
  if(!image.complete)await new Promise(resolve=>image.addEventListener('load',resolve,{once:true}));
  try{await image.decode()}catch{}
  const naturalWidth=image.naturalWidth||1,naturalHeight=image.naturalHeight||1;
  const ratio=Math.min(1,512/Math.max(naturalWidth,naturalHeight));
  const width=Math.max(1,Math.round(naturalWidth*ratio)),height=Math.max(1,Math.round(naturalHeight*ratio));
  const canvas=document.createElement('canvas');canvas.width=width;canvas.height=height;
  const context=canvas.getContext('2d',{willReadFrequently:true});context.drawImage(image,0,0,width,height);
  const pixels=context.getImageData(0,0,width,height).data;
  let left=width,top=height,right=0,bottom=0,found=false;
  for(let y=0;y<height;y+=2)for(let x=0;x<width;x+=2){if(pixels[(y*width+x)*4+3]>12){found=true;left=Math.min(left,x);top=Math.min(top,y);right=Math.max(right,x);bottom=Math.max(bottom,y)}}
  if(!found)return{left:0,top:0,right:naturalWidth,bottom:naturalHeight,width:naturalWidth,height:naturalHeight};
  return{left:left/ratio,top:top/ratio,right:(right+2)/ratio,bottom:(bottom+2)/ratio,width:naturalWidth,height:naturalHeight};
}

function openPreview(story){
  if(!story.preview)return;
  closePreview();
  const backdrop=element('div','preview-backdrop');
  const modal=element('section','preview-modal');modal.setAttribute('role','dialog');modal.setAttribute('aria-modal','true');modal.setAttribute('aria-labelledby','preview-title');
  const header=element('header','preview-header');
  const heading=element('div');
  const meta=[story.preview.subtitle||'Начало истории',story.preview.estimatedReadingMinutes?`${story.preview.estimatedReadingMinutes} мин`:null].filter(Boolean).join(' · ');
  heading.append(element('p','preview-kicker',meta));
  const title=element('h2','',story.preview.title);title.id='preview-title';heading.append(title);
  const close=element('button','preview-close','×');close.setAttribute('aria-label','Закрыть превью');close.addEventListener('click',closePreview);
  header.append(heading,close);
  const body=element('div','preview-body');
  const scroll=element('div','preview-scroll');
  const stage=element('div','preview-stage');stage.dataset.story=story.id;stage.style.backgroundImage=`url("${story.cover}")`;
  const stageShade=element('span','preview-stage-shade');stageShade.setAttribute('aria-hidden','true');stage.append(stageShade);
  const characterImages=new Map();
  Object.entries(story.preview.characters||{}).forEach(([id,character])=>{const image=element('img','preview-character');image.src=character.image;image.alt='';image.setAttribute('aria-hidden','true');stage.append(image);characterImages.set(id,image)});
  let currentCharacter;
  let characterQueue=[];
  let characterPlaying=false;
  let characterStopped=false;
  let characterHoldTimer;
  const bounds=new WeakMap();
  const layoutCharacter=async image=>{
    let box=bounds.get(image);if(!box){box=await measureVisibleImage(image);bounds.set(image,box)}
    const portrait=matchMedia('(orientation: portrait) and (max-width: 700px)').matches;
    const visibleHeight=Math.max(1,box.bottom-box.top);
    const scale=stage.clientHeight*(portrait?1.14:.92)/visibleHeight;
    image.style.width=`${box.width*scale}px`;image.style.height=`${box.height*scale}px`;
    image.style.top=`${(portrait?stage.clientHeight*.015:Math.min(112,stage.clientHeight*.13))-box.top*scale}px`;
    image.style.right=`${-(box.width-box.right)*scale-stage.clientWidth*(portrait?.055:.015)}px`;
  };
  const enterCharacter=image=>{
    image.getAnimations().forEach(animation=>animation.cancel());image.classList.add('is-current');image.classList.remove('is-previous');
    const distance=stage.clientWidth+image.getBoundingClientRect().width*1.45;
    image.animate([{opacity:0,transform:`translateX(${distance}px) scale(.97)`,filter:'brightness(.58) saturate(.66)'},{opacity:1,transform:'translateX(0) scale(1)',filter:'brightness(.92) saturate(.88)'}],{duration:1450,easing:'cubic-bezier(.16,.72,.24,1)',fill:'both'});
  };
  const exitCharacter=image=>{
    image.getAnimations().forEach(animation=>animation.cancel());image.classList.remove('is-current');image.classList.add('is-previous');
    const distance=stage.clientWidth+image.getBoundingClientRect().width*1.45;
    const animation=image.animate([{opacity:1,transform:'translateX(0) scale(1)'},{opacity:0,transform:`translateX(${distance}px) scale(.97)`}],{duration:1100,easing:'cubic-bezier(.4,0,.55,1)',fill:'both'});
    animation.finished.catch(()=>{}).then(()=>image.classList.remove('is-previous'));
  };
  const playCharacterQueue=async()=>{
    if(characterPlaying||characterStopped)return;characterPlaying=true;
    while(characterQueue.length&&!characterStopped){
      const id=characterQueue.shift();if(!id||id===currentCharacter)continue;
      const previous=currentCharacter,nextImage=characterImages.get(id);if(!nextImage)continue;
      await Promise.all([layoutCharacter(nextImage),previous&&characterImages.has(previous)?layoutCharacter(characterImages.get(previous)):Promise.resolve()]);
      if(characterStopped)break;
      currentCharacter=id;if(previous&&characterImages.has(previous))exitCharacter(characterImages.get(previous));enterCharacter(nextImage);
      await new Promise(resolve=>{characterHoldTimer=setTimeout(resolve,1550)});
    }
    characterPlaying=false;
  };
  const showCharacter=id=>{
    if(!id||characterQueue.at(-1)===id||(!characterPlaying&&characterQueue.length===0&&id===currentCharacter))return;
    characterQueue.push(id);playCharacterQueue();
  };
  story.preview.blocks.forEach(block=>{
    if(block.type==='character'){const marker=element('span','preview-character-marker');marker.dataset.character=block.character||'';scroll.append(marker);return}
    if(block.type==='separator'){scroll.append(element('hr','preview-separator'));return}
    if(block.type==='dialogue'){
      const dialogue=element('div','preview-dialogue');
      dialogue.append(element('p','preview-speaker',block.speaker||''),element('p','',block.text||''));
      scroll.append(dialogue);return;
    }
    scroll.append(element('p','preview-narration',block.text||''));
  });
  const continuation=readerLink(story);
  if(continuation)scroll.append(continuation);
  else scroll.append(element('p','preview-ending','Продолжение — в приложении'));
  const markers=[...scroll.querySelectorAll('[data-character]')];let requestedMarker=0;
  scroll.addEventListener('scroll',()=>{const atEnd=scroll.scrollTop+scroll.clientHeight>=scroll.scrollHeight-2;const threshold=atEnd?scroll.scrollHeight:scroll.scrollTop+scroll.clientHeight*.42;let nextIndex=0;markers.forEach((marker,index)=>{if(marker.offsetTop<=threshold)nextIndex=index});if(nextIndex===requestedMarker)return;const direction=nextIndex>requestedMarker?1:-1;for(let index=requestedMarker+direction;direction>0?index<=nextIndex:index>=nextIndex;index+=direction)showCharacter(markers[index]?.dataset.character);requestedMarker=nextIndex});
  body.append(scroll,stage);modal.append(header,body);backdrop.append(modal);
  backdrop.addEventListener('click',event=>{if(event.target===backdrop)closePreview()});
  backdrop.addEventListener('pointerdown',event=>event.stopPropagation());
  backdrop.addEventListener('pointerup',event=>event.stopPropagation());
  site.append(backdrop);previewModal=backdrop;showCharacter(story.preview.blocks.find(block=>block.type==='character')?.character);close.focus();
  const resizeObserver=new ResizeObserver(()=>characterImages.forEach(image=>layoutCharacter(image)));resizeObserver.observe(stage);previewCleanup=()=>{characterStopped=true;characterQueue=[];clearTimeout(characterHoldTimer);resizeObserver.disconnect();characterImages.forEach(image=>image.getAnimations().forEach(animation=>animation.cancel()))};
}

function goTo(index){
  if(!stories.length)return;closePreview();
  const previous=stage.children[active];const nextIndex=(index+stories.length)%stories.length;if(nextIndex===active)return;
  if(previous){previous.style.visibility='visible';previous.style.pointerEvents='none'}
  active=nextIndex;renderActive();const next=stage.children[active];
  previous?.getAnimations().forEach(animation=>animation.cancel());next?.getAnimations().forEach(animation=>animation.cancel());
  const leaving=previous?.animate([{opacity:1},{opacity:0}],{duration:850,easing:'ease',fill:'none'});
  leaving?.finished.catch(()=>{}).then(()=>{previous.style.visibility='';previous.style.pointerEvents=''});
  next?.animate([{opacity:0},{opacity:1}],{duration:850,easing:'ease',fill:'none'});
}

function makeSlide(story,index){
  const slide=element('section','story-screen');
  slide.id=story.id;
  slide.style.setProperty('--accent',story.accent);
  slide.setAttribute('aria-labelledby',`${story.id}-title`);
  const art=element('div','story-art');art.style.backgroundImage=`url("${story.cover}")`;art.setAttribute('aria-hidden','true');
  const shade=element('div','story-shade');shade.setAttribute('aria-hidden','true');
  const copy=element('div','story-copy');
  copy.append(element('p','app-name',`Кострома · ${String(index+1).padStart(2,'0')}`));
  const meta=element('div','story-meta');
  if(story.genre)meta.append(element('p','genre',story.genre));
  if(story.releaseStage==='beta'){
    const status=element('span','story-status','Бета');
    status.title='История находится в разработке';
    meta.append(status);
  }
  copy.append(meta);
  const title=element('h1','',story.title);title.id=`${story.id}-title`;copy.append(title);
  const rule=element('span','rule');rule.setAttribute('aria-hidden','true');copy.append(rule);
  if(story.description)copy.append(element('p','annotation',story.description));
  const read=readerLink(story);if(read)copy.append(read);
  if(story.preview){const trigger=element('button','preview-trigger','Читать начало  →');trigger.addEventListener('click',()=>openPreview(story));copy.append(trigger)}
  slide.append(art,shade,copy);
  return slide;
}

async function loadStories(){
  const manifestResponse=await fetch(manifestUrl,{cache:'no-store'});
  if(!manifestResponse.ok)throw new Error('manifest');
  const manifest=await manifestResponse.json();
  const releases=Object.entries(manifest.stories||{});
  if(!releases.length)throw new Error('empty');
  stories=await Promise.all(releases.map(async([storyId,version],index)=>{
    const root=`${contentRoot}/stories/${encodeURIComponent(storyId)}/${encodeURIComponent(version)}`;
    const previewRoot=`${root}/preview`;
    const [response,previewResponse]=await Promise.all([fetch(`${root}/card.json`,{cache:'no-store'}),fetch(`${previewRoot}/preview.json`,{cache:'no-store'})]);
    if(!response.ok)throw new Error(storyId);
    const card=await response.json();
    const rawPreview=previewResponse.ok?await previewResponse.json():undefined;
    const preview=rawPreview?{...rawPreview,characters:Object.fromEntries(Object.entries(rawPreview.characters||{}).map(([id,character])=>[id,{...character,image:`${previewRoot}/${character.image}`}]))}:undefined;
    return{id:card.storyId||storyId,title:card.title||storyId,genre:card.genre||'',releaseStage:card.releaseStage==='beta'?'beta':'stable',description:card.description||'',cover:`${root}/${card.cover||'cover.png'}`,accent:accents[index%accents.length],preview};
  }));
  stage.replaceChildren(...stories.map(makeSlide));
  dots.replaceChildren(...stories.map((story,index)=>{const dot=element('button','dot');dot.setAttribute('aria-label',`Открыть историю «${story.title}»`);dot.addEventListener('click',()=>goTo(index));return dot}));
  nav.hidden=stories.length<2;
  renderActive();
}

document.querySelector('.previous').addEventListener('click',()=>goTo(active-1));
document.querySelector('.next').addEventListener('click',()=>goTo(active+1));
window.addEventListener('keydown',event=>{if(event.key==='Escape'&&previewModal){closePreview();return}if(previewModal)return;if(event.key==='ArrowRight'||event.key==='PageDown')goTo(active+1);if(event.key==='ArrowLeft'||event.key==='PageUp')goTo(active-1)});
site.addEventListener('pointerdown',event=>{pointerStart=event.clientX});
site.addEventListener('pointerup',event=>{if(pointerStart===null)return;const distance=event.clientX-pointerStart;if(Math.abs(distance)>48)goTo(active+(distance<0?1:-1));pointerStart=null});
loadWebPlayer().then(loadStories).catch(()=>{stage.innerHTML='<div class="catalog-state">Истории временно недоступны</div>'});
