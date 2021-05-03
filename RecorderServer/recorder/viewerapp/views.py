from django.shortcuts import render
from django.template import loader
from django.http import HttpResponse
from django.views.decorators.csrf import csrf_exempt
from recorderapp.models import Game
from recorderapp.serializers import GameSerializer

# Create your views here.

@csrf_exempt
def index(request):
    game = GameSerializer(Game.objects.all(), many=True)
    data = []
    for g in game.data:
        tmp = {}
        tmp['id'] = g['id']
        tmp['mapName'] = g['mapName']
        tmp['time'] = g['time']
        tmp['gameOverReason'] = g['gameOverReason']
        data.append(tmp)
    print(data)
    template = loader.get_template('viewerapp/index.html')
    return  HttpResponse(template.render())
