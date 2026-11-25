// Run with the project's licensed Unity editor and both target modules installed.
const fs = require('node:fs');
const path = require('node:path');
const {spawnSync} = require('node:child_process');

const editor = process.argv[2];
if (!editor || !fs.existsSync(editor)) throw new Error('Pass the full path to the Unity editor executable.');
const project = path.resolve(__dirname, '../RememberMeUnity');
const output = path.join(project, 'Build');
fs.mkdirSync(output, {recursive: true});
for (const [platform, method, artifact] of [
  ['Win64', 'Build', 'Windows/RememberMe.exe'],
  ['WebGL', 'BuildForGitHubPages', 'GitHubPages/docs/index.html']
]) {
  const log = path.join(output, platform + '-build.log');
  const result = spawnSync(editor, ['-batchmode', '-nographics', '-quit', '-projectPath', project,
    '-buildTarget', platform, '-executeMethod', 'RememberMe.Editor.Act1Builder.' + method, '-logFile', log],
    {stdio: 'inherit'});
  if (result.error) throw result.error;
  if (result.status !== 0 || !fs.existsSync(path.join(output, artifact))) throw new Error(platform + ' build failed. See ' + log);
  console.log(platform + ' ready at ' + path.join(output, artifact));
}
fs.copyFileSync(path.join(project, 'Assets/Art/Fonts/Tinos-LICENSE.txt'), path.join(output, 'Windows/Tinos-LICENSE.txt'));
