// Technical packing only: resize generated portraits, align generated sprite cells,
// and extract the existing elderly male. No programmatic character illustration.
const fs = require('fs');
const path = require('path');
const sharp = require(process.env.RM_SHARP_MODULE || 'sharp');
const root = path.resolve(__dirname, '..');
const raw = path.join(root, 'generation');
const assets = path.join(root, 'assets/img');
function runs(values) {
    const out=[]; let start=-1;
    values.forEach((v,i)=> {if(v && start<0) start=i; if(!v && start>=0){out.push([start,i]);start=-1;}});
    if(start>=0)out.push([start,values.length]); return out;
}
async function sprite(file, output) {
    const input=path.join(raw,file);
    const {data,info}=await sharp(input).ensureAlpha().raw().toBuffer({resolveWithObject:true});
    const {width:w,height:h}=info;
    const xs=Array(w).fill(0),ys=Array(h).fill(0);
    for(let y=0;y<h;y++)for(let x=0;x<w;x++)if(data[(y*w+x)*4+3]>128){xs[x]++;ys[y]++;}
    const xr=runs(xs.map(n=>n>8)).sort((a,b)=>(b[1]-b[0])-(a[1]-a[0])).slice(0,3).sort((a,b)=>a[0]-b[0]);
    const yr=runs(ys.map(n=>n>8)).sort((a,b)=>(b[1]-b[0])-(a[1]-a[0])).slice(0,4).sort((a,b)=>a[0]-b[0]);
    if(xr.length!==3||yr.length!==4)throw Error('Expected 3x4 isolated sprite groups: '+file);
    const xcuts=[0,...xr.slice(1).map((r,i)=>Math.round((xr[i][1]+r[0])/2)),w];
    const ycuts=[0,...yr.slice(1).map((r,i)=>Math.round((yr[i][1]+r[0])/2)),h];
    const bounds=[];
    for(let row=0;row<4;row++)for(let col=0;col<3;col++){
        let left=w,top=h,right=-1,bottom=-1;
        for(let y=ycuts[row];y<ycuts[row+1];y++)for(let x=xcuts[col];x<xcuts[col+1];x++){
            if(data[(y*w+x)*4+3]>64){left=Math.min(left,x);right=Math.max(right,x);top=Math.min(top,y);bottom=Math.max(bottom,y);}
        }
        if(right<left)throw Error('Empty sprite cell');
        bounds.push({left,top,width:right-left+1,height:bottom-top+1,row,col});
    }
    const scale=40/Math.max(...bounds.map(b=>b.height));
    const composite=[];
    for(const b of bounds){
        const width=Math.round(b.width*scale),height=Math.round(b.height*scale);
        if(width>42)throw Error('Sprite too wide');
        const buffer=await sharp(input).extract({left:b.left,top:b.top,width:b.width,height:b.height}).resize(width,height,{kernel:'lanczos3'}).png().toBuffer();
        composite.push({input:buffer,left:b.col*48+Math.floor((48-width)/2),top:b.row*48+44-height});
    }
    await sharp({create:{width:144,height:192,channels:4,background:'#00000000'}}).composite(composite).png().toFile(path.join(assets,'characters',output));
    return {file:output,source:info,bounds,scale,cell:[48,48],baseline:44};
}
(async()=>{
    for(const name of ['UncleJames','Aunt','Grandpa']){
        await sharp(path.join(raw,name+'_faces.png')).resize(576,288,{fit:'fill',kernel:'lanczos3'}).png().toFile(path.join(assets,'faces','RM_Face_'+name+'.png'));
    }
    const results=[];
    results.push(await sprite('UncleJames_sprites.png','$RM_UncleJames.png'));
    results.push(await sprite('Aunt_sprites.png','$RM_Aunt.png'));
    await sharp(path.join(root,'../RememberMeAct1/img/characters/RM_Family.png')).extract({left:432,top:0,width:144,height:192}).png().toFile(path.join(assets,'characters','$RM_Grandpa.png'));
    fs.writeFileSync(path.join(root,'review/asset_packing.json'),JSON.stringify(results,null,2));
    console.log('Packed 3 face sheets (576x288) and 3 walking sheets (144x192).');
})().catch(e=>{console.error(e.message);process.exit(1);});
