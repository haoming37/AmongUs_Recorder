from django.urls import path
from viewerapp import views

urlpatterns = [
    path('', views.index),
]