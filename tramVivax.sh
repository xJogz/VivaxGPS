#!/bin/bash

socat -d -d PTY,link=/tmp/vivax1,raw,echo=0 PTY,link=/tmp/vivax2,raw,echo=0 &

# Définir le port virtuel
PORT="/tmp/vivax2"

# Boucle infinie pour envoyer la trame toutes les secondes
while true; do
    echo "LOG, LOC2, 14303, 8192, 0, 0, 71, 3, 2, 4, 140, 0" > $PORT
    echo "Trame envoyée : LOG, LOC2, 14303, 8192, 0, 0, 71, 3, 2, 4, 140, 0"
    sleep 2  # Attente d'une seconde avant d'envoyer la prochaine trame
done
