from recorderapp.models import Game, Day, Frame
from rest_framework import serializers

class GameSerializer(serializers.ModelSerializer):
    class Meta:
        model = Game
        fields = ['id', 'mapName', 'numDays', 'gameOverReason', 'players', 'time']

class DaySerializer(serializers.ModelSerializer):
    class Meta:
        model = Day
        fields = ['id', 'dayId', 'numFrames', 'deadPlayers', 'exiledPlayers']

class FrameSerializer(serializers.ModelSerializer):
    class Meta:
        model = Frame
        fields = ['id', 'frameId', 'eventId', 'time', 'players', 'customField']