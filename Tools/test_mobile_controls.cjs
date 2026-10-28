// Unit tests for the shipping page's input handlers using an isolated fake DOM.
const fs=require('fs'),vm=require('vm'),assert=require('assert');
const html=fs.readFileSync('Assets/WebGLTemplates/Alice/index.html','utf8');
const elements=new Map(),messages=[];
function element(id){if(elements.has(id))return elements.get(id);const events={},classes=new Set();const e={id,events,style:{setProperty(k,v){this[k]=v}},dataset:{},classList:{add:x=>classes.add(x),contains:x=>classes.has(x),toggle:(x,on)=>on?classes.add(x):classes.delete(x)},addEventListener:(n,f)=>(events[n]??=[]).push(f),setPointerCapture(){},getBoundingClientRect:()=>({left:20,top:200,width:140,height:140}),focus(){},remove(){}};elements.set(id,e);return e;}
const attack=element('attack');attack.dataset.key='4';const take=element('take-cat');take.dataset.key='5';
const root=element('html'),docEvents={},winEvents={};
const context={console,URLSearchParams,navigator:{userAgent:'Android Mobile'},screen:{},location:{hostname:'localhost'},setTimeout:()=>{},document:{documentElement:root,hidden:false,body:{appendChild(){}},head:{appendChild(){}},createElement:()=>element('loader'),getElementById:element,querySelectorAll:q=>q==='[data-key]'?[attack,take]:[],addEventListener:(n,f)=>(docEvents[n]??=[]).push(f)},window:{devicePixelRatio:3,innerWidth:844,innerHeight:390,addEventListener:(n,f)=>(winEvents[n]??=[]).push(f),scrollTo(){}}};
vm.createContext(context);vm.runInContext(html.match(/<script>([\s\S]*?)<\/script>/)[1],context);
context.messages=messages;vm.runInContext("instance={SendMessage:(...args)=>messages.push(args)}",context);
const emit=(el,type,id,x=90,y=270)=>{for(const f of el.events[type]||[])f({pointerId:id,clientX:x,clientY:y,preventDefault(){}});};
const lastStick=()=>messages.filter(m=>m[1]==='BrowserJoystick').at(-1)[2].split(',').map(Number);
const joy=element('joystick');context.window.aliceTouchState(1,0);
emit(joy,'pointerdown',1,138,222);let [x,y]=lastStick();assert(x>.6&&y>.6,'diagonal jump + run');
emit(attack,'pointerdown',2);assert.equal(messages.at(-1)[2],'16','second finger attacks');
emit(attack,'pointerup',2);assert(lastStick()[0]>.6,'attack release preserves stick');
emit(joy,'pointermove',1,42,318);[x,y]=lastStick();assert(x<-.6&&y<-.6,'left + crouch');
emit(joy,'pointerdown',3,138,270);assert(lastStick()[0]<0,'second finger cannot steal joystick');
emit(joy,'pointercancel',1);assert.deepEqual(lastStick(),[0,0],'cancel stops movement');
emit(joy,'pointerdown',4,138,270);context.window.aliceTouchState(0,0);assert.deepEqual(lastStick(),[0,0],'dialogue/pause clears stick');assert(!element('touch-controls').classList.contains('active'));
context.window.aliceTouchState(1,1);assert(take.classList.contains('available'),'contextual kitten button');
emit(joy,'pointerdown',5,138,270);for(const f of winEvents.blur)f();assert.deepEqual(lastStick(),[0,0],'focus loss stops movement');
assert(!html.includes('data-key="0"')&&!html.includes('data-key="2"'),'legacy directional buttons removed');
for(const [width,height] of [[844,390],[932,430],[1024,768]]){
 context.window.innerWidth=width;context.window.innerHeight=height;vm.runInContext('sizeMobileViewport()',context);
 assert.equal(element('game').style.width,width+'px');assert.equal(element('game').style.height,height+'px');assert.equal(element('game').style.left,'0px');assert.equal(element('game').style.top,'0px');
}
// Browser toolbar collapse and high-DPI render buffer must use one viewport.
context.window.devicePixelRatio=3;
context.window.visualViewport={width:844,height:350,offsetLeft:0,offsetTop:0};
vm.runInContext('sizeMobileViewport()',context);

assert.equal(element('mobile-viewport').style['--vh'],'3.5px');
context.window.visualViewport.height=390;
vm.runInContext('sizeMobileViewport()',context);
assert.equal(element('game').style.height,'390px');
assert.equal(element('mobile-viewport').style['--vh'],'3.9px');
assert.equal(element('mobile-viewport').style['--top-key'],'76px','header buttons are usable and do not change with toolbar');
// Repeated resize events over a simulated three-minute session, including browser zoom.
for(let second=0;second<=180;second++){
 const zoom=second<60?1:second<120?2:1.5;
 context.window.visualViewport={width:844/zoom,height:390/zoom,scale:zoom,offsetLeft:0,offsetTop:0};
 vm.runInContext('sizeMobileViewport()',context);
 assert.equal(element('mobile-viewport').style['--top-key'],'76px');
 const inverse=Number(element('mobile-viewport').style.transform.match(/scale\(([^)]+)\)/)[1]);
 assert(Math.abs(76*inverse*zoom-76)<.01,'visible button size remains constant during browser zoom');
 assert.equal(element('mobile-viewport').style['--attack-size'],'112px');
}
assert(html.includes('translate(-41.9921875%,-54.678363%)'),'inventory alpha bounds centered');
assert(html.includes('translate(-56.0546875%,-47.076023%)'),'pause alpha bounds centered');
assert(html.includes('matchWebGLToCanvasSize:true'),'buffer resolution must have one explicit owner');
assert(!html.includes('canvas.width='),'do not bypass Unity when changing its render buffer');
assert(html.includes('place-content:end center'),'loading stays at bottom');
vm.runInContext('enterMobileFullscreen()',context);assert(messages.some(m=>m[1]==='BrowserStart'),'mobile tap skips duplicate Unity splash');
assert(!html.includes('НАЖАТЬ И ИГРАТЬ'),'no extra poster instruction');
for(const name of ['gesturestart','gesturechange','gestureend','touchstart','touchmove']){
 let prevented=false;for(const f of docEvents[name]||[])f({touches:[{},{}],preventDefault(){prevented=true}});
 assert(prevented,'Safari two-finger gesture is blocked: '+name);
}
console.log('PASS: mobile diagonal movement, multitouch, cancellation, pause, full viewport at three sizes, single splash and contextual rescue');
