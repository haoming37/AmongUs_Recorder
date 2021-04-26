from django.db import models

# Create your models here.

class Game(models.Model):
    mapName = models.CharField(max_length=255)
    numDays = models.IntegerField(default=0)
    days = models.TextField(default='[]')
    gameOverReason = models.IntegerField()
    players = models.TextField(default='', blank=True)
    time = models.DateTimeField()

    class Meta:
        ordering = ['id']

class Day(models.Model):
    dayId = models.IntegerField(default=0)
    numFrames = models.IntegerField(default=0)
    frames = models.TextField(default='[]')
    deadPlayers = models.TextField(default='[]')
    exiledPlayers = models.TextField(default='[]')

    class Meta:
        ordering = ['id']

class Frame(models.Model):
    frameId = models.IntegerField(default=0)
    eventId = models.IntegerField(default=0)
    time = models.DateTimeField()
    players = models.TextField(default='[]')
    customField = models.TextField(blank=True)

    class Meta:
        ordering = ['id']