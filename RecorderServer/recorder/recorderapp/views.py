from django.http import HttpResponse, JsonResponse
from django.views.decorators.csrf import csrf_exempt
from rest_framework.parsers import JSONParser
from recorderapp.models import Game, Day, Frame
from recorderapp.serializers import GameSerializer, DaySerializer, FrameSerializer
import json

# path('games/', views.games),
@csrf_exempt
def games(request):
    if request.method == 'GET':
        games = Game.objects.all()
        serializer = GameSerializer(games, many=True)
        return JsonResponse(serializer.data, safe=False)

    if request.method == 'POST':
        data = JSONParser().parse(request)
        serializer = GameSerializer(data=data)
        if serializer.is_valid():
            serializer.save()
            return JsonResponse(serializer.data, status=201)
        return JsonResponse(serializer.errors, status=400)

# path('games/<int:gameId>/', views.games_id),
@csrf_exempt
def games_id(request, gameId):
    game = Game.objects.get(id=gameId)
    if request.method == 'GET':
        serializer = GameSerializer(game)
        return JsonResponse(serializer.data, safe=False)

    if request.method == 'PUT':
        data = JSONParser().parse(request)
        serializer = GameSerializer(game, data=data)
        if serializer.is_valid():
            serializer.save()
            return JsonResponse(serializer.data)
        return JsonResponse(serializer.errors, status=400)


# path('games/<int:gameId>/days', views.days),
@csrf_exempt
def days(request, gameId):
    game = Game.objects.get(id=gameId)
    if request.method == 'GET':
        days = []
        dayIdList = json.loads(game.days)
        for dayId in dayIdList:
            print(dayId)
            day = Day.objects.get(id=dayId)
            day = DaySerializer(day)
            days.append(day.data)
        return JsonResponse(days, safe=False)

    if request.method == 'POST':
        data = JSONParser().parse(request)
        days = json.loads(game.days)
        print(days)
        data['dayId'] = len(days) + 1
        serializer = DaySerializer(data=data)
        if serializer.is_valid():
            serializer.save()
            days.append(serializer.data['id'])
            game.days = json.dumps(days)
            game.numDays = len(days)
            game.save()
            return JsonResponse(serializer.data, status=201)
        return JsonResponse(serializer.errors, status=400)

# path('games/<int:gameId>/days/<int:dayId>', views.days_id),
@csrf_exempt
def days_id(request, gameId, dayId):
    game = Game.objects.get(id=gameId)
    dayIdList = json.loads(game.days)
    day = Day.objects.get(id=dayIdList[dayId-1])

    if request.method == 'GET':
        day = DaySerializer(day)
        return JsonResponse(day.data, safe=False)

    if request.method == 'PUT':
        data = JSONParser().parse(request)
        serializer = DaySerializer(day, data=data)
        if serializer.is_valid():
            serializer.save()
            return JsonResponse(serializer.data, status=201)
        return JsonResponse(serializer.errors, status=400)
    
    if request.method == 'DELETE':
        for frameId in json.loads(day['frames']):
            frame = Frame.objects.get(id=frameId)
            frame.delete()
        day.delete()
            

# path('games/<int:gameId>/days/<int:dayId>/frames', views.frames),
@csrf_exempt
def frames(request, gameId, dayId):
    game = Game.objects.get(id=gameId)
    dayIdList = json.loads(game.days)
    day = Day.objects.get(id=dayIdList[dayId -1])
    framesList = json.loads(day.frames)

    if request.method == 'GET':
        frames = []
        for frameId in framesList:
            frame = Frame.objects.get(id=frameId)
            frame = FrameSerializer(frame)
            frames.append(frame.data)
        return JsonResponse(frames, safe=False)

    if request.method == 'POST':
        # 受け取ったデータを処理
        data = JSONParser().parse(request)

        frames = []

        # 単一データの場合
        if isinstance(data, dict):
            frames.append(data)
        # 複数データの場合
        elif isinstance(data, list):
            frames = data
        flag = True
        for data in frames:
            data['frameId'] = len(framesList) + 1
            serializer = FrameSerializer(data=data)
            if serializer.is_valid():
                serializer.save()
                framesList.append(serializer.data['id'])
                day.frames = json.dumps(framesList)
                day.numFrames = len(framesList)
                print(day.numFrames)
                day.save()
            else:
                flag = False
        if flag:
            return JsonResponse(serializer.data, status=201)
        return JsonResponse(serializer.errors, status=400)
    pass

# path('games/<int:gameId>/days/<int:dayId>/frames/<int:frameId>', views.frames_id),
@csrf_exempt
def frames_id(request, gameId, dayId, frameId):
    game = Game.objects.get(id=gameId)
    dayIdList = json.loads(game.days)
    day = Day.objects.get(id=dayIdList[dayId -1])
    frameIdList = json.loads(day.frames)
    frame = Frame.objects.get(id=frameIdList[frameId -1])

    if request.method == 'GET':
        serializer = FrameSerializer(frame)
        return JsonResponse(serializer.data, safe=False)

    if request.metho == 'PUT':
        data = JSONParser().parse(request)
        serializer = FrameSerializer(frame, data=data)
        if serializer.is_valid():
            serializer.save()
            return JsonResponse(serializer.data, status=201)   
        return JsonResponse(serializer.errors, status=400)


