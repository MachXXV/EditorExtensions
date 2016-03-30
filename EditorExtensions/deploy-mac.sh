mkdir -p $HOME'/Library/Application Support/Steam/steamapps/common/Kerbal Space Program/GameData/EditorExtensions/'
cp bin/Debug/EditorExtensions.dll $HOME'/Library/Application Support/Steam/steamapps/common/Kerbal Space Program/GameData/EditorExtensions/'
cp bin/Debug/EditorExtensions.version $HOME'/Library/Application Support/Steam/steamapps/common/Kerbal Space Program/GameData/EditorExtensions/'

mkdir -p $HOME'/Library/Application Support/Steam/steamapps/common/Kerbal Space Program/GameData/EditorExtensions/Textures/'
cp bin/Debug/Textures/*.png $HOME'/Library/Application Support/Steam/steamapps/common/Kerbal Space Program/GameData/EditorExtensions/Textures/'
