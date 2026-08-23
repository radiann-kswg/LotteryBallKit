# CONTRIBUTING — LotteryBallKit

キットを改造・再生成するときの手順書です。単に使うだけなら [`README.md`](README.md) だけで足ります。

How to modify and regenerate the kit. If you only want to *use* it, [`README.md`](README.md) is enough.

---

## 1. リポジトリの構造 / Repository layout

| パス | 役割 |
| --- | --- |
| `Assets/LotteryBallKit/` | **配布されるパッケージ本体。** UPM パッケージのルートでもある（`package.json` あり） |
| `BlenderSources/` | 編集用の原本。Unity のインポート対象外に置いてある |
| `Assets/`（その他） / `Packages/` / `ProjectSettings/` | 動作確認用のホストUnityプロジェクト。配布物ではない |

`Assets/LotteryBallKit/` の外にあるものは、あくまで開発・確認用の足場です。
配布物に何かを足したいときは必ず `Assets/LotteryBallKit/` の中に置いてください。

Everything outside `Assets/LotteryBallKit/` is just scaffolding for development. Anything meant to ship
must live inside `Assets/LotteryBallKit/`.

### 原本の二重管理について / Why the .blend exists twice

Blender 原本は 2 か所にあります。**`BlenderSources/LotteryBall.blend` が正本です。**

- `BlenderSources/LotteryBall.blend` … 編集する実体。Unity はこのフォルダをインポートしない
- `Assets/LotteryBallKit/Sources/LotteryBall.blend.bytes` … 配布用の複製。`.bytes` にしているのは、
  Unity が `.blend` を見つけると自動でモデルとしてインポートしてしまい、FBX と二重になるため

`.blend` を編集したら、**必ず両方を更新**してください。

```powershell
Copy-Item BlenderSources\LotteryBall.blend Assets\LotteryBallKit\Sources\LotteryBall.blend.bytes -Force
```

`BlenderSources/LotteryBall.blend` is the source of truth; the `.bytes` copy inside the package is the
redistributable duplicate (renamed so Unity doesn't auto-import it as a second model). Update both.

---

## 2. ライセンス衛生 / License hygiene

**このリポジトリは全体が CC BY 4.0 です。** つまり、CC BY 4.0 で配れないものを 1 つでも混ぜた時点で
リポジトリのライセンス表示が嘘になります。以下は特に混入しやすいので注意してください。

- **他プロジェクトのボールスキン**（キャラクター画像）。原典の RouletteSphereChaser では
  `Assets/Textures/BallSkins/` がライセンス対象外として除外されていました。ここには持ち込まないこと
- **Blender ファイルに残る画像データブロック参照。** テクスチャを外しても、`bpy.data.images` に
  データブロックだけが残り、外部の絶対パスを指し続けることがあります（下記 3-3 参照）
- **フォント本体。** `PenchantManufacture.otf` は同作者・CC BY 4.0 ですが、このリポジトリには同梱していません

This repository is CC BY 4.0 *in its entirety*. A single non-CC-BY file makes the license statement false.
Watch out for: character ball skins from other projects, stale image datablocks left inside the `.blend`,
and font binaries.

---

## 3. Blender 原本の編集 / Editing the Blender source

`BlenderSources/LotteryBall.blend`（Blender 5.x で作成）を開きます。

### 3-1. メッシュ構成 / Mesh layout

- **本体** … キューブを球体化したメッシュ（384面）。全周UV。マテリアルスロット 0 = `BallBody`
- **番号デカール** … 球面から 0.2mm 浮かせた円ディスク。マテリアルスロット 1 = `BallNumber`

デカールを球面から浮かせているのは Z-fighting 回避のためです。球の半径を変えるときは
この浮かせ量も比例させてください。

The number decal floats 0.2 mm above the sphere to avoid Z-fighting. Scale that offset with the radius.

### 3-2. FBX の書き出し / Exporting the FBX

`File → Export → FBX (.fbx)` で `Assets/LotteryBallKit/Models/LotteryBall.fbx` へ上書きします。

- Limit to: **Selected Objects**（`PreviewCam` などを含めないため）
- Apply Scale: **FBX All**
- Forward `-Z Forward` / Up `Y Up`（Unity 既定）
- マテリアルスロットの**並び順を変えないこと**。`NumberBall.cs` は submesh 0 = 本体 / submesh 1 = 番号 を前提にしています

Do **not** reorder the material slots — `NumberBall.cs` assumes submesh 0 = body, submesh 1 = number.

書き出し後、Unity 側で `NumberBall.prefab` のメッシュ参照とマテリアル割り当てが外れていないか確認してください。

### 3-3. 未使用データブロックの掃除 / Purging stale datablocks

Blender は「もう使っていない画像」もファイル内に残します。外部プロジェクトの絶対パスを
指したまま残ると、ライセンス上まずいだけでなく、他人が開いたときにリンク切れとして見えます。

保存前に **`File → Clean Up → Purge Unused Data`（Recursive を有効）** を実行してください。
それでも `bpy.data.images` に残る場合は、Blender の Scripting タブで以下を流します。

Blender keeps unused images inside the file. Run `File → Clean Up → Purge Unused Data` (recursive)
before saving; if datablocks survive, run this in the Scripting tab:

```python
import bpy

KEEP = {"NumberAtlas.png", "BallSkins_Sample.png", "Render Result", "Viewer Node"}

for img in list(bpy.data.images):
    if img.name in KEEP:
        img.use_fake_user = False
        continue
    print("removing", img.name, img.filepath)
    bpy.data.images.remove(img, do_unlink=True)

# 残す画像は必ずリポジトリ内の相対パスへ（外部の絶対パスを残さない）
for img in bpy.data.images:
    if img.filepath:
        img.filepath = "//" + img.name

bpy.ops.outliner.orphans_purge(do_recursive=True)
bpy.ops.wm.save_mainfile()
print([ (i.name, i.filepath) for i in bpy.data.images ])
```

保存後、リポジトリ外を指す参照が残っていないか確認できます。

Verify afterwards that nothing points outside the repository. 一番確実なのは Blender の Outliner を
**Blender File** 表示に切り替えて Images を目視することですが、コマンドラインでも確認できます
（Blender 5.x の `.blend` は zstd 圧縮なので、展開してから文字列を探します）:

```bash
# pip install zstandard
python3 -c "
import re, zstandard
raw = zstandard.ZstdDecompressor().decompress(
    open('BlenderSources/LotteryBall.blend','rb').read(), max_output_size=1<<26)
for s in sorted(set(re.findall(rb'[ -~]{6,}', raw))):
    if s.startswith(b'//') or re.match(rb'[A-Za-z]:[/\\\\]', s):
        print(s.decode())
"
```

出力に `//NumberAtlas.png` `//BallSkins_Sample.png` 以外のパス（特に `D:/...` のような絶対パス）が
出てきたら、まだ掃除しきれていません。

#### 現状の未処理分 / Known leftovers (as of v1.0.0)

`BlenderSources/LotteryBall.blend` には、以下がまだ残っています。**画像の実体（ピクセル）は
pack されていないため配布物にバイナリは含まれていません**が、参照だけが残っている状態です。
次に Blender で開いたときに §3-3 の手順で除去してください。

The `.blend` still carries these references. **No pixel data is packed into the file**, so nothing
non-CC-BY actually ships — but the dangling references should be purged on the next edit pass.

| データブロック | filepath | 対応 |
| --- | --- | --- |
| `RefBallTex.png` | `D:/Unity UserFile/.../RouletteSphereChaser/Assets/Textures/BallSkins/RefBallTex.png` | **削除**（CC BY 対象外のボールスキン） |
| `BallSkins_Sample.png.001` | `//../Assets/Textures/BallSkins_Sample.png` | 削除（重複。リポジトリ外を指している） |
| `NumberAtlas.png.001` | — | 削除（重複） |

---

## 4. 番号アトラスの再生成 / Regenerating the number atlas

`Assets/LotteryBallKit/Sources/gen_number_atlas.py` が `NumberAtlas.png`（10×10 タイル・128px/タイル）を生成します。

### 前提 / Prerequisites

- Python 3 + Pillow（`pip install pillow`）
- `PenchantManufacture.otf`（同作者・CC BY 4.0）。**リポジトリには同梱していません**。
  別書体を使う場合は `--font` で差し替えてください

### 実行 / Running

リポジトリのルートから:

```bash
python Assets/LotteryBallKit/Sources/gen_number_atlas.py --font /path/to/PenchantManufacture.otf
```

出力は `Assets/LotteryBallKit/Textures/NumberAtlas.png` に上書きされます。

### レイアウト規約 / Layout contract

タイル `(col, row)` と番号 `n` の対応は **`col = n % 10`, `row_from_top = 9 - n / 10`** です。
UV 原点が左下なので、**下段が 0〜9、最上段が 90〜99** になります。

`NumberBall.cs` はこの規約に依存しています:

```csharp
new Vector4(1f, 1f, 0.1f * (number % 10), 0.1f * (number / 10))
```

レイアウトを変えるなら、スクリプト側の UV オフセット計算も必ず合わせてください。

The atlas layout and `NumberBall.cs`'s UV offset are a contract — change one, change the other.

数字の大きさは全番号で統一されています（2桁の "10" が収まるサイズに 1桁も揃える）。
`FIT_R` を上げると数字が大きくなりますが、デカールディスクの縁からはみ出さない範囲に留めてください。

---

## 5. スキンの描き方 / Authoring ball skins

キャンバスは **2:1**（例 2048×1024）。`BallUV_Template.png` をガイドレイヤーに敷いてください。

- **左円 = 前半球**（正面）。方位等距離図法。円の中心が顔の中心
- **右円 = 後半球**。左右鏡像なので、**後ろから見た絵をそのまま描けます**
- **赤い弧** = 番号デカールに隠れる範囲。ここに情報を置かないこと

`BallUV_Template.psd` にレイヤー付きの原本があります（`BlenderSources/` 側のみ）。

Canvas is 2:1. Left circle = front hemisphere (azimuthal equidistant, centre = face centre);
right circle = back hemisphere, mirrored, so you can draw the rear view directly.
The red arc marks the area hidden by the number decal — keep it clear.

---

## 6. 変更を出すとき / Before you commit

- [ ] `Library/` `Temp/` `Logs/` `obj/` `UserSettings/` を含めていない（`.gitignore` 済み）
- [ ] `.blend` を編集したなら `Assets/LotteryBallKit/Sources/LotteryBall.blend.bytes` も更新した
- [ ] `.blend` から未使用画像データブロックを purge した（§3-3）
- [ ] 追加したファイルはすべて CC BY 4.0 で配布できる（§2）
- [ ] `.meta` ファイルを一緒にコミットした（欠けると他プロジェクトで参照が壊れます）
- [ ] 配布内容を変えたなら `Assets/LotteryBallKit/package.json` の `version` と `CHANGELOG.md` を更新した
