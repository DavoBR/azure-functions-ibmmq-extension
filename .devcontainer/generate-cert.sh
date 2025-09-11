#!/usr/bin/bash
# Run this script in simple init container
# to generate self-signed certs for dev use

set -e

CERT_DIR="./certs"
CERT_KEY="${CERT_DIR}/dev.key.pem"
CERT_CRT="${CERT_DIR}/dev.cert.pem"
# CERT_NAME="dev-cert"

mkdir -p "$CERT_DIR"

# Generate self-signed cert if it doesn't exist or is an empty file
# The Git repo contains a placeholder empty files to prevent timing issues
if [ ! -f "$CERT_KEY" ] || [ ! -s "$CERT_KEY" ] || [ ! -f "$CERT_CRT" ] || [ ! -s "$CERT_CRT" ]; then
    echo "Generating self-signed dev certificate..."
    openssl req -x509 -nodes -days 365 \
        -newkey rsa:2048 \
        -keyout "$CERT_KEY" \
        -out "$CERT_CRT" \
        -subj "/CN=localhost"
    echo "Certificate generated at $CERT_DIR"
fi

# Install certificate to trusted store
# echo "Installing certificate to trusted store..."
# cp "$CERT_CRT" "/usr/local/share/ca-certificates/${CERT_NAME}.crt"
# update-ca-certificates
# echo "Certificate trusted."