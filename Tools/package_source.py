from pathlib import Path
from zipfile import ZipFile,ZIP_DEFLATED
root=Path(__file__).resolve().parents[1]
with ZipFile(root/'Releases/AliceInSchizoforest-UnityProject.zip','w',ZIP_DEFLATED,6) as z:
 for folder in ['Assets','Packages','ProjectSettings','Tools','Portfolio']:
  for file in (root/folder).rglob('*'):
   if file.is_file() and file.suffix not in ['.log','.dmp']:z.write(file,Path(root.name)/file.relative_to(root))
 for item in [root/'QA/v1.7.4',root/'QA/RESULTS_RU.md',root/'QA/UI_1.7.0.md',root/'QA/UI_1.7.2.md',root/'QA/UI_1.7.3.md',root/'QA/RELEASE_1.7.4.md',root/'QA/validation.txt']:
  files=item.rglob('*') if item.is_dir() else [item]
  for file in files:
   if file.is_file() and file.suffix not in ['.log','.dmp']:z.write(file,Path(root.name)/file.relative_to(root))
 for name in ['README.md','.gitignore']:z.write(root/name,Path(root.name)/name)
print('Unity project archive created')


