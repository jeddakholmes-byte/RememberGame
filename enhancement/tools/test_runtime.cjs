/* Browser integration checks against the project's actual RPG Maker MV runtime.
 * Position setup uses engine APIs; dialogue advances with Enter. Not a manual playtest.
 */
const fs=require('fs');
const path=require('path');
const assert=require('assert');
const {chromium}=require(process.env.RM_PLAYWRIGHT_MODULE || 'playwright');
const root=path.resolve(__dirname,'..');
const result={date:new Date().toISOString(),checks:[],errors:[],messages:[],shots:[]};
let browser,page;
const check=(name,ok)=>{assert(ok,name);result.checks.push(name);console.log('PASS '+name);};
async function state(){return page.evaluate(()=>({
    scene:SceneManager._scene&&SceneManager._scene.constructor.name,
    map:window.$gameMap&&$gameMap.mapId(),
    busy:!!(window.$gameMap&&($gameMap.isEventRunning()||$gameMessage.isBusy())),
    text:window.$gameMessage?$gameMessage.allText():'',
    pause:!!(SceneManager._scene&&SceneManager._scene._messageWindow&&SceneManager._scene._messageWindow.pause),
    player:window.$gamePlayer?[$gamePlayer.x,$gamePlayer.y,$gamePlayer.direction()]:[],
    switches:window.$gameSwitches?$gameSwitches._data:[],
    hint:SceneManager._scene&&SceneManager._scene._rmHintText,
    grandma:window.$gameMap&&$gameMap.mapId()===2?[$gameMap.event(2).x,$gameMap.event(2).y]:[],
    brightness:window.$gameScreen?$gameScreen.brightness():0
}));}
async function shot(name){await page.screenshot({path:path.join(root,'review',name+'.png')});result.shots.push(name);}
async function advance(until,timeout=90000,capture=false){
    const start=Date.now();let last='';const captured=new Set();
    while(Date.now()-start<timeout){
        const s=await state(); if(until(s))return s;
        if(s.text&&s.text!==last){result.messages.push({map:s.map,text:s.text,player:s.player,grandma:s.grandma});last=s.text;}
        if(capture&&s.pause){
            const marks={'I need to go home.':'04_flashback_open','Let me go home!':'05_flashback_door','I CAN\'T DO THIS AGAIN.':'06_flashback_outburst','Your name is on it.':'08_mom_approaches','For Grace.':'09_box'};
            for(const [text,name] of Object.entries(marks))if(s.text.includes(text)&&!captured.has(name)){await shot(name);captured.add(name);}
        }
        if(capture&&s.map===2&&s.player[1]===14&&s.busy&&!captured.has('07_step_back')&&s.brightness===255){await shot('07_step_back');captured.add('07_step_back');}
        if(s.pause){await page.keyboard.press('Enter', {delay:120});await page.waitForTimeout(90);}
        else if(s.text){await page.keyboard.down('Enter');await page.waitForTimeout(40);await page.keyboard.up('Enter');}
        else await page.waitForTimeout(70);
        if(result.errors.length)throw Error(result.errors[0]);
    }
    throw Error('Timed out: '+JSON.stringify(await state()));
}
async function interact(x,y,d){
    await page.evaluate(([x,y,d])=>{$gamePlayer.locate(x,y);$gamePlayer.setDirection(d);},[x,y,d]);
    await page.waitForTimeout(160);await page.keyboard.press('Enter', {delay:120});await page.waitForTimeout(180);
}
(async()=>{
    browser=await chromium.launch({channel:'chrome',headless:true,args:['--autoplay-policy=no-user-gesture-required']});
    page=await browser.newPage({viewport:{width:960,height:720}});
    page.on('pageerror',e=>result.errors.push(e.message));
    page.on('response',r=>{if(r.status()>=400)result.errors.push(r.status()+' '+r.url());});
    await page.goto('http://127.0.0.1:8766/');
    await page.waitForFunction(()=>window.SceneManager&&SceneManager._scene instanceof Scene_Title&&SceneManager._scene.isActive());
    await page.waitForTimeout(500);console.log('Title ready');await page.keyboard.press('Enter', {delay:120});
    await advance(s=>s.map===1&&s.switches[1]&&!s.busy);
    await shot('01_cemetery');check('Opening completes and returns control',true);
    await page.keyboard.press('Escape', {delay:120});
    await page.waitForFunction(()=>SceneManager._scene instanceof Scene_Menu);
    await page.waitForTimeout(200);await shot('02_pause_menu');
    check('Story menu has Resume, Save, Options and Return to Title',await page.evaluate(()=>SceneManager._scene._commandWindow._list.map(x=>x.name).join('|')==='Resume|Save|Options|Return to Title'));
    await page.keyboard.press('Escape', {delay:120});await page.waitForFunction(()=>SceneManager._scene instanceof Scene_Map);
    await page.waitForTimeout(200);
    await interact(8,10,8);
    await advance(s=>s.pause&&s.text.includes('Take your time'));
    await shot('03_uncle_portrait');await advance(s=>!s.busy);
    check('Uncle face loads and dialogue completes',true);
    await interact(17,11,8);await advance(s=>s.pause&&s.text.includes('tissue'));await shot('03b_aunt_portrait');await advance(s=>!s.busy);
    await interact(16,8,8);await advance(s=>s.pause&&s.text.includes('Grandpa'));await shot('03c_grandpa_portrait');await advance(s=>!s.busy);
    check('Aunt and Grandpa interactions load',true);
    await interact(10,7,8);await advance(s=>!s.busy);
    const count=result.messages.length;
    await interact(13,7,8);await advance(s=>!s.busy);
    check('Both flower positions share the shorter repeat',result.messages.slice(count).filter(x=>x.text).every(x=>!x.text.includes('There are so many flowers')));
    await page.evaluate(()=>{$gamePlayer.locate(11,7);$gamePlayer.setDirection(8);});await page.waitForTimeout(200);
    check('Context hint appears facing the grave',(await state()).hint.includes("Grandma's stone"));await shot('03d_interaction_hint');
    await page.keyboard.press('Enter', {delay:120});await page.waitForTimeout(150);await advance(s=>!s.busy);
    check('Main grave sets VisitedGrave',(await state()).switches[2]===true);
    await page.evaluate(()=>{$gameVariables.setValue(1,4);$gameSwitches.setValue(6,true);});await page.waitForTimeout(400);
    check('Mom reminder does not replay after visiting the main grave',!(await state()).busy&&!(await state()).switches[6]);
    // Save/load round trip in browser local storage; no original save files touched.
    check('Save/load round trip works',await page.evaluate(()=>{const ok=DataManager.saveGame(1);$gameSwitches.setValue(2,false);const loaded=DataManager.loadGame(1);return ok&&loaded&&$gameSwitches.value(2);}));
    await page.evaluate(()=>{$gamePlayer.locate(13,7);$gamePlayer.setDirection(6);});await page.waitForTimeout(150);
    await page.keyboard.press('Enter', {delay:120});await page.waitForTimeout(150);
    await advance(s=>s.scene==='Scene_Title',120000,true);
    const flash=result.messages.filter(m=>m.map===2);
    check('Grandma reaches the door before saying No',flash.some(m=>m.text.endsWith('\nNo.')&&m.grandma[0]===12&&m.grandma[1]===4));
    check('Grace visibly steps backward to the hallway',result.shots.includes('07_step_back'));
    check('Flashback returns, box sequence completes, title returns',result.messages.some(m=>m.text.includes("I wonder what's inside.")));
    check('No browser errors or missing requests',result.errors.length===0);
    result.status='passed';
})().catch(e=>{result.status='failed';result.failure=e.message;console.error('FAIL '+e.message);process.exitCode=1;}).finally(async()=>{
    fs.writeFileSync(path.join(root,'review/runtime_checks.json'),JSON.stringify(result,null,2));
    if(browser)await browser.close();
});
