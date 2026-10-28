from pathlib import Path
import zipfile,hashlib,json
root=Path(__file__).resolve().parents[1]
release=root/'Releases';release.mkdir(exist_ok=True)
result={}
for platform in ['Windows','WebGL']:
 source=root/'Builds'/platform
 if not source.exists():continue
 target=release/f'AliceInSchizoforest-{platform}.zip'
 with zipfile.ZipFile(target,'w',zipfile.ZIP_DEFLATED,6) as z:
  for file in source.rglob('*'):
   if file.is_file() and '.vercel' not in file.parts and not any(x in file.name for x in ['.save.json','.settings.json','.pdb','.log']):z.write(file,file.relative_to(source))
 with zipfile.ZipFile(target) as z:
  assert z.testzip() is None
  assert ('Alice.exe' if platform=='Windows' else 'index.html') in z.namelist()
 result[platform]={'bytes':target.stat().st_size,'sha256':hashlib.sha256(target.read_bytes()).hexdigest()}
(release/'checksums.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps(result,indent=2))
