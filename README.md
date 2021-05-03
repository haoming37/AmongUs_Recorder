<!-- TOC -->

- [クライアント導入方法](#クライアント導入方法)
  - [前提](#前提)
  - [ビルド](#ビルド)
- [サーバー起動方法](#サーバー起動方法)
  - [前提](#前提-1)
  - [起動準備](#起動準備)
  - [起動](#起動)
  - [API仕様](#api仕様)
    - [games/](#games)
    - [games/\<id\>/](#gamesid)
    - [games/\<id\>/days/](#gamesiddays)
    - [games/\<id\>/days/\<id\>/](#gamesiddaysid)
- [動画作成方法](#動画作成方法)

<!-- /TOC -->
# クライアント導入方法
## 前提
- BepInEx導入済  
- dotnetコマンド導入済  
- 環境変数AMONGUSにAmong Usのインストールディレクトリ登録済

##  ビルド
ビルドするとBePInExのpluginsディレクトリに自動で展開される
```
$ git clone https://github.com/haoming37/AmongUs_Recorder.git
$ cd AmongUs_Recorder
$ git submodule init
$ git submodule update
$ cd RecorderClient
$ dotnet restore
$ dotnet build
```

# サーバー起動方法
## 前提
- Python3インストール済
- pipでdjango、djangoresetframeworkをインストール済

## 起動準備
```bash
$ cd RecorderServer/Recorder
$ python manage.py makemigrate recorderapp
$ python manage.py migrate
```
## 起動
クライアントからデータを溜め込むのみの場合はここまで実施する  
※DEBUGモードになるので外部公開する場合は起動方法を変更する
```bash
$ python manage.py runserver
```

## API仕様
ベースURL  
http://localhost:8080/recorder/

### games/
GET: ゲーム一覧のJSONを返却する  
POST: 新規ゲームを作成する


### games/\<id\>/
GET: IDで指定したゲームのJSONを返却する  
PUT: IDで指定したゲームのデータを更新する  
DELETE: IDで指定したゲームのデータを削除する

GET返り値サンプル
```json
 {"id": 115, "mapName": "MiraShip(Clone)", "numDays": 1, "gameOverReason": 0, "players": "[{\"name\":\"Lankypear\",\"role\":\"Sheriff\",\"isDead\":false,\"colorId\":2,\"playerId\":0,\"x\":-2.86335754,\"y\":2.584118,\"z\":0.002584118},{\"name\":\"Lankypear 1\",\"role\":\"Trickster\",\"isDead\":false,\"colorId\":3,\"playerId\":1,\"x\":-1.33745193,\"y\":2.48187637,\"z\":0.00248187641},{\"name\":\"Haoming\",\"role\":\"Time Master\",\"isDead\":false,\"colorId\":18,\"playerId\":2,\"x\":-1.07275391,\"y\":2.55553818,\"z\":0.0025555382},{\"name\":\"Lankypear 2\",\"role\":\"Engineer\",\"isDead\":false,\"colorId\":4,\"playerId\":3,\"x\":-0.8169498,\"y\":2.638719,\"z\":0.002638719}]", "time": "2021-04-28T06:17:56.676263Z"}, {"id": 117, "mapName": "Airship(Clone)", "numDays": 1, "gameOverReason": 0, "players": "[{\"name\":\"Lankypear\",\"role\":\"Engineer\",\"isDead\":false,\"colorId\":2,\"playerId\":0,\"x\":-1.1808449,\"y\":2.43470883,\"z\":0.00243470888},{\"name\":\"Lankypear 1\",\"role\":\"Sheriff\",\"isDead\":false,\"colorId\":3,\"playerId\":1,\"x\":-1.37865067,\"y\":2.43457413,\"z\":0.002434574},{\"name\":\"Lankypear 2\",\"role\":\"Time Master\",\"isDead\":false,\"colorId\":4,\"playerId\":3,\"x\":-0.8613701,\"y\":2.58869553,\"z\":0.00258869561},{\"name\":\"Haoming\",\"role\":\"Trickster\",\"isDead\":false,\"colorId\":18,\"playerId\":2,\"x\":-1.0725863,\"y\":2.555186,\"z\":0.002555186}]", "time": "2021-04-28T06:33:00.667447Z"}]
```

### games/\<id\>/days/
GET: IDで指定したゲーム内のDay一覧のJSONを返却する  
POST: 新規Dayを作成する

### games/\<id\>/days/\<id\>/
GET:IDで指定したDayのJSONを返却する  
PUT:IDで指定したDayのデータを更新する  
DELETE:IDで指定したDayのデータを削除する

GET返り値サンプル
```json
{"id": 176, "dayId": 1, "numFrames": 1750, "deadPlayers": "[3]", "exiledPlayers": "[]"}
```

# 動画作成方法

##　動画にしたいゲームのIDを確認する
```
$ ./Player/player.py --gameid=<gameId>
```