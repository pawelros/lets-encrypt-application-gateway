#!/bin/sh

# certbot renew --deploy-hook /path/to/deploy-hook-script

set -e

for domain in $RENEWED_DOMAINS; do
        case $domain in
        example.com)
                daemon_cert_root=/etc/some-daemon/certs

                # Make sure the certificate and private key files are
                # never world readable, even just for an instant while
                # we're copying them into daemon_cert_root.
                umask 077

                cp "$RENEWED_LINEAGE/fullchain.pem" "$daemon_cert_root/$domain.cert"
                cp "$RENEWED_LINEAGE/privkey.pem" "$daemon_cert_root/$domain.key"

                # Apply the proper file ownership and permissions for
                # the daemon to read its certificate and key.
                chown some-daemon "$daemon_cert_root/$domain.cert" \
                        "$daemon_cert_root/$domain.key"
                chmod 400 "$daemon_cert_root/$domain.cert" \
                        "$daemon_cert_root/$domain.key"

                service some-daemon restart >/dev/null
                ;;
        esac
done