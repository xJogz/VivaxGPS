#!/bin/bash

# Adresse de broadcast
BROADCAST_ADDRESS="92.142.1.255"
PORT="9001"

# Boucle infinie pour envoyer une trame GPS NMEA simulée toutes les secondes
while true; do
  # Trame NMEA simulée (ex: GPGGA pour une position GPS)
  NMEA_TRAME="$GPGGA,123519,4807.038,N,01131.000,E,1,12,1.0,500.0,M,46.9,M,,*47"
  
  # Envoi de la trame en UDP en broadcast
  echo "\$GPGGA,123519,4807.038,N,01131.000,E,1,12,1.0,500.0,M,46.9,M,,*47" | nc -u -b $BROADCAST_ADDRESS $PORT
  
  # Attendre 1 seconde avant d'envoyer la suivante
  sleep 1
done

