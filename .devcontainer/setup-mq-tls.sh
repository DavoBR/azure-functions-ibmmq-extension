#!/usr/bin/bash
set -e

CERT_DIR="/etc/mqm/pki/keys"
CERT_FILE="$CERT_DIR/dev.cert.pem"
KEY_FILE="$CERT_DIR/dev.key.pem"
KDB_FILE="$CERT_DIR/key.kdb"
STH_FILE="$CERT_DIR/key.sth"
PKCS12_FILE="$CERT_DIR/dev.p12"
LABEL="ibmcert"
PASSWORD="passw0rd"
QMGR_NAME="QMGR"

if [ ! -f "$CERT_FILE" ] || [ ! -f "$KEY_FILE" ]; then
  echo "❌ Missing PEM cert or key in $CERT_DIR"
  exit 1
fi

echo "Setting up MQ key database inside container..."

# Clean up old files
rm -f "$KDB_FILE" "$STH_FILE" "$PKCS12_FILE"

# Create CMS key database
runmqakm -keydb -create -db "$KDB_FILE" -pw "$PASSWORD" -type cms -stash

# Convert PEM -> PKCS#12
openssl pkcs12 -export \
  -in "$CERT_FILE" \
  -inkey "$KEY_FILE" \
  -out "$PKCS12_FILE" \
  -name "$LABEL" \
  -passout pass:"$PASSWORD"

# Import PKCS#12 into KDB
runmqakm -cert -import -target "$KDB_FILE" -file "$PKCS12_FILE" -pw "$PASSWORD"

# Configure QMGR
echo "ALTER QMGR SSLKEYR('$CERT_DIR/key')" | runmqsc "$QMGR_NAME"
echo "DEFINE CHANNEL(DEV.SVRCONN) CHLTYPE(SVRCONN) TRPTYPE(TCP) SSLCIPH(TLS_RSA_WITH_AES_256_CBC_SHA256) SSLCAUTH(OPTIONAL)" | runmqsc "$QMGR_NAME"
echo "REFRESH SECURITY TYPE(SSL)" | runmqsc "$QMGR_NAME"

echo "MQ TLS setup complete."
