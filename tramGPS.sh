#!/bin/bash

# Adresse de broadcast
BROADCAST_ADDRESS="192.168.1.255"
PORT="9001"

# Boucle infinie pour envoyer une trame GPS NMEA simulée toutes les secondes
while true; do
  # Trame NMEA simulée (ex: GPGGA pour une position GPS)
  NMEA_TRAME="\$GPGGA,123519,4807.038,N,01131.000,E,1,12,1.0,500.0,M,46.9,M,,*47"
  
  # Envoi de la trame en UDP en broadcast
  echo "$NMEA_TRAME" | socat - UDP-DATAGRAM:$BROADCAST_ADDRESS:$PORT,broadcast
  
  # Afficher la trame envoyée dans le terminal
  echo "Trame envoyée: $NMEA_TRAME"
  
  # Attendre 1 seconde avant d'envoyer la suivante
  sleep 1
done

