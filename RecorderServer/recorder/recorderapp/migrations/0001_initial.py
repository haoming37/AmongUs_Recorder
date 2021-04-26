# Generated by Django 3.1.7 on 2021-04-25 11:31

from django.db import migrations, models


class Migration(migrations.Migration):

    initial = True

    dependencies = [
    ]

    operations = [
        migrations.CreateModel(
            name='Day',
            fields=[
                ('id', models.AutoField(auto_created=True, primary_key=True, serialize=False, verbose_name='ID')),
                ('frames', models.TextField()),
                ('deadplayers', models.TextField()),
                ('exiledPlayers', models.TextField()),
            ],
            options={
                'ordering': ['id'],
            },
        ),
        migrations.CreateModel(
            name='Frame',
            fields=[
                ('id', models.AutoField(auto_created=True, primary_key=True, serialize=False, verbose_name='ID')),
                ('eventid', models.IntegerField(default=0)),
                ('time', models.DateTimeField()),
                ('players', models.TextField()),
                ('customField', models.TextField(blank=True)),
            ],
            options={
                'ordering': ['id'],
            },
        ),
        migrations.CreateModel(
            name='Game',
            fields=[
                ('id', models.AutoField(auto_created=True, primary_key=True, serialize=False, verbose_name='ID')),
                ('mapName', models.CharField(max_length=255)),
                ('days', models.TextField()),
                ('gameOverReason', models.IntegerField()),
                ('players', models.TextField()),
                ('time', models.DateTimeField()),
            ],
            options={
                'ordering': ['id'],
            },
        ),
    ]
