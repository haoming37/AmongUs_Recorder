import json
import time
import requests
from enum import Enum
import cv2
import matplotlib.pyplot as plt
import matplotlib.animation as animation
import numpy as np
from PIL import Image
from PIL import ImageDraw
from PIL import ImageFont

# 文字描画用リソース
font = ImageFont.truetype('meiryo.ttc', 16)
font_overlay = ImageFont.truetype('meiryob.ttc', 16, index=2)

# Amongus座標系の1の重みがおおよそ画像での12pixel
# Airship画像の場合の位置
# OFFSET Airship(470.5, 330) 重み 18
# OFFSET Polus(50, 100) 重み26
def convy(y):
    return int(220 - y*26) # skeld
    return int(750 - y*26) # mira
    return int(100 - y*26) # polus
    return int(1.5*(220 - (y*12))) # airship

def convx(x):
    return int(670 + 26*x) # skeld
    return int(375 + 26*x) # mira
    return int(50 + 26*x) # polus
    return int(1.5*((x*12) + 315)) # airship
    
def getColorById(colorId):
    return {
        0: (255,255,255),
        1: (255,120,0),
        2: (0,255,0),
        3: (0,0,255),
        4: (0,0,0),
        5: (255,255,0),
        6: (255,0,255),
        7: (0,255,255)
    }.get(colorId, )

class GameOverReason(Enum):
    HumansByVote = 0
    HumansByTask = 1
    ImpostorByVote =2
    ImpostorByKill = 3
    ImpostorBySabotage = 4
    ImpostorDisconnect = 5
    HumansDisconnect = 6

def getAliveImageById(colorId):
    return{
        0: cv2.imread('emojis/aured.png', cv2.IMREAD_UNCHANGED),
        1: cv2.imread('emojis/aublue.png', cv2.IMREAD_UNCHANGED),
        2: cv2.imread('emojis/aupink.png', cv2.IMREAD_UNCHANGED),
        3: cv2.imread('emojis/augreen.png', cv2.IMREAD_UNCHANGED),
        4: cv2.imread('emojis/auorange.png', cv2.IMREAD_UNCHANGED),
        5: cv2.imread('emojis/auyellow.png', cv2.IMREAD_UNCHANGED),
        6: cv2.imread('emojis/aublack.png', cv2.IMREAD_UNCHANGED),
        7: cv2.imread('emojis/auwhite.png', cv2.IMREAD_UNCHANGED),
        8: cv2.imread('emojis/aupurple.png', cv2.IMREAD_UNCHANGED),
        9: cv2.imread('emojis/aubrown.png', cv2.IMREAD_UNCHANGED),
        10: cv2.imread('emojis/aucyan.png', cv2.IMREAD_UNCHANGED),
        11:  cv2.imread('emojis/aulime.png', cv2.IMREAD_UNCHANGED),
        12: cv2.imread('emojis/aured.png', cv2.IMREAD_UNCHANGED),
        13: cv2.imread('emojis/aublue.png', cv2.IMREAD_UNCHANGED),
        14: cv2.imread('emojis/aupink.png', cv2.IMREAD_UNCHANGED),
        15: cv2.imread('emojis/augreen.png', cv2.IMREAD_UNCHANGED),
        16: cv2.imread('emojis/auorange.png', cv2.IMREAD_UNCHANGED),
        17: cv2.imread('emojis/auyellow.png', cv2.IMREAD_UNCHANGED),
        18: cv2.imread('emojis/aublack.png', cv2.IMREAD_UNCHANGED),
        19: cv2.imread('emojis/auwhite.png', cv2.IMREAD_UNCHANGED),
        20: cv2.imread('emojis/aupurple.png', cv2.IMREAD_UNCHANGED),
        21: cv2.imread('emojis/aubrown.png', cv2.IMREAD_UNCHANGED),
        22: cv2.imread('emojis/aucyan.png', cv2.IMREAD_UNCHANGED),
        23:  cv2.imread('emojis/aulime.png', cv2.IMREAD_UNCHANGED),
    }.get(colorId)

def getDeadImageById(colorId):
    return{
        0: cv2.imread('emojis/aureddead.png', cv2.IMREAD_UNCHANGED),
        1: cv2.imread('emojis/aubluedead.png', cv2.IMREAD_UNCHANGED),
        2: cv2.imread('emojis/aupinkdead.png', cv2.IMREAD_UNCHANGED),
        3: cv2.imread('emojis/augreendead.png', cv2.IMREAD_UNCHANGED),
        4: cv2.imread('emojis/auorangedead.png', cv2.IMREAD_UNCHANGED),
        5: cv2.imread('emojis/auyellowdead.png', cv2.IMREAD_UNCHANGED),
        6: cv2.imread('emojis/aublackdead.png', cv2.IMREAD_UNCHANGED),
        7: cv2.imread('emojis/auwhitedead.png', cv2.IMREAD_UNCHANGED),
        8: cv2.imread('emojis/aupurpledead.png', cv2.IMREAD_UNCHANGED),
        9: cv2.imread('emojis/aubrowndead.png', cv2.IMREAD_UNCHANGED),
        10: cv2.imread('emojis/aucyandead.png', cv2.IMREAD_UNCHANGED),
        11: cv2.imread('emojis/aulimedead.png', cv2.IMREAD_UNCHANGED),
        12: cv2.imread('emojis/aureddead.png', cv2.IMREAD_UNCHANGED),
        13: cv2.imread('emojis/aubluedead.png', cv2.IMREAD_UNCHANGED),
        14: cv2.imread('emojis/aupinkdead.png', cv2.IMREAD_UNCHANGED),
        15: cv2.imread('emojis/augreendead.png', cv2.IMREAD_UNCHANGED),
        16: cv2.imread('emojis/auorangedead.png', cv2.IMREAD_UNCHANGED),
        17: cv2.imread('emojis/auyellowdead.png', cv2.IMREAD_UNCHANGED),
        18: cv2.imread('emojis/aublackdead.png', cv2.IMREAD_UNCHANGED),
        19: cv2.imread('emojis/auwhitedead.png', cv2.IMREAD_UNCHANGED),
        20: cv2.imread('emojis/aupurpledead.png', cv2.IMREAD_UNCHANGED),
        21: cv2.imread('emojis/aubrowndead.png', cv2.IMREAD_UNCHANGED),
        22: cv2.imread('emojis/aucyandead.png', cv2.IMREAD_UNCHANGED),
        23:  cv2.imread('emojis/aulimedead.png', cv2.IMREAD_UNCHANGED),
    }.get(colorId)

def getTextPos(pos, length):
    width = 8
    height = 10
    y = pos[1] - height
    x = pos[0] - (width/2) - (width*length/2)
    return (int(x), int(y))

def unique(in_list):
    tmp_list = []
    for x in in_list:
        if not x in tmp_list:
            tmp_list.append(x)
    return tmp_list

def main():

    # JSON読み込み
    #with open('replay08.json') as f:
    #    game = json.load(f)
    #mapname = game['map']

    # サーバーからJSONファイルを読み込み
    url = 'http://localhost:8000/recorder/games/'
    gameId = 87
    res = requests.get(url + str(gameId) + "/")
    content = res.content.decode()
    content = json.loads(content)
    content['players'] = json.loads(content['players'])
    content['days'] = []
    for i in range(content['numDays']):
        dayId = i +1
        res = requests.get(url + str(gameId) + "/days/" + str(dayId) + "/")
        day = json.loads(res.content.decode())
        res = requests.get(url + str(gameId) + "/days/" + str(dayId) + "/frames/")
        day['deadPlayers'] = json.loads(day['deadPlayers'])
        day['exiledPlayers'] = json.loads(day['exiledPlayers'])
        day['frames'] = json.loads(res.content.decode())
        for index in range(len(day['frames'])):
            day['frames'][index]['players'] = json.loads(day['frames'][index]['players'])

        content['days'].append(day)

    # マップ画像読み込み
    game = content
    mapName = game['mapName']
    if mapName == "AirShip(Clone)":
        org_img = cv2.imread('map/airship.png')
    elif mapName == "PolusShip(Clone)":
        org_img = cv2.imread('map/polus.png')
    elif mapName == "SkeldShip(Clone)":
        org_img = cv2.imread('map/skeld.png')
    elif mapName == "MiraShip(Clone)":
        org_img = cv2.imread('map/mira.png')
    
    org_img = cv2.cvtColor(org_img, cv2.COLOR_BGR2RGB)


    # 動画生成準備
    fourcc = cv2.VideoWriter_fourcc('m', 'p', '4', 'v')
    # video = cv2.VideoWriter('video.mp4', fourcc, 20.0, (1200, 638))
    video = cv2.VideoWriter('video.mp4', fourcc, 20.0, (1280, 720))

    for day in game['days']:

        # 投票勝利の場合は最終日をスキップする
        index = game['days'].index(day)
        gameOverReason = game['gameOverReason']
        if index + 1 == len(game['days']):
            if gameOverReason == GameOverReason.HumansByVote or GameOverReason.ImpostorByVote:
                continue
            
        # この日に死んだプレイヤーの場所を保持する
        deadPlayers = {}

        # この日以前に死んだプレイヤーのID
        if index > 0:
            killedPlayers = game['days'][index -1]['deadPlayers']
        else:
            killedPlayers = []
        exiledPlayers = day['exiledPlayers']
        print('この日以前にキルされたプレイヤー')
        print(killedPlayers)
        print('この日以前に追放されたプレイヤー')
        print(exiledPlayers)

        # 初期フレーム時のキャラ位置
        pos1stframe = {}
        frame = day['frames'][0]
        for player in frame['players']:
            playerid = player['playerId']
            x = player['x']
            y = player['y']
            z = player['z']
            pos1stframe[playerid] = [x, y, z]

        # フレームごとの画像を作成
        for frame in day['frames']:
            # マップ画像
            img = org_img.copy()
            src = Image.fromarray(img)
            src = src.convert('RGBA')

            # オーバーレイインフォ用テキスト
            info = ""
            info += "Day" + str(index +1) + "\n"

            # プレイヤーアイコン描画
            for player in frame['players']:
                role = player['role']
                name = player['name']
                playerid = player['playerId']
                x = convx(player['x'])
                y = convy(player['y'])
                center = (x, y)

                # オーバーレイにプレイヤー情報を追加
                text = name + "(" + role + "): " 
                if player['isDead']:
                    text += "死亡"
                else:
                    text += "生存中"
                info += text + '\n'

                # 前日から死んでいる場合は描画しない
                playerid = player['playerId']
                if playerid in killedPlayers or playerid in exiledPlayers:
                    continue

                # 初期位置から動いていない場合は描画しない
                pos = pos1stframe[player['playerId']]
                if pos[0] == player['x'] and pos[1] == player['y'] and pos[2] == player['z']:
                    continue

                if player['isDead']:
                    if not playerid in  deadPlayers:
                        deadPlayers[playerid] = center
                    center = deadPlayers[playerid]
                    icon = getDeadImageById(player['colorId'])
                else:
                    icon = getAliveImageById(player['colorId'])
                icon = cv2.cvtColor(icon, cv2.COLOR_BGRA2RGBA)
                icon = Image.fromarray(icon)
                icon = icon.convert('RGBA')
                tmp = Image.new('RGBA', src.size, (255, 255, 255, 0))
                tmp.paste(icon, (int(center[0]-(icon.size[0])/2), int(center[1]-icon.size[1])), icon)
                src = Image.alpha_composite(src, tmp)
                # プレイヤー名描画
                textpos = getTextPos(center, len(name))
                draw = ImageDraw.Draw(src)
                draw.text(textpos, name, font=font, fill=(255,255,255,255))

            # オーバーレイを描画
            overlay = Image.new('RGBA', src.size)
            draw = ImageDraw.Draw(overlay)
            draw.rectangle([(0,0), (500,250)], fill=(255,255,255,100), outline=(255,255,255,255), width=2)
            draw.text((10,10), info, font=font_overlay, fill=(0,0,0,255))
            src = Image.alpha_composite(src, overlay)

            # 動画に書き込み
            img = cv2.cvtColor(np.asarray(src), cv2.COLOR_RGBA2BGR)
            img = cv2.resize(img, (1280, 720))
            for i in range(2):
                video.write(img)

        # 会議結果をオーバーレイに描画するためのリソース準備
        img = org_img.copy()
        src = Image.fromarray(img)
        src = src.convert('RGBA')
        text = ""
        players = day['frames'][0]['players']
        index = game['days'].index(day)

        # この日に殺されたプレイヤーを取得
        print(day['deadPlayers'])
        if len(day['deadPlayers']) > 0:
            text += "死んだプレイヤー:"
            for playerid in day['deadPlayers']:
                if not playerid in killedPlayers:
                    if not playerid in exiledPlayers:
                        for player in players:
                            if player['playerId'] == playerid:
                                text += player['name'] + "(" + player['role'] + ") "
            text += "\n"

        # この日に追放されたプレイヤーを取得
        index = game['days'].index(day)
        if index > 0:
            tomorrow = game['days'][index+1]
            counter = 0
            if len(tomorrow['exiledPlayers']) != 0:
                for playerid in tomorrow['exiledPlayers']:
                    if not playerid in day['exiledPlayers']:
                        for player in players:
                            if player['playerId'] == playerid:
                                counter += 1
                                text += "追放されたプレイヤー:"
                                text += player['name'] + "(" + player['role'] + ")" + "\n"
            if counter == 0:
                text += "会議がスキップされました\n"


        # 会議結果をオーバーレイに描画
        overlay = Image.new('RGBA', src.size)
        draw = ImageDraw.Draw(overlay)
        sizex = 500
        sizey = 250
        x = int(src.size[0]/2 - sizex/2)
        y = int(src.size[1]/2 - sizey/2)
        draw.rectangle([(x,y), (x+sizex,y+sizey)], fill=(255,255,255,100), outline=(255,255,255,255), width=2)
        draw.text((x+10,y+10), text, font=font_overlay, fill=(0,0,0,255))
        src = Image.alpha_composite(src, overlay)

        # 動画に書き込み
        img = cv2.cvtColor(np.asarray(src), cv2.COLOR_RGBA2BGR)
        img = cv2.resize(img, (1280, 720))
        for i in range(200):
            video.write(img)

    # 試合結果をオーバーレイに描画
    img = org_img.copy()
    src = Image.fromarray(img)
    src = src.convert('RGBA')
    text = "試合終了\n"
    reason = GameOverReason(game['gameOverReason'])
    if reason == GameOverReason.HumansByTask or reason == GameOverReason.HumansByVote or reason == GameOverReason.HumansDisconnect:
        text += "クルーメイトの勝利"
    else:
        text += "インポスターの勝利"
    overlay = Image.new('RGBA', src.size)
    draw = ImageDraw.Draw(overlay)
    sizex = 500
    sizey = 250
    x = int(src.size[0]/2 - sizex/2)
    y = int(src.size[1]/2 - sizey/2)
    draw.rectangle([(x,y), (x+sizex,y+sizey)], fill=(255,255,255,100), outline=(255,255,255,255), width=2)
    draw.text((x+10,y+10), text, font=font_overlay, fill=(0,0,0,255))
    src = Image.alpha_composite(src, overlay)
    img = cv2.cvtColor(np.asarray(src), cv2.COLOR_RGBA2BGR)
    img = cv2.resize(img, (1280, 720))
    for i in range(200):
        video.write(img)

    # 動画作成終了
    video.release()

if __name__ == "__main__":
    main()