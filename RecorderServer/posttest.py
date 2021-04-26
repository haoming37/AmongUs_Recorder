import requests
import json
import datetime

# game
#url = 'http://localhost:8000/recorder/games/'
#data = {}
#data['mapName'] = 'Airship'
#data['gameOverReason'] = 0
#data['players'] = '[]'
#data['days'] = '[]'
#data['time'] = datetime.datetime.now().isoformat()

# days
url = 'http://localhost:8000/recorder/games/1/days/'
data = {}
data['frames'] = '[]'
data['deadPlayers'] = '[]'
data['exiledPlayers'] = '[]'

# frames
#url = 'http://localhost:8000/recorder/games/1/days/1/frames/'
#data = {}
#data['eventId'] = 0
#data['time'] = datetime.datetime.now().isoformat()
#data['players'] = '[]'
#data['customField'] = 'aaaa'

print(data)
data = json.dumps(data)
ret = requests.post(url, data=data)
print(ret)
