from django.urls import path
from recorderapp import views

urlpatterns = [
    path('games/', views.games),
    path('games/<int:gameId>/', views.games_id),
    path('games/<int:gameId>/days/', views.days),
    path('games/<int:gameId>/days/<int:dayId>/', views.days_id),
    path('games/<int:gameId>/days/<int:dayId>/frames/', views.frames),
    path('games/<int:gameId>/days/<int:dayId>/frames/<int:frameId>/', views.frames_id),
]