# Generated by Django 3.1.7 on 2021-04-25 21:44

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('recorderapp', '0003_auto_20210425_2225'),
    ]

    operations = [
        migrations.AddField(
            model_name='day',
            name='numFrames',
            field=models.IntegerField(default=0),
        ),
        migrations.AddField(
            model_name='game',
            name='numDays',
            field=models.IntegerField(default=0),
        ),
        migrations.AlterField(
            model_name='day',
            name='deadPlayers',
            field=models.TextField(default='[]'),
        ),
        migrations.AlterField(
            model_name='day',
            name='exiledPlayers',
            field=models.TextField(default='[]'),
        ),
        migrations.AlterField(
            model_name='day',
            name='frames',
            field=models.TextField(default='[]'),
        ),
        migrations.AlterField(
            model_name='frame',
            name='players',
            field=models.TextField(default='[]'),
        ),
        migrations.AlterField(
            model_name='game',
            name='players',
            field=models.TextField(blank=True, default=''),
        ),
    ]
