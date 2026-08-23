# Changelog

All notable changes to LotteryBallKit are documented here.
書式は [Keep a Changelog](https://keepachangelog.com/ja/1.1.0/) に、バージョンは [SemVer](https://semver.org/lang/ja/) に従います。

## [1.0.0] - 2026-08-23

### Added

- `NumberBall.prefab`（径0.1m・Rigidbody CCD・SphereCollider）と `NumberBall.cs`
- 番号アトラス `NumberAtlas.png`（0〜99・10×10）とアトラス生成スクリプト `gen_number_atlas.py`
- スキン作画用UVガイド `BallUV_Template.png` とサンプルスキン `BallSkins_Sample.png`
- Blender 原本の同梱（`Sources/LotteryBall.blend.bytes`）
- UPM パッケージ化（`package.json` / `LotteryBallKit.asmdef`）

### Notes

- RouletteSphereChaser の抽選ボールを単体アセットとして切り出したもの
- 全体を CC BY 4.0 で提供
